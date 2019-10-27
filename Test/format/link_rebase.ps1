param (
    [string]$outputDirBase = '',
    [string]$config = '',
    [string]$runtime = ''
)


# include common settings
. ./common_format.ps1


# Globals

# The name of this test case.
Set-Variable -Name caseName -Value 'link_rebase' -Option ReadOnly -Scope Script


# copy input dir

$actualInputDir = CreateOutputDir $outputDirBase "$caseName.input"
Copy-Item "$inputsDir/$caseName/*" $actualInputDir -Recurse

# call format_base.ps1
& $formatScriptPath `
    -FromDir "$actualInputDir" `
    -FromExtensions @('.md', '.markdown') `
    -FromFormats @('markdown', 'markdown') `
    -ToDir (CreateOutputDir $outputDirBase $caseName) `
    -ToExtensions @('.html', '.htm') `
    -ToFormats @('html', 'html') `
    -Filter (GetFormatFilterCommandLine $config $runtime) `
    -RebaseOtherRelativeLinks $true `
    -OtherExtensionMap @{'.yaml'='.yaml'} `
    -OtherWriteOptions @('--standalone', "--template=$templatePath") `
    -Rebuild $true
