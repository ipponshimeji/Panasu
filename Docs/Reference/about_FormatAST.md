# About FormatAST

* [SHORT DESCRIPTION](#SHORT-DESCRIPTION)
* [LONG DESCRIPTION](#LONG-DESCRIPTION)
* [COMMAND SYNTAX](#COMMAND-SYNTAX)
* [PARAMETERS](#PARAMETERS)
* [AST MODIFICATION](#AST-MODIFICATION)


## SHORT DESCRIPTION

Describes the features of FormatAST which is the default filter of [Format-Documents.ps1](Format-Documents.md) script. 


## LONG DESCRIPTION

FormatAST is a [pandoc filter](https://pandoc.org/filters.html).
[Format-Documents.ps1](Format-Documents.md) uses FormatAST as its default filter when it formats source document files.

FormatAST is a .NET Core application.
It read pandoc AST from standard input, and write modified AST to standard output.
See [AST MODIFICATION](#AST-MODIFICATION) how it modifies the AST.


## COMMAND SYNTAX

```
dotnet FormatAST.dll [options]
```

Usually parameters for this filter are embedded in metadata in the input AST.
See [PARAMETERS](#PARAMETERS) for details of parameters.

However you can specify or overwrite some parameters by command line options.
The correspondence between command line option and parameter is as follows: 

#### --ExtensionMap <mapping> or -m <mapping>

Corresponds to `ExtensionMap` parameter.
Its form must be `"<from extension>:<to extension>"`, for example `".md:.html"`.
This option can be specified multiple times to define multiple mappings.

#### --FromBaseDirPath <path> or -fd <path>

Corresponds to `FromBaseDirPath` parameter.

#### --FromFileRelPath <path> or -ff <relative path>

Corresponds to `FromFileRelPath` parameter.

#### --RebaseOtherRelativeLinks or -r

Corresponds to `RebaseOtherRelativeLinks` parameter.
If this option is set, the `RebaseOtherRelativeLinks` parameter is set to true.

#### --ToBaseDirPath <path> or -td <path>

Corresponds to `ToBaseDirPath` parameter.

#### --ToFileRelPath <path> or -tf <relative path>

Corresponds to `ToFileRelPath` parameter.


## PARAMETERS

Parameters are embedded in the metadata of the input AST as yaml data.

#### ExtensionMap: mapping

The mapping of extensions.
This parameter is optional.

In the filtering, this filter replaces extension of a relative link in the input AST
if the extension is a target of the extension mappings.
See [Extension replacement](#Extension-replacement) for details of the extension replacement.

Example:

```yaml
_Param.ExtensionMap:
    .md: ".html"
    .markdown: ".html"
```

#### FromBaseDirPath: string

The path of the base directory where the source document files are located.
This parameter is mandatory.

Example:

```yaml
_Param.FromBaseDirPath: "c:/docs/md"
```

#### FromFileRelPath: string

The path of the source document file which is being converted.
The path must be relative from the path of FromBaseDirPath parameter
This parameter is mandatory.

Note that this file may not be the direct input of this filter,
but it is the original source of this formatting.

Example:

```yaml
_Param.FromFileRelPath: "sub/a.md"
```

#### RebaseOtherRelativeLinks: bool

This parameter is optional. Its default value is true.

If this parameter is true, a relative link whose extension is not target of extension mapping
is rebased from the directory of ToBaseDirPath
so that the links keep to reference the files in the original location.

If this parameter is false, such links are not changed. They references the files under the directory of ToBaseDirPath.

See [Rebasing relative links to non extension mapping target](#Rebasing-relative-links-to-non-extension-mapping-target) for details of the rebasing.

Example:

```yaml
_Param.RebaseOtherRelativeLinks: false
```

#### ToBaseDirPath: string

The path of the base directory where the formatted document files are output.
This parameter is mandatory.

Example:

```yaml
_Param.ToBaseDirPath: "c:/docs/html"
```

#### ToFileRelPath: string

The path of the formatted document file.
The path must be relative from the path of ToBaseDirPath parameter
This parameter is mandatory.

Note that this file may not be the direct output of this filter,
but it is the final output of this formatting.

```yaml
_Param.ToFileRelPath: "sub/a.html"
```


## AST MODIFICATION

This filter makes the following modifications to the input AST.

### Extension replacement

This filter replaces the extension of a relative link in the input AST
if the extension is a target of extension mappings given by ExtensionMap parameter.

For example, a link to "a.md" in the input AST is replaced to "a.html"
if the extension mappings have the mapping from ".md" to ".html".  

### Title arrangement

This filter arranges the document title with `pandoc`'s title handling.

`pandoc` recognizes document's title only by its `title` metadata,
and it emits it as a level-1 header regardless of existence of other level-1 headers
in some target formats such as html.

So the filter modifies the input AST as follows:

* When the input AST has no `title` metadata,
  the filter adds equivalent contents of the first level-1 header if it exists.
* remove all level-1 headers

In conclusion, if you write only one leve-1 header at the top of your source document file,
its output will be what you expect.

### Rebasing relative links to non extension mapping target

If RebaseOtherRelativeLinks parameter is true,
a relative link in the input AST whose extension is not target of extension mappings
is rebased from the directory of ToBaseDirPath parameter
so that the link keep to reference the file in the original location.

For example, a link to "a.png" in the input AST is rebased to "../md/a.png"
if FromBaseDirPath is "/docs/md" and ToBaseDirPath is "/docs/html".

If RebaseOtherRelativeLinks parameter is false,
the filter does not change such links.
The link target is supposed to be located in ToBaseDirPath.
Some scripts which use this filter, such as [Format-Documents.ps1](Format-Documents.md),
copies such files from FromBaseDirPath to ToBaseDirPath in this case.

### Macro processing

This filter processes the following macros in metadata in the input AST:

* condition
* rebase

See [about_Macros](about_Macros.md) for details of macro processing.
