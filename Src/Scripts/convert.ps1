<# 
.SYNOPSIS 
    Converts documents using pandoc.
.DESCRIPTION 
    TBU
.PARAMETER InputDir 
    The directory where the input documents are stored.
    The default value is '../md'.
.PARAMETER InputFormat 
    The format of the input documents.
    The default value is 'markdown'.
.PARAMETER InputExtension 
    The extension of the input document files.
    The default value is '.md'.
.PARAMETER OutputDir 
    The directory where the output documents are going to be stored.
    The default value is '../html'.
.PARAMETER OutputFormat 
    The format of the output documents.
    The default value is 'html'.
.PARAMETER OutputExtension 
    The extension of the output document files.
    The default value is '.html'.
.PARAMETER OtherOptions 
    The array of other options to be provided to pandoc.
    The default value is @('--standalone').
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
    [string]$inputDir = '../md',
    [string]$inputFormat = 'markdown',
    [string]$inputExtension = '.md',
    [string]$outputDir = '..\html',
    [string]$outputFormat = 'html',
    [string]$outputExtension = '.html',
    [string]$filter = '',
    [string[]]$otherOptions = @('--standalone'),
    [Switch]
    [bool]$rebuild = $false
)

function IsUpToDate([string]$sourceFile, [string]$destFile) {
    if (Test-Path $destFile -PathType Leaf) {
        # destFile exists
        $sourceWriteTime = $(Get-ItemProperty $sourceFile).LastWriteTimeUtc
        $destWriteTime = $(Get-ItemProperty $destFile).LastWriteTimeUtc
        $sourceWriteTime -lt $destWriteTime
    } else {
        # destFile does not exist
        $false
    }
}

function RunOnShell([string]$commandLine) {
    if ([System.IO.Path]::DirectorySeparatorChar -eq '\') {
        # on Windows
        Invoke-Expression "`"$env:ComSpec`" /c `"$commandLine`""
    } else {
        # on other than Windows
        Invoke-Expression "$env:SHELL -c '$commandLine'" 
    }

    # returns the result of execution
    $?
}

function Convert([string]$inputFileRelPath) {
    # preparations
    $outputFileRelPath = [System.IO.Path]::ChangeExtension($inputFileRelPath, $outputExtension)
    $inputFilePath = Join-Path $inputDir $inputFileRelPath
    $outputFilePath = Join-Path $outputDir $outputFileRelPath

    if ((-not $rebuild) -and (IsUpToDate $inputFilePath $outputFilePath)) {
        # no need to build

        # return null to indicate that the output file is not updated
        $null
    } else {
        # build the output 

        # make sure that the output directory exists
        $outputFileDir = Split-Path $outputFilePath -Parent
        if (-not (Test-Path $outputFileDir -PathType Container)) {
            New-Item $outputFileDir -ItemType Directory
        }

        # check existence of the metadata file
        $metadataFilePath = "$inputFilePath.metadata.yaml"
        $metadataOption = ''
        if (Test-Path $metadataFilePath -PathType Leaf) {
            $metadataOption = "--metadata-file=$metadataFilePath"
        }

        # run pandoc
        if ([string]::IsNullOrWhiteSpace($filter)) {
            # with no filter
            pandoc $otherOptions $metadataOption -f $inputFormat -t $outputFormat -o $outputFilePath $inputFilePath 
        } else {
            # with the specified filter
            RunOnShell (
                'pandoc $metadataOption -f $inputFormat -t json $inputFilePath | ' `
                + $filter `
                + ' | pandoc -f json -t $outputFormat -o $outputFilePath'
            )
        }

        # return the path of the output file
        if ($?) {
            $outputFileRelPath
        } else {
            # pandoc failed
            $null
        }
    }
}


# make paths to absolute path
$inputDir = Convert-Path $inputDir
$outputDir = Convert-Path $outputDir

# convert all input files in the input directory
Get-ChildItem $inputDir -File -Name -Recurse -Include "*$inputExtension" `
  | ForEach-Object { Convert $_ }
