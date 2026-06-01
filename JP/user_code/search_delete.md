# 検索条件でまとめて削除する（SearchDelete）

`SearchDelete` は、**検索条件に一致するデータをまとめて削除する**機能です。
Id を 1 件ずつ指定して削除するのではなく、「条件に当てはまるデータをすべて削除する」用途に使います。

> **この機能はプロコードからのみ利用できます。** デザイナの設定やスクリプトからは指定できません。

## 使い方

保存処理に渡す `ModuleSubmitData` の `SearchDelete` に、削除したいデータの検索条件を追加するだけです。

```csharp
using Codeer.LowCode.Blazor.DataIO;           // ModuleSubmitData / Where などの拡張メソッド
using Codeer.LowCode.Blazor.Repository.Match; // SearchCondition<T>

// 「完了」状態の TaskItem をまとめて削除する
var submit = new ModuleSubmitData { ModuleName = "TaskItem" };
submit.SearchDelete.Add(new SearchCondition<TaskItem>().Where(e => e.Status!.Value == "完了"));

await Module.Services.ModuleDataService.SubmitAsync(new List<ModuleSubmitData> { submit });
```

- `ModuleName` には対象モジュール名を指定します。
- `SearchDelete` に渡す条件の組み立て方は、データ取得のときと同じ `SearchCondition<T>` です。`.Where(...)` の書き方は [C# でのデータ取得ヘルパ](module_data_io_extensions.md) を参照してください。
- `TaskItem` はモジュールに対応した C# クラスです（デザイナの右クリック **Create FieldData Class** で生成できます）。

サーバーのカスタマイズコード（`ModuleDataIO`）から実行する場合は次のようになります。

```csharp
await moduleDataIO.SubmitWithTransactionAsync(new List<ModuleSubmitData> { submit });
```

## 追加・更新・削除との併用、複数条件

- `SearchDelete` は複数追加でき、それぞれの条件が適用されます（和集合で削除されます）。
- 同じ `ModuleSubmitData` に `Add` / `Update` / `Delete` と混在させることもできます。
- これらは **1 つのトランザクション**で実行されます。途中で失敗した場合はすべて巻き戻され、**中途半端に一部だけ削除された状態は残りません**。

```csharp
var submit = new ModuleSubmitData { ModuleName = "TaskItem" };
submit.SearchDelete.Add(new SearchCondition<TaskItem>().Where(e => e.Status!.Value == "完了"));
submit.SearchDelete.Add(new SearchCondition<TaskItem>().Where(e => e.Status!.Value == "キャンセル"));
await Module.Services.ModuleDataService.SubmitAsync(new List<ModuleSubmitData> { submit });
```

## 注意点（安全のために）

- **条件を指定しない `SearchDelete` は例外になります。** 条件が無いと「全件一致」となり、意図しない全データ削除につながるため、あえて拒否しています。
- 一致したデータはすべて削除されます。閲覧権限（データ参照条件）で見えないデータは削除対象になりません。

### 本当に全件削除したいとき

全件を削除したい場合は、**常に真になる明示的な条件**を指定します。たとえば連番の Id であれば、`0` は実在しない Id なので次の条件で全件に一致します。

```csharp
var submit = new ModuleSubmitData { ModuleName = "TaskItem" };
submit.SearchDelete.Add(new SearchCondition<TaskItem>().Where(e => e.Id!.Value != "0"));
await Module.Services.ModuleDataService.SubmitAsync(new List<ModuleSubmitData> { submit });
```

## 関連

- [C# でのデータ取得ヘルパ](module_data_io_extensions.md) … 条件（`SearchCondition<T>` / `.Where`）の書き方
- [プロコード](../overview/procode.md) … プロコードでの拡張全般
