. ../common.ps1

Set-Variable -Name formatDir -Value (Split-Path -Parent $MyInvocation.MyCommand.Path) -Option ReadOnly -Scope Script
Set-Variable -Name inputsDir -Value "$formatDir/inputs" -Option ReadOnly -Scope Script
Set-Variable -Name formatScriptPath -Value "$repositoryDir/Src/Scripts/format_base.ps1" -Option ReadOnly -Scope Script
Set-Variable -Name templatePath -Value "$formatDir/template.html" -Option ReadOnly -Scope Script


function GetFormatFilterCommandLine([string]$config, [string]$runtime) {
    GetFilterCommandLine 'FormatAST' 'FormatAST.dll' $config $runtime
}