#!/usr/bin/env pwsh

# The template of wrapper script to call Format-Documents.ps1.  
# Copy this script as 'format.ps1' and set your default settings.

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
    [switch]
    [bool]$Rebuild = $false,
    [switch]
    [bool]$Silent = $false,
    [string]$PanasuPath = "$(Split-Path -Parent $MyInvocation.MyCommand.Path)/Panasu"
)

# call Format-Documents.ps1
# Format-Documents.ps1 is called by evaluating commandline string
# to handle switch options such as -Rebuild or -Silent.
$commandLine = @"
$PanasuPath/Format-Documents.ps1" `
    -FromDir '$FromDir' `
    -FromExtensions '$FromExtensions' `
    -FromFormats '$FromFormats' `
    -ToDir '$ToDir' `
    -ToExtensions '$ToExtensions' `
    -ToFormats '$ToFormats' `
    -MetadataFiles '$MetadataFiles' `
    -Filter '$Filter' `
    -RebaseOtherRelativeLinks '$RebaseOtherRelativeLinks' `
    -OtherExtensionMap '$OtherExtensionMap' `
    -OtherReadOptions '$OtherReadOptions' `
    -OtherWriteOptions '$OtherWriteOptions'
"@
if ($Rebuild) {
    $commandLine += ' -Rebuild'
}
if ($Silent) {
    $commandLine += ' -Silent'
}

Invoke-Expression $commandLine