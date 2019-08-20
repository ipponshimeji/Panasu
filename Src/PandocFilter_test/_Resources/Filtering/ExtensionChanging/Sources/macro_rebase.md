% DocTitle

dummy

---
# absolute
var1:
  _macro: "rebase"
  value: "http://www.example.org/absolute.css"

# relative
var2: 
  _macro: "rebase"
  value: "styles/doc.css"

# list
var3:
  - {_macro: "rebase", value: "styles/doc1.css"}
  - {_macro: "rebase", value: "styles/doc2.css"}
---