# Test for Rebuild option

The cases to test the Rebuild option of format_base.ps1 script.


## Point of View

format_base.ps1 updates all target files if the Rebuild option is true.
Otherwise it updates only out-of-date targets. 


## Cases

A test script in this test runs its case in following way:

1. Prepares both from and to dir and populates initial files into the both dir.
2. Changes timestamp of the prepared files so that the files have the condition to be tested.
3. Invokes format_base.ps1 command.

#### rebuild_false_source.ps1

It tests whether format_base.ps1 updates the target if:

* The target is absent in the to dir. or,
* Its source is newer than the target. or,
* The attached metadata file of its source is newer than the target.

#### rebuild_false_commonmetadata.ps1

It tests whether format_base.ps1 updates the target if:

* The common metadata file which is specified in MetadataFiles option
  is newer than the target.

#### rebuild_true.ps1

It tests whether format_base.ps1 updates all targets even if the target is up-to-date.
