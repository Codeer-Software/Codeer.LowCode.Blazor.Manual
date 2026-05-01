# 検索条件 (SearchCondition / MatchCondition)

モジュール間のデータ関連付け、検索フィルタ、アクセス制御に使用される条件定義のリファレンス。

---

## SearchCondition

データ取得全体の条件を定義する。ListField, LinkField, SelectField 等の `SearchCondition` プロパティで使用。

### プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `ModuleName` | string | `""` | 検索対象のモジュール名 |
| `Condition` | MatchConditionBase | | フィルタ条件（後述の型のいずれか） |
| `LimitCount` | int? | null | 取得件数の上限。**整数のみ（`50.0` は不可、`50` と書くこと）** |
| `SelectFields` | List\<string\> | `[]` | 取得するフィールドのリスト（空で全フィールド） |
| `SortConditions` | List\<SortCondition\> | `[]` | ソート条件のリスト |
| `SortFieldVariable` | string | `""` | ソートに使用するフィールド変数パス（旧方式） |
| `SortDescending` | bool | `false` | 降順ソート（旧方式） |

### SortCondition

| プロパティ | 型 | 説明 |
|---|---|---|
| `Variable` | string | ソート対象の変数パス（例: `"Name.Value"`, `"CreatedDate.Value"`） |
| `IsDescending` | bool | `true` で降順、`false` で昇順 |

### JSON例

```json
{
  "LimitCount": 50,
  "SelectFields": [],
  "SortConditions": [
    { "Variable": "CreatedDate.Value", "IsDescending": true },
    { "Variable": "Name.Value", "IsDescending": false }
  ],
  "ModuleName": "Product",
  "Condition": {
    "IsOrMatch": false,
    "IsNot": false,
    "Children": [],
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
  }
}
```

---

## MatchConditionBase の型

条件は `TypeFullName` で型を識別するポリモーフィック構造。以下の4種類がある。

### 1. MultiMatchCondition - 複合条件（AND/OR）

複数の条件を AND または OR で結合する。

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition`

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `IsOrMatch` | bool | `false` | `true` で OR結合、`false` で AND結合 |
| `IsNot` | bool | `false` | `true` で条件全体を否定（NOT） |
| `Children` | List\<MatchConditionBase\> | `[]` | 子条件のリスト |

```json
{
  "IsOrMatch": false,
  "IsNot": false,
  "Children": [
    { /* 子条件1 */ },
    { /* 子条件2 */ }
  ],
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
}
```

### 2. FieldValueMatchCondition - フィールドと固定値の比較

フィールドの値を固定値と比較する。

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition`

| プロパティ | 型 | 説明 |
|---|---|---|
| `SearchTargetVariable` | string | 検索対象の変数パス（例: `"Status.Value"`） |
| `Comparison` | MatchComparison | 比較演算子 |
| `Value` | MultiTypeValue | 比較する固定値 |

```json
{
  "SearchTargetVariable": "Status.Value",
  "Comparison": "Equal",
  "Value": {
    "Value": "Active"
  },
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
}
```

#### MultiTypeValue 構造

```json
{
  "Value": "文字列値"
}
```

または詳細形式:

```json
{
  "String": "テキスト値",
  "Decimal": 100,
  "Bool": true,
  "DateOnly": "2025-01-01",
  "TimeOnly": "09:00:00",
  "DateTime": "2025-01-01T00:00:00",
  "Binary": null
}
```

### 3. FieldVariableMatchCondition - フィールド同士の比較

2つのフィールドの値を比較する。親子関係のフィルタリングに最も多用される。

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition`

| プロパティ | 型 | 説明 |
|---|---|---|
| `SearchTargetVariable` | string | 検索対象（子モジュール側）の変数パス |
| `Comparison` | MatchComparison | 比較演算子 |
| `Variable` | string | 比較元（親モジュール側）の変数パス |

```json
{
  "SearchTargetVariable": "OwnerId.Value",
  "Comparison": "Equal",
  "Variable": "Id.Value",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
}
```

### 4. FieldValueMatchConditionNonNull - Null除外付きフィールドと固定値の比較

FieldValueMatchCondition と同じだが、Null値を除外した上で比較する。

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchConditionNonNull`

プロパティは FieldValueMatchCondition と同じ。

---

## MatchComparison 一覧

