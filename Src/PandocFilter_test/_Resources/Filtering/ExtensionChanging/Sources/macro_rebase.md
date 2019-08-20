% DocTitle

dummy

---
# absolute
var1:
  _macro: "rebase"
  target: "http://www.example.org/absolute.css"

# relative
var2: 
  _macro: "rebase"
  target: "styles/doc.css"

# list
var3:
  - {_macro: "rebase", target: "styles/doc1.css"}
  - {_macro: "rebase", target: "styles/doc2.css"}
---