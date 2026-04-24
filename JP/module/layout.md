# レイアウト

Module の詳細・一覧・検索の各画面は、レイアウトの中に Field を配置して作ります。
Codeer.LowCode.Blazor は以下のレイアウトをサポートしています。

| レイアウト | 特徴 | 用途 |
|---|---|---|
| **Grid** | 行×列のグリッドに配置 | 業務アプリの入力画面の基本 |
| **Canvas** | 自由な座標に配置 | ダッシュボード・帳票風画面 |
| **Tab** | 複数の内容をタブで切り替え | 画面が長くなる時の分割 |
| **Flow**（Grid.IsFlowLayout） | 左から右へ順に並べて折り返す | タグ・アイコン列 |

Module の Root 要素は Grid レイアウトです。多くの場合 **Grid を基本**にして、必要に応じて Canvas・Tab・Flow を組み合わせます。

---

## Grid レイアウト

行と列のグリッドに要素を配置します。入れ子が可能で、セルの中にさらに Grid や Canvas を入れられます。

<img src="images/layout/grid_design.png" alt="grid_design" width="400" style="border: 1px solid;">
<img src="images/layout/grid.png" alt="grid" width="400" style="border: 1px solid;">

### 列幅の決定ルール

列幅は以下の優先度で決まります:

1. **IsAutoFillWrap 行** — 列幅は CSS Grid が制御（カラム個別指定は無効）
2. **Width 指定**（固定幅） — カラムは正確に指定 px
3. **MinWidth 指定** — `flex: 1` で均等に伸び、最小幅を保証
4. **Width / MinWidth 指定なし** — `flex: 1` で均等に伸びる

| プロパティ | 用途 |
|---|---|
| **Width** | 固定幅（px） |
| **MinWidth** | 最小幅。不足時に折り返す |
| **MaxWidth** | 最大幅（MinWidth と併用） |
| **IsAutoFillWrap** | 折り返し時に自動で均等割り（MinWidth 必須、MaxWidth は無効） |

### 標準のマージン・パディング

Grid は標準でいくつかのマージンやパディングを含んでいます。要素間に適切なスペースが確保されるようになっています。

- **Grid** — 通常は無し。`IsBordered` オン時は内側に `1rem` のパディング
- **Row** — 下部に `1rem` のマージン
- **Column** — 左右に `0.75rem` のパディング（Grid / Canvas を配置した場合は条件付きで適用なし）

`app.css` から上書き可能:

```css
/* IsBordered な Grid のパディングを上書き */
div.grid-bordered {
  --default-padding-top: 20px;
  --default-padding-right: 40px;
  --default-padding-bottom: 80px;
  --default-padding-left: 160px;
}

/* Row の上下マージンを上書き */
div.grid-row {
  --default-margin-top: 20px;
  --default-margin-bottom: 40px;
}

/* Column の左右パディングを上書き */
div.grid-column {
  --default-padding-left: 20px;
  --default-padding-right: 40px;
}
```

### Grid プロパティ

| プロパティ | 説明 |
|---|---|
| **Name** | 識別子 |
| **IsViewOnly** | 読み取り専用 |
| **IsBordered** | 枠（カード）として描画 |
| **IsFlowLayout** | 左→右に並べて折り返すフローモード |
| **IsFillAvailable** | ページ末尾で空き領域を埋める |
| **IsExpandable** | 開閉可能にする |
| **ExpanderLabel** | 開閉時のラベル |
| **IsExpanderDefaultOpened** | 初期状態で開くか |
| **BackgroundColor** / **Color** | 色設定 |

### Row プロパティ

| プロパティ | 説明 |
|---|---|
| **IsWrap** | 自動折り返し |
| **Height** | 行の高さ |
| **IsRowMarginRemoved** | 行マージンの削除 |
| **FillAvailable** | 空き領域を埋める高さまで広げる（末尾 Row で有効） |
| **Margin** | マージンの上書き |

### Column プロパティ

| プロパティ | 説明 |
|---|---|
| **Width** | 固定幅（px） |
| **MinWidth** / **MaxWidth** | 最小・最大幅 |
| **BackgroundColor** | 背景色 |
| **Border** | 罫線 |
| **HorizontalAlignment** | 水平位置（`start` / `center` / `end` / `stretch`） |
| **VerticalAlignment** | 垂直位置（`top` / `middle` / `end` / `stretch`） |
| **CanResize** | ユーザーによるリサイズ許可 |
| **Padding** | パディングの上書き |

