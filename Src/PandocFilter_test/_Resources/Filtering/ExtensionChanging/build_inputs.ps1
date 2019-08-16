#!/usr/bin/env pwsh

param (
    [bool]$rebuild = $false
)


# Script Globals

$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourcesDir = Join-Path $baseDir 'Sources'
$inputsDir = Join-Path $baseDir 'Inputs'


# Functions

function IsUpToDate([string]$sourceFile, [string]$destFile) {
    if (-not (Test-Path $destFile -PathType Leaf)) {
        # destFile does not exist
        $false  # not up-to-date
    } else {
        # destFile exists
        $destWriteTime = $(Get-ItemProperty $destFile).LastWriteTimeUtc
        $sourceWriteTime = $(Get-ItemProperty $sourceFile).LastWriteTimeUtc

        ($sourceWriteTime -le $destWriteTime)
    }
}

function ProcessFile([string]$fileRelPath) {
    # decide the path of the from file and the to file
    $fromFilePath = Join-Path $sourcesDir $fileRelPath
    $toFilePath = Join-Path $inputsDir ([System.IO.Path]::ChangeExtension($fileRelPath, '.input.json'))

    if ((-not $rebuild) -and (IsUpToDate $fromFilePath $toFilePath)) {
        # no need to copy
        "Skipped (up-to-date): ${fileRelPath}"
    } else {
        # make sure that the target directory exists
        $toFileDir = Split-Path $toFilePath -Parent
        if (-not (Test-Path $toFileDir -PathType Container)) {
            New-Item $toFileDir -ItemType Directory | Out-Null
            if (-not $?) {
                Write-Error "Failed: ${fileRelPath}"
                return;
            }
        }
    
        # convert the file by pandoc
        & 'pandoc' -o $toFilePath -t json $fromFilePath

        # report to output
        if ($LastExitCode -eq 0) {
            "Convert: ${fileRelPath}"
        } else {
            Write-Error "Failed: ${fileRelPath}"
        }
    }
}


# Main Procedure

Get-ChildItem $sourcesDir -File -Name -Recurse `
  | ForEach-Object { ProcessFile $_ }
