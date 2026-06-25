# 検索条件 (SearchCondition / MatchCondition)

モジュール間のデータ関連付け、検索フィルタ、アクセス制御に使用される条件定義のリファレンス。

---

## C# クラス定義 (真実の源)

ソースコード `Source/Codeer.LowCode.Blazor/Repository/Match/*.cs` および `Source/Codeer.LowCode.Blazor/Repository/MultiTypeValue.cs` から転記。JSON のスキーマの **唯一の真実**。`MatchConditionBase` 派生と `MultiTypeValue` 派生は `JsonAbstract` の polymorphic デシリアライズ対象なので、`TypeFullName` を **必ず** 書く。

```csharp
public class ModuleMatchCondition
{
    public string ModuleName { get; set; } = string.Empty;
    public MatchConditionBase? Condition { get; set; }    // null 可、または MatchConditionBase 派生 (要 TypeFullName)
}

public class SortCondition
{
    public string Variable { get; set; } = string.Empty;
    public bool IsDescending { get; set; }
}

public class SearchCondition : ModuleMatchCondition
{
    public int? LimitCount { get; set; }
    public List<string> SelectFields { get; set; } = new();
    public List<SortCondition> SortConditions { get; set; } = new();
    [Obsolete] public string SortFieldVariable { get; set; } = string.Empty;
    [Obsolete] public bool SortDescending { get; set; }
    // 親 ModuleMatchCondition から: ModuleName, Condition
}

// --- MatchCondition (polymorphic, TypeFullName 必須) ---
public abstract class MatchConditionBase : JsonAbstract { }

public class MultiMatchCondition : MatchConditionBase
{
    // TypeFullName: Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition
    public bool IsOrMatch { get; set; }
    public bool IsNot { get; set; }
    public List<MatchConditionBase> Children { get; set; } = new();   // 子も TypeFullName 必須
    public string Name { get; set; } = string.Empty;
}

public class FieldVariableMatchCondition : MatchConditionBase, IFieldMatchCondition
{
    // TypeFullName: Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition
    public string SearchTargetVariable { get; set; } = string.Empty;
    public MatchComparison Comparison { get; set; }   // enum: Equal/NotEqual/LessThan/.../Like/In/NotIn/Exists/NotExists
    public string Variable { get; set; } = string.Empty;
}

public class FieldValueMatchCondition : MatchConditionBase, IFieldMatchCondition
{
    // TypeFullName: Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition
    public string SearchTargetVariable { get; set; } = string.Empty;
    public MatchComparison Comparison { get; set; }
    public MultiTypeValue Value { get; set; } = new NullValue();   // ★ Value も TypeFullName 必須 (下記参照)
}

public class FieldValueMatchConditionNonNull : FieldValueMatchCondition
{
    // TypeFullName: Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchConditionNonNull
}

public class FieldMatchCondition : MultiMatchCondition
{
    // TypeFullName: Codeer.LowCode.Blazor.Repository.Match.FieldMatchCondition
    public string FieldName { get; set; } = string.Empty;
}

// --- MultiTypeValue (polymorphic、FieldValueMatchCondition.Value 等で使う、TypeFullName 必須) ---
public abstract class MultiTypeValue : JsonAbstract { }

public class NullValue        : MultiTypeValue { }                          // .NullValue
public class StringValue      : MultiTypeValue { public string?   Value { get; set; } }   // .StringValue
public class DecimalValue     : MultiTypeValue { public decimal?  Value { get; set; } }   // .DecimalValue (整数も decimal で格納)
public class BooleanValue     : MultiTypeValue { public bool?     Value { get; set; } }   // .BooleanValue
public class DateOnlyValue    : MultiTypeValue { public DateOnly? Value { get; set; } }   // .DateOnlyValue
public class TimeOnlyValue    : MultiTypeValue { public TimeOnly? Value { get; set; } }   // .TimeOnlyValue
public class DateTimeValue    : MultiTypeValue { public DateTime? Value { get; set; } }   // .DateTimeValue
public class BinaryValue      : MultiTypeValue { public byte[]?   Value { get; set; } }   // .BinaryValue
public class EnumValue<T>     : MultiTypeValue { public T?        Value { get; set; } }   // .EnumValue`1[[T...]]
public class ListValue<T>     : MultiTypeValue { public List<T>?  Value { get; set; } }   // .ListValue`1[[T...]]
```

> **注意 (Claude 向け)**:
> - `MatchConditionBase` 派生 (= `Condition` プロパティや `Children` 配列要素) は **必ず** `TypeFullName` を含めること。書き忘れるとデシリアライズ失敗。
> - `FieldValueMatchCondition.Value` は `MultiTypeValue` 派生で、これも **必ず** `TypeFullName` を含める。例: `"Value": {"Value": "AH", "TypeFullName": "Codeer.LowCode.Blazor.Repository.StringValue"}`。
> - 数値は整数も `DecimalValue` を使う (`IntValue` のような型は無い)。
> - `Children` は `List<MatchConditionBase>` = JSON 配列。`Condition` 単体は object。

## SearchCondition

データ取得全体の条件を定義する。**同じ `SearchCondition` 型を、抽出対象を持つフィールドが共通で使う**:

- `ListField` / `DetailListField` / `TileListField` … 一覧に出す子データの抽出条件
- `LinkField` … 選択候補にする参照先データの抽出条件
- `SelectField`（候補をモジュールから引く場合）… 候補データの抽出条件

書き方（`ModuleName` + `Condition` + 各 MatchCondition の構造）はどのフィールドでも全く同じ。
以降の説明・例は ListField を題材にしているが、Link/Select でもそのまま当てはまる。

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

#### 現在のログインユーザーで絞り込む（データ権限の行レベルセキュリティ）

`Variable` に **`CurrentUser.<フィールド名>.Value`** を指定すると、**現在ログインしているユーザー**の値を参照できる。
データ権限（DataReadCondition / DataWriteCondition）で「自分が作成したデータだけ読める/書ける」のような行レベルセキュリティを実現する定番。

- 現在ユーザーの主キー（Id）は `CurrentUser.Id.Value`。AppUser モジュールの任意フィールドも `CurrentUser.<フィールド名>.Value` で参照可。
- **誤り例**: 「作成者＝ログインユーザー」を `Variable: "Creator.Value"`（＝自分自身と比較）にしてはいけない。比較相手の現在ユーザーは必ず `CurrentUser....` で表す。

```jsonc
// 例: DataReadCondition「作成者(Creator)が現在のログインユーザーのデータだけ表示」
{
  "SearchTargetVariable": "Creator.Value",
  "Comparison": "Equal",
  "Variable": "CurrentUser.Id.Value",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
}
```

(ユーザー権限 UserReadCondition / UserWriteCondition でロール等を見る場合は、`FieldValueMatchConditionNonNull` で現在ユーザーのフィールドを固定値と比較する。例: `SearchTargetVariable: "Role.Value"` == `"admin"`。)

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

実際のJSONでは、FieldValueMatchCondition や FieldVariableMatchCondition が `FieldMatchCondition` でラップされていることがある（デザイナUIが生成する形）。

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Match.FieldMatchCondition`

