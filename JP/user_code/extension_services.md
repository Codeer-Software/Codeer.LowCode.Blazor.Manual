# 拡張サービスの登録と利用（Services.Provider）

カスタム Field や ProCode コンポーネントから、**自分で作ったサービスクラス**を使う方法を説明します。

Codeer.LowCode.Blazor は標準の DI（Dependency Injection）の仕組みをそのまま使っています。
自作のサービスを DI コンテナに登録しておけば、カスタム Field・コードビハインド・ProCode コンポーネントのどこからでも同じインスタンスに到達できます。

- 複数の ProCode コンポーネント間で状態を共有したい
- 自作の WebAPI クライアントやキャッシュをアプリ全体で 1 つだけ持ちたい
- カスタム Field の実行時処理から外部ライブラリのサービスを呼びたい

といった場合に使います。

## どこで登録するか

### Web アプリの場合

**Client プロジェクトの `Program.cs`** で、`builder.Build()` より前に登録します。
テンプレートで作成したソリューションには標準サービスの登録コードがすでにあるので（`Client.Shared` プロジェクトの `ServiceInitializer` クラスにある `AddSharedServices()` を `Program.cs` から呼んでいます）、その並びに追加するだけです。

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSharedServices();
builder.Services.AddScoped<INavigationService, NavigationService>();

// ★ 自作サービスをここで登録する
builder.Services.AddScoped<AppState>();

// ... (HttpClient の登録など)

await builder.Build().RunAsync();
```

サービスクラス自体は Client プロジェクトに置いても構いませんが、ProCode コンポーネントと同じく **`Client.Shared` プロジェクト**に置くと整理しやすくなります。

### WinForms / WPF デスクトップアプリの場合

`ServiceCollection` を構築している箇所（テンプレートでは `ServiceInitializer` クラスの `AddSharedServices()` を呼んでいる周辺）に同じように追加します。

### ライフタイムの選び方

| 登録方法 | インスタンスの寿命 |
|---|---|
| `AddSingleton<T>()` | アプリ全体で 1 つ |
| `AddScoped<T>()` | スコープごとに 1 つ |
| `AddTransient<T>()` | 取り出すたびに新規作成 |

Blazor WebAssembly ではスコープが実質 1 つ（ブラウザのタブ単位）なので、`AddSingleton` と `AddScoped` はほぼ同じ意味になります。状態を共有するサービスは `AddScoped` で登録しておくのが無難です。

## サービスクラスの例

特別な基底クラスは不要で、普通の C# クラスをそのまま登録できます。

```csharp
// 画面をまたいで状態を共有するサービスの例
public class AppState
{
    public string? SelectedCustomerId { get; set; }

    public event Action? Changed;
    public void NotifyChanged() => Changed?.Invoke();
}
```

登録済みの他のサービスをコンストラクタで受け取ることもできます（コンストラクタインジェクション）。

```csharp
// HttpClient を使う自作 WebAPI クライアントの例
public class WeatherApiClient
{
    private readonly HttpClient _http;
    public WeatherApiClient(HttpClient http) => _http = http;

    public async Task<string> GetForecastAsync(string city)
        => await _http.GetStringAsync($"api/weather/{city}");
}
```

## どこから取り出すか

フレームワークの主要クラスからは、標準サービスを束ねた `Services` に到達できます。
その **`Services.Provider`**（`IServiceProvider`）から、登録した自作サービスを取り出します。

```csharp
using Microsoft.Extensions.DependencyInjection;

var appState = Services.Provider.GetRequiredService<AppState>();
```

拡張ポイントごとの到達方法は次の通りです。

| 拡張ポイント | 取り出し方 |
|---|---|
| カスタム Field（`FieldBase` 派生） | 自身の `Services.Provider` |
| コードビハインド（`ProCodeBehindBase`） | `Module.Services.Provider` |
| ProCode コンポーネント（`ProCodeComponentBase`） | `@inject` または `Field.Services.Provider` |
| ProCode ページ（`ProCodeModuleBase`） | `@inject` |
| カスタム Field の表示コンポーネント（`FieldComponentBase<T>`） | `@inject` または `Field.Services.Provider` |

Razor コンポーネント（`@inject` が使える場所）では通常の Blazor と同じく `@inject` で受け取るのが簡単です。
どちらの方法でも**同じインスタンス**が返ります。

`GetRequiredService<T>()` は未登録なら例外、`GetService<T>()` は未登録なら `null` を返します。登録し忘れに早く気付けるよう、基本は `GetRequiredService` を使ってください。

### カスタム Field から使う

`FieldBase` 派生クラスは `Services` プロパティを持っているので、そこから `Provider` をたどります。

```csharp
using Microsoft.Extensions.DependencyInjection;

