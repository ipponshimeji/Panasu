# Format-Documents.ps1

指定したディレクトリ下の一群のドキュメントをpandocを用いてフォーマット変換します。

* [構文](#構文)
* [説明](#説明)
* [パラメーター](#パラメーター)
* [入力](#入力)
* [出力](#出力)
* [関連項目](#関連項目)


## 構文

```
Format-Documents.ps1
  [-FromDir <String>]
  [-FromExtensions <String[]>]
  [-FromFormats <String[]>]
  [-ToDir <String>]
  [-ToExtensions <String[]>]
  [-ToFormats <String[]>]
  [-MetadataFiles <String[]>]
  [-Filter <String>]
  [-StickOtherRelativeLinks <Bool>]
  [-OtherExtensionMap <Hashtable>]
  [-OtherReadOptions <String[]>]
  [-OtherWriteOptions <String[]>]
  [-Rebuild <Bool>]
  [-Silent <Bool>]
  [-NoOutput <Bool>]
  [<CommonParameters>]
```


## 説明

**Format-Documents.ps1**スクリプトは、`FromDir`パラメーターによって指定された入力ディレクトリ下にある
一群のソースドキュメントファイルをフォーマットします。
フォーマットされたドキュメントファイルは`ToDir`パラメータによって指定された出力ディレクトリへ出力されます。

#### フォーマットの過程

このスクリプトはソースドキュメントファイルのフォーマットに`pandoc`を用います。
このスクリプトを実行するためには、`pandoc` 2.3以降がインストールされていなければなりません。

各ファイルのフォーマットは以下のように行われます:

1. `pandoc`がソースドキュメントファイルを読み込み、それをAST (Abstract Syntax Tree)に変換します。
2. `Filter`パラメーターによって指定されたフィルターがASTを変更します。
3. `pandoc`がフィルタされたASTを読み込み、それを出力ディレクトリに目的の形式で書き込みます。

ソースドキュメントファイルの拡張子、ソースドキュメントのフォーマット名、フォーマットされたドキュメントファイルの拡張子、
およびフォーマットされたドキュメントのフォーマット名の関係はそれぞれ`FromExtensions`, `FromFormats`, `ToExtensions`
および`ToFormats`によって指定されます。
ここで、「フォーマット名」は`pandoc`の-fオプションや-tオプションに指定することのできる名前です。

デフォルトのフィルターである`FilterAST`がこのスクリプトとともに提供されています。
`FilterAST`がどのようにASTを変更するかは[about_FilterAST](about_FilterAST.ja.md)を参照してください。

`FromExtensions`パラメーターに含まれる拡張子をもつファイルのみがフォーマットの対象になります。
このスクリプトは他のファイルを処理しませんが、
`StickOtherRelativeLinks`パラメーターがfalseの場合はいくつかのファイルが`ToDir`へコピーされることがあります。
詳細は[StickOtherRelativeLinksパラメーターの説明](#-StickOtherRelativeLinks)を参照してください。

#### 拡張子の対応

デフォルトのフィルターは入力ASTに含まれる相対リンクの拡張子を置き換えます。
例えば、入力AST中の"a.md"へのリンクは"a.html"へのリンクへ置き換えられます。
この置き換えで用いられる拡張子の対応は以下から構成されます。

* `FromExtensions`パラメーターで指定された拡張子から`ToExtensions`パラメーターで指定された拡張子への対応
* `OtherExtensionMap`パラメーターで指定された対応

この対応の対象外である拡張子は置き換えられません。

#### メタデータ

`pandoc`がドキュメントを変換する際に、
ソースドキュメントともに、
変換における「パラメーター」としてメタデータを入力することができます。

このスクリプトでは、いくつかの方法でメタデータを指定することができます。

* `pandoc`では、markdownなどいくつかの文書形式において、
  ソースドキュメントにメタデータを埋め込むことができます。
  詳細は[Pandoc User's Guide](https://pandoc.org/MANUAL.html)の[Metadata blocks](https://pandoc.org/MANUAL.html#metadata-blocks)を参照してください。
  ただし、この記法を用いるとソースドキュメントのmarkdown形式としての互換性が低下する可能性があります。
* 同じディレクトリに"&lt;ソースドキュメントファイル名&gt;.metadata.yaml"という名前のファイルが存在すれば、
  このスクリプトはそのファイルをソースドキュメントのメタデータとして扱います。
  例えば、"a.md.metadata.yaml"という名前のファイルが存在すれば、このスクリプトはそれを"a.md"のメタデータとして扱います。
  メタデータファイルはYAML形式でなければなりません。
* 一連のフォーマットで共通に用いるメタデータファイルを`MetadataFiles`パラメーターに指定することができます。
  メタデータファイルはYAML形式でなければなりません。

このスクリプトはメタデータファイルを結合し、その中にフィルターへのパラメーターを埋め込み、それを`pandoc`に渡します。

デフォルトのフィルターを含むいくつかのフィルターはメタデータマクロをサポートしています。
詳細は[about_MetadataMacro](about_MetadataMacro.ja.md)を参照してください。


## パラメーター

### -Filter

フォーマット過程の中で用いるpandocフィルターのコマンドライン。

このパラメーターがnullか空文字列の場合は、"dotnet $scriptDir/FilterAST.dll"が用いられます。
ここで、$scriptDirはこのスクリプトが格納されているディレクトリです。
FilterASTの詳細については、[about_FilterAST](about_FilterAST.ja.md)を参照してください。

このスクリプトは、フィルターへのパラメーターをソースドキュメントのメタデータに埋め込みます。
ソースドキュメントとメタデータはASTに変換され、指定されたフィルタがそれを読み込みます。
その結果、フィルタは入力ASTのメタデータからパラメーターを参照することができます。

メタデータに埋め込まれるフィルターへのパラメーターは、
デフォルトのフィルターである`FilterAST`に必要とされるパラメーターです。
カスタムフィルターを指定する場合、
そのフィルターは入力ASTを通してこれらのパラメーターを利用することができます。
パラメーターの詳細については、[about_FilterAST](about_FilterAST.ja.md)を参照してください。

|||
|:--|:--| 
| Type: | String |
| Position: | Named |
| Default value: | '' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -FromDir

フォーマットされるソースドキュメントファイルが格納されているディレクトリ。

|||
|:--|:--| 
| 型: | String |
| 位置: | Named |
| 規定値: | '../md' |
| パイプライン入力を許可する: | False |
| ワイルドカード文字を許可する: | False |


### -FromExtensions

ソースドキュメントファイルの拡張子の配列。

|||
|:--|:--| 
| 型: | String[] |
| 位置: | Named |
| 規定値: | @('.md') |
| パイプライン入力を許可する: | False |
| ワイルドカード文字を許可する: | False |

### -FromFormats

ソースドキュメントファイルの形式の配列。

配列の項目はpandocの-fオプションに指定できる値でなければなりません。

配列の項目は、`FromExtensions`の項目にそれぞれ対応します。
このパラメーターの配列長は`FromExtensions`の配列長と同じでなければなりません。

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @('markdown') |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -MetadataFiles

一連のフォーマットで共通に用いるメタデータを記述したYAMLファイルの配列。
指定されたすべてのメタデータはすべてのソースドキュメントファイルに適用されます。

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @() |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -NoOutput

この値がTrueの場合、このスクリプトは出力として何も返しません。
この値がFalseの場合、このスクリプトは処理したファイルのリストを格納したオブジェクトを返します。

|||
|:--|:--| 
| Type: | Bool |
| Position: | Named |
| Default value: | False |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -OtherExtensionMap

`FromExtensions`から`ToExtensions`への対応以外の拡張子マッピングを指定します。
フィルターは相対リンクの拡張子を拡張子マッピングに従って変換します。

'.yaml'='.yaml'のように同じ拡張子のマッピングを指定することで、
相対リンクの拡張子置換を抑止しつつ、
この拡張子のファイルを`StickOtherRelativeLinks`オプションによるリンク変更やコピーの対象から除外することができます。

|||
|:--|:--| 
| Type: | Hashtable |
| Position: | Named |
| Default value: | @{'.yaml'='.yaml'} |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -OtherReadOptions

ソースドキュメントファイルを読み込むためにpandocが実行される際に
pandocに渡すその他のオプションの配列です。

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @() |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -OtherWriteOptions

フォーマットされたドキュメントファイルを書き込むためにpandocが実行される際に
pandocに渡すその他のオプションの配列です。

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @('--standalone') |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -StickOtherRelativeLinks

このパラメーターがTrueの場合、拡張子マッピングの対象ではないファイルへの相対リンクを変更して、
それらのリンクが元々の場所のファイルを参照し続けるようにします。

このパラメーターの値がFalseの場合、このスクリプトはフォーマットされたファイルとともに
そのようなファイルを出力ディレクトリへコピーし、それらファイルへのリンクを変更しません。

|||
|:--|:--| 
| Type: | Bool |
| Position: | Named |
| Default value: | True |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -Rebuild

この値がTrueの場合、すべてのファイルを処理します。
この値がFalseの場合、更新されたファイルのみを処理します。

|||
|:--|:--| 
| Type: | Bool |
| Position: | Named |
| Default value: | False |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -Silent

この値がTrueの場合、進行状況は表示されません。
この値がFalseの場合、処理されたファイル名が進行状況として表示されます。

|||
|:--|:--| 
| Type: | Bool |
| Position: | Named |
| Default value: | False |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -ToDir

フォーマットされたドキュメントファイルが格納されるディレクトリ。

|||
|:--|:--| 
| Type: | String |
| Position: | Named |
| Default value: | '../html' |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -ToExtensions

フォーマットされたドキュメントファイルの拡張子の配列。

配列の項目は、`FromExtensions`の項目にそれぞれ対応します。
このパラメーターの配列長は`FromExtensions`の配列長と同じでなければなりません。

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @('.html') |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -ToFormats

フォーマットされたドキュメントファイルの形式の配列。

配列の項目はpandocの-tオプションに指定できる値でなければなりません。

配列の項目は、`FromExtensions`の項目にそれぞれ対応します。
このパラメーターの配列長は`FromExtensions`の配列長と同じでなければなりません。

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @('html') |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |


## 入力

### なし

このスクリプトはパイプから入力を受け取りません。


## 出力

### None または PSCustomObject

NoOutputパラメーターがTrueの場合、このスクリプトは何も出力しません。
それ以外の場合、以下のプロパティをもつオブジェクトを出力します。

* `Copied`: このスクリプトが出力フォルダへコピーしたファイルのリスト。
* `Failed`: このスクリプトがフォーマットに失敗したファイルのリスト。
* `Formatted`: このスクリプトがフォーマットに成功したファイルのリスト。
* `NotTarget`: このスクリプトが処理しなかったファイルのリスト。
* `UpToDate`: 結果が最新であるため、このスクリプトが処理をスキップしたファイルのリスト.

ファイルは入力ディレクトリからの相対パスで表現されます。


## 関連項目

[about_FilterAST](about_FilterAST.ja.md)
[about_MetadataMacros](about_MetadataMacros.ja.md)