> **重要 (Claude 向け・よくある誤り)**:
> `FieldMatchCondition` は `MultiMatchCondition` のサブクラス。つまり中身は **`Children` 配列**に入れる。
> **`Condition` という単体プロパティは存在しない**。`"Condition": {...}` と書くと、その中身は未知プロパティとして
> **デシリアライズ時に黙って捨てられ、空の条件になる**（＝フィルタが効かないのに「設定した」ように見える）。
> 実際の構造は次のとおり（`Condition` ではなく `Children` に入れる）:

```json
{
  "IsOrMatch": false,
  "IsNot": false,
  "FieldName": "",
  "Children": [
    {
      "SearchTargetVariable": "Status.Value",
      "Comparison": "Equal",
      "Value": { "Value": "Active", "TypeFullName": "Codeer.LowCode.Blazor.StringValue" },
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
    }
  ],
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldMatchCondition"
}
```

なお、`FieldMatchCondition` ラッパーは必須ではない。`FieldValueMatchCondition` / `FieldVariableMatchCondition` を
親の `MultiMatchCondition.Children` に**直接**入れてもよい（下の OR条件例がその形）。迷ったら直接入れる方が安全。

---

## 実用例

### 親子関係（ヘッダ明細）のフィルタ ★最重要パターン

ヘッダ(親)モジュールの ListField / DetailListField で、**自分の Id に紐づく明細(子)だけ**を表示する。
ヘッダ明細を作るときに必ず使うパターン。ポイントは2つ：

1. **`FieldVariableMatchCondition` を使う**（固定値比較の `FieldValueMatchCondition` ではない）。
   親子の突き合わせは「子のフィールド = 親のフィールド」という**フィールド同士の比較**なので、
   値型の `Value` ではなく `Variable`（比較元の変数パス）を使う。
2. `SearchTargetVariable` = **子モジュール側**のフィールド（親への外部キー。例: 子が持つ LinkField `Invoice.Value`）、
   `Variable` = **親(=このモジュール)側**のフィールド（通常 `Id.Value`）。

下記は見積モジュールの ListField で、子 `QuotationDetail` の `OwnerId` が自分の `Id` と一致するものを表示する例
（実在モジュールから転記した**そのまま動く形**）:

```json
{
  "LimitCount": null,
  "SortFieldVariable": "SortIndex.Value",
  "ModuleName": "QuotationDetail",
  "Condition": {
    "IsOrMatch": false,
    "IsNot": false,
    "Children": [
      {
        "IsOrMatch": false,
        "IsNot": false,
        "FieldName": "",
        "Children": [
          {
            "SearchTargetVariable": "OwnerId.Value",
            "Comparison": "Equal",
            "Variable": "Id.Value",
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
          }
        ],
        "Name": "",
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldMatchCondition"
      }
    ],
    "Name": "",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
  }
}
```

ラッパーを省いて `FieldVariableMatchCondition` を `MultiMatchCondition.Children` に直接入れた、より簡潔な等価形:

```json
{
  "ModuleName": "QuotationDetail",
  "Condition": {
    "IsOrMatch": false,
    "IsNot": false,
    "Children": [
      {
        "SearchTargetVariable": "OwnerId.Value",
        "Comparison": "Equal",
        "Variable": "Id.Value",
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
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
