# test.ps1

Tests Panasu features.

* [Syntax](#Syntax)
* [Description](#Description)
* [Examples](#Examples)
* [Inputs](#Inputs)
* [Outputs](#Outputs)
* [Related Links](#Related-Links)


## Syntax

```
test.ps1
  [[-TestScriptPath] <String>]
  [-OutputDir <String>]
  [-FormatDocumentsScript <String>]
  [-FormatDocumentsFilter <String>]
  [-Config <String>]
  [-Runtime <String>]
```


## Description

The **test.ps1** script tests the features of Panasu.

It uses [Pester](https://github.com/pester/Pester) as its test framework.
Pester 4.0.5 or later must be available to run this script.

The subjects to be tested include following features:

* Format-Documents.ps1 script
* utility functions defined in test script itself



## Examples

### Example 1: Test the build output

```powershell
PS C:\Panasu\Test\Test> ./test.ps1
```

This command tests the build outout from Panasu.sln
of the default configuration and the default target runtime, 
that is `Debug` and `netcoreapp3.0` respectively.

The Panasu.sln must be built successfully for the configuration and the target runtime before the command is executed.

### Example 2: Test the build output of the specific configuration and target runtime

```powershell
PS C:\Panasu\Test\Test> ./test.ps1 -Configuration Release -Runtime netcoreapp2.2
```

This command tests the build outout from Panasu.sln
of `Release` configuration and .NET Core 2.2 target. 

The Panasu.sln must be built successfully for the configuration and the target runtime before the command is executed.

### Example 3: Test the Panasu in the specified directory

```powershell
PS C:\Panasu\Test\Test> ./test.ps1 -FormatDocumentsScript 'C:\Panasu\Format-Documents.ps1' -formatDocumentsFilter 'dotnet C:\Panasu\FormatAST.dll'
```

This command tests Panasu located in `C:\Panasu`.

### Example 4: Reserve test output 

```powershell
PS C:\Panasu\Test\Test> ./test.ps1 -OutputDir 'C:\Temp\Test'
```

This command test the build output of Panasu.sln.
The output directories of formatting test cases are created under `C:\Temp\Test` and they are not deleted after the testing.

### Example 5: Run the specific Test 

```powershell
PS C:\Panasu\Test\Test> ./test.ps1 -testScriptPath 'Format-Documents.Tests.ps1'
```

This command test only test cases written in Format-Documents.Tests.ps1.

## Parameters

### -Config

Semantically, it specifies the name of configuration by which the testing files are built.

Accurately, it specifies the name of the directory which is a part of the build output path and means its configuration.
That is, the `<Configuration>` part of the following path,
which is typical output path of a C# project:

```
(Project Dir)/bin/<Configuration>/<Runtime>
```

Generally it is either `Debug` or `Release`.

This value is used if the script tests the built files directly. 

If this parameter is omitted or its value is an empty string,
the script decides the default value.
The current default value is `Debug`.
Note that the default value will be changed
if the default configuration of Panasu is changed.  

|||
|:--|:--| 
| Type: | String |
| Position: | Named |
| Default value: | '' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |


### -FormatDocumentsFilter

Specifies the command line of the filter which is given to Format-Documents.ps1.
Generally, it forms as follows:

```
dotnet <path to FormatAST.dll>
```

If this parameter is omitted or its value is an empty string,
the script uses the following value.

```
dotnet <Working Copy Root>/Src/FormatAST/bin/<Configuration>/<Runtime>/FormatAST.dll
```

Where `<Configuration>` and `<Runtime>` are values of `Configuration` and `Runtime` parameter respectively.
That is, the script uses the build output of the specified configuration and target runtime.

|||
|:--|:--| 
| Type: | String |
| Position: | Named |
| Default value: | '' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -FormatDocumentsScript

Specifies the path to the Format-Documents.ps1 script to be tested.

If this parameter is omitted or its value is an empty string,
the script uses the following value.

```
<Working Copy Root>/Src/Scripts/Format-Documents.ps1
```

That is, the script uses the developing script in the working copy.

|||
|:--|:--| 
| Type: | String |
| Position: | Named |
| Default value: | '' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -OutputDir

Specifies the path of the output directory.

Some test cases in this test format sample sources into output directories and verify them.
Those output directories are created under this path.

If this parameter is omitted or its value is an empty string,
the script creates a temporary directory and use it as the root of the output directories for the cases.
The temporary directory is removed after the test is completed.
So you must specify this parameter explicitly if you want to verify the test output.

|||
|:--|:--| 
| Type: | String |
| Position: | Named |
| Default value: | '' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -Runtime

Semantically, it specifies the name of platform for which the testing files are built.

Accurately, it specifies the name of the directory which is a part of the build output path and means its target platform.
That is, the `<Runtime>` part of the following path,
which is typical output path of a C# project:

```
(Project Dir)/bin/<Configuration>/<Runtime>
```

This value is used if the script tests the built files directly. 

If this parameter is omitted or its value is an empty string,
the script decides the default value.
The current default value is `netcoreapp3.0`,
that is the name for .NET Core 3.0 platform.
Note that the default value will be changed
if the default target platform of Panasu is changed.  

|||
|:--|:--| 
| Type: | String |
| Position: | Named |
| Default value: | '' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -TestScriptPath

Specifies the path of the test scripts to be run.

This value is passed to the `Path` value of `Script` parameter of `Invoke-Pester` commandlet.

If this parameter is omitted or its value is an empty string,
the script uses the following path:

```
*.Tests.ps1
```

|||
|:--|:--| 
| Type: | String |
| Position: | 0 |
| Default value: | '' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |


## Inputs

### None

You cannot pipe input to this script.

## Outputs

### None

This scripts does not generate any output.


## Related Links

* [test_reserving_output.ps1](test_reserving_output.md)