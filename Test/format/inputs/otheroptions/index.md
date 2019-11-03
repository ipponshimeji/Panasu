# OtherOptions

Test of OtherReadOptions and OtherWriteOptions command line options.

### point of view

* The options specified in the OtherReadOptions command line option are passed to pandoc when it is invoked to read source.
* The options specified in the OtherWriteOptions command line option are passed to pandoc when it is invoked to write target.

In this test, a metadata "testparam" is defined in OtherReadOptions,
and a template which format the "testparam" metadata at the last of body is given in OtherWriteOptions.

### assertion

The following line must be "TestParam: Hello".
