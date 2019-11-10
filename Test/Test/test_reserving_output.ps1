param (
    [parameter(mandatory=$true)]
    [string]$outputDir,
    [string]$testScript = '*.Tests.ps1',
    [string]$formatDocumentsScript = '',
    [string]$formatDocumentsFilter = '',
    [string]$config = '',
    [string]$runtime = ''
)


## check argument

# prepare the output dir
if (Test-Path $outputDir -PathType Container) {
    # The directory exists.
    # remove all its contents
    Remove-Item "$outputDir/*" -Recurse -ErrorAction Stop | Out-Null
} else {
    # The directory does not exist.
    # create it
    New-Item $outputDir -ItemType Directory -ErrorAction Stop | Out-Null
}

## test

Invoke-Pester -Script @{
    Path = $testScript;
    Parameters = @{
        OutputDirBase = $outputDir;
        FormatDocumentsScript = $formatDocumentsScript;
        FormatDocumentsFilter = $formatDocumentsFilter;
        Config = $config;
        Runtime = $runtime
    }
}