| 値 | SQL相当 | 説明 |
|---|---|---|
| `Equal` | `=` | 等しい |
| `NotEqual` | `!=` / `<>` | 等しくない |
| `LessThan` | `<` | 未満 |
| `LessThanOrEqual` | `<=` | 以下 |
| `GreaterThan` | `>` | 超過 |
| `GreaterThanOrEqual` | `>=` | 以上 |
| `Like` | `LIKE` | パターン一致 |
| `In` | `IN` | リスト内に存在 |
| `NotIn` | `NOT IN` | リスト内に存在しない |
| `Exists` | `EXISTS` | 存在する（非null） |
| `NotExists` | `NOT EXISTS` | 存在しない（null） |

---

## Variable パスの書式

変数パスは `"フィールド名.Value"` 形式で記述する。

| パス例 | 説明 |
|---|---|
| `Id.Value` | Id フィールドの値 |
| `Name.Value` | Name フィールドの値 |
| `Status.Value` | Status フィールドの値 |
| `CreatedDate.Value` | CreatedDate フィールドの値 |
| `CategoryId.Value` | LinkField の外部キー値（DbColumn に保存されている値） |
| `CategoryId.Name.Value` | LinkField のリンク先モジュールの Name フィールド値 |

### LinkField の変数パスに関する注意

LinkField の外部キー値を参照する場合は **`LinkFieldName.Value`** を使う。

```
✅ 正しい: ExpenseReportId.Value      （LinkField 自体の値 = 外部キー）
❌ 誤り:   ExpenseReportId.Id.Value   （存在しないパス）
```

`LinkFieldName.ReferenceFieldName.Value` の形式は、リンク先モジュールの **別のフィールド** にアクセスする場合にのみ使用する（例: `CategoryId.Name.Value` でリンク先の Name を取得）。

---

## FieldMatchCondition ラッパー

実際のJSONでは、FieldValueMatchCondition や FieldVariableMatchCondition が `FieldMatchCondition` でラップされている場合がある。

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Match.FieldMatchCondition`

```json
{
  "Condition": {
    "SearchTargetVariable": "Status.Value",
    "Comparison": "Equal",
    "Value": { "Value": "Active" },
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
  },
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldMatchCondition"
}
```

---

## 実用例

### 親子関係のフィルタ（見積 → 見積明細）

見積モジュールの ListField で、自分の Id に紐づく明細を表示する。

```json
{
  "LimitCount": null,
  "SortFieldVariable": "SortIndex.Value",
  "ModuleName": "QuotationDetail",
  "Condition": {
    "IsOrMatch": false,
    "Children": [
      {
        "Condition": {
          "SearchTargetVariable": "OwnerId.Value",
          "Comparison": "Equal",
          "Variable": "Id.Value",
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
        },
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldMatchCondition"
      }
    ],
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
  }
}
```

### 固定値による等値フィルタ

特定の倉庫コードが "003" のデータのみを表示する。

```json
{
  "LimitCount": null,
  "ModuleName": "Car",
  "Condition": {
    "IsOrMatch": false,
    "Children": [
      {
        "Condition": {
          "SearchTargetVariable": "Storehouse.Value",
          "Comparison": "Equal",
          "Value": {
            "String": "003",
            "Decimal": null,
            "Bool": null,
            "DateOnly": null,
            "TimeOnly": null,
            "DateTime": null
          },
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
        },
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldMatchCondition"
      }
    ],
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
  }
}
```

### OR条件の組み合わせ

ステータスが "Active" または "Pending" のデータを取得する。

```json
{
  "ModuleName": "Task",
  "Condition": {
    "IsOrMatch": true,
    "IsNot": false,
    "Children": [
      {
        "SearchTargetVariable": "Status.Value",
        "Comparison": "Equal",
        "Value": { "Value": "Active" },
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
      },
      {
        "SearchTargetVariable": "Status.Value",
        "Comparison": "Equal",
        "Value": { "Value": "Pending" },
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
      }
    ],
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
  }
}
```

### 範囲フィルタ

価格が 1000 以上 5000 以下のデータを取得する。

```json
{
  "ModuleName": "Product",
  "Condition": {
    "IsOrMatch": false,
    "IsNot": false,
    "Children": [
      {
        "SearchTargetVariable": "Price.Value",
        "Comparison": "GreaterThanOrEqual",
        "Value": { "Value": "1000" },
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
      },
      {
        "SearchTargetVariable": "Price.Value",
        "Comparison": "LessThanOrEqual",
        "Value": { "Value": "5000" },
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
      }
    ],
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
  }
}
```

### 条件なし（全件取得）

```json
{
  "ModuleName": "Category",
  "Condition": {
    "IsOrMatch": false,
    "IsNot": false,
    "Children": [],
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
  }
}
```
