#!/usr/bin/env pwsh

<# 
.SYNOPSIS 

Formats the documents under the specified directory using pandoc.

.DESCRIPTION 

The Format-Documents.ps1 script formats source document files under the input directory specified by FromDir parameter.
The formatted document files are output in the output directory specified by ToDir parameter.

pandoc is used to format source document files.
pandoc 2.3 or later must be installed to run this script.
Formatting of each file is performed as follows:

1. pandoc reads a source document file and converts it into AST (Abstract Syntax Tree). 
2. The filter specified by Filter parameter modify the AST.
3. pandoc reads the filtered AST and writes it into the output directory in the target format. 

The relation of the extension of source document file, the format name of source document,
the extension of formatted document file and the format name of formatted document is specified by
FromExtensions, FromFormats, ToExtensions and ToFormats parameter respectively,
where "format name" means a value which can be passed to -f or -t option of pandoc.

The default filter provided along with this script make some changes to the input AST.

* It changes extension of a relative link in the AST. 

It changes the extension of a relative link in the input AST
if the extension is a target of extension mapping.
The extension mapping is defined by mapping from FromExtensions parameter to ToExtensions parameter and
OtherExtensionMap parameter.

For example, a link to 'a.md' in a source document file is converted to 'a.html'
if FromExtensions is @('.md') and ToExtension is @('.html').

.PARAMETER FromDir 

The directory where the source document files to be formatted are located. 

.PARAMETER FromExtensions 

The array of extension for source document files.

.PARAMETER FromFormats 

The array of format name for source document files.
Each name must be one which can be passed to -f option of pandoc.
Each item in this parameter corresponds to the item in FromExtensions respectively.
The array length of this parameter must be same to the one of FromExtensions.

.PARAMETER ToDir 

The directory where the formatted document files are going to be stored.

.PARAMETER ToExtensions

The array of extension for formatted document files.
Each item in this parameter corresponds to the item in FromExtensions respectively.
The array length of this parameter must be same to the one of FromExtensions.

.PARAMETER ToFormats 

The array of format for formatted document files.
Each value must be one which can be passed to -f option of pandoc.
Each item in this argument corresponds to the item in FromExtensions respectively.
The array length of this parameter must be same to the one of FromExtensions.

.PARAMETER MetadataFiles

The array of yaml file which describes metadata for this formatting.
The all specified metadata are attached to all source document files. 

.PARAMETER Filter 

The command line of the pandoc filter to be used.
If this parameter is empty string, "dotnet $scriptDir/FormatAST.dll" is used,
where $scriptDir is the directory where this script is located.

.PARAMETER RebaseOtherRelativeLinks 

If this parameter is True, relative links to files which are not target of extension mapping
should be rebased so that the links keep to reference the files in the original location.

If this parameter is false, this script copys such files into the output directory
along with the formatted files, and do not rebase links to the files.

.PARAMETER OtherExtensionMap 

The extension mappings other than the pair of FromExtensions and ToExtensions.
This script replaces those extensions in relative links according the extension mapping.

By specifying a mapping from an extension to the same extension, for example '.yaml'='.yaml',
files with the extension are excluded from the target of rebasing by RebaseOtherRelativeLinks option,
suppressing the extension change of relative links to the files. 

.PARAMETER OtherReadOptions 

The array of other read option to be passed to pandoc
when it is invoked to read the source document files.

.PARAMETER OtherWriteOptions 

The array of other write option to be passed to pandoc
when it is invoked to write the formatted document files.

.PARAMETER Rebuild

If this parameter is True, this script formats all source document files.
If this parameter is False, this script formats only updated files.

.PARAMETER Silent

If this parameter is True, this script does not display progress.
If this parameter is False, this script displays progress, that is name of processed file.

.PARAMETER NoOutput

If this parameter is True, this script returns no object.
If this parameter is False, this script returns an object which stores the list of processed file.

.INPUTS

None. You cannot pipe input to this script.

.OUTPUTS

None or PSCustomObject.

This script outputs none if NoOutput parameter is True.
Otherwise it outputs an object which has following properties:

* Copied: The list of file which this script copies to the output directory.
* Failed: The list of file for which this script fails in processing.
* Formatted: The list of file which this script successfully formats.
* NotTarget: The list of file which this script does not process.
* UpToDate: The list of file for which this script skips processing because it is up-to-date.

