dummy

---
#### parameters
parameters-PandocUtil.PandocFilter.Filters.FormattingFilter:
  FromBaseDirPath: "/tmp/From/"
  FromFileRelPath: "x/macro_condition_from-file.md"
  ToBaseDirPath: "/tmp/To/"
  ToFileRelPath: "x/macro_condition_from-file.html"
  RebaseOtherRelativeLinks: false
  ExtensionMap:
    .md: ".html"


#### test cases

# single_true-case
single_true-case:
  _macro: "condition"
  from-file: "macro_condition_from-file.md"
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
    - "macro_condition_from-file.md"
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
  from-file: "macro_condition_from-file.md"
  false-case: "NG"

# remove_false-case
remove_false-case:
  _macro: "condition"
  from-file: "other.md"
  true-case: "NG"

# nested-macro
nested-macro:
  _macro: "condition"
  from-file: "macro_condition_file.md"
  true-case:
    _macro: "condition"
    from-file: "other.md"
    false-case: "OK"
  false-case: "NG"
---