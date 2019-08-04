<# 
.SYNOPSIS 
    Converts documents using pandoc.
.DESCRIPTION 
    TBU
.PARAMETER FromDir 
    The directory where the input documents are stored.
    The default value is '../md'.
.PARAMETER FromFormat 
    The format of the input documents.
    The default value is 'markdown'.
.PARAMETER FromExtension 
    The extension of the input document files.
    The default value is '.md'.
.PARAMETER ToDir 
    The directory where the output documents are going to be stored.
    The default value is '../html'.
.PARAMETER ToFormat 
    The format of the output documents.
    The default value is 'html'.
.PARAMETER ToExtension 
    The extension of the output document files.
    The default value is '.html'.
.PARAMETER RebaseOtherRelativeLinks 
    TBU
    The default value is $true.
.PARAMETER OtherExtensionMap 
    TBU
.PARAMETER OtherOptions 
    The array of other options to be provided to pandoc.
    The default value is @('--standalone').
.PARAMETER Filter 
    The command line of the pandoc filter to be used.
.PARAMETER Rebuild 
    Whether converts all documents.
    By default, only updated documents are converted.
.INPUTS
    None.
.OUTPUTS
    The paths of converted documents.
    The path is relative to the output directory.
#>
param (
    [string]$fromDir = '../md',
    [string]$fromFormat = 'markdown',
    [string]$fromExtension = '.md',
    [string]$toDir = '../html',
    [string]$toFormat = 'html',
    [string]$toExtension = '.html',
    [string]$filter = 'dotnet.exe ExtensionChanger.dll -R $fromFilePath $toFilePath',
    [bool]$rebaseOtherRelativeLinks = $true,
    [hashtable]$otherExtensionMap = @{},
    [string[]]$otherOptions = @('--standalone'),
    [Switch]
    [bool]$rebuild = $false
)


function IsUpToDate([string[]]$sourceFiles, [string]$destFile) {
    if (-not (Test-Path $destFile -PathType Leaf)) {
        # destFile does not exist
        $false  # not up-to-date
    } else {
        # destFile exists
        $destWriteTime = $(Get-ItemProperty $destFile).LastWriteTimeUtc
        foreach ($sourceFile in $sourceFiles) {
            $sourceWriteTime = $(Get-ItemProperty $sourceFile).LastWriteTimeUtc
            if ($destWriteTime -lt $sourceWriteTime) {
                return $false   # not up-to-date
            }
        }
        $true   # up-to-date
    }
}

function RunOnShell([string]$commandLine) {
    if ([System.IO.Path]::DirectorySeparatorChar -eq '\') {
        # on Windows
        Invoke-Expression "$env:ComSpec /c `"$commandLine`""
    } else {
        # on other than Windows
        Invoke-Expression "$env:SHELL -c '$commandLine'" 
    }

    # returns the result of execution
    $?
}

function ConvertFile([string]$fromFileRelPath) {
    # preparations
    $toFileRelPath = [System.IO.Path]::ChangeExtension($fromFileRelPath, $toExtension)
    $fromFilePath = Join-Path $fromDir $fromFileRelPath
    $toFilePath = Join-Path $toDir $toFileRelPath
    $sourceFiles = @($fromFilePath)
    $metadataOption = ''

    # check existence of the metadata file
    $metadataFilePath = "$fromFilePath.metadata.yaml"
    if (Test-Path $metadataFilePath -PathType Leaf) {
        $sourceFiles += $metadataFilePath
        $metadataOption = "--metadata-file=$metadataFilePath"
    }

    if ((-not $rebuild) -and (IsUpToDate $sourceFiles $toFilePath)) {
        # no need to convert
        "${fromFileRelPath}: Skipped (up-to-date)."
    } else {
        # convert the output 

        # make sure that the target directory exists
        $toFileDir = Split-Path $toFilePath -Parent
        if (-not (Test-Path $toFileDir -PathType Container)) {
            New-Item $toFileDir -ItemType Directory | Out-Null
        }

        # run pandoc
        if ([string]::IsNullOrWhiteSpace($filter)) {
            # with no filter
            pandoc $otherOptions $metadataOption -f $fromFormat -t $toFormat -o $toFilePath $inputFilePath
            $succeeded = $?
        } else {
            # with the specified filter
            $commandLine = "pandoc $otherOptions $metadataOption -f $fromFormat -t json $fromFilePath | " `
            + (Invoke-Expression "`"$filter`"") `
            + " | pandoc $otherOptions -f json -t $toFormat -o $toFilePath"
            $succeeded = RunOnShell $commandLine
        }

        # reported
        if ($succeeded) {
            "${fromFileRelPath}: Converted to '$toFileRelPath'."
        } else {
            # pandoc failed
            Write-Error "${fromFileRelPath}: Conversion failed."
        }
    }
}

function CopyFile([string]$fromFileRelPath) {
    $fromFilePath = Join-Path $fromDir $fromFileRelPath
    $toFilePath = Join-Path $toDir $fromFileRelPath

    if ((-not $rebuild) -and (IsUpToDate $fromFilePath $toFilePath)) {
        # no need to copy
        "${fromFileRelPath}: Skipped (up-to-date)."
    } else {
        # copy the file

        # make sure that the target directory exists
        $toFileDir = Split-Path $toFilePath -Parent
        if (-not (Test-Path $toFileDir -PathType Container)) {
            New-Item $toFileDir -ItemType Directory | Out-Null
        }

        # copy the file
        Copy-Item $fromFilePath -Destination $toFilePath
        "${fromFileRelPath}: Copied."
    }
}

function ProcessFile([string]$fromFileRelPath) {
    $extension = [System.IO.Path]::GetExtension($fromFileRelPath)
    if ($extension -eq $fromExtension) {
        # target of this conversion session
        ConvertFile $fromFileRelPath
    } elseif ($otherExtensionMap.ContainsKey($extension)) {
        # target of another conversion session
        # Do nothing. The other session will process it.
        "${fromFileRelPath}: Skipped (not a target)."
    } else {
        # other files
        # copy the file if you don't want rebasing
        if (-not $rebaseOtherRelativeLinks) {
            CopyFile $fromFileRelPath
        } else {
            "${fromFileRelPath}: Skipped (not a target)."
        }
    }
}


# make the paths absolute
$fromDir = Convert-Path $fromDir
$toDir = Convert-Path $toDir

# convert all input files in the input directory
Get-ChildItem $fromDir -File -Name -Recurse `
  | ForEach-Object { ProcessFile $_ }
