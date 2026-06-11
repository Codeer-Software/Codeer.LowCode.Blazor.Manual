# OptimisticLockingField - 楽観的排他制御

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.OptimisticLockingFieldDesign`

楽観的排他制御（Optimistic Concurrency Control）を実現するフィールド。レコード更新時にバージョン値を比較し、他のユーザーによる変更を検出する。`FieldDesignBase` を直接継承する。

## C# クラス定義 (真実の源)

```csharp
public class OptimisticLockingFieldDesign : FieldDesignBase
{
    public string DbColumn { get; set; } = string.Empty;
    public bool IncrementVersion { get; set; }
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
    // 注意: DbValueFieldDesignBase ではないので DbColumn は親階層から来ない (このクラス独自)
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | バージョン管理用のDB列名。snake_case推奨。 |
| `IncrementVersion` | bool | `false` | `true` の場合、更新時にバージョン番号を自動インクリメントする。`false` の場合はタイムスタンプ等の別メカニズムを使用。 |

## JSON例

### 整数バージョン番号による楽観ロック

```json
{
  "DbColumn": "version",
  "IncrementVersion": true,
  "IgnoreModification": false,
  "Name": "Version",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.OptimisticLockingFieldDesign"
}
```

### タイムスタンプベースの楽観ロック

```json
{
  "DbColumn": "updated_at",
  "IncrementVersion": false,
  "IgnoreModification": false,
  "Name": "OptimisticLock",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.OptimisticLockingFieldDesign"
}
```

## ランタイム動作

- **排他制御の仕組み:** レコード読み込み時にバージョン値を取得し、更新時にDB上の現在のバージョン値と比較する。値が一致しない場合、他のユーザーがレコードを変更したことを意味し、更新は失敗する。
- **IncrementVersion = true:** 整数型のバージョン番号を使用する。更新が成功するたびにバージョン番号が自動的に +1 される。
- **IncrementVersion = false:** タイムスタンプや外部で管理される値を使用する。アプリケーション側でバージョン値の更新を行う。
- **UIには表示されない:** 隠しフィールドとして動作し、画面上には表示されない。
- 検索対象外。
- 一括データ更新（BulkDataUpdate）は無効化される。

---

## DOM構造（CSS用）

### 表示（通常は非表示で使用）

```html
<span class="d-block py-2 text" style="[インラインスタイル]">0x1A2B3C...</span>
```

**注意:** 楽観ロックフィールドは通常レイアウトに配置しない（内部的に使用される）。レイアウトに配置した場合は16進数文字列として表示される。
