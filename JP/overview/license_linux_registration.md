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

マルチコンテナアプリケーション（WebアプリとWebサーバーソフト等が別々）の場合は、Webアプリが実行されるコンテナでmachine-idを設定してください。

```bash
# ホストマシンの /etc/machine-id をマウントする場合
$ docker run -v /etc/machine-id:/etc/machine-id:ro ...
```

`machine-id` を設定後いずれかの方法で `license.json` を取得しアプリケーションディレクトリへ配置してください

- ホストマシン上でCLIツールを使用して生成された `license.json` ファイルをDockerコンテナ上へコピー
- Dockerコンテナ内でホストマシンで取得するときと同様にしてライセンスを取得

#### オフライン登録

オフライン登録用データを対象コンピュータ上で最初に生成します。

LicenseRegisterCli.dllを含んだフォルダーを対象のコンピュータにコピーし次のコマンドを実行してください。

```bash
dotnet ./LicenseRegisterCli.dll activate -k "<YOUR_LICENSE_KEY>" -c "<YOUR_LICENSE_NAME>" -o
```

ライセンスページの登録に使えるライセンス登録データが出力されるのでそれを使って[オフライン認証](./license_web_registration.md)用文字列を取得し、任意のファイルに保存しておいてください。
その後次のコマンドを実行しライセンスを読み込んでください。

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

実行が完了すると、コンソールにオフライン解除キーが出力されます。このキーを使用して[ライセンス認証を解除](./license_web_cancellation.md)してください。
`-e` オプションで設定したパスに同様の解除キーが保存されています。

その後、`license.json`をアプリケーションディレクトリから削除してください。

## 関連情報
- [オフライン（WEB経由）で登録する方法](./license_web_registration.md)
- [オフライン（WEB経由）で解除する方法](./license_web_cancellation.md)