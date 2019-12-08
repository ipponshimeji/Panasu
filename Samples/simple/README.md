# Simple sample of Panasu

## How to use this sample

1. Note that .NET Core and PowerShell Core are required to run Panasu.
1. Install Panasu.
   Download Panasu from [Releases Page](https://github.com/ipponshimeji/Panasu/releases)
   and extract it to appropriate directory
   such as in `~/.local`.
1. Copy `format.template.ps1` file in the installed Panasu into this sample's `_scripts` directory
   and rename it as `format.ps1`.
1. Replace the default value of `PanasuPath` parameter of the `_scripts/format.ps1` script
   to point your Panasu directory.
   For example, if you locate Panasu in `~/.local/Panasu`, replace the script as follows:

    ```powershell
        [string]$PanasuPath = "$HOME/.local/Panasu"
    ```
1. Also replace the default value of `MetadataFiles` parameter of the `_scripts/format.ps1` as follows:

    ```powershell
        [string[]]$MetadataFiles = @('html.metadata.yaml'),
    ```

1. Run `format.ps1` on the `_scripts` directory.
