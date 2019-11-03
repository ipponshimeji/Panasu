param (
    [string]$outputDirBase = '',
    [string]$config = '',
    [string]$runtime = ''
)


./simple.ps1 $outputDirBase $config $runtime
./link_basic.ps1 $outputDirBase $config $runtime
./link_norebase.ps1 $outputDirBase $config $runtime
./link_rebase.ps1 $outputDirBase $config $runtime
./title.ps1 $outputDirBase $config $runtime
./macro_rebase.ps1 $outputDirBase $config $runtime
./otheroptions.ps1 $outputDirBase $config $runtime
