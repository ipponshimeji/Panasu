dummy

---
#### parameters
_Param.FromBaseDirPath: "/tmp/From/"
_Param.FromFileRelPath: "x/y/z/macro_rebase.md"
_Param.ToBaseDirPath: "/tmp/To/"
_Param.ToFileRelPath: "x/y/z/macro_rebase.html"
_Param.RebaseOtherRelativeLinks: false
_Param.ExtensionMap:
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

# nested-macro
nested-macro: 
  _macro: "rebase"
  target:
    _macro: "condition"
    from-file: "x/y/z/macro_rebase.md"
    true-case: "styles/ok.css"
    false-case: "styles/ng.css"

# in-array
in-array:
  - {_macro: "rebase", target: "styles/doc1.css"}
  - {_macro: "rebase", target: "styles/doc2.css"}

# in-object
in-object:
  style-1: {_macro: "rebase", target: "styles/doc1.css"}
  style-2: {_macro: "rebase", target: "styles/doc2.css"}

---