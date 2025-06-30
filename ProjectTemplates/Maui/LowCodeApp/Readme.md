# ビルドエラー

## 内容

- 実行後にリビルドしようとすると発生する場合がある。VisualStudio起動直後でも発生したことがある。
- 対策後にリビルド可能になっても再発する。

## 対策

- VisualStudioを終了させてから、プロジェクトの`bin`と`obj`を削除してリビルドしてみる。
- それでもだめならPC再起動。`bin`、`obj`フォルダの削除をせずPC再起動のみで直った場合もあり。

## ログ

以下のようなログが出力される

```log
1>------ すべてのリビルド開始: プロジェクト:MauiApptest, 構成: Debug Any CPU ------
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018: "DiscoverPrecompressedAssets" タスクが予期せずに失敗しました。
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018: System.ArgumentException: 同一のキーを含む項目が既に追加されています。
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 System.ThrowHelper.ThrowArgumentException(ExceptionResource resource)
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 System.Collections.Generic.Dictionary`2.Insert(TKey key, TValue value, Boolean add)
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 System.Linq.Enumerable.ToDictionary[TSource,TKey,TElement](IEnumerable`1 source, Func`2 keySelector, Func`2 elementSelector, IEqualityComparer`1 comparer)
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 Microsoft.AspNetCore.StaticWebAssets.Tasks.DiscoverPrecompressedAssets.Execute()
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 Microsoft.Build.BackEnd.TaskExecutionHost.Execute()
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 Microsoft.Build.BackEnd.TaskBuilder.<ExecuteInstantiatedTask>d__26.MoveNext()
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018: "DiscoverPrecompressedAssets" タスクが予期せずに失敗しました。
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018: System.ArgumentException: 同一のキーを含む項目が既に追加されています。
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 System.ThrowHelper.ThrowArgumentException(ExceptionResource resource)
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 System.Collections.Generic.Dictionary`2.Insert(TKey key, TValue value, Boolean add)
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 System.Linq.Enumerable.ToDictionary[TSource,TKey,TElement](IEnumerable`1 source, Func`2 keySelector, Func`2 elementSelector, IEqualityComparer`1 comparer)
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 Microsoft.AspNetCore.StaticWebAssets.Tasks.DiscoverPrecompressedAssets.Execute()
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 Microsoft.Build.BackEnd.TaskExecutionHost.Execute()
1>C:\Program Files\dotnet\sdk\9.0.300\Sdks\Microsoft.NET.Sdk.StaticWebAssets\targets\Microsoft.NET.Sdk.StaticWebAssets.Compression.targets(289,5): error MSB4018:    場所 Microsoft.Build.BackEnd.TaskBuilder.<ExecuteInstantiatedTask>d__26.MoveNext()
1>プロジェクト "MauiApptest.csproj" のビルドが終了しました -- 失敗。
```

---

# ビルドエラー

## 内容

- `Codder.LowCode.Blazor`のソリューションに新規作成でMAUIプロジェクトを追加してビルドすると発生。
- MAUIプロジェクトのプロパティを見ると、MAUIのアプリではなくコンソールアプリのプロパティとして認識されている状態。（iOSターゲットやAndroidターゲットの設定が無い状態）

## 対策

- `dotnet workload restore`をコマンドプロンプトを開いて、追加したプロジェクトのフォルダで実行してからPC再起動で直った？
- →コマンド実行するといろいろダウンロードされて数分かかる

## ログ

以下のようなログが出力される

