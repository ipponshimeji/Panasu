#!/usr/bin/env pwsh

<# 
.SYNOPSIS 

Formats the documents under the specified directory using pandoc.

.DESCRIPTION 

The Format-Documents.ps1 script formats source document files under the input directory given by FromDir parameter.
The formatted document files are output in the output directory given by ToDir parameter.

Formatting process

This script uses pandoc to format source document files.
pandoc 2.3 or later must be installed to run this script.

Formatting of each file is performed as follows:

1. `pandoc` reads a source document file and converts it into AST (Abstract Syntax Tree). 
2. The filter specified by Filter parameter modify the AST.
3. `pandoc` reads the filtered AST and writes it into the output directory in the target format. 

The relation of the extension of source document file, the format name of source document,
the extension of formatted document file and the format name of formatted document is given by
FromExtensions, FromFormats, ToExtensions and ToFormats parameters respectively,
where "format name" means a value which can be passed to -f or -t option of `pandoc`.

The default filter, FilterAST is provided along with this script.
See about_FilterAST for how it modifies the AST.

Only files whose extension is contained in FromExtensions parameter are formatted.
This script does not process other files,
but some files are copied to ToDir if StickOtherRelativeLinks parameter is false.
See the description of StickOtherRelativeLinks parameter for details.

Extension mapping

The default filter replaces the extension of relative links in the input AST.
For example, a link to "a.md" in the input AST is replaced to one to "a.html".
The extension mappings used in this replacement consists of the following mappings:

* from extensions given by FromExtensions parameter to ones given by ToExtensions parameter
* the mapping specified by OtherExtensionMap parameter

An extension which is not a target of the extension mappings is not replaced.

Metadata

When pandoc converts a document,
you can give metadata along with the source document as "parameter" of conversion.

There are some ways to provide metadata in this script's process.

* In pandoc, metadata can be embedded in a source document in some formats such as markdown.
  See Metadata blocks in Pandoc User's Guide.
  Note that this notation may decrease compatibility of the source document as markdown format.
* If file named "<source document file name>.metadata.yaml" exists in the same directory,
  this script takes it as metadata for the source document.
  For example, this script regards "a.md.metadata.yaml" as metadata of "a.md" if it exists.
  The metadata file must be a YAML file.
* You can specify metadata files which are commonly used for this formatting process by MetadataFiles parameter.
  The metadata file must be a YAML file.

This scripts combines metadata files, embeds parameters for the filter into it and passes it to pandoc.

Some filters, including the default filter, support metadata macro.
See about_MetadataMacro for details.

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

The array of yaml file which describes common metadata for this formatting session.
The all specified metadata are attached to all source document files. 

.PARAMETER Filter 

The command line of the pandoc filter to be used in the formatting process.

If this parameter is null or empty string, "dotnet $scriptDir/FilterAST.dll" is used,
where $scriptDir is the directory where this script is located.
See about_FilterAST for details about FilterAST.

This script embeds the parameters for the filter into the metadata of the source document.
The source document and metadata are converted into AST,
and the specified filter reads the AST.
Then the filter can reference the parameters from the metadata in the input AST.

The parameters for filter embedded in metadata are ones which are requied in the default filter, FilterAST.
So if you specify a custom filter, the filter can use those parameters via the input AST.
See about_FilterAST for the details about parameters.

.PARAMETER StickOtherRelativeLinks 

If this parameter is True, relative links to files which are not target of extension mapping
should be changed so that the links keep to reference the files in the original location.

If this parameter is False, this script copies such files into the output directory
along with the formatted files, and do not change links to the files.

.PARAMETER OtherExtensionMap 

The extension mappings other than the mappings from FromExtensions to ToExtensions parameter.
The filter replaces those extensions in relative links according the extension mappings.

By specifying a mapping from an extension to the same extension, for example '.yaml'='.yaml',
files with the extension are excluded from the target of rebasing or copying by StickOtherRelativeLinks parameter,
suppressing the extension replacement of relative links to the files. 

.PARAMETER OtherReadOptions 

The array of other read option to be passed to pandoc
when it is invoked to read the source document files.

.PARAMETER OtherWriteOptions 

The array of other write option to be passed to pandoc
when it is invoked to write the formatted document files.

.PARAMETER Rebuild

If this parameter is True, this script processes all files.
If this parameter is False, this script processes only updated files.

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

.LINK

about_FilterAST

.LINK

about_MetadataMacros
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
    [bool]$StickOtherRelativeLinks = $true,
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
        if ($StickOtherRelativeLinks) {
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
_Param.StickOtherRelativeLinks: $rebase

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
        if ($StickOtherRelativeLinks) {
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
    $Filter = "dotnet $scriptDir/FilterAST.dll"
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
if (-not $NoOutput) {
    return New-Object PSObject -Property @{
        Formatted = $log_formatted;
        Copied = $log_copied;
        UpToDate = $log_uptodate;
        NotTarget = $log_nottarget;
        Failed = $log_failed
    }
}
