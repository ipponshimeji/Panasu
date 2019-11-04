param (
    [string]$outputDirBase = '',
    [string]$config = '',
    [string]$runtime = ''
)


## include common settings
. ./common_format.ps1


## Globals

# The name of this test case.
Set-Variable -Name caseName -Value 'rebuild_false_source' -Option ReadOnly -Scope Script

# The command line of the filter.
$filter = GetFormatFilterCommandLine $config $runtime

# time stamps
# The 2 seconds difference is decided because the timestamp resolution of old FAT is 2 seconds.
$now = [DateTime]::UtcNow
$fromTimeStamp = $now.AddSeconds(-4) 
$toTimeStamp = $now.AddSeconds(-2) 


## setup input dir
$fromDir = CreateOutputDir $outputDirBase "$caseName.from"
Copy-Item "$inputsDir/$caseName/*" $fromDir -Recurse

# set time stamps of from files to 4 seconds ago
Get-ChildItem $fromDir -File -Recurse | ForEach-Object { $_.LastWriteTimeUtc = $fromTimeStamp }
# change time stamps of 'should be updated' trigger files to now
@(
    'index.md';
    'shouldbeupdated_solo.md';
    'shouldbeupdated_withmetadata.md.metadata.yaml';
    'shouldbeupdated_copy.txt'
) | ForEach-Object { (Get-Item "$fromDir/$_").LastWriteTimeUtc = $now }


## setup output dir
$toDir = (CreateOutputDir $outputDirBase $caseName)
Copy-Item "$inputsDir/$caseName.to/*" $toDir -Recurse
# set time stamps of to files to 2 seconds ago
Get-ChildItem $toDir -File -Recurse | ForEach-Object { $_.LastWriteTimeUtc = $toTimeStamp }


## call format_base.ps1
& $formatScriptPath `
    -FromDir $fromDir `
    -ToDir $toDir `
    -Filter $filter `
    -RebaseOtherRelativeLinks $false `
    -OtherWriteOptions @('--standalone', "--template=$templatePath") `
    -Rebuild $false
