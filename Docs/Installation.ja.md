# インストール

## 動作環境

「必要ソフトウェア」で説明するpandoc, .NET Core, PowerShellの三つが動く環境なら動くはずです。
原理的には、Windows, Linux, macOSで動くはず。

作者はもっぱらWindows上で使っていますが、
Ubuntu 18.04上でもやってみたら動きました。


## 必要ソフトウェア

以下のソフトウェアがインストールされている必要があります。

* pandoc
* .NET Core
* PowerShell (PowerShell Core または Windows PowerShell)


### pandoc

[インストール方法のページ](https://pandoc.org/installing.html)

このツールは文書の変換にpandocを使います。
`--metadata-file`オプションが使えると便利なので、
これをサポートしている2.3以降がお薦めです。

なお、手元でUbuntu 18.04でふつうに`apt-get install pandoc`してみたところ、
1.19がインストールされました。
ちょっと古いし、HTMLに変換した場合に特殊文字の扱いがおかしいところがある。


### .NET Core

[インストール方法のページ](https://dotnet.microsoft.com/download)

実行するだけならば、.NET Core Runtimeだけで十分です（SDKは不要）。

### PowerShell

[インストール方法のページ](https://docs.microsoft.com/powershell/scripting/install/installing-powershell)


## インストール

インストーラーはありません。
[リリースノート](../Releases/README.ja.md)ページからzipファイルをダウンロードして、
適当なフォルダに展開します。

具体的な使い方は、[チュートリアル](Tutorial.ja.md)を参照してください。