The files are represented in relative path from the input directory.
#>
param (
    [string]$FromDir = '../md',
    [string[]]$FromExtensions = @('.md'),
    [string[]]$FromFormats = @('markdown'),
    [string]$ToDir = '../html',
    [string[]]$ToExtensions = @('.html'),
    [string[]]$ToFormats = @('html'),
    [string[]]$MetadataFiles = @(),
    [string]$Filter = '',   # the default value is set in the script body
    [bool]$RebaseOtherRelativeLinks = $true,
    [hashtable]$OtherExtensionMap = @{'.yaml'='.yaml'},
    [string[]]$OtherReadOptions = @(),
    [string[]]$OtherWriteOptions = @('--standalone'),
    [bool]$Rebuild = $false,
    [bool]$Silent = $false,
    [bool]$NoOutput = $false
)


# Script Globals

# result constants
Set-Variable -Name 'Result_Formatted' -Value 0 -Option Constant -Scope Script
Set-Variable -Name 'Result_Copied' -Value 1 -Option Constant -Scope Script
Set-Variable -Name 'Result_Skipped_UpToDate' -Value 2 -Option Constant -Scope Script
Set-Variable -Name 'Result_Skipped_NotTarget' -Value 3 -Option Constant -Scope Script
Set-Variable -Name 'Result_Failed' -Value 4 -Option Constant -Scope Script

# the directory where this script is located
Set-Variable -Name scriptDir -Value (Split-Path -Parent $MyInvocation.MyCommand.Path) -Option ReadOnly -Scope Script

# result log
$log_formatted = @()
$log_copied = @()
$log_uptodate = @()
$log_nottarget = @()
$log_failed = @()


# Functions

function Report([string]$fromfileRelPath, [string]$toFileRelPath, [int]$result) {
    # select the description for the result
    switch ($result) {
        $Result_Formatted { $description = "Formatted to '$toFileRelPath'."; $script:log_formatted += $fromfileRelPath }
        $Result_Copied { $description = 'Copied.'; $script:log_copied += $fromfileRelPath }
        $Result_Skipped_UpToDate { $description = 'Skipped (up-to-date).' ; $script:log_uptodate += $fromfileRelPath }
        $Result_Skipped_NotTarget { $description = 'Skipped (not a target).' ; $script:log_nottarget += $fromfileRelPath }
        $Result_Failed { $description = 'Failed.' ; $script:log_failed += $fromfileRelPath }
        default { Write-Error "Invalid result code: $result" -ErrorAction Stop }
    }

    # report the result
    $message = "${fromfileRelPath}: $description"
    if ($result -eq $Result_Failed) {
        Write-Error $message
    } elseif (-not $Silent) {
        Write-Host $message
    }
}

function IsUpToDate([string[]]$sourceFiles, [string]$destFile) {
    if (-not (Test-Path $destFile -PathType Leaf)) {
        # destFile does not exist
        $false  # not up-to-date
    } else {
        # destFile exists
        $destWriteTime = $(Get-ItemProperty $destFile).LastWriteTimeUtc

        # is there updated source?
        $oneOfUpdatedSource = $sourceFiles `
          | Where-Object { $destWriteTime -lt $(Get-ItemProperty $_).LastWriteTimeUtc } `
          | Select-Object -First 1

        -not [bool]$oneOfUpdatedSource
    }
}

