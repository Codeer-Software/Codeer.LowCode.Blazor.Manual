# プロコードでのデータ保存（追加・更新・削除）

データの追加・更新・削除は `ModuleSubmitData` を組み立てて保存処理に渡すことで行います。
**この保存処理はプロコードから利用します。**

## 共通の流れ

```csharp
using Codeer.LowCode.Blazor.DataIO;          // ModuleSubmitData / ModuleDeleteInfo
using Codeer.LowCode.Blazor.Repository.Data; // ModuleData / 各 FieldData / IdFieldData

var submit = new ModuleSubmitData { ModuleName = "Customer" };
// ... ここで Add / Update / Delete を設定する ...

var results = await Module.Services.ModuleDataService.SubmitAsync(new List<ModuleSubmitData> { submit });
```

- `ModuleName` には対象モジュール名を指定します。
- 1 回の保存で複数の `ModuleSubmitData` を渡せます。
- フロント（コードビハインドなど）は `IModuleDataService.SubmitAsync`、サーバーのカスタマイズコード（`ModuleDataIO`）は `SubmitWithTransactionAsync` を使います。

データの値は、モジュールに対応した C# クラス（デザイナの右クリック **Create FieldData Class** で生成）を `ModuleData.Create(...)` に渡して作ります。クラスの書き方は [C# でのデータ取得ヘルパ](module_data_io_extensions.md) を参照してください。

## 追加（Add）

```csharp
var newId = IdFieldData.NewId(); // 新規データ用の仮の Id

var customer = new Customer
{
    Id   = newId,
    Name = new TextFieldData { Value = "山田太郎" },
    Age  = new NumberFieldData { Value = 30 },
};

var submit = new ModuleSubmitData { ModuleName = "Customer", Id = newId.Value! };
submit.Add.Add(ModuleData.Create(customer));

var results = await Module.Services.ModuleDataService.SubmitAsync(new List<ModuleSubmitData> { submit });
```

- 自動採番の Id の場合は `IdFieldData.NewId()` で仮の Id を入れておきます。保存後に実際の Id が割り当てられ、結果から取得できます（[結果の確認](#結果の確認)）。
- 手動でキーを決めるモジュールの場合は、その値を Id に設定します。

## 更新（Update）

更新対象の Id と、**変更したいフィールドだけ**を入れた `ModuleData` を `Update` に追加します。含めなかったフィールドは変更されません。

```csharp
var customer = new Customer
{
    Id  = new IdFieldData { Value = "123" },   // 更新対象の Id
    Age = new NumberFieldData { Value = 31 },  // 変更するフィールドだけ入れる
};

var submit = new ModuleSubmitData { ModuleName = "Customer", Id = "123" };
submit.Update.Add(ModuleData.Create(customer));

await Module.Services.ModuleDataService.SubmitAsync(new List<ModuleSubmitData> { submit });
```

> 楽観ロックを使うモジュールでは、対象データが他で更新されていると更新は失敗します。

## 削除（Delete）

`ModuleDeleteInfo` に対象の Id を指定します。

```csharp
var submit = new ModuleSubmitData { ModuleName = "Customer" };
submit.Delete.Add(new ModuleDeleteInfo { ModuleName = "Customer", Id = "123" });

await Module.Services.ModuleDataService.SubmitAsync(new List<ModuleSubmitData> { submit });
```

検索条件に一致するデータをまとめて削除したい場合は [検索条件でまとめて削除する（SearchDelete）](search_delete.md) を使います。

## まとめて実行する／トランザクション

- 1 つの `ModuleSubmitData` に `Add` / `Update` / `Delete` / `SearchDelete` を混在させられます。複数の `ModuleSubmitData` を一度に渡すこともできます。
- これらは **1 つのトランザクション**で実行されます。途中で失敗した場合はすべて巻き戻り、一部だけ反映された状態は残りません。

## 結果の確認

`SubmitAsync` は `ModuleSubmitResult` のリストを返します。

| プロパティ | 説明 |
|----|----|
| `DestinationId` | 保存後の実際の Id（自動採番では割り当てられた Id） |
| `TemporaryIdMap` | 仮の Id → 実際の Id の対応 |
| `ExceptionMessage` | エラーがあればその内容（空なら成功） |

```csharp
var results = await Module.Services.ModuleDataService.SubmitAsync(new List<ModuleSubmitData> { submit });
if (results == null || results.Any(r => !string.IsNullOrEmpty(r.ExceptionMessage)))
{
    // 失敗（トランザクションはロールバック済み）
}
```

## 権限

モジュールの作成・更新・削除の可否設定に従います。許可されていない操作を含む保存はエラーになります。

## 関連

- [検索条件でまとめて削除する（SearchDelete）](search_delete.md)
- [C# でのデータ取得ヘルパ](module_data_io_extensions.md)
- [プロコード](../overview/procode.md)
