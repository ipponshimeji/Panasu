# Format-Documents.ps1

Formats the documents under the specified directory using pandoc.

* [Syntax](#Syntax)
* [Description](#Description)
* [Parameters](#Parameters)
* [Inputs](#Inputs)
* [Outputs](#Outputs)
* [Related Links](#Related-Links)


## Syntax

```
Format-Documents.ps1
  [-FromDir <String>]
  [-FromExtensions <String[]>]
  [-FromFormats <String[]>]
  [-ToDir <String>]
  [-ToExtensions <String[]>]
  [-ToFormats <String[]>]
  [-MetadataFiles <String[]>]
  [-Filter <String>]
  [-RebaseOtherRelativeLinks <Bool>]
  [-OtherExtensionMap <Hashtable>]
  [-OtherReadOptions <String[]>]
  [-OtherWriteOptions <String[]>]
  [-Rebuild <Bool>]
  [-Silent <Bool>]
  [-NoOutput <Bool>]
  [<CommonParameters>]
```


## Description

The **Format-Documents.ps1** script formats source document files under the input directory given by `FromDir` parameter.
The formatted document files are output in the output directory given by `ToDir` parameter.

#### Formatting process

This script uses `pandoc` to format source document files.
`pandoc` 2.3 or later must be installed to run this script.

Formatting of each file is performed as follows:

1. `pandoc` reads a source document file and converts it into AST (Abstract Syntax Tree). 
2. The filter specified by `Filter` parameter modifies the AST.
3. `pandoc` reads the filtered AST and writes it into the output directory in the target format. 

The relation of the extension of source document file, the format name of source document,
the extension of formatted document file and the format name of formatted document is given by
`FromExtensions`, `FromFormats`, `ToExtensions` and `ToFormats` parameters respectively,
where "format name" means a value which can be passed to -f or -t option of `pandoc`.

The default filter, `FormatAST` is provided along with this script.
See [about_FormatAST](about_FormatAST.md) for how it modifies the AST.

Only files whose extension is contained in `FromExtensions` parameter are formatted.
This script does not process other files,
but some files are copied to `ToDir` if `RebaseOtherRelativeLinks` parameter is false.
See [the description of RebaseOtherRelativeLinks parameter](#-RebaseOtherRelativeLinks) for details.

#### Extension mapping

The default filter replaces the extension of relative links in the input AST.
For example, a link to "a.md" in the input AST is replaced to one to "a.html".
The extension mappings used in this replacement consists of the following mappings:

* from extensions given by `FromExtensions` parameter to ones given by `ToExtensions` parameter
* the mapping specified by `OtherExtensionMap` parameter

An extension which is not a target of the extension mappings is not replaced.

#### Metadata

When `pandoc` converts a document,
you can give metadata along with the source document as "parameter" of conversion.

There are some ways to provide metadata in this script's process.

* In `pandoc`, metadata can be embedded in a source document in some formats such as markdown.
  See [Metadata blocks](https://pandoc.org/MANUAL.html#metadata-blocks) in [Pandoc User's Guide](https://pandoc.org/MANUAL.html).
  Note that this notation may decrease compatibility of the source document as markdown format.
* If file named "&lt;source document file name&gt;.metadata.yaml" exists in the same directory,
  this script takes it as metadata for the source document.
  For example, this script regards "a.md.metadata.yaml" as metadata of "a.md" if it exists.
  The metadata file must be a YAML file.
* You can specify metadata files which are commonly used for this formatting process by `MetadataFiles` parameter.
  The metadata file must be a YAML file.

This scripts combines metadata files, embeds parameters for the filter into it and passes it to `pandoc`.

Some filters, including the default filter, support metadata macro.
See [about_MetadataMacro](about_MetadataMacro.md) for details.


## Parameters

### -Filter

The command line of the pandoc filter to be used in the formatting process.

If this parameter is null or empty string, "dotnet $scriptDir/FormatAST.dll" is used,
where $scriptDir is the directory where this script is located.
See [about_FormatAST](about_FormatAST.md) for details about FormatAST.

This script embeds the parameters for the filter into the metadata of the source document.
The source document and metadata are converted into AST,
and the specified filter reads the AST.
Then the filter can reference the parameters from the metadata in the input AST.

The parameters for filter embedded in metadata are ones which are requied in the default filter, `FormatAST`.
So if you specify a custom filter, the filter can use those parameters via the input AST.
See [about_FormatAST](about_FormatAST.md) for the details about parameters.

|||
|:--|:--| 
| Type: | String |
| Position: | Named |
| Default value: | '' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -FromDir

The directory where the source document files to be formatted are located. 

|||
|:--|:--| 
| Type: | String |
| Position: | Named |
| Default value: | '../md' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |


### -FromExtensions

The array of extension for source document files.

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @('.md') |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -FromFormats

The array of format name for source document files.

Each name must be one which can be passed to -f option of pandoc.

Each item in this argument corresponds to the item in `FromExtensions` respectively.
The array length of this parameter must be same to the one of `FromExtensions`.

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @('markdown') |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -MetadataFiles

The array of yaml file which describes common metadata for this formatting session.
The all specified metadata are attached to all source document files. 

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @() |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -NoOutput

If this parameter is True, this script returns no object.
If this parameter is False, this script returns an object which stores the list of processed file.

|||
|:--|:--| 
| Type: | Bool |
| Position: | Named |
| Default value: | False |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -OtherExtensionMap

The extension mappings other than the mappings from `FromExtensions` to `ToExtensions` parameter.
The filter replaces those extensions in relative links according the extension mappings.

By specifying a mapping from an extension to the same extension, for example '.yaml'='.yaml',
files with the extension are excluded from the target of rebasing or copying by `RebaseOtherRelativeLinks` parameter,
suppressing the extension replacement of relative links to the files. 

|||
|:--|:--| 
| Type: | Hashtable |
| Position: | Named |
| Default value: | @{'.yaml'='.yaml'} |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -OtherReadOptions

The array of other read option to be passed to pandoc
when it is invoked to read the source document files.

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @() |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -OtherWriteOptions

The array of other write option to be passed to pandoc
when it is invoked to write the formatted document files.

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @('--standalone') |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -RebaseOtherRelativeLinks

If this parameter is True, relative links to files which are not target of extension mapping
should be changed so that the links keep to reference the files in the original location.

If this parameter is False, this script copies such files into the output directory along with the formatted files,
and do not change links to the files.

|||
|:--|:--| 
| Type: | Bool |
| Position: | Named |
| Default value: | True |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -Rebuild

If this parameter is True, this script processes all files.
If this parameter is False, this script processes only updated files.

|||
|:--|:--| 
| Type: | Bool |
| Position: | Named |
| Default value: | False |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -Silent

If this parameter is True, this script does not display progress.
If this parameter is False, this script displays progress, that is name of processed file.

|||
|:--|:--| 
| Type: | Bool |
| Position: | Named |
| Default value: | False |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -ToDir

The directory where the formatted document files are going to be stored.

|||
|:--|:--| 
| Type: | String |
| Position: | Named |
| Default value: | '../html' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -ToExtensions

The array of extension for formatted document files.

Each item in this parameter corresponds to the item in `FromExtensions` respectively.
The array length of this parameter must be same to the one of `FromExtensions`.

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @('.html') |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -ToFormats

The array of format for formatted document files.

Each value must be one which can be passed to -t option of pandoc.

Each item in this argument corresponds to the item in `FromExtensions` respectively.
The array length of this parameter must be same to the one of `FromExtensions`.

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @('html') |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |


## Inputs

### None

You cannot pipe input to this script.


## Outputs

### None or PSCustomObject

This script outputs none if NoOutput parameter is True.
Otherwise it outputs an object which has following properties:

* `Copied`: The list of file which this script copies to the output directory.
* `Failed`: The list of file for which this script fails in processing.
* `Formatted`: The list of file which this script successfully formats.
* `NotTarget`: The list of file which this script does not process.
* `UpToDate`: The list of file for which this script skips processing because it is up-to-date.

The files are represented in relative path from the input directory.


## Related Links

[about_FormatAST](about_FormatAST.md)
[about_MetadataMacros](about_MetadataMacros.md)
