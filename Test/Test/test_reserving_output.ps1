#!/usr/bin/env pwsh

<#
.SYNOPSIS

Tests Panasu features reserving test output.
This is a specialized version of test.ps1.

.DESCRIPTION

The test_reserving_output.ps1 script is the same to test.ps1 except:

  -OutputDir parameter is mandatory and it can be specified as the first positioned parameter.

See the help of test.ps1 for details, such as parameters, inputs or outputs.

.INPUTS

See inputs of test.ps1.


.OUTPUTS

See outputs of test.ps1.


.EXAMPLE

test_reserving_output.ps1 'C:\Temp\Test'

This command test the build output of Panasu.sln.
The output directories of formatting test cases are created under `C:\Temp\Test` and they are not deleted after the test.

.LINK

test.ps1
#>
param (
    [parameter(mandatory=$true, position=0)]
    [string]$OutputDir,
    [string]$TestScript = '',
    [string]$FormatDocumentsScript = '',
    [string]$FormatDocumentsFilter = '',
    [string]$Config = '',
    [string]$Runtime = '',
    [string]$OtherOptions = ''
)


# prepare the output dir
if (Test-Path $OutputDir -PathType Container) {
    # The directory exists.
    # remove all its contents
    Remove-Item "$OutputDir/*" -Recurse -ErrorAction Stop | Out-Null
} else {
    # The directory does not exist.
    # create it
    New-Item $OutputDir -ItemType Directory -ErrorAction Stop | Out-Null
}

# test
./test.ps1 `
    -TestScript $TestScript `
    -OutputDir $OutputDir `
    -FormatDocumentsScript $FormatDocumentsScript `
    -FormatDocumentsFilter $FormatDocumentsFilter `
    -Config $Config `
    -Runtime $Runtime `
    -OtherOptions $OtherOptions
