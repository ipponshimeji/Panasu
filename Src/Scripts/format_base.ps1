#!/usr/bin/env pwsh

<# 
.SYNOPSIS 
    Formats documents using pandoc.
.DESCRIPTION 
    This script formats a group of source documents using pandoc.
.PARAMETER FromDir 
    The directory where the source documents are stored.
    The default value is '../md'.
.PARAMETER FromExtensions 
    The extensions of the source document files.
    The default value is @('.md').
.PARAMETER FromFormats 
    The formats of the source documents.
    Each value must be one which can be passed to -f option of pandoc.
    Each item in this argument corresponds to the item in FromExtensions respectively.
    The length of this argument must be same to the length of FromExtensions.
    The default value is @('markdown').
.PARAMETER ToDir 
    The directory where the destination documents are going to be stored.
    The default value is '../html'.
.PARAMETER ToExtensions 
    The extensions of the destination document files.
    Each item in this argument corresponds to the item in FromExtensions respectively.
    The length of this argument must be same to the length of FromExtensions.
    The default value is @('.html').
.PARAMETER ToFormats 
    The format of the destination documents.
    Each value must be one which can be passed to -t option of pandoc.
    Each item in this argument corresponds to the item in FromExtensions respectively.
    The length of this argument must be same to the length of FromExtensions.
    The default value is @('html').
.PARAMETER MetadataFiles 
    The yaml files which describe metadata for this formatting. 
.PARAMETER RebaseOtherRelativeLinks 
    Whether relative links which are not target of extension mapping
    should be rebase so that the links keep to reference its target
    in the original location.
    If this value is $false, this script copys such files into $ToDir
    along with the formatted files, and do not rebase links to the
    files.
    The default value is $true.
.PARAMETER OtherExtensionMap 
    The extension mappings other than the pair of FromExtensions and ToExtensions.
    This script replaces those extensions in relative links.
.PARAMETER OtherReadOptions 
    The array of other read options to be provided to pandoc.
    The default value is @().
.PARAMETER OtherWriteOptions 
    The array of other write options to be provided to pandoc.
    The default value is @('--standalone').
.PARAMETER Filter 
    The command line of the pandoc filter to be used.
.PARAMETER Rebuild 
    Whether formats all source documents.
    By default, only updated source documents are formatted.
.INPUTS
    None.
.OUTPUTS
    The lines which consist of the path of document and how it is processed.
    The path is relative to $FromDir.
    The last line is simple statistics of the formatting.
