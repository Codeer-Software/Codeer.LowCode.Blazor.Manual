# DetailListField - 明細リストフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.DetailListFieldDesign`

インライン編集可能な子レコードの一覧。ListField がテーブル形式（ListLayout）で表示するのに対し、DetailListField は各行を DetailLayout（フォーム形式）で表示する。マスタ-ディテール形式のフォームで子レコードをインライン編集する場合に使用する。

> 行となる「行モジュール」を別途定義し、`SearchCondition.ModuleName` で参照する構成パターン（ListField/TileListField と行モジュールを共有する方法、デモデータの作り方）は [../RowModulePattern.md](../RowModulePattern.md) を参照。

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。
> 一覧フィールド共通プロパティ（DisplayName, SearchCondition, PagerPosition, UseIndexSort, DeleteTogether, CanCreate, CanUpdate, CanDelete, CanUserSort, CanSelect, OnDataChanged, OnSearchDataChanged, OnSelectedIndexChanged, OnSelectedIndexChanging, OnDoubleClickRow）は [_FieldCommon.md](_FieldCommon.md) の「ListFieldDesignBase」を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `LayoutName` | string | `""` | 使用するDetailLayout名。空の場合はデフォルトレイアウト。対象モジュール（`SearchCondition.ModuleName`）のDetailLayoutを指定する。各行がこのレイアウトでフォーム表示される。 |

## ListField との違い

| 特徴 | ListField | DetailListField |
|---|---|---|
| レイアウト種別 | ListLayout（テーブル列形式） | DetailLayout（フォーム形式） |
| 行の表示 | 列ヘッダー付きの行として表示 | フォームレイアウトで表示 |
| 主な用途 | 一覧表示、行クリックで詳細遷移 | インライン編集、マスタ-ディテール |
| 詳細遷移 | `CanNavigateToDetail` で対応 | なし（行自体が編集フォーム） |
| ModuleLayoutType | List | Detail |

## JSON例

### 基本的な明細リスト（注文明細のインライン編集）

```json
{
  "LayoutName": "",
  "DisplayName": "注文明細",
  "SearchCondition": {
    "LimitCount": 0,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "OrderDetail",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [
        {
          "SearchTargetVariable": "OrderId.Value",
          "Comparison": "Equal",
          "Variable": "Id.Value",
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
        }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "PagerPosition": "Top",
  "UseIndexSort": false,
  "DeleteTogether": true,
  "CanCreate": true,
  "CanUpdate": true,
  "CanDelete": true,
  "CanUserSort": true,
  "CanSelect": false,
  "OnDataChanged": "",
  "OnSearchDataChanged": "",
  "OnSelectedIndexChanged": "",
  "OnSelectedIndexChanging": "",
  "OnDoubleClickRow": "",
  "IgnoreModification": false,
  "Name": "Details",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DetailListFieldDesign"
}
```

### 読み取り専用の明細表示

```json
{
  "LayoutName": "ReadOnlyDetail",
  "DisplayName": "履歴",
  "SearchCondition": {
    "LimitCount": 10,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "ChangeHistory",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [
        {
          "SearchTargetVariable": "ParentId.Value",
          "Comparison": "Equal",
          "Variable": "Id.Value",
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
        }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "PagerPosition": "Top",
  "UseIndexSort": false,
  "DeleteTogether": false,
  "CanCreate": false,
  "CanUpdate": false,
  "CanDelete": false,
  "CanUserSort": true,
  "CanSelect": false,
  "OnDataChanged": "",
  "OnSearchDataChanged": "",
  "OnSelectedIndexChanged": "",
  "OnSelectedIndexChanging": "",
  "OnDoubleClickRow": "",
  "IgnoreModification": false,
  "Name": "HistoryList",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DetailListFieldDesign"
}
```

## ランタイム動作

- 各行が `DetailLayout` のフォーム形式でレンダリングされる。`ModuleLayoutType.Detail` として扱われる。
- `SearchCondition` に基づいて対象モジュールからデータを取得する。親子関係の場合、`FieldVariableMatchCondition` で親のIDと子の外部キーを関連付ける。
- `DeleteTogether = true` の場合、親レコード削除時に子レコードも一括削除される（カスケード削除）。マスタ-ディテール関係で一般的に使用。
- `CanCreate = true` で行の新規追加、`CanUpdate = true` で行の編集、`CanDelete = true` で行の削除が可能。
- ページネーション、ソートは `ListFieldDesignBase` の共通機能として対応。

## 検索

- `Exists` / `NotExists` 比較演算子で、子レコードの存在/非存在を検索条件にできる。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `SelectedIndex` | int | 読み書き | 選択中の行インデックス |
| `Rows` | List\<Module\> | 読み取り | 現在のページの行リスト |
| `RowCount` | int | 読み取り | 現在の行数 |
| `AllowLoad` | bool | 読み書き | データ読み込みを許可するか |

### メソッド

| メソッド | 説明 |
|---|---|
| `Reload()` | データを再読み込み |
| `AddRow()` | 空の行を追加 |
| `AddRow(Module src)` | モジュールから行を追加 |
| `DeleteRow(Module row)` | 行を削除 |
| `DeleteAllRows()` | 全行削除 |
| `SetAdditionalCondition(ModuleSearcher searcher)` | 追加検索条件を設定 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 明細リスト

```html
<div class="detail-container">
  <!-- 操作ボタン行 -->
  <div class="detail-controls mb-2">
    <button class="btn btn-sm btn-outline-primary" data-system="create">追加</button>
    <button class="btn btn-sm btn-outline-danger" data-system="delete">削除</button>
  </div>

  <!-- 各明細行 -->
  <div class="detail-row [selected]">
    <!-- GridLayoutRenderer で各行のレイアウトを描画 -->
    <div data-layout="grid">
      <!-- ... GridLayoutDesign の構造 ... -->
    </div>
  </div>
</div>
```

### CSSセレクタ例

```css
/* 明細コンテナ */
[data-name="Details"] .detail-container {
  border: 1px solid #dee2e6;
  border-radius: 0.25rem;
  padding: 1rem;
}

/* 各明細行 */
[data-name="Details"] .detail-row {
  border-bottom: 1px solid #eee;
  padding: 0.5rem 0;
}

/* 選択中の行 */
[data-name="Details"] .detail-row.selected {
  background-color: #e8f4ff;
}

/* 操作ボタン */
[data-name="Details"] .detail-controls .btn {
  margin-right: 0.5rem;
}
```