---

## Canvas レイアウト

自由な座標に要素を配置できます。ドラッグ＆ドロップでサイズと位置を決めます。

<img src="images/layout/canvas_design.png" alt="canvas_design" width="400" style="border: 1px solid;">
<img src="images/layout/canvas.png" alt="canvas" width="400" style="border: 1px solid;">

### Canvas プロパティ

| プロパティ | 説明 |
|---|---|
| **IsViewOnly** | 読み取り専用 |
| **IsBordered** | 枠を描画 |

### Element（Canvas 上の各要素）のプロパティ

| プロパティ | 説明 |
|---|---|
| **Width** / **Height** | サイズ |
| **ZIndex** | 重なり順 |

---

## Tab レイアウト

複数のグループをタブで切り替え表示します。情報量が多い画面を整理するのに便利です。

### 使い方

- Tab レイアウトを配置
- 各タブに表示するコンテンツ（Grid 等）を配置
- タブヘッダーのラベルを設定

---

## Flow モード（Grid.IsFlowLayout）

Grid の `IsFlowLayout` をオンにすると、**行・列の構造を無視して横一列に並べ、右端で折り返す**モードになります。

<img src="images/layout/flow_design.png" alt="flow_design" width="400" style="border: 1px solid;">
<img src="images/layout/flow.png" alt="flow" width="400" style="border: 1px solid;">

タグ・アイコン列・ボタンバーなどに向きます。

---

## FillAvailable（残領域に広げる）

Row の `FillAvailable` をオンにすると、**その Row が Grid の末尾である場合**に、ページの残り領域を埋める高さまで広がります。
表（ListField）をページ全体の高さで表示したい時などに使います。

<img src="images/layout/FillAvailable_design.png" alt="FillAvailable_design" width="400" style="border: 1px solid;">
<img src="images/layout/FillAvailable.png" alt="FillAvailable" width="400" style="border: 1px solid;">

---

## Field のレイアウト個別プロパティ

レイアウトに配置した Field は、Field 本来のプロパティ以外に、レイアウト個別のプロパティも持ちます。

| プロパティ | 説明 |
|---|---|
| **IsViewOnly** | 読み取り専用 |
| **FontFamily** / **FontSize** / **FontWeight** / **FontStyle** | フォント指定 |
| **Color** | 文字色 |

---

## デザイナ上の表示との差異

デザイナ上の表示は、ブラウザ上の表示と完全には一致しません。
最終的な表示を確認するには、デプロイ後に Web ブラウザで確認してください。

---

## スクリプトから

### レイアウト共通

| プロパティ | 型 | 説明 |
|---|---|---|
| `Name` | string | レイアウト名 |
| `LayoutName` | string | （同上） |
| `ModuleLayoutType` | ModuleLayoutType | `Detail` / `List` / `Search` |
| `IsEnabled` | bool | 全体の有効・無効 |
| `IsVisible` | bool | 表示・非表示 |
| `IsViewOnly` | bool | 読み取り専用 |
| `IsExpanded` | bool | 開閉状態（Expandable Grid） |

### SearchLayout 固有

| プロパティ | 型 | 説明 |
|---|---|---|
| `IsOrMatch` | bool? | Or 検索かどうか |

### よく使う例

```csharp
// 初期化前にリストのロードを止める
void DetailLayoutDesign_OnBeforeInitialization()
{
    ListCookingStep.AllowLoad = false;
}

// 条件に応じてセクションを隠す
AdvancedGrid.IsVisible = IsAdmin.Value;
```

---

## 動画ガイド

- [レイアウトガイド - 概要編](https://www.youtube.com/watch?v=DepPNToMjGE)
- [レイアウトガイド - Grid 編 基本的な使い方](https://www.youtube.com/watch?v=Y7a9al6Wk3Y)

※レイアウトシリーズの動画は順次追加予定

---

## 関連項目

- [Module 概要](module.md)
- [詳細設定](module_detail.md) / [一覧設定](module_list.md) / [検索設定](module_search.md)
- [Document Outline と Property パネル](DocumentOutline.md)
- [Field 一覧](../fields/field.md)
