# インストール

## 動作環境

「必要ソフトウェア」で説明するpandoc, .NET Core, PowerShellの三つが動く環境なら動きます。
原理的には、Windows, Linux, macOSで動くはず。

作者はもっぱらWindows上で使っていますが、
Ubuntu 18.04上でもやってみたら動きました。


## 必要ソフトウェア

以下のソフトウェアがインストールされている必要があります。

* pandoc
* .NET Core
* PowerShell (PowerShell Core または Windows PowerShell)


### pandoc 2.3以降

[インストール方法のページ](https://pandoc.org/installing.html)

このツールは文書の変換にpandocを使います。
`--metadata-file`オプションを利用するため、
これをサポートしている2.3以降を使用してください。

なお、手元でUbuntu 18.04でふつうに`apt-get install pandoc`してみたところ、
1.19がインストールされました。

### .NET Core 3.1以降

[インストール方法のページ](https://dotnet.microsoft.com/download)

実行するだけならば、.NET Core Runtimeだけで十分です（SDKは不要）。

### PowerShell (PowerShell 6以降、またはWindows PowerShell 5以降)

[インストール方法のページ](https://docs.microsoft.com/powershell/scripting/install/installing-powershell)


## インストール

インストーラーはありません。
[Releases](https://github.com/ipponshimeji/Panasu/releases/)ページからzipファイルをダウンロードして、
適当なフォルダに展開します。

具体的な使い方は、[チュートリアル](Tutorial.ja.md)を参照してください。