```log
1>------ すべてのリビルド開始: プロジェクト:LowCodeApp (App\Maui\LowCodeApp), 構成: Debug Any CPU ------
1>C:\Program Files\dotnet\sdk\8.0.411\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.ImportWorkloads.targets(38,5): error NETSDK1147: このプロジェクトをビルドするには、次のワークロードをインストールする必要があります: maui-android
1>C:\Program Files\dotnet\sdk\8.0.411\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.ImportWorkloads.targets(38,5): error NETSDK1147: これらのワークロードをインストールするには、次のコマンドを実行します: dotnet workload restore
1>プロジェクト "LowCodeApp.csproj" のビルドが終了しました -- 失敗。
1>C:\Program Files\dotnet\sdk\8.0.411\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.ImportWorkloads.targets(38,5): error NETSDK1147: このプロジェクトをビルドするには、次のワークロードをインストールする必要があります: maui-tizen
1>C:\Program Files\dotnet\sdk\8.0.411\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.ImportWorkloads.targets(38,5): error NETSDK1147: これらのワークロードをインストールするには、次のコマンドを実行します: dotnet workload restore
1>C:\Program Files\dotnet\sdk\8.0.411\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.ImportWorkloads.targets(38,5): error NETSDK1147: このプロジェクトをビルドするには、次のワークロードをインストールする必要があります: maui-maccatalyst
1>C:\Program Files\dotnet\sdk\8.0.411\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.ImportWorkloads.targets(38,5): error NETSDK1147: これらのワークロードをインストールするには、次のコマンドを実行します: dotnet workload restore
1>プロジェクト "LowCodeApp.csproj" のビルドが終了しました -- 失敗。
1>C:\Program Files\dotnet\sdk\8.0.411\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.ImportWorkloads.targets(38,5): error NETSDK1147: このプロジェクトをビルドするには、次のワークロードをインストールする必要があります: maui-ios
1>C:\Program Files\dotnet\sdk\8.0.411\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.ImportWorkloads.targets(38,5): error NETSDK1147: これらのワークロードをインストールするには、次のコマンドを実行します: dotnet workload restore
1>プロジェクト "LowCodeApp.csproj" のビルドが終了しました -- 失敗。
```

---

# 実行エラー

## 内容

- 「デバッグするには、プロジェクトを配置する必要があります。構成マネージャーで配置を有効にしてください。」と出て起動できず。
- リビルド後に実行すると発生することがある。

## 対策

- リビルドで直った

## ログ

以下のようなログが出力される

```log
新しいクリーンなレイアウトを作成しています...
レイアウトにファイル (合計 43 MB) をコピーしています...
必要なフレームワークがインストールされているかどうかを確認しています...
アプリケーションをレイアウトから実行するように登録しています...
DEP0700: アプリケーションの登録に失敗しました。[0x80073CF6] AppxManifest.xml(34,27): エラー 0x80070002: スプラッシュ画面画像 [splashSplashScreen.png] が見つからないため、パッケージ com.companyname.lowcodeapp_9zz4h110yvjzm をインストールまたは更新できません。アプリケーションのスプラッシュ画面として使用できる画像がパッケージに含まれていることと、パッケージ マニフェストがパッケージ内の適切な場所、つまりこのスプラッシュ画面画像がある場所をポイントしていることを確認してください。
```

---

# 実行エラー

## 内容

- 「デバッグするには、プロジェクトを配置する必要があります。構成マネージャーで配置を有効にしてください。」と出て起動できず。
- リビルド後に実行すると発生することがある。

## 対策

- リビルドで直った

## ログ

以下のようなログが出力される

```log
DEP1700: レシピ ファイル "C:\lowcode\Source\App\Maui\bin\Debug\net8.0-android\LowCodeApp.build.appxrecipe" は存在しません。プロジェクトをビルドする必要がある可能性があります。
```

---

# サーバーの設定

## アドレスの設定

サーバーのプロジェクトプロパティで、
デバッグ→全般→デバッグ起動プロファイルUIを開く→アプリのURLの欄を
「`http://0.0.0.0:5140`」
に変更する。

`https`を入れるとAndroid/iOS側で別途対応が必要なようで接続できなくなってしまう。
「`localhost`」の場合はAndroid/iOSや別PCからのアクセスができない為、「`0.0.0.0`」とすることで接続できるようになる。
 
## UseHotReloadの設定

サーバーの`appsettings.Development.json`の「`UseHotReload`」を「`false`」に変更する。
Android/iOS側の初期化時に例外が出てしまうのでOFFにする。

---

# 接続先の設定

## appsettings.Development.jsonの作成

「appsettings.Development.json.template」をコピーして「appsettings.Development.json」にリネームして使用してください。

