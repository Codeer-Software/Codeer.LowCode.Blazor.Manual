# Linuxベースのシステム

Linuxベースのシステムでライセンス登録する場合コマンドラインツールから登録することができます。
対象のコンピュータ上で次のコマンドを実行することでライセンスを取得・削除することが可能です。

#### 登録

LicenseRegisterCli.dllを含んだ実行ファイルを対象のコンピュータにコピーし次のコマンドを実行してください。

```bash
./LicenseRegisterCli -k "<YOUR_LICENSE_KEY>" activate
```

実行後、`license.json` ファイルが生成されます。このファイルを実際に使用するCodeerLowCodeアプリケーションのディレクトリにコピーしてください。

#### 削除

LicenseRegisterCli.dllを含んだ実行ファイルを対象のコンピュータにコピーし次のコマンドを実行してください。

```bash
./LicenseRegisterCli deactivate
```

### Docker上のLinuxベースのシステム

Docker上のLinuxベースシステムでライセンス登録する場合、コンテナ上の `/etc/machine-id` にホストマシンのIDを設定する必要があります。
コンテナ実行時にホストマシンの `/etc/machine-id` をマウントするか、コンテナ内で `/etc/machine-id` を設定してください。

```bash
# ホストマシンの /etc/machine-id をマウントする場合
$ docker run -v /etc/machine-id:/etc/machine-id:ro ...
```

その後、Linuxベースのシステムと同様にコマンドを実行してください。
