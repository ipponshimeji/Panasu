param (
    [string]$outputDirBase = '',
    [string]$config = '',
    [string]$runtime = ''
)


# include common settings
. ./common_format.ps1


# Globals

# The name of this test case.
Set-Variable -Name caseName -Value 'link_norebase' -Option ReadOnly -Scope Script


# call format_base.ps1
& $formatScriptPath `
    -FromDir "$inputsDir/$caseName" `
    -FromExtensions @('.md', '.markdown') `
    -FromFormats @('markdown', 'markdown') `
    -ToDir (CreateOutputDir $outputDirBase $caseName) `
    -ToExtensions @('.html', '.htm') `
    -ToFormats @('html', 'html') `
    -Filter (GetFormatFilterCommandLine $config $runtime) `
    -RebaseOtherRelativeLinks $false `
    -OtherExtensionMap @{'.yaml'='.yaml'} `
    -OtherWriteOptions @('--standalone', "--template=$templatePath") `
    -Rebuild $true
