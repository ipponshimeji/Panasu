# test_reserving_output.ps1

Tests Panasu features reserving test output.
This is a specialized version of [test.ps1](test.md).

* [Syntax](#Syntax)
* [Description](#Description)
* [Examples](#Examples)
* [Inputs](#Inputs)
* [Outputs](#Outputs)
* [Related Links](#Related-Links)


## Syntax

```
test_reserving_output.ps1
  [-OutputDir] <String>
  [-TestScript <String>]
  [-FormatDocumentsScript <String>]
  [-FormatDocumentsFilter <String>]
  [-Config <String>]
  [-Runtime <String>]
  [<CommonParameters>]
```


## Description

The **test_reserving_output.ps1** script is the same to [test.ps1](test.md) except:

* `-OutputDir` parameter is mandatory and it can be specified as the first positioned parameter.


## Examples

### Example 1: Test the build output reserving the test output

```powershell
PS C:\Panasu\Test\Test> ./test_reserving_output.ps1 'C:\Temp\Test'
```

This command tests the build output of Panasu.sln.
The output directories of formatting test cases are created under `C:\Temp\Test` and they are not deleted after the test.


## Parameters

same to [test.ps1](test.md#Parameters)


## Inputs

same to [test.ps1](test.md#Inputs)


## Outputs

same to [test.ps1](test.md#Outputs)


## Related Links

* [test.ps1](test.md)