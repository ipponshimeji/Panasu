# Sub Page

The metadata mentioned below is written in the common metadata file,
which is specified in the command line of the format_base.ps1.

## "rebase" macro

#### point of view

A "rebase" macro changes a given path from relative from output base dir to relative from this document. 
In this test case, the "rebase" macro is used to specify a css path in "css" metadata.

#### assertions

```
The background of this block must be magenta.
```

## "condition" macro

#### point of view

A "condition" macro selects a value of metadata based on the given condition.
In this test case, the "condition" macro is used to define contents "include-after" metadata.

#### assertions

The following block must be:

* "from-file" condition: true
