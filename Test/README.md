# Test for Panasu

This page describes how to run the test for Panasu.


## Prerequisites

To run the test, the following software is required.

### Prerequisites for Panasu

That includes .NET Core and PowerShell (PowerShell Core or Windows PowerShell).

### Pester

This test requires [Pester](https://github.com/pester/Pester) 4.0.5 or later.

Windows 10 contains pre-installed Pester, but its version is old.
Update Pester according to the description in [Installation and Update](https://github.com/pester/Pester/wiki/Installation-and-Update)


## Test

Run the [test.ps1](test.md) to test Panasu.
See the [Examples section of test.ps1 reference](test.md#Examples) for the actual usage.

There are specialized version of test.ps1 to ease to test some cases.

* [test_reserving_output.ps1](test_reserving_output.md): Runs test reserving the formatting samples. 
* [test_release.ps1](test_release.md): Tests files collected in the `Src/__Release/Panasu` directory.


## References

* [test.ps1](test.md)
* [test_reserving_output.ps1](test_reserving_output.md)
* [test_release.ps1](test_release.md)