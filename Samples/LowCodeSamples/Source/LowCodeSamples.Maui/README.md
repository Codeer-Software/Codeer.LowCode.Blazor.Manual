# LowCodeSamples.Maui (.NET MAUI クライアント)

LowCodeSamples のサーバー (`LowCodeSamples.Server`) に接続する Android / iOS クライアント。
Starter リポジトリの MAUI ホスト (`Source/Hosts/Maui/LowCodeApp.Maui`) を土台に、デモサイト向けに次の 2 点だけ変えている。

- **認証なし**: デモサーバーはログイン画面を持たず `api/account/current_user` が固定の操作ユーザーを返すため、
  ログイン画面 (`Pages/Login.razor`) と Cookie / antiforgery の受け渡し (`Services/ServerConnection`) を外している
- **端末センサー**: `MobileSensorField` (Client.Shared/Samples/MobileSensor) 用に位置情報・加速度センサーの実装
  (`MobileSensorImpl.cs`) を登録し、AndroidManifest.xml / Info.plist に位置情報の許可を足している

アプリは薄いクライアントで、起動時にサーバーからデザインを取得する。画面の変更はサーバー側のデザインのデプロイだけで反映され、ストアの更新は要らない。

## 前提

- .NET 10 SDK と `maui-android` / `maui-ios` ワークロード (`dotnet workload install maui-android maui-ios`、
  または Visual Studio インストーラーの「.NET Multi-platform App UI 開発」)
- Android: Android SDK + エミュレータまたは実機 (Visual Studio のワークロードで入る)
- iOS: Xcode の入った Mac とのペアリング。Mac が無くても iOS ターゲットはコンパイルできるが、パッケージ化・実行はできない

## サーバーのアドレス

`appsettings.json` の `Server:BaseUrl` が既定の接続先。アプリの *Settings* 画面で実行時に変更できる (MAUI `Preferences` に保存)。

```json
{
  "Server": {
    "BaseUrl": "https://10.0.2.2:7169/"
  }
}
```

- `10.0.2.2` は Android エミュレータから PC の `localhost` を指すアドレス。iOS シミュレータは `localhost` のままでよい
- 既定はサーバーの `https` 起動プロファイル (`https://localhost:7169`) を指す。ASP.NET Core の開発証明書は `localhost` 向けで端末からは信頼されないため、
  **Debug ビルドは証明書を検証しない** (`ServerConnection.CreateHttpClient`)。Release ビルドは通常どおり検証するので正規の証明書が要る
- 実機は PC の LAN アドレス (例 `https://192.168.1.10:7169/`) を指定し、サーバーを全インターフェースで待ち受けさせる (起動プロファイルの `applicationUrl` を `https://0.0.0.0:7169` に)
- 開発用に平文 `http` を許可している (`AndroidManifest.xml` の `android:usesCleartextTraffic="true"`、`Info.plist` の `NSAllowsArbitraryLoads`)。サーバーが https 専用なら両方外す
- 環境固有の上書きは `appsettings.Development.json` に置く (同じ形。存在すれば自動で読む。gitignore 対象)

## 実行

サーバーはこのアプリをホストしない (WebAssembly クライアントと違う) ので、両方を起動する。
MAUI プロジェクトを他のスタートアッププロジェクトと同時にデバッグするのは不安定なので別々に起動する:

1. まず `LowCodeSamples.Server` を起動する (起動プロファイル `https`)。*デバッグ → デバッグなしで開始* でよい
2. このプロジェクトを Android エミュレータ / iOS シミュレータ / 実機で起動する

サーバー URL はアプリのタイトルバーの *Settings* (Android は ⋮ メニュー) から変えられる。*Reset to default* で `appsettings.json` の値に戻る。

## 仕組み

- `MauiProgram.cs` はブラウザ版クライアントと同じ共有サービス (`AddSharedServices`) と、`Server:BaseUrl` を指す `HttpClient` を登録する
- `Pages/LowCodePage.razor` がローコードページのホスト。起動時に `api/account/current_user` で操作ユーザーを解決する
- デザインは WebView ごとに 1 回読み込む。デザインをデプロイし直したらタイトルバーの *Reload* (WebView を作り直す)。
  サーバー側のホットリロード (`UseHotReload`) は端末が信頼できる SignalR 接続が要り、開発証明書のままでは静かに無効になる

## 公開

`LowCodeSamples.Maui.csproj` の `ApplicationId`、`ApplicationTitle`、アイコン (`Resources/AppIcon`)、スプラッシュ (`Resources/Splash`) を変え、
.NET MAUI の標準的な Android / iOS の公開手順に従う。
