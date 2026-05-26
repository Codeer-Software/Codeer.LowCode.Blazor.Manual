# TileListField - タイル/カード形式の一覧表示

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.TileListFieldDesign`

レコードをタイル（カード）形式で一覧表示するフィールド。テーブル行の代わりに、各レコードを DetailLayout を使ったカードとしてグリッド状に配置する。商品カタログやギャラリーなど、ビジュアル重視の一覧に適している。`ListFieldDesignBase` を継承する。

> 行となる「行モジュール」を別途定義し、`SearchCondition.ModuleName` で参照する構成パターン（ListField/DetailListField と行モジュールを共有する方法、デモデータの作り方）は [../RowModulePattern.md](../RowModulePattern.md) を参照。

## C# クラス定義 (真実の源)

```csharp
public class TileListFieldDesign : ListFieldDesignBase, IFillHeightFieldDesign
{
    public override string LayoutName { get; set; } = "";
    public int TileWidth { get; set; } = 200;
    public bool FillSpaces { get; set; }
    // 親 ListFieldDesignBase / FieldDesignBase の全プロパティを継承 (詳細は _FieldCommon.md)
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。
> 一覧フィールド共通プロパティ（DisplayName, SearchCondition, LayoutName, PagerPosition, UseIndexSort, DeleteTogether, CanCreate, CanUpdate, CanDelete, CanUserSort, CanSelect, OnDataChanged, OnSearchDataChanged, OnSelectedIndexChanged, OnSelectedIndexChanging, OnDoubleClickRow）は [_FieldCommon.md](_FieldCommon.md) の ListFieldDesignBase を参照。

### TileListField 固有のプロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `LayoutName` | string | `""` | 各タイル/カードの表示に使用する DetailLayout 名。対象モジュールの DetailLayouts から指定する。空の場合はデフォルトレイアウト。 |
| `TileWidth` | int | `200` | 各タイルの幅（px）。**整数のみ（`200.0` は不可、`200` と書くこと）** |
| `FillSpaces` | bool | `false` | `true` の場合、タイル間の余白を均等に埋めて全幅を使用する。 |

## 重要: タイルとなる Module の DetailLayout はカード化する (絶対常識)

TileListField (および DetailListField) で参照されるタイル単位の Module の `DetailLayouts[""].Layout` は、**最外 Grid を `IsBordered: true` にしてカード化** する。これがないとタイルとタイルの境界が見えず、画面上で1ブロックに見えない。

```json
"DetailLayouts": {
  "": {
    ...
    "Layout": {
      "IsBordered": true,    ← これを必ず true に
      ...
    }
  }
}
```

詳細は [DetailListField.md](DetailListField.md#重要-行となる-module-の-detaillayout-はカード化する-絶対常識) も参照。

## JSON例

### 商品カタログのタイル一覧

```json
{
  "LayoutName": "TileCard",
  "TileWidth": 250,
  "FillSpaces": false,
  "DisplayName": "商品カタログ",
  "SearchCondition": {
    "LimitCount": 24,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Product",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "PagerPosition": "Bottom",
  "UseIndexSort": false,
  "DeleteTogether": false,
  "CanCreate": false,
  "CanUpdate": false,
  "CanDelete": false,
  "CanUserSort": false,
  "CanSelect": false,
  "OnDataChanged": "",
  "OnSearchDataChanged": "",
  "OnSelectedIndexChanged": "",
  "OnSelectedIndexChanging": "",
  "OnDoubleClickRow": "",
  "IgnoreModification": false,
  "Name": "ProductTiles",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TileListFieldDesign"
}
```

### ギャラリー表示（余白を均等配置）

```json
{
  "LayoutName": "GalleryCard",
  "TileWidth": 200,
  "FillSpaces": true,
  "DisplayName": "画像ギャラリー",
  "SearchCondition": {
    "LimitCount": 50,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Photo",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "PagerPosition": "Bottom",
  "UseIndexSort": false,
  "DeleteTogether": false,
  "CanCreate": false,
  "CanUpdate": false,
  "CanDelete": true,
  "CanUserSort": false,
  "CanSelect": true,
  "OnDataChanged": "",
  "OnSearchDataChanged": "",
  "OnSelectedIndexChanged": "Gallery_OnSelectedIndexChanged",
  "OnSelectedIndexChanging": "",
  "OnDoubleClickRow": "",
  "IgnoreModification": false,
  "Name": "Gallery",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TileListFieldDesign"
}
```

## ランタイム動作

- **ListField との違い:** ListField がテーブル形式（列ヘッダー + 行）で表示するのに対し、TileListField は各レコードを DetailLayout で定義されたカードとしてグリッド状に配置する。
- **LayoutName:** 対象モジュールの `DetailLayouts` に定義されたレイアウトを使用して各タイルをレンダリングする。DetailLayout のグリッド構造がそのままカードの内容になる。
- **TileWidth:** 各タイルの横幅をピクセル単位で指定する。コンテナ幅に応じてタイルの列数が自動計算される。
- **FillSpaces:** `true` の場合、タイル間の余白が均等に分配されてコンテナ全幅を使用する。`false` の場合、タイルは左詰めで配置される。
- ListFieldDesignBase の `SearchCondition` や `PagerPosition` 等の一覧共通機能はそのまま使用できる。
- スクリプトイベント（`OnSelectedIndexChanged`, `OnDoubleClickRow` 等）も ListField と同様に動作する。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `SelectedIndex` | int | 読み書き | 選択中の行インデックス |
| `Page` | int | 読み取り | 現在のページ番号 |
| `PageCount` | int | 読み取り | 総ページ数 |
| `TotalCount` | int | 読み取り | 全行数 |
| `Rows` | List\<Module\> | 読み取り | 現在のページの行リスト |
| `RowCount` | int | 読み取り | 現在のページの行数 |
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
| `Paging(int page)` | ページ遷移 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### タイル一覧

```html
<div class="tile-container" style="display: grid; grid-template-columns: repeat([列数], 1fr); gap: [間隔]">
  <div class="tile [can-select] [selected]">
    <!-- GridLayoutRenderer で各タイルのレイアウトを描画 -->
    <div data-layout="grid">
      <!-- ... GridLayoutDesign の構造 ... -->
    </div>
  </div>
</div>
```

### CSSセレクタ例

```css
/* タイルコンテナのギャップ */
[data-name="Products"] .tile-container {
  gap: 1rem;
}

/* 各タイルカード */
[data-name="Products"] .tile {
  border: 1px solid #dee2e6;
  border-radius: 0.5rem;
  padding: 1rem;
  transition: box-shadow 0.2s;
}

[data-name="Products"] .tile:hover {
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

/* 選択中のタイル */
[data-name="Products"] .tile.selected {
  border-color: #0d6efd;
  box-shadow: 0 0 0 2px rgba(13, 110, 253, 0.25);
}
```