function RunOnShell([string]$commandLine) {
    if ([System.IO.Path]::DirectorySeparatorChar -eq '\') {
        # on Windows
        Invoke-Expression "$env:ComSpec /c `"$commandLine`""
    } else {
        # on other than Windows
        $shell = "$env:SHELL"
        if ([string]::IsNullOrEmpty($shell)) {
            $shell = '/bin/bash'
        }
        Invoke-Expression "$shell -c '$commandLine'" 
    }

    # returns the result of execution
    ($LastExitCode -eq 0)
}

function EnsureDirectoryExists([string]$filePath) {
    $dirPath = Split-Path $filePath -Parent
    if (Test-Path $dirPath -PathType Container) {
        $true
    } else {
        New-Item $dirPath -ItemType Directory | Out-Null
        $?
    }
}

function EscapeForYamlSingleQuotedString([string]$str) {
    $str.Replace("'", "''")
}

function CreateCombinedMetadataFile([string[]]$metadataFilePaths, [string]$fromFileRelPath, [string]$toFileRelPath) {
    $combinedMetadataFile = New-TemporaryFile -ErrorAction Stop
    try {
        # append the contents of the given metadata files
        if (0 -lt $metadataFilePaths.Count) {
            Get-Content $metadataFilePaths | Add-Content $combinedMetadataFile
        }

        # append the parameters
        if ($RebaseOtherRelativeLinks) {
            $rebase = "true"
        } else {
            $rebase = "false"
        }
        $rawAttribute = '{=_Param}'
        $params = @"
_Param.FromBaseDirPath: '``$(EscapeForYamlSingleQuotedString $FromDir)``$rawAttribute'
_Param.FromFileRelPath: '``$(EscapeForYamlSingleQuotedString $fromFileRelPath)``$rawAttribute'
_Param.ToBaseDirPath: '``$(EscapeForYamlSingleQuotedString $ToDir)``$rawAttribute'
_Param.ToFileRelPath: '``$(EscapeForYamlSingleQuotedString $toFileRelPath)``$rawAttribute'
_Param.RebaseOtherRelativeLinks: $rebase

"@
        if (0 -lt $extensionMap.Count) {
            $params += "_Param.ExtensionMap:`n"
            foreach ($key in $extensionMap.Keys) {
                $value = $extensionMap[$key]
                $params += "  ${key}: '``$(EscapeForYamlSingleQuotedString $value)``$rawAttribute'`n"
            }
        }
        Add-Content $combinedMetadataFile -Value $params
    } catch {
        Remove-Item $combinedMetadataFile
        $combinedMetadataFile = ''
    }

    $combinedMetadataFile
}

function FormatFile([string]$fromFileRelPath, [hashtable]$format) {
    # preparations
    $toFileRelPath = [System.IO.Path]::ChangeExtension($fromFileRelPath, $format.toExtension)
    $fromFilePath = Join-Path $FromDir $fromFileRelPath
    $toFilePath = Join-Path $ToDir $toFileRelPath

    # check existence of the metadata file
    if (0 -lt $MetadataFiles.Count) {
        $allMetadataFiles = $MetadataFiles.Clone()
    } else {
        $allMetadataFiles = @()
    }

    $sourceMetadataFilePath = "$fromFilePath.metadata.yaml"
    if (Test-Path $sourceMetadataFilePath -PathType Leaf) {
        $allMetadataFiles += $sourceMetadataFilePath
    }

    $sourceFiles = $allMetadataFiles.Clone()
    $sourceFiles += $fromFilePath
    if ((-not $Rebuild) -and (IsUpToDate $sourceFiles $toFilePath)) {
        # the to file is up-to-data
        Report $fromFileRelPath $toFileRelPath $Result_Skipped_UpToDate
    } else {
        # format the document 

        # make sure that the target directory exists
        if (-not (EnsureDirectoryExists $toFilePath)) {
            Report $fromFileRelPath $toFileRelPath $Result_Failed
            return
        }

        $combinedMetadataFile = CreateCombinedMetadataFile $allMetadataFiles $fromFileRelPath $toFileRelPath
        if ([string]::IsNullOrEmpty($combinedMetadataFile)) {
            Report $fromFileRelPath $toFileRelPath $Result_Failed
            return
        }
        $metadataOption = "--metadata-file=$combinedMetadataFile"
        try {
            # run pandoc
            if ([string]::IsNullOrWhiteSpace($Filter)) {
                # with no filter
                pandoc $OtherReadOptions $OtherWriteOptions $metadataOption -f $format.fromFormat -t $format.toFormat -o $toFilePath $fromFilePath
                $succeeded = ($LastExitCode -eq 0)
            } else {
                # with the specified filter
                # Note that this pipeline must not be run on PowerShell but *normal* shell.
                # Because pipeline of PowerShell is not designed to connect native programs.
                $commandLine = "pandoc $OtherReadOptions $metadataOption -f $($format.fromFormat) -t json $fromFilePath | " `
                + (Invoke-Expression "`"$Filter`"") `
                + " | pandoc $OtherWriteOptions -f json -t $($format.toFormat) -o $toFilePath"
                $succeeded = RunOnShell $commandLine
            }
        } finally {
            if (-not [string]::IsNullOrEmpty($combinedMetadataFile)) {
                Remove-Item $combinedMetadataFile
            }
        }

        # reported
        if ($succeeded) {
            $result = $Result_Formatted
        } else {
            $result =  $Result_Failed
        }
        Report $fromFileRelPath $toFileRelPath $result
    }
}

