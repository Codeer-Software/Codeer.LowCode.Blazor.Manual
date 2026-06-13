# 行モジュールパターン (Row Module Pattern)

`ListField` / `DetailListField` / `TileListField` で表示される 1 行分のデータ構造は、
**別のモジュール**として定義する。これを「行モジュール (row module)」と呼ぶ。

行モジュールを 1 つ用意しておけば、同じデータを **List / DetailList / TileList の 3 形式
すべてで** 表示できる（同じ行モジュールを複数の親モジュールから参照することも可能）。

## 構成

```
親モジュール (parent)             行モジュール (row)
─────────────────             ─────────────────
ListField     ──┐
DetailListField ─┼──> SearchCondition.ModuleName ──> CardItem
TileListField ──┘                                     ├─ DetailLayouts[""]  ← DetailList/TileList が描画に使用
                                                      └─ ListLayouts[""]    ← ListField が描画に使用
```

### 各 List 系 Field がどのレイアウトを描画に使うか

| Field 種別 | 行の描画に使う行モジュール側のレイアウト |
|---|---|
| `ListFieldDesign` | `ListLayouts[LayoutName]` の `Elements`（テーブルの各列） |
| `DetailListFieldDesign` | `DetailLayouts[LayoutName]` の `Layout`（行ごとに小さなフォーム描画） |
| `TileListFieldDesign` | `DetailLayouts[LayoutName]` の `Layout`（タイルごとに描画） |

→ **行モジュールには `DetailLayouts` と `ListLayouts` の両方を定義しておく**と、
3 形式すべてで再利用可能になる。

### どれを選ぶか (意味ではなく用途で選ぶ)

上の表は「各 Field がどのレイアウトを描くか」という**仕組み**。実際にどれを選ぶかは**子レコードの見せ方**で決める:

| 子レコードの見え方 | 選ぶ Field |
|---|---|
| **列の揃った均一な表** (日付・科目・金額… を 1 行ずつ) ← ヘッダ＋明細はこれ | **`ListField`** |
| 1 レコードが**複雑なフォーム** (項目多・縦組み・行ごとにラベル) | `DetailListField` (子を `IsBordered:true` でカード化) |
| カード／タイルを**グリッド状**に並べる | `TileListField` |

