param (
    [string]$outputDirBase = '',
    [string]$formatDocumentsScript = '',
    [string]$formatDocumentsFilter = '',
    [string]$config = '',
    [string]$runtime = ''
)

$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# import common utilities
. "$here\Utilities_common.ps1"

# register DirHaveEqualContentsTo assertion to Pester
# It asserts whether the given actual dir have the same contents to the expected's.
# Note that Add-AssertionOperator is supported Pester 4.0.5 or later.
CheckPesterVersion
Add-AssertionOperator -Name DirHaveEqualContentsTo -Test $function:DirHaveEqualContentsTo


## Common settings

# give default values 
if ([string]::IsNullOrEmpty($formatDocumentsScript)) {
    $formatDocumentsScript = "$repositoryDir/Src/Scripts/Format-Documents.ps1"
}
if ([string]::IsNullOrEmpty($formatDocumentsFilter)) {
    $formatDocumentsFilter = GetFilter 'FilterAST' 'FilterAST.dll' $config $runtime
}

# The full path to the Format-Documents.ps1 script to be invoked.
Set-Variable -Name scriptPath -Value $formatDocumentsScript -Option ReadOnly -Scope Script

# The filter command line to be specified Filter option of Format-Documents script.
# It may contain developing module in build output path.
#   ex. 'dotnet E:/Panasu/Src/FilterAST/bin/Debug/net6.0/FilterAST.dll'
Set-Variable -Name filter -Value $formatDocumentsFilter -Option ReadOnly -Scope Script

# The path of the directory where the resources for this test are stored.
Set-Variable -Name resourcesDir -Value "$resourcesDirBase/Format-Documents" -Option ReadOnly -Scope Script

# The path of the directory where the input resources are stored.
Set-Variable -Name inputsDir -Value "$resourcesDir/Inputs" -Option ReadOnly -Scope Script

# The path of the directory where the answer resources are stored.
Set-Variable -Name answersDir -Value "$resourcesDir/Answers" -Option ReadOnly -Scope Script

# The path of the template for HTML format.
# It is used to simplify the output in the most formatting in this test.
Set-Variable -Name commonTemplatePath -Value "$inputsDir/template.html" -Option ReadOnly -Scope Script


## Tests

