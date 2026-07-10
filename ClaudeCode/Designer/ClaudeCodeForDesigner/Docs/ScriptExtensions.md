# スクリプト拡張の仕組みと独自拡張の追加方法

スクリプトエンジンはアプリケーション側で拡張可能。`ScriptRuntimeTypeManager` に型やサービスを登録することで、
スクリプトから新しいクラスやサービスにアクセスできるようになる。

**登録済みサービス・型の一覧と使い方 (Excel / WebApi / Toaster / Mail 等) はこのドキュメントには載せない。**
この環境で実際に使える正確な一覧 (メンバーシグネチャ・使用例付き) は、デザイナ exe の `script-catalog`
サブコマンドが生成する `temporary/_script_catalog.md` を参照 (入力補完と同じ型モデルから生成されるため、
スクリプトで呼べないメンバーは載らない)。このドキュメントは「カタログに載る側 (拡張) を作る方法」を扱う。

---

## 拡張の仕組み

標準テンプレートの Excel / WebApi / Toaster / Mail は `Codeer.LowCode.Blazor.Extras` パッケージが提供し
(ソースは MIT で公開)、`Source/App/WebApp.Client.Shared/Services/AppInfoService.cs` で以下のように登録される。

```csharp
// テンプレート固有の登録
_scriptRuntimeTypeManager.AddService(loadingService);
_scriptRuntimeTypeManager.AddType<LoadingService.LoadingScope>();

// Extras パッケージの組み込みスクリプトオブジェクトを一括登録
// (型: Excel, ExcelCellIndex, WebApiResult, MailMessage /
//  サービス: WebApiService, Toaster, MailService / [ScriptInject] 用インジェクター)
ExtrasClientInitializer.Initialize(this, http, logger, toaster);
```

メール送信・Excel PDF 変換などのエンドポイント URL はアプリ（コントローラを持つ側）の持ち物なので、
`Source/App/WebApp.Client.Shared/Services/ServiceInitializer.cs` で各機能の static プロパティに設定する。

```csharp
MailService.SendMailEndPoint = "/api/mail";
Codeer.LowCode.Blazor.Extras.ScriptObjects.Excel.ConvertPdfEndPoint = "api/excel/pdf";
```

---

## 拡張の追加方法

プロジェクト固有のサービスやクラスを追加するには、`AppInfoService.cs` で `ScriptRuntimeTypeManager` に登録する。

### サービスの追加（スクリプト内でサービス名で直接アクセス）

```csharp
// 1. サービスクラスを作成
public class MyService
{
    [ScriptInject]
    public Services? Services { get; set; }

    [ScriptInject]
    public IHttpService? Http { get; set; }

    [ScriptName("DoSomething")]
    public async Task<string> DoSomethingAsync(string input)
    {
        // 実装
        return $"Processed: {input}";
    }

    [ScriptHide]
    public void InternalMethod() { }  // スクリプトから非表示
}

// 2. AppInfoService で登録
_scriptRuntimeTypeManager.AddService(new MyService());
```

スクリプト側:
```csharp
var result = MyService.DoSomething("test");
```

### 型の追加（スクリプト内で `new` で生成可能）

```csharp
// 1. クラスを作成
public class MyData
{
    public string Name { get; set; } = "";
    public int Value { get; set; }

    public string Format()
    {
        return $"{Name}: {Value}";
    }
}

// 2. AppInfoService で登録
_scriptRuntimeTypeManager.AddType<MyData>();
```

スクリプト側:
```csharp
var data = new MyData();
data.Name = "test";
data.Value = 100;
Logger.Log(data.Format());
```

### カスタムインジェクター

`[ScriptInject]` 属性が付いたプロパティに自動注入される値を登録する。

```csharp
_scriptRuntimeTypeManager.AddCustomInjector(() => myDependency);
```

インジェクターは**プロパティの型と完全一致**で引かれる。`Func<IHttpService>` で登録したら
受け側プロパティも `IHttpService` 型にする（具象型 `HttpService` では注入されない）。

---

## スクリプト属性リファレンス

拡張クラスで使用可能な属性。

| 属性 | 適用先 | 説明 |
|---|---|---|
| `[ScriptName("Name")]` | メソッド, プロパティ | スクリプト内での名前を変更。例: `GetAsync` → `Get` |
| `[ScriptHide]` | メソッド, プロパティ | スクリプトから非表示にする |
| `[ScriptInject]` | プロパティ | フレームワークサービスを自動注入 |
| `[ScriptMethodToProperty("Name")]` | メソッド | 非同期メソッドをプロパティとして公開。例: `SetValueAsync` → `Value` のセッター |

登録した拡張がスクリプトからどう見えるか（メンバーの取捨・シグネチャ変換の結果）は、
`script-catalog` を再実行して `temporary/_script_catalog.md` で確認できる。
リフレクションで表現できない情報（使い方・例・注意）を AI に渡したい場合は、
デザイナ拡張の初期化コードで `ScriptObjectCatalog.Add(type, markdown)` を呼ぶとカタログと AI チャットに載る
（Extras の `ScriptObjectDocs/*.md` が実例）。
