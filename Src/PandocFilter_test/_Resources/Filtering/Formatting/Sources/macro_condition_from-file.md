dummy

---
#### parameters
_Param.FromBaseDirPath: "/tmp/From/"
_Param.FromFileRelPath: "x/macro_condition_from-file.md"
_Param.ToBaseDirPath: "/tmp/To/"
_Param.ToFileRelPath: "x/macro_condition_from-file.html"
_Param.RebaseOtherRelativeLinks: false
_Param.ExtensionMap:
  .md: ".html"


#### test cases

# single_true-case
single_true-case:
  _macro: "condition"
  from-file: "x/macro_condition_from-file.md"
  true-case: "OK"
  false-case: "NG"

# single_false-case
single_false-case:
  _macro: "condition"
  from-file: "other.md"
  true-case: "NG"
  false-case: "OK"

# multiple_true-case
multiple_true-case:
  _macro: "condition"
  from-file:
    - "other-1.txt"
    - "x/macro_condition_from-file.md"
    - "other-2.md"
  true-case: "OK"
  false-case: "NG"

# multiple_false-case
multiple_false-case:
  _macro: "condition"
  from-file:
    - "other-1.txt"
    - "other-2.md"
    - "other-3.md"
  true-case: "NG"
  false-case: "OK"

# remove_true-case
remove_true-case:
  _macro: "condition"
  from-file: "x/macro_condition_from-file.md"
  false-case: "NG"

# remove_false-case
remove_false-case:
  _macro: "condition"
  from-file: "other.md"
  true-case: "NG"

# nested-macro
nested-macro:
  _macro: "condition"
  from-file: "x/macro_condition_from-file.md"
  true-case:
    _macro: "condition"
    from-file: "other.md"
    false-case: "OK"
  false-case: "NG"

# in-array
in-array:
  - _macro: "condition"
    from-file: "other.md"
    true-case: "NG"
    false-case: "OK"
  - _macro: "condition"
    from-file: "other-md"
    true-case: "OK"
  - _macro: "condition"
    from-file: "x/macro_condition_from-file.md"
    true-case: "OK"
    false-case: "NG"

# in-object
in-object:
  item-1:
    _macro: "condition"
    from-file: "other.md"
    true-case: "NG"
    false-case: "OK"
  item-2:
    _macro: "condition"
    from-file: "other-md"
    true-case: "OK"
  item-3:
    _macro: "condition"
    from-file: "x/macro_condition_from-file.md"
    true-case: "OK"
    false-case: "NG"

---