param (
    [string]$Config = 'Release',
    [string]$Runtime = 'netcoreapp3.0'
)

# The full path to the Format-Documents.ps1 script to be invoked.
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Set-Variable -Name srcDir -Value $scriptDir -Option ReadOnly -Scope Script
Set-Variable -Name releaseDir -Value (Join-Path $srcDir '__Release') -Option ReadOnly -Scope Script
Set-Variable -Name contentsDir -Value (Join-Path $releaseDir 'Panasu') -Option ReadOnly -Scope Script
Set-Variable -Name repDir -Value (Join-Path $srcDir '..') -Option ReadOnly -Scope Script


## detect version

# read the package version from the solution's common property settings
$props = [xml](Get-Content "$srcDir/_Configuring/Properties.props")
$packageVersion = $props.Project.PropertyGroup.version


## prepare output dir

# prepare the release dir
if (Test-Path $releaseDir -PathType Container) {
    # The directory exists.
    # remove all its contents
    Remove-Item "$releaseDir/*" -Recurse -ErrorAction Stop | Out-Null
} else {
    # The directory does not exist.
    # create it
    New-Item $releaseDir -ItemType Directory -ErrorAction Stop | Out-Null
}

# create contents dir
New-Item $contentsDir -ItemType Directory -ErrorAction Stop | Out-Null


## collect files

# FormatAST files
Copy-Item "$srcDir/FormatAST/bin/$Config/$Runtime/publish/*" $contentsDir -Recurse -Exclude 'FormatAST.exe'

# scripts
Copy-Item "$srcDir/Scripts/*" $contentsDir


## zip the files

$packageFile = "$releaseDir/Panasu_$packageVersion.zip"
Compress-Archive -Path "$contentsDir" -DestinationPath $packageFile
(Get-FileHash $packageFile -Algorithm SHA256).Hash | Set-Content "$packageFile.sha256"
