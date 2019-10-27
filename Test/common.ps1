# Script Globals

Set-Variable -Name testDir -Value (Split-Path -Parent $MyInvocation.MyCommand.Path) -Option ReadOnly -Scope Script
Set-Variable -Name repositoryDir -Value "$testDir/.." -Option ReadOnly -Scope Script
Set-Variable -Name defaultConfig -Value 'Debug' -Option ReadOnly -Scope Script
Set-Variable -Name defaultRuntime -Value 'netcoreapp3.0' -Option ReadOnly -Scope Script


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
        Remove-Item "$outputDirPath/*" -Recurse | Out-Null
    } else {
        New-Item $outputDirPath -ItemType Directory -ErrorAction Stop | Out-Null
    }

    # return the path of the output directory
    $outputDirPath
}

function GetFilterPath([string]$project, [string]$fileName, [string]$config, [string]$runtime) {
    # adjust the parameters
    if ([string]::IsNullOrEmpty($config)) {
        $config = $defaultConfig
    }
    if ([string]::IsNullOrEmpty($runtime)) {
        $runtime = $defaultRuntime
    }

    # create the path of the filter
    $filterPath = "$repositoryDir/Src/$project/bin/$config/$runtime/$fileName"
    if (-not (Test-Path $filterPath -PathType Leaf)) {
        Write-Error "The filter module '$filterPath' is not found."
        exit 1
    }

    # return the path of the filter
    $filterPath
}

function GetFilterCommandLine([string]$project, [string]$fileName, [string]$config, [string]$runtime) {
    # get the path of the filter
    $filterPath = GetFilterPath $project $fileName $config $runtime

    # return the command line
    "dotnet $filterPath"
}
