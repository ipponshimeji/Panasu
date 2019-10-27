param (
    [string]$outputDirBase = 'E:\Temp\Test',
    [string]$config = '',
    [string]$runtime = ''
)


./simple.ps1 $outputDirBase $config $runtime
./link_basic.ps1 $outputDirBase $config $runtime
./link_norebase.ps1 $outputDirBase $config $runtime
# ./link_rebase.ps1 $outputDirBase $config $runtime
