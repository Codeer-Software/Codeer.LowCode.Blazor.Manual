# Linuxベースのシステム

Linuxベースのシステムでライセンス登録する場合コマンドラインツールから登録することができます。
対象のコンピュータ上で次のコマンドを実行することでライセンスを取得・削除することが可能です。

#### 登録

LicenseRegisterCli.dllを含んだフォルダーを対象のコンピュータにコピーし次のコマンドを実行してください。

```bash
dotnet ./LicenseRegisterCli.dll activate -k "<YOUR_LICENSE_KEY>" -c "<YOUR_LICENSE_NAME>"
```

実行後、`license.json` ファイルが生成されます。このファイルを実際に使用するCodeerLowCodeアプリケーションのディレクトリにコピーしてください。
アプリケーションを再起動すると、ライセンス情報が適用されます。

#### 削除

LicenseRegisterCli.dllを含んだフォルダを対象のコンピュータにコピーし次のコマンドを実行してください。

```bash
dotnet ./LicenseRegisterCli.dll deactivate
```
実行が完了したら、`license.json`をアプリケーションディレクトリから削除してください。

### Docker上のLinuxベースのシステム

Docker上のLinuxベースシステムでライセンス使用する場合、コンテナ上の `/etc/machine-id` にホストマシンのIDを設定する必要があります。
コンテナ実行時にホストマシンの `/etc/machine-id` をマウントするか、コンテナ内で `/etc/machine-id` を設定してください。

```bash
# ホストマシンの /etc/machine-id をマウントする場合
$ docker run -v /etc/machine-id:/etc/machine-id:ro ...
```

`machine-id` を設定後いずれかの方法で `license.json` を取得しアプリケーションディレクトリへ配置してください

- ホストマシン上でCLIツールを使用して生成された `license.json` ファイルをDockerコンテナ上へコピー
- Dockerコンテナ内でホストマシンで取得するときと同様にしてライセンスを取得

#### オフライン登録

オフライン認証用の文字列を任意のファイルに保存しておいてください。

LicenseRegisterCli.dllを含んだフォルダーを対象のコンピュータにコピーし次のコマンドを実行してください。

```bash
dotnet ./LicenseRegisterCli.dll import -i "<YOUR_IMPORT_PATH>"
```

実行後、`license.json` ファイルが生成されます。このファイルを実際に使用するCodeerLowCodeアプリケーションのディレクトリにコピーしてください。
アプリケーションを再起動すると、ライセンス情報が適用されます。

#### オフライン解除

LicenseRegisterCli.dllを含んだフォルダーを対象のコンピュータにコピーし次のコマンドを実行してください。

```bash
dotnet ./LicenseRegisterCli.dll revoke -e "<YOUR_EXPORT_PATH>"
```

実行後、コンソールにオフライン解除キーが出力されます。このキーを使用してライセンス認証を解除してください。
`-e` オプションで設定したパスに同様の解除キーが保存されています。
