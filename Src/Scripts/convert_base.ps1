#!/usr/bin/env pwsh

<# 
.SYNOPSIS 
    Converts documents using pandoc.
.DESCRIPTION 
    TBU
.PARAMETER FromDir 
    The directory where the input documents are stored.
    The default value is '../md'.
.PARAMETER FromFormat 
    The format of the input documents.
    This value must be one which can be passed to -f option of pandoc.
    The default value is 'markdown'.
.PARAMETER FromExtension 
    The extension of the input document files.
    The default value is '.md'.
.PARAMETER ToDir 
    The directory where the output documents are going to be stored.
    The default value is '../html'.
.PARAMETER ToFormat 
    The format of the output documents.
    This value must be one which can be passed to -t option of pandoc.
    The default value is 'html'.
.PARAMETER ToExtension 
    The extension of the output document files.
    The default value is '.html'.
.PARAMETER RebaseOtherRelativeLinks 
    Whether relative links which are not target of extension mapping
    should be rebase so that the links keep to reference its target
    in the original location.
    If this value is $false, this script copys such files into $ToDir
    along with the converted files, and do not rebase links to the
    files.
    The default value is $true.
.PARAMETER OtherExtensionMap 
    The extension mappings other than the pair of FromExtension and ToExtension.
    This script converts those extensions in relative links.
.PARAMETER OtherReadOptions 
    The array of other read options to be provided to pandoc.
    The default value is @().
.PARAMETER OtherWriteOptions 
    The array of other options to be provided to pandoc.
    The default value is @('--standalone').
.PARAMETER Filter 
    The command line of the pandoc filter to be used.
.PARAMETER Rebuild 
    Whether converts all documents.
    By default, only updated documents are converted.
.INPUTS
    None.
.OUTPUTS
    The paths of converted documents.
    The path is relative to the output directory.
#>
param (
    [string]$fromDir = '../md',
    [string]$fromFormat = 'markdown',
    [string]$fromExtension = '.md',
    [string]$toDir = '../html',
    [string]$toFormat = 'html',
    [string]$toExtension = '.html',
    [string]$filter = "dotnet $(Split-Path -Parent $MyInvocation.MyCommand.Path)/ExtensionChanger.dll -R $fromDir $fromFileRelPath $toDir $toFileRelPath",
    [bool]$rebaseOtherRelativeLinks = $true,
    [hashtable]$otherExtensionMap = @{},
    [string[]]$otherReadOptions = @(),
    [string[]]$otherWriteOptions = @('--standalone'),
    [bool]$rebuild = $false
)


# Script Globals

# result constants
Set-Variable -Name 'Result_Converted' -Value 0 -Option Constant -Scope Script
Set-Variable -Name 'Result_Copied' -Value 1 -Option Constant -Scope Script
Set-Variable -Name 'Result_Skipped_UpToDate' -Value 2 -Option Constant -Scope Script
Set-Variable -Name 'Result_Skipped_NotTarget' -Value 3 -Option Constant -Scope Script
Set-Variable -Name 'Result_Failed' -Value 4 -Option Constant -Scope Script

# variables
[int[]]$resultCount = @(0; 0; 0; 0; 0)


# Functions

function Report([string]$fromfileRelPath, [string]$toFileRelPath, [int]$result) {
    # select the description for the result
    switch ($result) {
        $Result_Converted { $description = "Converted to '$toFileRelPath'." }
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

function ConvertFile([string]$fromFileRelPath) {
    # preparations
    $toFileRelPath = [System.IO.Path]::ChangeExtension($fromFileRelPath, $toExtension)
    $fromFilePath = Join-Path $fromDir $fromFileRelPath
    $toFilePath = Join-Path $toDir $toFileRelPath
    $sourceFiles = @($fromFilePath)
    $metadataOption = ''

    # check existence of the metadata file
    $metadataFilePath = "$fromFilePath.metadata.yaml"
    if (Test-Path $metadataFilePath -PathType Leaf) {
        $sourceFiles += $metadataFilePath
        $metadataOption = "--metadata-file=$metadataFilePath"
    }

    if ((-not $rebuild) -and (IsUpToDate $sourceFiles $toFilePath)) {
        # the to file is up-to-data
        Report $fromFileRelPath $toFileRelPath $Result_Skipped_UpToDate
    } else {
        # convert the output 

        # make sure that the target directory exists
        if (-not (EnsureDirectoryExists $toFilePath)) {
            Report $fromFileRelPath $toFileRelPath $Result_Failed
            return
        }

        # run pandoc
        if ([string]::IsNullOrWhiteSpace($filter)) {
            # with no filter
            pandoc $metadataOption $otherReadOptions $otherWriteOptions -f $fromFormat -t $toFormat -o $toFilePath $inputFilePath
            $succeeded = ($LastExitCode -eq 0)
        } else {
            # with the specified filter
            # Note that this pipeline must not be run on PowerShell but *normal* shell.
            # Because pipeline of PowerShell is not designed to connect native programs.
            $commandLine = "pandoc $metadataOption $otherReadOptions -f $fromFormat -t json $fromFilePath | " `
            + (Invoke-Expression "`"$filter`"") `
            + " | pandoc $otherWriteOptions -f json -t $toFormat -o $toFilePath"
            $succeeded = RunOnShell $commandLine
        }

        # reported
        if ($succeeded) {
            $result = $Result_Converted
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
    if ($extension -eq $fromExtension) {
        # target of this conversion session
        ConvertFile $fromFileRelPath
    } elseif ($otherExtensionMap.ContainsKey($extension)) {
        # target of another conversion session
        # Do nothing. The other session will process it.
        Report $fromFileRelPath $toFileRelPath $Result_Skipped_NotTarget
    } else {
        # other files
        # copy the file if you don't want rebasing
        if (-not $rebaseOtherRelativeLinks) {
            CopyFile $fromFileRelPath
        } else {
            Report $fromFileRelPath $toFileRelPath $Result_Skipped_NotTarget
        }
    }
}


# Script Main

# make the paths absolute
$fromDir = Convert-Path $fromDir -ErrorAction Stop
$toDir = Convert-Path $toDir -ErrorAction Stop

# process all files in the input directory
Get-ChildItem $fromDir -File -Name -Recurse `
  | ForEach-Object { ProcessFile $_ }

# report the result
"Converted: $($resultCount[0]), Copied: $($resultCount[1]), Failed: $($resultCount[4]), Up-To-Date: $($resultCount[2]), Not-Target: $($resultCount[3])"
