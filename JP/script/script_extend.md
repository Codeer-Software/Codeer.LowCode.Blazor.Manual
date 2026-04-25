# スクリプトの拡張

`.NET` のクラス・独自サービスをスクリプトから呼べるようにするには、
ユーザーコードの `IAppInfoService` 実装の中で `ScriptRuntimeTypeManager` に登録します。

VS テンプレートで作ったプロジェクトでは `WebApp.Client.Shared/Services/AppInfoService.cs` がその場所です。

```csharp
public class AppInfoService : IAppInfoServiceExtension
{
    readonly ScriptRuntimeTypeManager _scriptRuntimeTypeManager = new();

    public AppInfoService(...)
    {
        // ↓ ここで型・サービスを登録する
        _scriptRuntimeTypeManager.AddType<MyType>();
        _scriptRuntimeTypeManager.AddService(new MyService(...));
    }

    public ScriptRuntimeTypeManager GetScriptRuntimeTypeManager()
        => _scriptRuntimeTypeManager;
}
```

---

## AddType — 型を追加

スクリプトから `new T()` で作れる型、または静的メンバを呼び出せる型として登録します。

```csharp
_scriptRuntimeTypeManager.AddType<MemoryStream>();
_scriptRuntimeTypeManager.AddType(typeof(Math));
_scriptRuntimeTypeManager.AddType<MidpointRounding>();
```

スクリプト側:

```csharp
var ms = new MemoryStream();
var v = Math.Round(1.23, MidpointRounding.AwayFromZero);
```

### 登録される性質

`AddType` で渡された型は、内部で次のように分類されます。

| 型の特徴 | スクリプトでの扱い |
|---|---|
| `enum` | `EnumName.Member` で値が取れる |
| 公開コンストラクタを持つ | `new T()` で生成可能 |
| インスタンスメンバを持つ | 受け取った値のメソッドを呼べる |
| 静的メンバを持つ | 型名でアクセス可能（例: `Math.PI`） |

> 同名の型が登録済みなら無視されます。先勝ちです。

---

## AddService — サービスを追加

シングルトンとして登録します。スクリプト内では**型名で直接アクセス**できます。

```csharp
_scriptRuntimeTypeManager.AddService(new WebApiService(http, logger));
_scriptRuntimeTypeManager.AddService(new MyAppContext(...));
```

スクリプト側:

```csharp
var data = await WebApiService.Get("/testapi").JsonObject;
var user = MyAppContext.CurrentUser;
```

### サービス内に依存性を注入する

サービスやスクリプト内で `new` した型に対して、`Codeer.LowCode.Blazor.RequestInterfaces.Services` などを注入できます。
注入したいプロパティに `[ScriptInject]` を付けます。

```csharp
public class MyService
{
    [ScriptInject]
    public Services? Services { get; set; }   // ランタイムが自動で入れる

    public async Task DoWork() => await Services!.Logger.Log("hello");
}
```

### 任意の型を注入する

`AddCustomInjector` を使うと、独自の型をプロパティに注入できます。

```csharp
_scriptRuntimeTypeManager.AddCustomInjector(() => http);   // HttpService を注入

public class MyApi
{
    [ScriptInject]
    public HttpService? Http { get; set; }
}
```

---

## ScriptHide / ScriptName

公開する型・サービスのメンバを細かく制御するための属性です。

| 属性 | 用途 |
|---|---|
| `[ScriptHide]` | スクリプトから見えないようにする（補完にも出ない） |
| `[ScriptName("Foo")]` | スクリプト内での名前を変える（例: `FooAsync` → `Foo` で呼ばせる） |

```csharp
public class MyService
{
    [ScriptHide]
    public Services? Services { get; set; }

    [ScriptName("SendEmail")]
    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        ...
    }
}
```

スクリプト側:

```csharp
await MyService.SendEmail("a@b", "subject", "body");
```

---

## Designer 側でも認識させる

`AddType` / `AddService` で登録した型は、デザイナのスクリプトエディタの**補完にも反映**されます。デザイナ拡張については [デザイナのカスタマイズ](../designer/designer-customize.md) を参照してください。

---

## 例: 独自 WebAPI サービスを追加

#### 1. サービスを書く

```csharp
public class WeatherApiService
{
    [ScriptInject]
    public HttpService? Http { get; set; }

    [ScriptName("GetCurrent")]
    public async Task<WebApiResult> GetCurrentAsync(string city)
        => await Http!.GetAsync($"/api/weather?city={city}");
}
```

#### 2. AppInfoService に登録

```csharp
_scriptRuntimeTypeManager.AddService(new WeatherApiService());
```

#### 3. スクリプトで使う

```csharp
async Task ShowButton_OnClick()
{
    var result = await WeatherApiService.GetCurrent("Tokyo");
    Toaster.Success($"{result.JsonObject.temp} ℃");
}
```

---

## 関連項目

- [プロコード概要](../overview/procode.md) — Blazor コンポーネントの埋め込みも含む拡張全般
- [スクリプト概要](script.md)
- [組み込みサービスとテンプレート由来サービス](script_services.md)
- [デザイナのカスタマイズ](../designer/designer-customize.md)
- [ユーザーコード](../user_code/user_code.md)
