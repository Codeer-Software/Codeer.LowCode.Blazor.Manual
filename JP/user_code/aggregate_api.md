# 集計 API の結線 (既存アプリ)

集計の機能 (スクリプトの [ModuleAggregator / BatchAggregator](../script/script_module_aggregator.md)、Extras のクロス集計フィールド、ApexCharts の集計チャート) は、
アプリ (ホスト) が集計 API に対応している必要があります。

- **1.3.39 以降のテンプレート (VSIX / Starter) で作ったアプリ**: 最初から結線されています。この作業は不要です
- **それより前のテンプレートで作ったアプリ**: パッケージを 1.3.39 以降に上げたうえで、下の 2 か所 (Web アプリ) または 1 か所 (Windows アプリ) を追加します

結線していないアプリで集計を使うと、表やグラフの代わりに「The host does not implement IModuleDataService.AggregateAsync」のエラーが出ます (それ以外の機能には影響しません)。

## Web アプリ

### サーバー: ModuleDataController

`Controllers/ModuleDataController.cs` に集計の受け口を追加します。権限の確認は `ModuleDataIO.AggregateAsync` が一覧と同じ規則で行います。

```csharp
using Codeer.LowCode.Blazor.Repository.Match;   // AggregateCondition

[HttpPost("aggregate")]
public async Task<IActionResult> AggregateAsync(List<AggregateCondition> conditions)
{
    var results = await _dataService.ModuleDataIO.AggregateAsync(conditions);
    //結果は MessagePack で返す (一覧 list と同じ運び方)
    return File(new MemoryStream(MessagePackSerializer.Typeless.Serialize(results)), "application/octet-stream");
}
```

監査ログ (Extras) を有効にしているアプリは、一覧 (`list`) と同じく `[Audit(AuditCategory.DataRead)]` を付けて、`_audit.AddTarget(moduleName, null, "Aggregate")` で対象モジュールを記録します (Starter の Cookie テンプレートが例です)。

### クライアント: ModuleDataService

`LowCodeApp.Client.Shared/Services/ModuleDataService.cs` (`IModuleDataService` の実装) に追加します。

```csharp
using Codeer.LowCode.Blazor.Aggregation;   // AggregateResult

public async Task<List<AggregateResult>> AggregateAsync(List<AggregateCondition> conditions)
{
    var result = await _http.PostAsJsonReturnHttpResponseAsync($"/api/module_data/aggregate", conditions);
    if (result == null) throw new InvalidOperationException("Aggregation failed.");
    using var memory = (MemoryStream)await result.Content.ReadAsStreamAsync();
    return MessagePackSerializer.Typeless.Deserialize(memory) as List<AggregateResult>
        ?? throw new InvalidOperationException("Aggregation failed.");
}
```

失敗したときは空のリストを返さず、例外にしてください (表やグラフがその場にエラーを出します)。渡した定義と同じ数の結果を返すのが約束です。

## Windows アプリ (WPF / WinForms)

サーバーを持たず同じプロセスでデータにアクセスするので、`Services/ModuleDataService.cs` に 1 つ追加するだけです。

```csharp
using Codeer.LowCode.Blazor.Aggregation;

public async Task<List<AggregateResult>> AggregateAsync(List<AggregateCondition> conditions)
{
    await using var dbAccess = new DbAccessor(SystemConfig.Instance.DataSources);
    var temporaryFileManager = new TemporaryFileManager(dbAccess, SystemConfig.Instance.TemporaryFileTableInfo, FileStorageTable.Storages);
    var dataIO = new CustomizedModuleDataIO(DesignerService.GetDesignData(), new AuthenticationContext(), dbAccess, temporaryFileManager);
    return await dataIO.AggregateAsync(conditions);
}
```

## 関連項目

- [ユーザーコード](user_code.md)
- [ModuleAggregator / BatchAggregator](../script/script_module_aggregator.md)
- [可視化・ダッシュボード](../patterns/visualization_dashboard.md)
