# チュートリアル

## インストール（というか配置）

以下の構成のディレクトリを用意します。
これをプロジェクトと呼びます。

```
- Work
  - _scripts
    - PandocUtil
  - md
  - html
```

1. プロジェクトのルートディレクトリの名前は何でもかまいません。
   ここでは、仮に`Work`とします。
1. 変換したいmarkdownファイル群を`md`ディレクトリの下に格納します。
1. [Releases](https://github.com/ipponshimeji/PandocUtil/releases/)ページからダウンロードしたzipファイルの中身（`PandocUtil`ディレクトリ）を
   `_scripts`ディレクトリの下へコピーします。
1. `_scripts/PandocUtil/format.template.ps1`を`_scripts`ディレクトリへコピーして、
   `format.ps1`にリネームします。
   これがこのプロジェクト用のフォーマットスクリプトになります。
   必要があれば、このスクリプトをカスタマイズしていきます。

ディレクトリ構成はカスタマイズできますが、
このチュートリアルでは基本形としてこの構成で説明をします。

`_scripts/format.ps1`は、
その中身を見てもらえればすぐわかるように、
`_scripts/PandocUtil/format_base.ps1`を呼び出す単純なラッパースクリプトです。
このプロジェクトの必要に応じてデフォルト値を書き換えるためのものです。

デフォルトの構成では、PandocUtilは`_scripts`ディレクトリの下に置かれています。
この配置方法では、各プロジェクトごとにPandocUtilのコピーが置かれることになります。
一方、PandocUtilを別の場所に置いて共有することもできます。
その場合は、`_scripts/format.ps1`の以下の部分をPandocUtilの実際のディレクトリに置き換えてください。
これは、Windowsの自分のユーザープロファイルフォルダ内にPandocUtilを置いた場合の例です。

```powershell
...
param (
    ...
    [string]$pandocUtilPath = "$env:USERPROFILE/PandocUtil"  # PandocUtilの配置場所
)
```


## 各スクリプトのチュートリアル

* [formatスクリプト チュートリアル](Tutorial_format.ja.md)
* combineスクリプト チュートリアル（実装予定）
