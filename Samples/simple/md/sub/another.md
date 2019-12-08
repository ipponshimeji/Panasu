#  Another Page

[Back to the Top Page](../index.md)

## Common Metadata

Common metadata in this formatting session are given by common metadata file, `_scripts/html.metadata.yaml`.
For example, `lang` setting in the output HTML files is given by the `lang` metadata defined in the common metadata file.  
The common metadata file is specified in `MetadataFiles` parameter of the `format.ps1` script.


## CSS

The css file is also specified in the common metadata file using 'rebase' macro.
By the rebase macro, the relative path to the css file is adjusted to each file.
For example, css path for this page is "../styles/style.css" while one in ../index.html is "styles/style.css".

The background of the following code block should be yellow due to the applied css.

```c#
class Program {
    public static void Main() {
        Console.WriteLine("Hello World.");
    }
}
```

## Footer

The footer below is given by `include-after` metadata in the common metadata file.
See comment in `_script/html.metadata.yaml`.
