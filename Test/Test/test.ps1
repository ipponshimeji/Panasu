param (
    [string]$testScriptPath = '',
    [string]$outputDir = '',
    [string]$formatDocumentsScript = '',
    [string]$formatDocumentsFilter = '',
    [string]$config = '',
    [string]$runtime = ''
)

# set default value
if ([string]::IsNullOrEmpty($testScriptPath)) {
    $testScriptPath = '*.Tests.ps1'
}


# test
Invoke-Pester -Script @{
    Path = $testScriptPath;
    Parameters = @{
        OutputDirBase = $outputDir;
        FormatDocumentsScript = $formatDocumentsScript;
        FormatDocumentsFilter = $formatDocumentsFilter;
        Config = $config;
        Runtime = $runtime
    }
}
