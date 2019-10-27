param (
    [string]$outputDirBase = '',
    [string]$config = '',
    [string]$runtime = ''
)


# include common settings
. ./common_format.ps1


# Globals

# The name of this test case.
Set-Variable -Name caseName -Value 'simple' -Option ReadOnly -Scope Script


# call format_base.ps1
& $formatScriptPath `
    -FromDir "$inputsDir/$caseName" `
    -ToDir (CreateOutputDir $outputDirBase $caseName) `
    -Filter (GetFormatFilterCommandLine $config $runtime) `
    -RebaseOtherRelativeLinks $true `
    -OtherWriteOptions @('--standalone', "--template=$templatePath") `
    -Rebuild $true