[string]$tempDir = $null
[string]$outputDir
if ([string]::IsNullOrEmpty($outputDirBase)) {
    $tempDir = CreateTempDir
    $outputDir = $tempDir
} else {
    $outputDir = CreateOutputDir $outputDirBase 'Format-Documents'
}
try {

    Describe 'basic behavior' {
        It '[basic] formats all target files under the specified dir' {
            <#
                * It formats all target files under the FromDir into the
                  specified format using pandoc and the specified filter.
                * Formatted files are output in ToDir.
                * Internal links to a target file in target files are adjusted.
                    * ex. "a.md" -> "a.html" when it is formatting from .md to .html
                    * Link processing is tested in detail in the link tests.
            #>

            # arranage
            $caseName = 'basic'
            $fromDir = "$inputsDir/$caseName"
            $toDir = CreateOutputDir $outputDir $caseName

            # act
            $result = & $scriptPath `
                -FromDir $fromDir `
                -ToDir $toDir `
                -Filter $filter `
                -StickOtherRelativeLinks $true `
                -OtherWriteOptions @('--standalone', "--template=$commonTemplatePath") `
                -Rebuild $true `
                -Silent $true

            # assert
            $result.Formatted | Sort-Object | Should -BeExactly @(
                'a.md', 'index.md', "sub${pathSeparator}b.md"
            )
            $result.Copied | Should -HaveCount 0
            $result.UpToDate | Should -HaveCount 0
            $result.NotTarget | Should -HaveCount 0
            $result.Failed | Should -HaveCount 0
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }
    }

    Describe 'processing links' {
        It '[link_extension] changes extensions of relative links to files of the formatting type' {
            <#
                * It changes extensions of relative links to files of 
                  the formatting type.
                * The mapping of the extension is decided by:
                    * FromExtensions to ToExtensions mapping, and
                    * OtherExtensionMap, which is assumed that the mapping is
                      handled by other formatting session
                * Both hyper links and image links are processed.
                * Absolute links are not processed.
                * It handles url fragment correctly.

                In this test case, extension mapping is as follows:
                * by FromExtensions to ToExtensions mapping:
                    * '.md' to '.html'
                * by OtherExtensionMap
                    * '.text' to '.txt'
                    * '.gif' to '.png'
                According to its basic usage, in this scenario,
                .text files and .gif files are assumed to be converted into
                .txt and .png files respectively by another session.
                But to simplify the test, .txt and .png files are
                stored in the input dir and let them be copied to the output
                dir as non-formatting-target files
                by $false StickOtherRelativeLinks option.
                (StickOtherRelativeLinks behavior is tested in detail later)  
            #>

            # arrange
            $caseName = 'link_extension'
            $fromDir = "$inputsDir/$caseName"
            $toDir = CreateOutputDir $outputDir $caseName

            # act
            $result = & $scriptPath `
                -FromDir $fromDir `
                -ToDir $toDir `
                -Filter $filter `
                -StickOtherRelativeLinks $false `
                -OtherExtensionMap @{'.yaml'='.yaml'; '.text'='.txt'; '.gif'='.png'} `
                -OtherWriteOptions @('--standalone', "--template=$commonTemplatePath") `
                -Rebuild $true `
                -Silent $true

            # assert
            $result.Formatted | Sort-Object | Should -BeExactly @(
                "a${pathSeparator}to_parent.md", "b${pathSeparator}to_sibling.md", 'index.md', 'to_child.md', 'to_external.md', 'to_spouse.md'
            )
            $result.Copied | Sort-Object | Should -BeExactly @(
                "a${pathSeparator}A.png", "a${pathSeparator}A.txt", 'R.png', 'R.txt'
            )
            $result.UpToDate | Should -HaveCount 0
            $result.NotTarget | Sort-Object | Should -BeExactly @(
                "a${pathSeparator}A.gif", "a${pathSeparator}A.text", 'R.gif', 'R.text'
            )
            $result.Failed | Should -HaveCount 0
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }

        It '[link_nostick] copies non-formatting-target files if StickOtherRelativeLinks is false' {
            <#
                If StickOtherRelativeLinks is $false,
                it copies non-formatting-target files to ToDir.
                As a result, the relative links to such files are
                still valid after formatting.
            #>

            # arrange
            $caseName = 'link_nostick'
            $fromDir = "$inputsDir/$caseName"
            $toDir = CreateOutputDir $outputDir $caseName

            # act
            $result = & $scriptPath `
                -FromDir $fromDir `
                -FromExtensions @('.md', '.markdown') `
                -FromFormats @('markdown', 'markdown') `
                -ToDir $toDir `
                -ToExtensions @('.html', '.htm') `
                -ToFormats @('html', 'html') `
                -Filter $filter `
                -StickOtherRelativeLinks $false `
                -OtherExtensionMap @{'.yaml'='.yaml'} `
                -OtherWriteOptions @('--standalone', "--template=$commonTemplatePath") `
                -Rebuild $true `
                -Silent $true

            # assert
            $result.Formatted | Sort-Object | Should -BeExactly @(
                "a${pathSeparator}A.markdown", "a${pathSeparator}to_parent.md", "b${pathSeparator}B.markdown", "b${pathSeparator}to_sibling.md", 'index.md', 'R.markdown', 'to_child.md', 'to_spouse.md'
            )
            $result.Copied | Sort-Object | Should -BeExactly @(
                "a${pathSeparator}A.png", "a${pathSeparator}A.txt", "b${pathSeparator}B.png", "b${pathSeparator}B.txt", 'R.png', 'R.txt'
            )
            $result.UpToDate | Should -HaveCount 0
            $result.NotTarget | Sort-Object | Should -BeExactly @(
                "a${pathSeparator}A.yaml", "b${pathSeparator}B.yaml", 'R.yaml'
            )
            $result.Failed | Should -HaveCount 0
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }

        It '[link_stick] changes links to non-formatting-target files if StickOtherRelativeLinks is true' {
            <#
                If StickOtherRelativeLinks is $true,
                it does not copy non-formatting-target files to ToDir,
                but changes links so that they point the original files in FromDir.
            #>

            # arrange
            $caseName = 'link_stick'
            $toDir = CreateOutputDir $outputDir $caseName

            # copy input dir
            # to observe rebase behavior, copy input dir beside the output dir
            $fromDir = CreateOutputDir $outputDir "$caseName.from"
            Copy-Item "$inputsDir/$caseName/*" $fromDir -Recurse

            # act
            $result = & $scriptPath `
                -FromDir $fromDir `
                -FromExtensions @('.md', '.markdown') `
                -FromFormats @('markdown', 'markdown') `
                -ToDir $toDir `
                -ToExtensions @('.html', '.htm') `
                -ToFormats @('html', 'html') `
                -Filter $filter `
                -StickOtherRelativeLinks $true `
                -OtherExtensionMap @{'.yaml'='.yaml'} `
                -OtherWriteOptions @('--standalone', "--template=$commonTemplatePath") `
                -Rebuild $true `
                -Silent $true

            # assert
            $result.Formatted | Sort-Object | Should -BeExactly @(
                "a${pathSeparator}A.markdown", "a${pathSeparator}to_parent.md", "b${pathSeparator}B.markdown", "b${pathSeparator}to_sibling.md", 'index.md', 'R.markdown', 'to_child.md', 'to_spouse.md'
            )
            $result.Copied | Should -HaveCount 0
            $result.UpToDate | Should -HaveCount 0
            $result.NotTarget | Sort-Object | Should -BeExactly @(
                "a${pathSeparator}A.png", "a${pathSeparator}A.txt", "a${pathSeparator}A.yaml", "b${pathSeparator}B.png", "b${pathSeparator}B.txt", "b${pathSeparator}B.yaml", 'R.png', 'R.txt', 'R.yaml'
            )
            $result.Failed | Should -HaveCount 0
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }
    }

    Describe 'processing title' {
        It '[title] decides the title of a document from its contents' {
            <#
                * The title of the document is decided by following steps:
                    * If it has 'title' metadata, it is selected. Otherwise,
                    * If it has level-1 header, the first one is selected. Otherwise,
                    * No title is selected.
                      It causes an formatting error or warning in some target type
                      such as standalone 'html' target.
                * All level-1 headers other than the title are removed.
            #>
 
            # arrange
            $caseName = 'title'
            $fromDir = "$inputsDir/$caseName"
            $toDir = CreateOutputDir $outputDir $caseName

            # act
            Write-Host '!! A warning which complains about no title is expected below.'
            $result = & $scriptPath `
                -FromDir $fromDir `
                -ToDir $toDir `
                -Filter $filter `
                -StickOtherRelativeLinks $true `
                -OtherWriteOptions @('--standalone', "--template=$commonTemplatePath") `
                -Rebuild $true `
                -Silent $true

            # assert
            $result.Formatted | Sort-Object | Should -BeExactly @(
                'attached_metadata.md', 'embedded_metadata.md', 'h1.md', 'index.md', 'none.md'
            )
            $result.Copied | Should -HaveCount 0
            $result.UpToDate | Should -HaveCount 0
            $result.NotTarget | Should -BeExactly @('attached_metadata.md.metadata.yaml')
            $result.Failed | Should -HaveCount 0
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }
    }

    Describe 'processing macros' {
        It '[macro_norebase] processes macros written in metadata in source files' {
            <#
                It processes macros in metadata.
                Currently following macros are supported:
                * rebase
                * condition
                    * from-file

                In this test, macros are written in external metadata file (macro.metadata.yaml)
                and it is specified in MetadataFiles option.
            #>

            # arrange
            $caseName = 'macro_norebase'
            $fromDir = "$inputsDir/$caseName"
            $toDir = CreateOutputDir $outputDir $caseName

            # act
            $result = & $scriptPath `
                -FromDir $fromDir `
                -ToDir $toDir `
                -MetadataFiles @("$inputsDir/macro.metadata.yaml") `
                -Filter $filter `
                -StickOtherRelativeLinks $false `
                -OtherWriteOptions @('--standalone', "--template=$commonTemplatePath") `
                -Rebuild $true `
                -Silent $true

            # assert
            $result.Formatted | Sort-Object | Should -BeExactly @(
                'index.md', 'root.md', "sub${pathSeparator}sub.md"
            )
            $result.Copied | Sort-Object | Should -BeExactly @("styles${pathSeparator}test.css")
            $result.UpToDate | Should -HaveCount 0
            $result.NotTarget | Should -HaveCount 0
            $result.Failed | Should -HaveCount 0
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }
    }

    Describe '-OtherReadOptions and -OtherWriteOptions parameters' {
        It '[otheroptions] passes options specified by OtherReadOptions and OtherWriteOptions to pandoc' {
            <#
                Format-Documents.ps1 formats each source in the following step:
                1. invoke pandoc to read the source and convert it into AST
                2. invoke filter to modify the AST
                3. invoke pandoc to convert the AST to target format and write it
                base on the above,
                * The options specified in the OtherReadOptions option are passed to pandoc when it is invoked to read source.
                * The options specified in the OtherWriteOptions option are passed to pandoc when it is invoked to write target.

                In this test case,
                * a metadata "testparam" is defined in OtherReadOptions, and 
                * a template which format the "testparam" metadata at the last of body is given in OtherWriteOptions
                So it formats the value of the "testparam" metadata at the last of body.
            #>

            # arrange
            $caseName = 'otheroptions'
            $fromDir = "$inputsDir/$caseName"
            $toDir = CreateOutputDir $outputDir $caseName

            # act
            $result = & $scriptPath `
                -FromDir $fromDir `
                -ToDir $toDir `
                -Filter $filter `
                -StickOtherRelativeLinks $true `
                -OtherReadOptions @('--metadata=testparam:Hello') `
                -OtherWriteOptions @('--standalone', "--template=$inputsDir/$caseName.template.html") `
                -Rebuild $true `
                -Silent $true

            # assert
            $result.Formatted | Sort-Object | Should -BeExactly @('index.md')
            $result.Copied | Should -HaveCount 0
            $result.UpToDate | Should -HaveCount 0
            $result.NotTarget | Should -HaveCount 0
            $result.Failed | Should -HaveCount 0
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }
    }

    Describe '-Rebuild parameter' {
        <#
            Format-Documents.ps1 updates all target files if the Rebuild parameter is true.
            Otherwise it updates only out-of-date targets. 
        #>

        It '[rebuild_false_source] updates a target if one of its source file is updated, when Rebuild option is false' {
            <#
                When Rebuild option is false,
                * It formats a source if one of following file is newer than its target:
                    * source file
                    * attached metadata file of the source
                * It copies a non-formatting-target file if StickOtherRelativeLinks option is false
                  and it is newer than its target.
            #>

            ## arrange
            $caseName = 'rebuild_false_source'

            # time stamps
            # The 2 seconds difference is decided because the timestamp resolution of old FAT is 2 seconds.
            $now = [DateTime]::UtcNow
            $fromTimeStamp = $now.AddSeconds(-4) 
            $toTimeStamp = $now.AddSeconds(-2) 
            
            # setup from dir
            $fromDir = CreateOutputDir $outputDir "$caseName.from"
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
            
            # setup to dir
            $toDir = CreateOutputDir $outputDir $caseName
            Copy-Item "$inputsDir/$caseName.to/*" $toDir -Recurse
            # set time stamps of to files to 2 seconds ago
            Get-ChildItem $toDir -File -Recurse | ForEach-Object { $_.LastWriteTimeUtc = $toTimeStamp }
            
            ## act
            $result = & $scriptPath `
                -FromDir $fromDir `
                -ToDir $toDir `
                -Filter $filter `
                -StickOtherRelativeLinks $false `
                -OtherWriteOptions @('--standalone', "--template=$commonTemplatePath") `
                -Rebuild $false `
                -Silent $true

            ## assert
            $result.Formatted | Sort-Object | Should -BeExactly @(
                'absent_solo.md', 'index.md', 'shouldbeupdated_solo.md', 'shouldbeupdated_withmetadata.md'
            )
            $result.Copied | Sort-Object | Should -BeExactly @(
                'absent_copy.txt', 'shouldbeupdated_copy.txt' 
            )
            $result.UpToDate | Sort-Object | Should -BeExactly @(
                'shouldnotbeupdated_copy.txt', 'shouldnotbeupdated_solo.md', 'shouldnotbeupdated_withmetadata.md'
            )
            $result.NotTarget | Sort-Object | Should -BeExactly @(
                'shouldbeupdated_withmetadata.md.metadata.yaml', 'shouldnotbeupdated_withmetadata.md.metadata.yaml'
            )
            $result.Failed | Should -HaveCount 0
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }

        It '[rebuild_false_commonmetadata] formats all sources if one of common metadata files is updated, when Rebuild option is false' {
            <#
                When Rebuild option is false,
                * It formats all sources if one of common metadata files is newer than its target.
                  The common metadata files are specified in MetadataFiles option.
                Note that common metadata file has no effect on copying non-formatting-target files.
            #>

            ## arrange
            $caseName = 'rebuild_false_commonmetadata'

            # time stamps
            # The 2 seconds difference is decided because the timestamp resolution of old FAT is 2 seconds.
            $now = [DateTime]::UtcNow
            $fromTimeStamp = $now.AddSeconds(-4) 
            $toTimeStamp = $now.AddSeconds(-2) 

            # setup from dir
            $fromDir = CreateOutputDir $outputDir "$caseName.from"
            Copy-Item "$inputsDir/$caseName/*" $fromDir -Recurse
            # set time stamps of from files to 4 seconds ago
            Get-ChildItem $fromDir -File -Recurse | ForEach-Object { $_.LastWriteTimeUtc = $fromTimeStamp }
            # change time stamps of 'should be updated' trigger files to now
            @(
                'index.md';
                'commonmetadata2.yaml'
            ) | ForEach-Object { (Get-Item "$fromDir/$_").LastWriteTimeUtc = $now }

            # setup to dir
            $toDir = CreateOutputDir $outputDir $caseName
            Copy-Item "$inputsDir/$caseName.to/*" $toDir -Recurse
            # set time stamps of to files to 2 seconds ago
            Get-ChildItem $toDir -File -Recurse | ForEach-Object { $_.LastWriteTimeUtc = $toTimeStamp }

            ## act
            $result = & $scriptPath `
                -FromDir $fromDir `
                -ToDir $toDir `
                -Filter $filter `
                -MetadataFiles @("$fromDir/commonmetadata1.yaml"; "$fromDir/commonmetadata2.yaml") `
                -StickOtherRelativeLinks $false `
                -OtherWriteOptions @('--standalone', "--template=$commonTemplatePath") `
                -Rebuild $false `
                -Silent $true

            ## assert
            $result.Formatted | Sort-Object | Should -BeExactly @(
                'index.md', 'shouldbeupdated_solo.md'
            )
            $result.Copied | Should -HaveCount 0
            $result.UpToDate | Sort-Object | Should -BeExactly @(
                'shouldnotbeupdated_copy.txt'
            )
            $result.NotTarget | Sort-Object | Should -BeExactly @(
                'commonmetadata1.yaml', 'commonmetadata2.yaml'
            )
            $result.Failed | Should -HaveCount 0
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }

        It '[rebuild_true] updates all targets even if they are up-to-date, when Rebuild option is true' {
            <#
                When Rebuild option is true,
                all targets are updated even if they are up-to-date.
            #>

            ## arrange
            $caseName = 'rebuild_true'

            # time stamps
            # The 2 seconds difference is decided because the timestamp resolution of old FAT is 2 seconds.
            $now = [DateTime]::UtcNow
            $fromTimeStamp = $now.AddSeconds(-4) 
            $toTimeStamp = $now.AddSeconds(-2) 

            # setup from dir
            $fromDir = CreateOutputDir $outputDir "$caseName.from"
            Copy-Item "$inputsDir/$caseName/*" $fromDir -Recurse
            # set time stamps of from files to 4 seconds ago
            Get-ChildItem $fromDir -File -Recurse | ForEach-Object { $_.LastWriteTimeUtc = $fromTimeStamp }

            # setup to dir
            $toDir = CreateOutputDir $outputDir $caseName
            Copy-Item "$inputsDir/$caseName.to/*" $toDir -Recurse
            # set time stamps of to files to 2 seconds ago
            Get-ChildItem $toDir -File -Recurse | ForEach-Object { $_.LastWriteTimeUtc = $toTimeStamp }

            ## act
            $result = & $scriptPath `
                -FromDir $fromDir `
                -ToDir $toDir `
                -Filter $filter `
                -StickOtherRelativeLinks $false `
                -OtherWriteOptions @('--standalone', "--template=$commonTemplatePath") `
                -Rebuild $true `
                -Silent $true

            ## assert
            $result.Formatted | Sort-Object | Should -BeExactly @(
                'index.md', 'shouldbeupdated_solo.md'
            )
            $result.Copied | Sort-Object | Should -BeExactly @(
                'shouldbeupdated_copy.txt' 
            )
            $result.UpToDate | Should -HaveCount 0
            $result.NotTarget | Should -HaveCount 0
            $result.Failed | Should -HaveCount 0
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }
    }

    Describe 'handling formatting failure' {
        It '[failure] reports the list of files which it fails to format' {
            # arrange
            $caseName = 'failure'
            $fromDir = "$inputsDir/$caseName"
            $toDir = CreateOutputDir $outputDir $caseName

            # act
            Write-Host '!! Some errors are expected below. Now testing the case to fail in formatting.'
            $result = & $scriptPath `
                -FromDir $fromDir `
                -FromExtensions @('.md', '.json') `
                -FromFormats @('markdown', 'json') `
                -ToDir $toDir `
                -ToExtensions @('.html', '.txt') `
                -ToFormats @('html', 'plain') `
                -Filter $filter `
                -StickOtherRelativeLinks $false `
                -OtherWriteOptions @('--standalone', "--template=$commonTemplatePath") `
                -Rebuild $true `
                -Silent $true

            # assert
            $result.Formatted | Sort-Object | Should -BeExactly @('index.md')
            $result.Copied | Should -HaveCount 0
            $result.UpToDate | Should -HaveCount 0
            $result.NotTarget | Should -HaveCount 0
            $result.Failed | Sort-Object | Should -BeExactly @('invalid.json')
            $toDir | Should -DirHaveEqualContentsTo "$answersDir/$caseName"
        }
    }
} finally {
    if (-not [string]::IsNullOrEmpty($tempDir)) {
        Remove-Item $tempDir -Recurse | Out-Null
    }
}
