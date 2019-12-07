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
    [bool]$StickOtherRelativeLinks = $true,
    [hashtable]$OtherExtensionMap = @{'.yaml'='.yaml'},
    [string[]]$OtherReadOptions = @(),
    [string[]]$OtherWriteOptions = @('--standalone'),
    [switch]
    [bool]$Rebuild = $false,
    [switch]
    [bool]$Silent = $false,
    [bool]$NoOutput = $false,
    [string]$PanasuPath = "$(Split-Path -Parent $MyInvocation.MyCommand.Path)/Panasu"
)

# call Format-Documents.ps1
& "$PanasuPath/Format-Documents.ps1" `
    -FromDir $FromDir `
    -FromExtensions $FromExtensions `
    -FromFormats $FromFormats `
    -ToDir $ToDir `
    -ToExtensions $ToExtensions `
    -ToFormats $ToFormats `
    -MetadataFiles $MetadataFiles `
    -Filter $Filter `
    -StickOtherRelativeLinks $StickOtherRelativeLinks `
    -OtherExtensionMap $OtherExtensionMap `
    -OtherReadOptions $OtherReadOptions `
    -OtherWriteOptions $OtherWriteOptions `
    -Rebuild $Rebuild `
    -Silent $Silent `
    -NoOutput $NoOutput
