#!/usr/bin/env pwsh

<#
.SYNOPSIS

Tests Panasu features.

.DESCRIPTION

The test.ps1 script tests the features of Panasu.

It uses Pester as its test framework.
Pester 4.0.5 or later must be available to run this script.

The subjects to be tested include following features:

* Format-Documents.ps1 script
* utility functions defined in test script itself

.PARAMETER Config

Semantically, it specifies the name of configuration by which the testing files are built.

Accurately, it specifies the name of the directory which is a part of the build output path and means its configuration.
That is, the `<Configuration>` part of the following path, which is typical output path of a C# project:

(Project Dir)/bin/<Configuration>/<Runtime>

Generally it is either `Debug` or `Release`.

This value is used if the script tests the built files directly. 

If this parameter is omitted or its value is an empty string, the script decides the default value.
The current default value is `Debug`.
Note that the default value will be changed if the default configuration of Panasu is changed.  

.PARAMETER FormatDocumentsFilter

Specifies the command line of the filter which is given to Format-Documents.ps1.
Generally, it forms as follows:

dotnet <path to FilterAST.dll>

If this parameter is omitted or its value is an empty string, the script uses the following value.

dotnet <Working Copy Root>/Src/FilterAST/bin/<Configuration>/<Runtime>/FilterAST.dll

Where `<Configuration>` and `<Runtime>` are values of `Configuration` and `Runtime` parameter respectively.
That is, the script uses the build output of the specified configuration and target runtime.

.PARAMETER FormatDocumentsScript

Specifies the path to the Format-Documents.ps1 script to be tested.

If this parameter is omitted or its value is an empty string, the script uses the following value.

<Working Copy Root>/Src/Scripts/Format-Documents.ps1

That is, the script uses the developing script in the working copy.

.PARAMETER OtherOptions

Specifies the options which are passed to Invoke-Pester commandlet, such as -PassThru or -OutputFile.

.PARAMETER OutputDir

Specifies the path of the output directory.

Some test cases in this test format sample sources into output directories and verify them.
Those output directories are created under this path.

If this parameter is omitted or its value is an empty string, the script creates a temporary directory and use it as the root of the output directories for the cases.
The temporary directory is removed after the test is completed.
So you must specify this parameter explicitly if you want to verify the test output.

.PARAMETER Runtime

Semantically, it specifies the name of platform for which the testing files are built.

Accurately, it specifies the name of the directory which is a part of the build output path and means its target platform.
That is, the `<Runtime>` part of the following path, which is typical output path of a C# project:

(Project Dir)/bin/<Configuration>/<Runtime>

This value is used if the script tests the built files directly. 

If this parameter is omitted or its value is an empty string, the script decides the default value.
The current default value is `net6.0`, that is the name for .NET 6 platform.
Note that the default value will be changed if the default target platform of Panasu is changed.  

.PARAMETER TestScriptPath

Specifies the path of the test scripts to be run.

This value is passed to the `Path` value of `Script` parameter of `Invoke-Pester` commandlet.

If this parameter is omitted or its value is an empty string, the script uses the following path:

*.Tests.ps1

.INPUTS

None. You cannot pipe input to this script.

.OUTPUTS

None or PSCustomObject.
This script returns the output of Invoke-Pester commandlet which this script calls internally.

By default, Invoke-Pester does not generate any output.
But if you specify -PassThru parameter to Invoke-Pester through -OtherOptions parameter of this script, Invoke-Pester returns the object which describes the result of the test.

See the -PassThru parameter of Invoke-Pester commandlet for details. 

.EXAMPLE

test.ps1

This command tests the build outout from Panasu.sln of the default configuration and the default target runtime, that is `Debug` and `net6.0` respectively.

The Panasu.sln must be built successfully for the configuration and the target runtime before the command is executed.

.EXAMPLE

test.ps1 -Configuration Release -Runtime netcoreapp2.2

This command tests the build outout from Panasu.sln of `Release` configuration and .NET Core 2.2 target. 

The Panasu.sln must be built successfully for the configuration and the target runtime before the command is executed.

.EXAMPLE

test.ps1 -FormatDocumentsScript 'C:\Panasu\Format-Documents.ps1' -formatDocumentsFilter 'dotnet C:\Panasu\FilterAST.dll'

This command tests Panasu located in `C:\Panasu`.

.EXAMPLE

test.ps1 -OutputDir 'C:\Temp\Test'

This command tests the build output of Panasu.sln.
The output directories of formatting test cases are created under `C:\Temp\Test` and they are not deleted after the testing.

.EXAMPLE

test.ps1 -testScriptPath 'Format-Documents.Tests.ps1'

This command tests only test cases written in Format-Documents.Tests.ps1.

.EXAMPLE

test.ps1 -OtherOptions '-PassThru'

This command test the build output of Panasu.sln, and returns the object which describes the result of the test.

See -PassThru parameter of Invoke-Pester commandlet for details.

.LINK

Invoke-Pester
test_reserving_output.ps1
test_release.ps1
#>
param (
    [Parameter(position=0)]
    [string]$TestScriptPath = '',
    [string]$OutputDir = '',
    [string]$FormatDocumentsScript = '',
    [string]$FormatDocumentsFilter = '',
    [string]$Config = '',
    [string]$Runtime = '',
    [string]$OtherOptions = ''
)

# set default value
if ([string]::IsNullOrEmpty($TestScriptPath)) {
    $TestScriptPath = '*.Tests.ps1'
}


# test
$commandLine = @"
Invoke-Pester -Script @{
    Path = '$TestScriptPath';
    Parameters = @{
        OutputDirBase = '$OutputDir';
        FormatDocumentsScript = '$FormatDocumentsScript';
        FormatDocumentsFilter = '$FormatDocumentsFilter';
        Config = '$Config';
        Runtime = '$Runtime'
    }
}
"@
if (-not [string]::IsNullOrEmpty($OtherOptions)) {
    $commandLine += " $OtherOptions"
}

Invoke-Expression $commandLine