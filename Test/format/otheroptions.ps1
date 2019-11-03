param (
    [string]$outputDirBase = '',
    [string]$config = '',
    [string]$runtime = ''
)


# include common settings
. ./common_format.ps1


# Globals

# The name of this test case.
Set-Variable -Name caseName -Value 'otheroptions' -Option ReadOnly -Scope Script


# call format_base.ps1
& $formatScriptPath `
    -FromDir "$inputsDir/$caseName" `
    -ToDir (CreateOutputDir $outputDirBase $caseName) `
    -Filter (GetFormatFilterCommandLine $config $runtime) `
    -RebaseOtherRelativeLinks $true `
    -OtherReadOptions @('--metadata=testparam:Hello') `
    -OtherWriteOptions @('--standalone', "--template=$formatDir/$caseName.template.html") `
    -Rebuild $true
