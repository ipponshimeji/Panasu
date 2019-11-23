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
  [-RebaseOtherRelativeLinks <Bool>]
  [-OtherExtensionMap <Hashtable>]
  [-OtherReadOptions <String[]>]
  [-OtherWriteOptions <String[]>]
  [-Rebuild <Bool>]
  [-Silent <Bool>]
  [-NoOutput <Bool>]
  [<CommonParameters>]
```


## 説明

**Format-Documents.ps1**スクリプトは、FromDirパラメーターによって指定された入力ディレクトリ下にある
一群のソースドキュメントファイルをフォーマットします。
フォーマットされたドキュメントファイルはToDirパラメータによって指定された出力ディレクトリへ出力されます。

ソースドキュメントファイルのフォーマットは`pandoc`で行います。
このスクリプトを実行するには、`pandoc` 2.3以降がインストールされていなければなりません。
各ファイルのフォーマットは以下のように行われます:

1. `pandoc`がソースドキュメントファイルを読み込み、それをAST (Abstract Syntax Tree)に変換します。
2. Filterパラメーターによって指定されたフィルターがASTを変更します。
3. `pandoc`がフィルタされたASTを読み込み、それを出力ディレクトリに目的の形式で書き込みます。

ソースドキュメントファイルの拡張子、ソースドキュメントのフォーマット名、フォーマットされたドキュメントファイルの拡張子、
およびフォーマットされたドキュメントのフォーマット名の関係はそれぞれFromExtensions, FromFormats, ToExtensions
およびToFormatsによって指定されます。
ここで、「フォーマット名」は`pandoc`の-fオプションや-tオプションに指定することのできる名前です。

このスクリプトとともに提供されるデフォルトのフィルターは入力ASTに変更を加えます。
どのように入力ASTを変更するかは[about_FormatAST](about_FormatAST.ja.md)を参照してください。


## パラメーター

### -Filter

pandocのフィルターとして用いるコマンドライン。

このパラメーターがnullか空文字列の場合は、"dotnet $scriptDir/FormatAST.dll"が用いられます。
ここで、$scriptDirはこのスクリプトが格納されているディレクトリです。

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

配列の項目は、FromExtensionsの項目にそれぞれ対応します。
このパラメーターの配列長はFromExtensionsの配列長と同じでなければなりません。

|||
|:--|:--| 
| Type: | String[] |
| Position: | Named |
| Default value: | @('markdown') |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -MetadataFiles

このフォーマッティングで用いるメタデータを記述したYAMLファイルの配列。
指定されたすべてのメタデータはすべてのソースドキュメントファイルに付加されます。

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

FromExtensionsとToExtensionsの対応以外の拡張子マッピングを指定します。
このスクリプトは相対リンクの拡張子を拡張子マッピングに従って変換します。

'.yaml'='.yaml'のように同じ拡張子のマッピングを指定することで、
相対リンクの拡張子変換を抑止しつつ、
この拡張子のファイルをRebaseOtherRelativeLinksオプションによるリベースの対象から除外することができます。

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

### -RebaseOtherRelativeLinks

このパラメーターがTrueの場合、拡張子マッピングの対象ではないファイルへの相対リンクをリベースして、
それらのリンクが元々の場所のファイルを参照し続けるようにします。

このパラメーターの値がFalseの場合、このスクリプトはフォーマットされたファイルとともに
そのようなファイルを出力ディレクトリへコピーし、それらファイルへのリンクをリベースしません。

|||
|:--|:--| 
| Type: | Bool |
| Position: | Named |
| Default value: | True |
| Accept pipeline input: | False |
| Accept wildcard characters: | False |

### -Rebuild

この値がTrueの場合、すべてのソースドキュメントファイルをフォーマットします。
この値がFalseの場合、更新されたファイルのみをフォーマットします。

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

配列の項目は、FromExtensionsの項目にそれぞれ対応します。
このパラメーターの配列長はFromExtensionsの配列長と同じでなければなりません。

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

配列の項目は、FromExtensionsの項目にそれぞれ対応します。
このパラメーターの配列長はFromExtensionsの配列長と同じでなければなりません。

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

[about_FormatAST](about_FormatAST.ja.md)