public class ColorPickerField : ValueFieldBase<ColorPickerFieldDesign, ColorPickerFieldData, string>
{
    public ColorPickerField(ColorPickerFieldDesign design) : base(design) { }

    public override async Task SetValueAsync(string? value)
    {
        await base.SetValueAsync(value);

        // 自作サービスに状態を反映
        var appState = Services.Provider.GetRequiredService<AppState>();
        appState.SelectedCustomerId = value;
        appState.NotifyChanged();
    }
}
```

> **注意:** `Services` はフレームワークが Field の生成直後にセットします。**コンストラクタの時点ではまだ使えません**。`InitializeDataAsync` やイベント処理など、初期化以降の処理で使ってください。

### コードビハインドから使う

`ProCodeBehindBase` には `Module` プロパティがあるので、`Module.Services.Provider` でたどります。

```csharp
using Microsoft.Extensions.DependencyInjection;

public class CustomerDetail : ProCodeBehindBase
{
    public ButtonField? CheckWeather { get; set; }
    public TextField? City { get; set; }
    public TextField? Result { get; set; }

    public override async Task OnBeforeDetailInitializationAsync(string layoutName)
    {
        if (CheckWeather != null) CheckWeather.OnClickAsync = CheckWeather_Click;
        await Task.CompletedTask;
    }

    private async Task CheckWeather_Click()
    {
        var api = Module.Services.Provider.GetRequiredService<WeatherApiClient>();
        var forecast = await api.GetForecastAsync(City?.Value ?? string.Empty);
        if (Result != null) await Result.SetValueAsync(forecast);
    }
}
```

### ProCode コンポーネント / ProCode ページから使う

`ProCodeComponentBase` / `ProCodeModuleBase` は通常の Blazor コンポーネントなので、`@inject` がそのまま使えます。

```razor
@using Codeer.LowCode.Blazor.ProCode
@inherits ProCodeComponentBase
@inject AppState AppState
@implements IDisposable

<div>選択中の顧客: @AppState.SelectedCustomerId</div>

@code {
    protected override void OnInitialized()
        => AppState.Changed += StateHasChanged;

    public void Dispose()
        => AppState.Changed -= StateHasChanged;
}
```

C# コード側で取り出したい場合は、`ProCodeComponentBase` なら `Field.Services.Provider` でも同じインスタンスに到達できます。

## 標準サービス

`Services` には `Provider` のほかに、フレームワークの標準サービスがプロパティとして並んでいます。これらは `Provider` を経由せず直接使えます。

| プロパティ | 説明 |
|---|---|
| `AppInfoService` | アプリケーション情報の取得 |
| `ModuleDataService` | モジュールデータの取得・更新 |
| `NavigationService` | 画面遷移 |
| `UIService` | ダイアログ表示などの UI 操作 |
| `Logger` | ログ |

各サービスの役割は [ユーザーコード](user_code.md) を参照してください。

## スクリプトから使いたい場合

ここで説明した方法は C# コード（プロコード）から使う方法です。
デザイナで書く**スクリプト**から自作サービスを呼びたい場合は `AddService` で登録します。[スクリプトの拡張](../script/script_extend.md) を参照してください。

## 関連項目

- [プロコード概要](../overview/procode.md) — カスタム Field / コードビハインド / ProCode コンポーネントの作り方
- [ProCodeField](../fields/ProCode.md) — ProCode コンポーネントを画面に埋め込む Field
- [ユーザーコード](user_code.md) — テンプレートのコード構成と要求インターフェイス
- [スクリプトの拡張](../script/script_extend.md) — スクリプトから .NET コードを呼び出す
