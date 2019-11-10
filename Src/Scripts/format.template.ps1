#!/usr/bin/env pwsh

# The template of wrapper script to format_base.ps1.  
# Copy this script as 'format.ps1' and set your default settings.

param (
    [string]$fromDir = '../md',
    [string[]]$fromExtensions = @('.md'),
    [string[]]$fromFormats = @('markdown'),
    [string]$toDir = '../html',
    [string[]]$toExtensions = @('.html'),
    [string[]]$toFormats = @('html'),
    [string[]]$metadataFiles = @(),
    [string]$filter = '',
    [bool]$rebaseOtherRelativeLinks = $true,
    [hashtable]$otherExtensionMap = @{},
    [string[]]$otherReadOptions = @(),
    [string[]]$otherWriteOptions = @('--standalone'),
    [switch]
    [bool]$rebuild = $false,
    [bool]$silent = $false,
    [string]$pandocUtilPath = "$(Split-Path -Parent $MyInvocation.MyCommand.Path)/PandocUtil"
)


# call format_base.ps1
& "$pandocUtilPath/Format-Documents.ps1" `
    -FromDir $fromDir `
    -FromExtensions $fromExtensions `
    -FromFormats $fromFormats `
    -ToDir $toDir `
    -ToExtensions $toExtensions `
    -ToFormats $toFormats `
    -MetadataFiles $metadataFiles `
    -Filter $filter `
    -RebaseOtherRelativeLinks $rebaseOtherRelativeLinks `
    -OtherExtensionMap $otherExtensionMap `
    -OtherReadOptions $otherReadOptions `
    -OtherWriteOptions $otherWriteOptions `
    -Rebuild $rebuild `
    -Silent $silent