function CopyFile([string]$fromFileRelPath) {
    # preparations
    $fromFilePath = Join-Path $FromDir $fromFileRelPath
    $toFileRelPath = $fromFileRelPath
    $toFilePath = Join-Path $ToDir $toFileRelPath

    if ((-not $Rebuild) -and (IsUpToDate $fromFilePath $toFilePath)) {
        # no need to copy
        Report $fromFileRelPath $toFileRelPath $Result_Skipped_UpToDate
    } else {
        # copy the file

        # make sure that the target directory exists
        if (-not (EnsureDirectoryExists $toFilePath)) {
            Report $fromFileRelPath $toFileRelPath $Result_Failed
            return
        }

        # copy the file
        Copy-Item $fromFilePath -Destination $toFilePath
        if ($?) {
            $result = $Result_Copied
        } else {
            $result =  $Result_Failed
        }
        Report $fromFileRelPath $toFileRelPath $result
    }
}

function ProcessFile([string]$fromFileRelPath) {
    $extension = [System.IO.Path]::GetExtension($fromFileRelPath)

    $format = $formatMap[$extension]
    if ($null -ne $format) {
        # target of this formatting session
        FormatFile $fromFileRelPath $format
    } elseif ($OtherExtensionMap.ContainsKey($extension)) {
        # target of another formatting session
        # Do nothing. The other session will process it.
        Report $fromFileRelPath $toFileRelPath $Result_Skipped_NotTarget
    } else {
        # other files
        # copy the file if you don't want rebasing
        if ($RebaseOtherRelativeLinks) {
            Report $fromFileRelPath $toFileRelPath $Result_Skipped_NotTarget
        } else {
            CopyFile $fromFileRelPath
        }
    }
}


# Script Main

# collect formatting information

$formatCount = $FromExtensions.Length
@($FromFormats.Length; $ToExtensions.Length; $ToFormats.Length) `
  | ForEach-Object { 
        if ($_ -ne $formatCount) {
            Write-Error 'The length of $FromExtensions, $FromFormats, $ToExtensions and $ToFormats must be same.' -ErrorAction Stop
        }
    }

$formatMap = @{}
$extensionMap = $OtherExtensionMap.Clone()
for ($i = 0; $i -lt $formatCount; ++$i) {
    # add the info to the format map 
    $fromExtension = $FromExtensions[$i]
    $toExtension = $ToExtensions[$i]
    $formatMap[$fromExtension] = @{
        'fromExtension' = $fromExtension;
        'fromFormat' = $FromFormats[$i];
        'toExtension' = $toExtension;
        'toFormat' = $ToFormats[$i]
    }

    # add the info to the extension map
    $extensionMap[$fromExtension] = $toExtension
}

# give the default value for $Filter
if ([string]::IsNullOrEmpty($Filter)) {
    $Filter = "dotnet $scriptDir/FormatAST.dll"
} 

# make the paths absolute
# Note the working directory of PowerShell may differ from the current directory.
# That means you can not use [System.IO.Path]::GetFullPath() easily.
$workingDir = Convert-Path '.'
$FromDir = Convert-Path $FromDir -ErrorAction Stop  # $FromDir must exist
$ToDir = [System.IO.Path]::Combine($workingDir, $ToDir)
if (-not (Test-Path $ToDir -PathType Container)) {
    New-Item $ToDir -ItemType Directory -ErrorAction Stop | Out-Null
}
$MetadataFiles = $MetadataFiles | ForEach-Object { [System.IO.Path]::Combine($workingDir, $_) }

# process all files in the from directory
Get-ChildItem $fromDir -File -Name -Recurse | ForEach-Object { ProcessFile $_ }

# return the result
return New-Object PSObject -Property @{
    Formatted = $log_formatted;
    Copied = $log_copied;
    UpToDate = $log_uptodate;
    NotTarget = $log_nottarget;
    Failed = $log_failed
}

# report the result
# ''  # output blank line
# "Formatted: $($resultCount[0]), Copied: $($resultCount[1]), Failed: $($resultCount[4]), Up-To-Date: $($resultCount[2]), Not-Target: $($resultCount[3])"
