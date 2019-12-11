# コンテナイメージ

コンテナイメージを直接提供していませんが、
PanasuがインストールされたLinuxコンテナ（Ubuntu 18.04ベース）を作るDockerfileを提供しています。

## イメージのビルド方法

Linuxシステム上のDocker、
またはLinuxコンテナを動かしているWindowsシステム上のDocker Desktop for Windows上で以下のコマンドを実行します。

```shell
docker build -t panasu https://github.com/ipponshimeji/Panasu.git#:Docker/Ubuntu_18.04
```

プロキシ環境下では、`--build-arg`オプションでhttps_proxyとhttp_proxyの値を渡してください。

```shell
# bashの場合
docker build -t panasu --build-arg https_proxy=$https_proxy --build-arg http_proxy=$http_proxy https://github.com/ipponshimeji/Panasu.git#:Docker/Ubuntu_18.04

# PowerShellの場合
docker build -t panasu --build-arg https_proxy=$env:https_proxy --build-arg http_proxy=$env:http_proxy https://github.com/ipponshimeji/Panasu.git#:Docker/Ubuntu_18.04
```

これで、`panasu:latest`という名前のイメージがビルドされます。


## コンテナの実行方法

Linuxシステム上のDocker、
またはLinuxコンテナを動かしているWindowsシステム上のDocker Desktop for Windows上で、
以下のコマンドにより、
前項でビルドした`panasu:latest`イメージを実行します。
この場合、生成されたコンテナは`panasu`という名前になります。

```shell
docker run -it --name panasu panasu:latest
```

このイメージでは、
`~/.local/Panasu`にPanasuが、
`~/PanasuSample`にサンプルが格納されています。

サンプルのソースドキュメントを変換するには、
`~/PanasuSample/_script`ディレクトリ上で`format.ps1`を実行してください。

```shell
~/PanasuSample/_scripts# ./format.ps1
```

Panasuの具体的な利用方法については、
[チュートリアル](Tutorial.ja.md)を参照してください。

PanasuのスクリプトはPowerShellですので、
コンテナ内でPanasuスクリプトしか使わないのであれば、
起動コマンドをPowerShellにしておくとスクリプトの起動時間が早くなります。
その場合は、
以下のようにコンテナを実行します。

```shell
docker run -it --name panasu panasu:latest pwsh
```

ホスト環境のファイルを変換したい場合は、
ホスト環境のストレージをマウントするようにコンテナを実行してください。
例えば、以下のコマンドはホスト環境（Windows）の`C:\Work`をコンテナの`/mnt/host`にマウントします。

```shell
docker run -it --name panasu -v C:\Work:/mnt/host panasu:latest
```
