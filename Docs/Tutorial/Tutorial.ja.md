# チュートリアル

## インストール（というか配置）

以下の構成のディレクトリを用意します。
これをプロジェクトと呼びます。

```
- Work
  - _scripts
    - Panasu
  - md
  - html
```

1. プロジェクトのルートディレクトリの名前は何でもかまいません。
   ここでは、仮に`Work`とします。
1. 変換したいmarkdownファイル群を`md`ディレクトリの下に格納します。
1. [Releases](https://github.com/ipponshimeji/Panasu/releases/)ページからダウンロードしたzipファイルの中身（`Panasu`ディレクトリ）を
   `_scripts`ディレクトリの下へコピーします。
1. `_scripts/Panasu/format.template.ps1`を`_scripts`ディレクトリへコピーして、
   `format.ps1`にリネームします。
   これがこのプロジェクト用のフォーマットスクリプトになります。
   必要があれば、このスクリプトをカスタマイズしていきます。

ディレクトリ構成はカスタマイズできますが、
このチュートリアルでは基本形としてこの構成で説明をします。

`_scripts/format.ps1`は、
その中身を見てもらえればすぐわかるように、
`_scripts/Panasu/Format-Documents.ps1`を呼び出す単純なラッパースクリプトです。
このプロジェクトの必要に応じてデフォルト値を書き換えるためのものです。

デフォルトの構成では、Panasuは`_scripts`ディレクトリの下に置かれています。
この配置方法では、各プロジェクトごとにPanasuのコピーが置かれることになります。
一方、Panasuを別の場所に置いて共有することもできます。
その場合は、`_scripts/format.ps1`の以下の部分をPanasuの実際のディレクトリに置き換えてください。
これは、Windowsの自分のユーザープロファイルフォルダ内にPanasuを置いた場合の例です。

```powershell
...
param (
    ...
    [string]$PanasuPath = "$env:USERPROFILE/Panasu"  # Panasuの配置場所
)
```


## 各スクリプトのチュートリアル

* [formatスクリプト チュートリアル](Tutorial_format.ja.md)
* combineスクリプト チュートリアル（実装予定）
