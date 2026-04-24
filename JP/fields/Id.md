# IdField

## これは何か

**データを一意に識別するキー**となるフィールド。Module には System Field として `Id` が 1 つ必要です（追加・更新・削除の対象となるデータを特定するため）。

<img src="images/Id表示.png" alt="Id表示" style="border: 1px solid;">

## いつ使うか

- System Field として: Module のデータの主キー（**追加・更新・削除をするなら必須**）
- 一般 Field として: 別テーブルの Id を保持する外部キー的な用途

System Field の Id と一般 Field の Id の違いは本ページ下部の「[System Field の Id と一般 Field の Id](#system-field-の-id-と一般-field-の-id)」を参照してください。

---

## 3 つの生成方式

Id の値をどう決めるかで 3 つの方式があります。

| 方式 | 設定 | 用途 |
|---|---|---|
| **DB 自動生成** | `IsManualInput: false` + DB 側で AUTO_INCREMENT / SERIAL / IDENTITY | 連番の ID |
| **ユーザー入力** | `IsManualInput: true` | 見積番号・商品コードなど、人間が決める ID |
| **複合 Id** | `CompositeIdVariables` + `CompositeIdSeparator` | 複数 Field を結合した ID（例: 姓 + 名） |

---

## デザイナでの設定

<img src="images/Id設定.png" alt="Id設定" style="border: 1px solid;">

### 固有プロパティ

| プロパティ | 型 | 既定値 | 説明 |
|---|---|---|---|
| **DbColumn** | string | `""` | 対応する DB 列名 |
| **IsManualInput** | bool | `false` | ユーザー入力にする（オフなら DB 自動生成を期待） |
| **Placeholder** | string | `""` | 未入力時の案内文 |
| **CompositeIdVariables** | List\<string\> | `[]` | 複合 Id を構成する他 Field 名 |
| **CompositeIdSeparator** | string | `""` | 複合 Id の区切り文字 |
| **SearchComparisonDefaultValue** | MatchComparison? | null | 検索の既定比較（`Equal` / `Like`） |

共通プロパティ（Name, DisplayName, OnDataChanged など）は [Field 共通プロパティ](common_properties.md) を参照。

> **注意**: System Field の Id は **Name を "Id" から変更できません**。DB 列の名前が異なる場合は `DbColumn` プロパティで調整してください。

<img src="images/Id詳細.png" alt="Id詳細" style="border: 1px solid;">

---

## スクリプトから

### プロパティ

| 名前 | 型 | 説明 |
|---|---|---|
| `Value` | string? | Id の値 |
| `SearchValue` | string? | 検索値 |
| `SearchComparison` | MatchComparison | 検索比較（`Equal` / `Like`） |

共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

### よく使う例

```csharp
// Id 検索条件を動的に設定
await this.Id.SetSearchValueAsync("CUS-0001");
await this.Id.SetSearchComparisonAsync(MatchComparison.Like);

// ModuleSearcher で Id を条件にする
var searcher = new ModuleSearcher<Customer>();
searcher.AddEquals(c => c.Id.Value, selectedId);
var result = searcher.Execute();
```

---

## System Field の Id と一般 Field の Id

Id には 2 種類があります。

### System Field の Id

- Module の**主キー**。追加・更新・削除の際にこの値でデータを特定する
- Name は `"Id"` で固定（変更不可）
- **1 Module に必ず 1 つ必要**（DB 操作をする場合）

### 一般 Field の Id

- 主キーではなく、**別テーブルの Id を保持する用途**
- Name は自由に変更可能（例: `CustomerId`, `AuthorId`）
- 他モジュールと紐付けるための外部キーとして使う

### 具体例

`見積書` モジュールの場合:

| Field 名 | 種類 | 用途 |
|---|---|---|
| `Id` | System Field | 見積書を特定する主キー |
| `CustomerId` | 一般 Field（Id 型） | どの顧客の見積書かを指す外部キー |

`CustomerId` は `顧客` モジュールと紐づけるためのキーで、主キーではありません。

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [Field 全体](field.md) — System Field の一覧
- [Link](Link.md) — 他モジュールの Id を選択する GUI を提供
- [チュートリアル: はじめてのモジュール作成](../quickstart/first_module.md)
