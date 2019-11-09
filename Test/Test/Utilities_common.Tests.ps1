$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = 'Utilities_common.ps1'
. "$here\$sut"

Describe "DirHaveSameContentsTo" {
    $pathSeparator = [System.IO.Path]::DirectorySeparatorChar
    $resourcesDir = "$resourcesBaseDir/Utilities/DirHaveSameContentsTo"
    $expectedDir = "$resourcesDir/expected"

    Context "positive" {
        It "returns successful result for the dir which has the same contents" {
            $actualDir = "$resourcesDir/same"

            $result = DirHaveSameContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir

            $result.Succeeded | Should -Be $true
            $result.FailureMessage | Should -Be ''
        }

        It "returns failed result for the dir which has a missing file" {
            $actualDir = "$resourcesDir/missing"

            $result = DirHaveSameContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir

            $result.Succeeded | Should -Be $false
            $result.FailureMessage | Should -Be "An expected file is not found: sub${pathSeparator}b.txt"
        }

        It "returns failed result for the dir which has an unexpected file" {
            $actualDir = "$resourcesDir/unexpected"

            $result = DirHaveSameContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir

            $result.Succeeded | Should -Be $false
            $result.FailureMessage | Should -Be 'An unexpected file is found: c.txt'
        }

        It "returns failed result for the dir which has a file of different contents" {
            $actualDir = "$resourcesDir/different"

            $result = DirHaveSameContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir

            $result.Succeeded | Should -Be $false
            $result.FailureMessage | Should -Be "The actual file has different contents from expected: sub${pathSeparator}b.txt"
        }

        It "returns failed result for the dir which has mixed type of difference" {
            $actualDir = "$resourcesDir/mixed"

            $result = DirHaveSameContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir

            $result.Succeeded | Should -Be $false
            $result.FailureMessage | Should -Be (@(
                'An expected file is not found: a.txt',
                "The actual file has different contents from expected: sub${pathSeparator}b.txt",
                'An unexpected file is found: c.txt'
            ) -join "`n")
        }
    }

    Context "negative" {
        It "returns successful result for the dir which has different contents" {
            $actualDir = "$resourcesDir/missing"

            $result = DirHaveSameContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir -Negate

            $result.Succeeded | Should -Be $true
            $result.FailureMessage | Should -Be ''
        }

        It "returns failed result for the dir which has the same contents" {
            $actualDir = "$resourcesDir/same"

            $result = DirHaveSameContentsTo -ActualValue $actualDir -ExpectedValue $expectedDir -Negate

            $result.Succeeded | Should -Be $false
            $result.FailureMessage | Should -Be 'The actual dir has the same contents to the expected dir''s.'
        }
    }
}