#>
param (
    [string]$fromDir = '../md',
    [string[]]$fromExtensions = @('.md'),
    [string[]]$fromFormats = @('markdown'),
    [string]$toDir = '../html',
    [string[]]$toExtensions = @('.html'),
    [string[]]$toFormats = @('html'),
    [string[]]$metadataFiles = @(),
    [string]$filter = '',   # the default value is set in the script body
    [bool]$rebaseOtherRelativeLinks = $true,
    [hashtable]$otherExtensionMap = @{'.yaml'='.yaml'},
    [string[]]$otherReadOptions = @(),
    [string[]]$otherWriteOptions = @('--standalone'),
    [bool]$rebuild = $false
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

# result statistics
[int[]]$resultCount = @(0; 0; 0; 0; 0)


# Functions

function Report([string]$fromfileRelPath, [string]$toFileRelPath, [int]$result) {
    # select the description for the result
    switch ($result) {
        $Result_Formatted { $description = "Formatted to '$toFileRelPath'." }
        $Result_Copied { $description = 'Copied.' }
        $Result_Skipped_UpToDate { $description = 'Skipped (up-to-date).' }
        $Result_Skipped_NotTarget { $description = 'Skipped (not a target).' }
        $Result_Failed { $description = 'Failed.' }
        default { Write-Error "Invalid result code: $result" -ErrorAction Stop }
    }    

    # update the statistics
    ++$script:resultCount[$result]

    # report the result
    $message = "${fromfileRelPath}: $description"
    if ($result -eq $Result_Failed) {
        Write-Error $message
    } else {
        $message
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
        if ($rebaseOtherRelativeLinks) {
            $rebase = "true"
        } else {
            $rebase = "false"
        }
        $rawAttribute = '{=_Param}'
        $params = @"
_Param.FromBaseDirPath: '``$(EscapeForYamlSingleQuotedString $fromDir)``$rawAttribute'
_Param.FromFileRelPath: '``$(EscapeForYamlSingleQuotedString $fromFileRelPath)``$rawAttribute'
_Param.ToBaseDirPath: '``$(EscapeForYamlSingleQuotedString $toDir)``$rawAttribute'
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
    $fromFilePath = Join-Path $fromDir $fromFileRelPath
    $toFilePath = Join-Path $toDir $toFileRelPath

    # check existence of the metadata file
    if (0 -lt $metadataFiles.Count) {
        $allMetadataFiles = $metadataFiles.Clone()
    } else {
        $allMetadataFiles = @()
    }

    $sourceMetadataFilePath = "$fromFilePath.metadata.yaml"
    if (Test-Path $sourceMetadataFilePath -PathType Leaf) {
        $allMetadataFiles += $sourceMetadataFilePath
    }

    $sourceFiles = $allMetadataFiles.Clone()
    $sourceFiles += $fromFilePath
    if ((-not $rebuild) -and (IsUpToDate $sourceFiles $toFilePath)) {
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
            if ([string]::IsNullOrWhiteSpace($filter)) {
                # with no filter
                pandoc $otherReadOptions $otherWriteOptions $metadataOption -f $format.fromFormat -t $format.toFormat -o $toFilePath $fromFilePath
                $succeeded = ($LastExitCode -eq 0)
            } else {
                # with the specified filter
                # Note that this pipeline must not be run on PowerShell but *normal* shell.
                # Because pipeline of PowerShell is not designed to connect native programs.
                $commandLine = "pandoc $otherReadOptions $metadataOption -f $($format.fromFormat) -t json $fromFilePath | " `
                + (Invoke-Expression "`"$filter`"") `
                + " | pandoc $otherWriteOptions -f json -t $($format.toFormat) -o $toFilePath"
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
    $fromFilePath = Join-Path $fromDir $fromFileRelPath
    $toFileRelPath = $fromFileRelPath
    $toFilePath = Join-Path $toDir $toFileRelPath

    if ((-not $rebuild) -and (IsUpToDate $fromFilePath $toFilePath)) {
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
    } elseif ($otherExtensionMap.ContainsKey($extension)) {
        # target of another formatting session
        # Do nothing. The other session will process it.
        Report $fromFileRelPath $toFileRelPath $Result_Skipped_NotTarget
    } else {
        # other files
        # copy the file if you don't want rebasing
        if ($rebaseOtherRelativeLinks) {
            Report $fromFileRelPath $toFileRelPath $Result_Skipped_NotTarget
        } else {
            CopyFile $fromFileRelPath
        }
    }
}


# Script Main

# collect formatting information

$formatCount = $fromExtensions.Length
@($fromFormats.Length; $toExtensions.Length; $toFormats.Length) `
  | ForEach-Object { 
        if ($_ -ne $formatCount) {
            Write-Error 'The length of $fromExtensions, $fromFormats, $toExtensions and $toFormats must be same.' -ErrorAction Stop
        }
    }

$formatMap = @{}
$extensionMap = $otherExtensionMap.Clone()
for ($i = 0; $i -lt $formatCount; ++$i) {
    # add the info to the format map 
    $fromExtension = $fromExtensions[$i]
    $toExtension = $toExtensions[$i]
    $formatMap[$fromExtension] = @{
        'fromExtension' = $fromExtension;
        'fromFormat' = $fromFormats[$i];
        'toExtension' = $toExtension;
        'toFormat' = $toFormats[$i]
    }

    # add the info to the extension map
    $extensionMap[$fromExtension] = $toExtension
}

# give the default value for $filter
if ([string]::IsNullOrEmpty($filter)) {
    $filter = "dotnet $scriptDir/FormatASP.dll"
} 

# make the paths absolute
# Note the working directory of PowerShell may differ from the current directory.
# That means you can not use [System.IO.Path]::GetFullPath() easily.
$workingDir = Convert-Path '.'
$fromDir = Convert-Path $fromDir -ErrorAction Stop  # $fromDir must exist
$toDir = [System.IO.Path]::Combine($workingDir, $toDir)
if (-not (Test-Path $toDir -PathType Container)) {
    New-Item $toDir -ItemType Directory -ErrorAction Stop | Out-Null
}
$metadataFiles = $metadataFiles | ForEach-Object { [System.IO.Path]::Combine($workingDir, $_) }

# process all files in the from directory
Get-ChildItem $fromDir -File -Name -Recurse | ForEach-Object { ProcessFile $_ }

# report the result
''  # output blank line
"Formatted: $($resultCount[0]), Copied: $($resultCount[1]), Failed: $($resultCount[4]), Up-To-Date: $($resultCount[2]), Not-Target: $($resultCount[3])"
