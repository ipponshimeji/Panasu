$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = 'Utilities_common.ps1'
. "$here\$sut"

Describe 'DirHaveEqualContentsTo' {
    $pathSeparator = [System.IO.Path]::DirectorySeparatorChar
    $resourcesDir = "$resourcesDirBase/Utilities/DirHaveEqualContentsTo"
    $expectedDir = "$resourcesDir/expected"

    Context 'positive' {
        It 'returns successful result for the dir which has the same contents' {
            $actualDir = "$resourcesDir/same"

            $result = DirHaveEqualContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir

            $result.Succeeded | Should -Be $true
            $result.FailureMessage | Should -Be ''
        }

        It 'returns successful result for the dir whose contents only differ in new line code.' {
            $srcDir = "$resourcesDir/same"
            $tempDir = CreateTempDir
            try {
                # copy a.txt with LF as new line
                $contents = (Get-Content "$srcDir/a.txt" | ForEach-Object {"${_}`n"})
                Set-Content "$tempDir/a.txt" $contents -NoNewline

                # copy sub/b.txt with CRLF as new line
                New-Item "$tempDir/sub" -ItemType Directory -ErrorAction Stop | Out-Null
                $contents = (Get-Content "$srcDir/sub/b.txt" | ForEach-Object {"${_}`r`n"})
                Set-Content "$tempDir/sub/b.txt" $contents -NoNewline

                $result = DirHaveEqualContentsTo -ActualValue $tempDir -ExpectedValue $expectedDir
            } finally {
                Remove-Item $tempDir -Recurse | Out-Null
            }

            $result.Succeeded | Should -Be $true
            $result.FailureMessage | Should -Be ''
        }

        It 'returns failed result for the dir which has a missing file' {
            $actualDir = "$resourcesDir/missing"

            $result = DirHaveEqualContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir

            $result.Succeeded | Should -Be $false
            $result.FailureMessage | Should -Be "An expected file is not found: sub${pathSeparator}b.txt"
        }

        It 'returns failed result for the dir which has an unexpected file' {
            $actualDir = "$resourcesDir/unexpected"

            $result = DirHaveEqualContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir

            $result.Succeeded | Should -Be $false
            $result.FailureMessage | Should -Be 'An unexpected file is found: c.txt'
        }

        It 'returns failed result for the dir which has a file of different contents' {
            $actualDir = "$resourcesDir/different"

            $result = DirHaveEqualContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir

            $result.Succeeded | Should -Be $false
            $result.FailureMessage | Should -Be "The actual file has different contents from expected: sub${pathSeparator}b.txt"
        }

        It 'returns failed result for the dir which has mixed type of difference' {
            $actualDir = "$resourcesDir/mixed"

            $result = DirHaveEqualContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir

            $result.Succeeded | Should -Be $false
            $result.FailureMessage | Should -Be (@(
                'An expected file is not found: a.txt',
                "The actual file has different contents from expected: sub${pathSeparator}b.txt",
                'An unexpected file is found: c.txt'
            ) -join "`n")
        }
    }

    Context 'negative' {
        It 'returns successful result for the dir which has different contents' {
            $actualDir = "$resourcesDir/missing"

            $result = DirHaveEqualContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir -Negate

            $result.Succeeded | Should -Be $true
            $result.FailureMessage | Should -Be ''
        }

        It 'returns failed result for the dir which has the same contents' {
            $actualDir = "$resourcesDir/same"

            $result = DirHaveEqualContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir -Negate

            $result.Succeeded | Should -Be $false
            $result.FailureMessage | Should -Be 'The actual dir has the same contents to the expected dir''s.'
        }
    }
}
