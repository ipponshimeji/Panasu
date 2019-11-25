# About Metadata Macros

* [SHORT DESCRIPTION](#SHORT-DESCRIPTION)
* [LONG DESCRIPTION](#LONG-DESCRIPTION)
* [MACRO REFERENCE](#MACRO-REFERENCE)


## SHORT DESCRIPTION

Describes metadata macros features.


## LONG DESCRIPTION

Some pandoc filters provided in `Panasu` support metadata macros.
You can write a macro as a value of metadata of a source document
when you convert the source document with such filter.

In general, conversion of a source document in `Panasu` is performed as follows:

1. `pandoc` reads a source document file and converts it into AST (Abstract Syntax Tree). 
2. The specified filter modifies the AST.
3. `pandoc` reads the filtered AST and writes it in the target format. 

In this steps, the filter expands macros in the metadata in the AST.
As a result, the filter outputs AST with expanded metadata value,
and it is reflected in the final output document.

It depends on filter what types of macros are supported.
See the description of the filter.

A macro in YAML metadata blocks looks like as follows: 

```yaml
metadata-1:
  _macro: "condition"
  from-file: "a.md"
  true-case: "this document"
  false-case: "document a"
```

In this sample, the value of `metadata-1` is a macro.
A macro is a mapping which contains `_macro` key.
The value of the `_macro` key represents type of the macro,
and other keys are parameters for the macro.
In this case, the macro is a `condition` macro and it has three parameters:
`from-file`, `true-case` and `false-case`.

Metadata macros are expanded by the filter in converting process of the source document.
The following is an example of evaluated result of the `metadata-1` metadata above:

```yaml
metadata-1: "this document"
```

How the macro is expended is depends on the type of the macro.
See [MACRO REFERENCE](#MACRO-REFERENCE) for details.


## MACRO REFERENCE

### condition macro

A `condition` macro is expanded based on its "condition" which is described later.

If the condition is true, its expanded value is:

* the value of `true-case` parameter if it exists
* null if `true-case` parameter does not exist

If the condition is false, its expanded value is:

* the value of `false-case` parameter if it exists
* null if `false-case` parameter does not exist

If the expanded value is null, the metadata itself is removed.

This macro supports the following conditions:

#### from-file condition

If the macro has `from-file` parameter, from-file condition is evaluated.

The from-file condition is true if the path of the source document file is equal to the value of `from-file` parameter.
The paths are compared as relative path from the base directory for source documents,
and the comparison is case-insensitively.

For example, the following metadata

```yaml
css:
  _macro: "condition"
  from-file: "toc.md"
  true-case: "toc.css"
  false-case: "doc.css"
```

is expanded as follows if it is the metadata of "toc.md":

```yaml
css: "toc.css"
```

otherwise is expanded as follows:

```yaml
css: "doc.css"
```

### rebase macro

The expanded value of `rebase` macro is rebased `target` parameter value.
The value of `target` parameter is an absolute path or a relative path from the base directory where the converted document files are output.

* If it is an absolute path, the expanded value is the path with no change.
* If it is a relative path, the expanded value is the rebased relative path from the converted document file.

For example, the following metadata of "subdir/a.md"

```yaml
css: 
  _macro: "rebase"
  target: "styles/doc.css"
```

is expanded as follows:

```yaml
css: "../styles/doc.css"
```
