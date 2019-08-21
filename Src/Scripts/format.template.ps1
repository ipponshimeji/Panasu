#!/usr/bin/env pwsh

# The template of wrapper script to format_base.ps1.  
# Copy this script as 'format.ps1' and set your default settings.

param (
    [string]$fromDir = '../md',
    [string]$fromFormat = 'markdown',
    [string]$fromExtension = '.md',
    [string]$toDir = '../html',
    [string]$toFormat = 'html',
    [string]$toExtension = '.html',
    [string]$filter = "dotnet $(Split-Path -Parent $MyInvocation.MyCommand.Path)/PandocUtil/Formatter.dll -R `$fromDir `$fromFileRelPath `$toDir `$toFileRelPath",
    [bool]$rebaseOtherRelativeLinks = $true,
    [hashtable]$otherExtensionMap = @{},
    [string[]]$otherReadOptions = @(),
    [string[]]$otherWriteOptions = @('--standalone'),
    [switch]
    [bool]$rebuild = $false
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
& "$scriptDir/PandocUtil/format_base.ps1" `
    -FromDir $fromDir `
    -FromFormat $fromFormat `
    -FromExtension $fromExtension `
    -ToDir $toDir `
    -ToFormat $toFormat `
    -ToExtension $toExtension `
    -Filter $filter `
    -RebaseOtherRelativeLinks $rebaseOtherRelativeLinks `
    -OtherExtensionMap $otherExtensionMap `
    -OtherReadOptions $otherReadOptions `
    -OtherWriteOptions $otherWriteOptions `
    -Rebuild $rebuild
