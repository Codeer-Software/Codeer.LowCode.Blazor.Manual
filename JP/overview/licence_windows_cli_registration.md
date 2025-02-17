# WindowsでCliツールによる登録

Windowsでライセンス登録する場合コマンドラインツールから登録することもできます。
対象のコンピュータ上で次のコマンドを実行することでライセンスを登録・削除することが可能です。

## オンライン
下記コマンドを実行後、アプリケーションを再起動すると、ライセンス情報が適用されます。
### 登録

Visual StudioソリューションにLicenseRegisterCliプロジェクトが含まれています。
ビルドしてLicenseRegisterCli.exeを含んだフォルダーを対象のコンピュータにコピーし次のコマンドを実行してください。


```bash
./LicenseRegisterCli.exe activate -k "<YOUR_LICENSE_KEY>" -c "<YOUR_LICENSE_NAME>"
```

### 削除

LicenseRegisterCli.exeを含んだフォルダを対象のコンピュータにコピーし次のコマンドを実行してください。

```bash
./LicenseRegisterCli.exe deactivate
```


## オフライン
下記コマンドを実行後、アプリケーションを再起動すると、ライセンス情報が適用されます。

### 登録

オフライン登録用データを対象コンピュータ上で最初に生成します。

LicenseRegisterCli.exeを含んだフォルダーを対象のコンピュータにコピーし次のコマンドを実行してください。

```bash
./LicenseRegisterCli.exe activate -k "<YOUR_LICENSE_KEY>" -c "<YOUR_LICENSE_NAME>" -o
```

ライセンスページの登録に使えるライセンス登録データが出力されるのでそれを使って[オフライン認証](./license_web_registration.md)用文字列を取得し、任意のファイルに保存しておいてください。
その後次のコマンドを実行しライセンスを読み込んでください。

```bash
./LicenseRegisterCli.exe import -i "<YOUR_IMPORT_PATH>"
```

### 解除

LicenseRegisterCli.exeを含んだフォルダーを対象のコンピュータにコピーし次のコマンドを実行してください。

```bash
./LicenseRegisterCli.exe revoke -e "<YOUR_EXPORT_PATH>"
```

実行が完了すると、コンソールにオフライン解除キーが出力されます。このキーを使用して[ライセンス認証を解除](./license_web_cancellation.md)してください。
`-e` オプションで設定したパスに同様の解除キーが保存されています。


## 関連情報
- [オフライン（WEB経由）で登録する方法](./license_web_registration.md)
- [オフライン（WEB経由）で解除する方法](./license_web_cancellation.md)