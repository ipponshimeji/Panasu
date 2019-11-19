#!/usr/bin/env pwsh

<#
.SYNOPSIS

Tests Panasu features using files collected to create a release.
This is a specialized version of test.ps1.

.DESCRIPTION

The test_release.ps1 script is the same to test.ps1 except:

  It tests files in <Working Copy Dir>/Src/__Release/Panasu, where the files are collected to create a release.
  So the following parameters of test.ps1 are not used.
    * -FormatDocumentsScript
    * -FormatDocumentsFilter
    * -Config
    * -Runtime

See the help of test.ps1 for details, such as parameters, inputs or outputs.

.INPUTS

See inputs of test.ps1.


.OUTPUTS

See outputs of test.ps1.


.EXAMPLE

./test_release.ps1

This command tests Panasu features using files in <Working Copy Dir>/Src/__Release/Panasu.

.LINK

test.ps1
#>
param (
    [Parameter(position=0)]
    [string]$TestScriptPath = '',
    [string]$OutputDir = '',
    [string]$OtherOptions = ''
)

# test
$releaseDir = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) '../../Src/__Release'
$panasuDir = Join-Path $releaseDir 'Panasu'

# test
./test.ps1 `
    -TestScript $TestScript `
    -OutputDir $OutputDir `
    -FormatDocumentsScript "$panasuDir/Format-Documents.ps1" `
    -FormatDocumentsFilter '' `
    -Config $Config `
    -Runtime $Runtime `
    -OtherOptions $OtherOptions
