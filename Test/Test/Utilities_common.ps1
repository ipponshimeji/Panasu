## Script Globals

Set-Variable -Name minPesterVer -Value (New-Object 'System.Version' @(4,0,5)) -Option ReadOnly -Scope Script
Set-Variable -Name pathSeparator -Value ([System.IO.Path]::DirectorySeparatorChar) -Option ReadOnly -Scope Script
Set-Variable -Name testDir -Value (Split-Path -Parent $MyInvocation.MyCommand.Path) -Option ReadOnly -Scope Script
Set-Variable -Name resourcesDirBase -Value "$testDir/../_Resources" -Option ReadOnly -Scope Script
Set-Variable -Name repositoryDir -Value "$testDir/../.." -Option ReadOnly -Scope Script
Set-Variable -Name defaultConfig -Value 'Debug' -Option ReadOnly -Scope Script
Set-Variable -Name defaultRuntime -Value 'netcoreapp3.0' -Option ReadOnly -Scope Script


## Assertions

function DirHaveSameContentsTo([string]$actualValue, [string]$expectedValue, [switch]$negate) {
    [bool]$succeeded = $true
    [string[]]$messages = @()

    # create the set of actual files
    $actualFiles = New-Object 'System.Collections.Generic.HashSet[string]' `
                              -ArgumentList (,[string[]](Get-ChildItem $actualValue -File -Name -Recurse))

    # compare the each file in the directories
    Get-ChildItem $expectedValue -File -Name -Recurse | ForEach-Object {
        if (-not $actualFiles.Remove($_)) {
            # no corresponding actual file 
            $succeeded = $false
            $messages += "An expected file is not found: $_"
        } else {
            [string[]]$paths = @((Join-Path $expectedValue $_ -Resolve), (Join-Path $actualValue $_ -Resolve))
            [string[]]$hashs = (Get-FileHash $paths -Algorithm SHA256 | Select-Object -Property Hash )

            if ($hashs[0] -ne $hashs[1]) {
                $succeeded = $false
                $messages += "The actual file has different contents from expected: $_"
            }
        }
    }
    if (0 -lt $actualFiles.Count) {
        # there is an unexpected file
        $succeeded = $false
        $actualFiles | ForEach-Object { $messages += "An unexpected file is found: $_" }
    }

    # report the result
    if ($negate) {
        # negate the result
        if ($succeeded) {
            $message = 'The actual dir has the same contents to the expected dir''s.'
        } else {
            $message = ''
        }
        $succeeded = -not $succeeded
    } else {
        $message = $messages -join "`n"
    }
    return New-Object PSObject -Property @{
        Succeeded = $succeeded
        FailureMessage = $message
    }
}


## Utilities

function CheckPesterVersion() {
    if ((Get-Module Pester).Version -lt $minPesterVer) {
        Write-Error "Pester version $minPesterVer or later is required."
        exit 1
    } 
}

function CreateTempDir() {
    # create a temporary file
    $path = [System.IO.Path]::GetTempFileName()

    # convert the file to a directory
    Remove-Item $path
    New-Item $path -ItemType Directory -ErrorAction Stop | Out-Null

    # return the path of the directory
    $path
}

function CreateOutputDir([string]$baseDirPath, [string]$dirName) {
    # create a temporary directory if base dir is not specified
    if ([System.String]::IsNullOrEmpty($baseDirPath)) {
        $baseDirPath = CreateTempDir
    }

    # create the output directory
    $outputDirPath = [System.IO.Path]::Combine($baseDirPath, $dirName)
    if (Test-Path $outputDirPath -PathType Container) {
        # The directory exists.
        # remove all its contents
        Remove-Item "$outputDirPath/*" -Recurse -ErrorAction Stop | Out-Null
    } else {
        # The directory does not exist.
        # create it
        New-Item $outputDirPath -ItemType Directory -ErrorAction Stop | Out-Null
    }

    # return the path of the output directory
    $outputDirPath
}

function GetFilterModulePath([string]$project, [string]$fileName, [string]$config, [string]$runtime) {
    # adjust the parameters
    if ([string]::IsNullOrEmpty($config)) {
        $config = $defaultConfig
    }
    if ([string]::IsNullOrEmpty($runtime)) {
        $runtime = $defaultRuntime
    }

    # create the path of the module
    $modulePath = "$repositoryDir/Src/$project/bin/$config/$runtime/$fileName"
    if (-not (Test-Path $modulePath -PathType Leaf)) {
        Write-Error "The filter module '$modulePath' is not found."
        exit 1
    }

    # return the path of the module
    $modulePath
}

function GetFilter([string]$project, [string]$fileName, [string]$config, [string]$runtime) {
    # return the command line
    "dotnet $(GetFilterModulePath $project $fileName $config $runtime)"
}
