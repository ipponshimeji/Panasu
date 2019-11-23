# test_release.ps1

Tests Panasu features using files collected to create a release.
This is a specialized version of [test.ps1](test.md).

* [Syntax](#Syntax)
* [Description](#Description)
* [Examples](#Examples)
* [Parameters](#Parameters)
* [Inputs](#Inputs)
* [Outputs](#Outputs)
* [Related Links](#Related-Links)


## Syntax

```
test_release.ps1
  [[-TestScriptPath] <string>]
  [-OutputDir <string>]
  [-OtherOptions <string>]
  [<CommonParameters>]
```


## Description

The **test_release.ps1** script is the same to [test.ps1](test.md) except:

* It tests files in `<Working Copy Dir>/Src/__Release/Panasu`,
  where the files are collected to create a release.
  So the following parameters of `test.ps1` are not used.
    * -FormatDocumentsScript
    * -FormatDocumentsFilter
    * -Config
    * -Runtime


## Examples

### Example 1: Test the files collected to create a release

```powershell
PS C:\Panasu\Test\Test> ./test_release.ps1
```

This command tests Panasu features using files in `<Working Copy Dir>/Src/__Release/Panasu`.


## Parameters

same to [test.ps1](test.md#Parameters)


## Inputs

same to [test.ps1](test.md#Inputs)


## Outputs

same to [test.ps1](test.md#Outputs)


## Related Links

* [test.ps1](test.md)
