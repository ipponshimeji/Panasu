# FilterASTについて

* [簡易説明](#簡易説明)
* [詳細説明](#詳細説明)
* [コマンドライン構文](#コマンドライン構文)
* [パラメーター](#パラメーター)
* [ASTへ加える変更](#ASTへ加える変更)
* [関連項目](#関連項目)


## 簡易説明

`FilterAST`の機能について説明します。
これは、[Format-Documents.ps1](Format-Documents.ja.md)スクリプトのデフォルトフィルターです。


## 詳細説明

`FilterAST`は[pandocフィルター](https://pandoc.org/filters.html)の一種です。
[Format-Documents.ps1](Format-Documents.ja.md)は、ソースドキュメントファイル群をフォーマットする際に、
`FilterAST`をデフォルトのフィルターとして用います。

実際には`FilterAST`は.NET Coreアプリケーションです。
`FilterAST`はpandocのASTを標準入力から読み込み、変更したASTを標準出力に出力します。
どのようにASTを変更するかについては、[ASTへの変更](#ASTへの変更)を参照してください。


## コマンドライン構文

`FilterAST`を実行するためのコマンドライン構文は以下の通りです。

```
dotnet FilterAST.dll [options]
```

通常、このフィルタへのパラメーターは入力ASTのメタデータ内に埋め込まれます。
パラメーターの詳細は[パラメーター](#パラメーター)を参照してください。

しかし、コマンドラインオプションによって、パラメーターを設定したり上書きしたりすることもできます。
コマンドラインオプションとパラメーターの対応は以下の通りです。

#### --ExtensionMap <mapping> または -m <mapping>

`ExtensionMap`パラメーターに対応します。
&lt;mapping&gt;の形式は、`"<from extension>:<to extension>"`でなければなりません（例. `".md:.html"`）。
このオプションは複数のマッピングを設定するために複数回指定することができます。

#### --FromBaseDirPath <path> または -fd <path>

`FromBaseDirPath`パラメーターに対応します。

#### --FromFileRelPath <path> または -ff <relative path>

`FromFileRelPath`パラメーターに対応します。

#### --RebaseOtherRelativeLinks または -r

`RebaseOtherRelativeLinks`パラメーターに対応します。
もしこのオプションが指定されると、
`RebaseOtherRelativeLinks`パラメータに`true`が設定されます。

#### --ToBaseDirPath <path> または -td <path>

`ToBaseDirPath`パラメーターに対応します。

#### --ToFileRelPath <path> または -tf <relative path>

`ToFileRelPath`パラメーターに対応します。


## パラメーター

パラメーターは入力ASTのメタデータの中に埋め込まれます。
メタデータ中でパラメーターに対応するキー名は、パラメーター名に`_Param.`をプレフィクスしたものです。
例えば、`ToFileRelPath`パラメーターに対するキー名は`_Param.ToFileRelPath`になります。

#### ExtensionMap: mapping

置き換える拡張子の対応です。
このパラメーターは省略可能です。

フィルター作業において、
フィルターは入力AST中の相対リンクの拡張子がこの拡張子対応の対象である場合、その拡張子を対応するものに置き換えます。
拡張子の置き換えの詳細については、[拡張子の置き換え](#拡張子の置き換え)を参照してください。

例:

```yaml
_Param.ExtensionMap:
    .md: ".html"
    .markdown: ".html"
```

#### FromBaseDirPath: string

ソースドキュメントファイルが格納されている基底ディレクトリのパスです。
このパラメーターは省略できません。

例:

```yaml
_Param.FromBaseDirPath: "c:/docs/md"
```

#### FromFileRelPath: string

フォーマットされているソースドキュメントファイルのパスです。
このパスは`FromBaseDirPath`パラメーターのパスからの相対パスでなければなりません。
このパラメーターは省略できません。

このファイルはこのフィルタへの直接の入力元ではなく、
フォーマット作業におけるオリジナルのソースであるかもしれないことに留意してください。

例:

```yaml
_Param.FromFileRelPath: "sub/a.md"
```

#### RebaseOtherRelativeLinks: bool

このパラメーターは省略可能です。省略値は`true`になります。

このパラメーターが`true`の場合、
拡張子が拡張子対応の対象でない相対リンクは、
オリジナルのファイルを参照しつづけるように変更されます。

個のパラメーターが`false`の場合、そのようなリンクは変更されません。
それらは`ToBaseDirPath`のディレクトリ下のファイルを参照することになります。

詳細については、[拡張子対応の対象外である相対リンクの変更](#拡張子対応の対象外である相対リンクの変更)を参照してください。

例:

```yaml
_Param.RebaseOtherRelativeLinks: false
```

#### ToBaseDirPath: string

フォーマットされたドキュメントファイルが格納される基底ディレクトリのパスです。
このパラメーターは省略できません。

例:

```yaml
_Param.ToBaseDirPath: "c:/docs/html"
```

#### ToFileRelPath: string

フォーマットされたドキュメントファイルのパスです。
このパスは`ToBaseDirPath`パラメーターのパスからの相対パスでなければなりません。
このパラメーターは省略できません。

このファイルはこのフィルタの直接の出力先ではなく、
フォーマット作業における最終出力先であるかもしれないことに留意してください。

例:

```yaml
_Param.ToFileRelPath: "sub/a.html"
```


## ASTへ加える変更

このフィルターは入力されたASTに対して以下の点を変更します。

### 拡張子の置き換え

このフィルタは、入力AST中に相対リンクがあり、
その拡張子が`ExtensionMap`パラメーターで指定された拡張子マッピングのの対象である場合、
拡張子をマッピングに従って置き換えます。

例えば、拡張子マッピングが".md"から".html"へのマッピングを含んでいる場合、
入力AST中の"a.md"は"a.html"に置き換えられます。

### タイトルの調整

このフィルタは文書のタイトルを調整します。

`pandoc`で例えば一般的なmarkdown文書をhtmlに変換すると、
レベル1ヘッダが重複して出力される等、
期待される出力にならないことがあります。

`pandoc`は文書の`title`メタデータしか文書のタイトルとして認識しません。
また、htmlなどの出力フォーマットに対しては、
`pandoc`は、他にレベル1ヘッダがあるかどうかに関わらず、
タイトルをレベル１ヘッダとして出力してしまいます。

そのため、このフィルターは入力ASTを以下のように変更します。

* 入力ASTに`title`メタデータが設定されていない場合、
  このフィルタは文書にレベル1ヘッダが存在すれば、
  最初のものと同等の内容を`title`メタデータとして追加します。
* 入力AST中のレベル1ヘッダをすべて削除します。

結果として、ソースドキュメントファイルの先頭にレベル1ヘッダをひとつだけ書いておけば、
その出力は期待するものになるでしょう。

### 拡張子対応の対象外である相対リンクの変更

`RebaseOtherRelativeLinks`パラメーターがtrueの場合、
拡張子マッピングの対象ではない拡張子をもつ入力AST中の相対リンクは、
`ToBaseDirPath`パラメーターのディレクトリ下からの相対リンクにリベースされ、
リンクはオリジナルの場所にあるファイルを参照し続けます。

例えば、
`FromBaseDirPath`が"/docs/md"で`ToBaseDirPath`が"/docs/html"の場合、
入力AST中の"a.png"へのリンクは、"../md/a.png"にリベースされます。

`RebaseOtherRelativeLinks`パラメーターがfalseの場合、
このフィルタはそれらのリンクを変更しません。
リンク先は`ToBaseDirPath`中にあると想定されます。
[Format-Documents.ps1](Format-Documents.ja.md)など、
このフィルタを用いるスクリプトの中には、
それらのファイルを`FromBaseDirPath`から`ToBaseDirPath`へコピーする機能をもつものがあります。

### マクロ処理

このフィルタは、入力ASTのメタデータ中の以下のマクロを処理します。

* condition
* rebase

マクロ処理の詳細については、[about_Macros](about_Macros.ja.md)を参照してください。


## 関連項目

* [Format-Documents.ps1](Format-Documents.ja.md)
* [about_MetadataMacros](about_MetadataMacros.ja.md)