> **「明細だから `DetailListField`」は誤り (最頻出の致命傷)。** `DetailListField` の "Detail" は「各行を DetailLayout (フォーム) で描く」という仕組みの話で、業務の「明細行」ではない。**ヘッダ＋明細の明細表は `ListField`。迷ったら `ListField`。** → [CommonMistakes #53](CommonMistakes.md) / [AppPatterns/header_detail.md](AppPatterns/header_detail.md)

## ポイント

1. **行モジュールも `DbTable=""` で OK**
   - DB を介さないテスト/デモ用のリストを作るときは、行モジュールも親モジュールも
     両方 `DbTable=""` にして、スクリプトの `OnAfterInitialization` で `AddRows` する。
2. **DetailList / TileList の行枠**
   - 行モジュールの `DetailLayouts[""]` の `GridLayoutDesign.IsBordered: true` を推奨
     （[LayoutGuidelines.md](LayoutGuidelines.md) の「DetailListField に入れるモジュールは IsBordered: true にする」参照）。
3. **選択状態のスタイリング**
   - 親モジュール側で `CanSelect: true` を設定すると、各行に `.can-select` クラス、
     選択中の行には `.selected` クラスが付与される。
   - CSS セレクタは [AppCss.md](AppCss.md) の「選択可能行 (.can-select)」セクションを参照
     （`table tr.can-select.selected` / `div.can-select.selected` / `.tile.selected`）。
   - `IsBordered: true` の行は `.card` クラスで描画されるが、`.card` の背景は
     **デフォルトで透過**になっているため、選択状態の青背景がカード越しに見える。

## 完全な例: テスト/デモ用の List/DetailList/TileList を 1 画面に並べる

実例: `Samples/PatternShowcase/App/Modules/` の `ListFieldOverview` / `DetailListFieldOverview` / `TileListFieldOverview`
（行モジュール `LookupCustomer`（List/Tile で共有）/ `DetailListDemoChild`（DetailList 用）を参照する構成）を参照。

### 1. 行モジュール (`CardItem.mod.json`)

```json
{
  "Name": "CardItem",
  "DataSourceName": "",
  "DbTable": "",
  "Fields": [
    { "Name": "Id",          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.IdFieldDesign",     "IsManualInput": true },
    { "Name": "Title",       "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign" },
    { "Name": "Status",      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SelectFieldDesign", "Candidates": ["未着手,1","進行中,2","完了,3"] },
    { "Name": "Description", "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign",   "IsMultiline": true }
  ],
  "DetailLayouts": {
    "": {
      "Layout": {
        "IsBordered": true,
        "Rows": [ /* Id / Title / Status / Description を並べる Grid */ ],
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
      }
    }
  },
  "ListLayouts": {
    "": {
      "Elements": [[
        { "FieldName": "Id" },
        { "FieldName": "Title" },
        { "FieldName": "Status" },
        { "FieldName": "Description" }
      ]]
    }
  }
}
```

- `DbTable=""` なので DB に依存しない。
- `DetailLayouts[""]` は DetailList / TileList が、`ListLayouts[""]` は ListField がそれぞれ使う。
- Id は `IsManualInput: true` にしておくと、スクリプトで値を入れられる（DB 採番に依存しない）。

### 2. 親モジュール (`CardListTest.mod.json`)

```json
{
  "Name": "CardListTest",
  "DataSourceName": "",
  "DbTable": "",
  "Fields": [
    {
      "Name": "List",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign",
      "CanSelect": true,
      "SearchCondition": { "ModuleName": "CardItem", "LimitCount": 50 }
    },
    {
      "Name": "DetailList",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DetailListFieldDesign",
      "CanSelect": true,
      "SearchCondition": { "ModuleName": "CardItem", "LimitCount": 50 }
    },
    {
      "Name": "TileList",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TileListFieldDesign",
      "CanSelect": true,
      "TileWidth": 320,
      "SearchCondition": { "ModuleName": "CardItem", "LimitCount": 50 }
    }
  ],
  "DetailLayouts": {
    "": {
      "OnAfterInitialization": "OnInit",
      "Layout": {
        "Rows": [
          { "Columns": [ { "Layout": { "FieldName": "List",       "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign" } } ] },
          { "Columns": [ { "Layout": { "FieldName": "DetailList", "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign" } } ] },
          { "Columns": [ { "Layout": { "FieldName": "TileList",   "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign" } } ] }
        ],
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
      }
    }
  },
  "ListLayouts": {},
  "SearchLayouts": {}
}
```

- 親モジュールも `DbTable=""`。`ListLayouts` は空 (`{}`) のままにしておく
  （[LayoutGuidelines.md](LayoutGuidelines.md) 「表示専用モジュールの ListLayout にフィールドを入れない」参照）。
- 3 つの List 系 Field がいずれも同じ `ModuleName: "CardItem"` を参照している点が肝。

### 3. デモデータ準備スクリプト (`CardListTest.mod.cs`)

```csharp
void OnInit()
{
    List.AddRows(3);
    foreach (var row in List.Rows) SetRowValues(row);

    DetailList.AddRows(3);
    foreach (var row in DetailList.Rows) SetRowValues(row);

    TileList.AddRows(3);
    foreach (var row in TileList.Rows) SetRowValues(row);
}

void SetRowValues(Module row)
{
    row.Id.Value = "ID-001";
    row.Title.Value = "サンプルタイトル";
    row.Status.Value = "2";
    row.Description.Value = "これは説明文のサンプルです。";
}
```

- `OnAfterInitialization` から呼ばれる `OnInit` で `AddRows(int)` を使い空行を挿入し、
  各 `row` (Module) に対して値をセットする。
- `SetRowValues(Module row)` で行モジュールのフィールドにアクセスする際、
  C# の `dynamic` のようにフィールド名で直接プロパティアクセスできる。

## 用途

- レイアウト/CSS の目視確認用テストハーネス（DB 不要）
- スクリプト動作の試験
- デザイナー上で複数表示形式を一度に比較したいとき
- 実 DB を用意する前のプロトタイプ
