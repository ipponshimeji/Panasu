dummy

---
#### parameters
parameters-PandocUtil.PandocFilter.Filters.FormattingFilter:
  FromBaseDirPath: "/tmp/From/"
  FromFileRelPath: "x/y/z/macro_rebase.md"
  ToBaseDirPath: "/tmp/To/"
  ToFileRelPath: "x/y/z/macro_rebase.html"
  RebaseOtherRelativeLinks: false
  ExtensionMap:
    .md: ".html"


#### test cases

# absolute
absolute:
  _macro: "rebase"
  target: "http://www.example.org/absolute.css"

# relative
relative: 
  _macro: "rebase"
  target: "styles/doc.css"

# in-list
in-list:
  - {_macro: "rebase", target: "styles/doc1.css"}
  - {_macro: "rebase", target: "styles/doc2.css"}

# nested-macro
nested-macro: 
  _macro: "rebase"
  target:
    _macro: "condition"
    from-file: "x/y/z/macro_rebase.md"
    true-case: "styles/ok.css"
    false-case: "styles/ng.css"
---