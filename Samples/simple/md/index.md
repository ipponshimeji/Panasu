# (title will be given by metadata in index.md.metadata.yaml)

The title above is given by metadata in `index.md.metadata.yaml`.

## Hyper Links

### Internal Link

The extension of a relative link to a file which is a target of the formatting
is changed to the one of the target format.

* an example of internal link: [Another Page](sub/another.md)
* an example of internal link with fragment: [CSS in Another Page](sub/another.md#css)

### External Link

Absolute links are not changed. 

* an example of external link: [Panasu](https://github.com/ipponshimeji/Panasu)


## Image

A relative link to a file which is not target of the formatting is handled as follows:

* If `StickOtherRelativeLinks` parameter is True,
  the link is changed to keep referencing the original file.
* If `StickOtherRelativeLinks` parameter is False,
  the link is not changed but the target file is copied to the output dir.

an example of image: ![pa](pa.png)


## Footer

The footer below is given by `include-after` metadata written in `_script/html.metadata.yaml`.
See comment in `_script/html.metadata.yaml`.
