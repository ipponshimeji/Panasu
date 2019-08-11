#!/usr/bin/env pwsh

# Globals

$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourcesDir = Join-Path $baseDir 'Sources'
$inputsDir = Join-Path $baseDir 'Inputs'


# Functions

function ProcessFile([string]$fileRelPath) {
    # decide the path of the from file and the to file
    $fromFilePath = Join-Path $sourcesDir $fileRelPath
    $toFilePath = Join-Path $inputsDir ([System.IO.Path]::ChangeExtension($fileRelPath, '.input.json'))

    # make sure that the target directory exists
    $toFileDir = Split-Path $toFilePath -Parent
    if (-not (Test-Path $toFileDir -PathType Container)) {
        New-Item $toFileDir -ItemType Directory | Out-Null
    }
    
    # convert the file by pandoc
    & 'pandoc' -o $toFilePath -t json $fromFilePath

    # report to output
    "Converted: $toFilePath"
}


# Main Procedure

Get-ChildItem $sourcesDir -File -Name -Recurse `
  | ForEach-Object { ProcessFile $_ }
