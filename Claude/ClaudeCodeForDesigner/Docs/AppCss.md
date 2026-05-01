# カスタムCSS (app.css)

デザイナフォルダのルートに `app.css` を配置すると、アプリケーション全体にカスタムCSSが適用される。
レイアウトやフィールドの `ClassName` プロパティと組み合わせることで、柔軟なスタイリングが可能。

---

## 1. app.css の配置と読み込み

### ファイル配置

```
App/
├── app.css                  ← ここに配置
├── app.clprj
├── designer.settings.json
├── Modules/
├── PageFrames/
└── Resources/
```

### 読み込みの仕組み

- `DesignDataFileManager` がアプリ起動時に `app.css` を読み込む
- `MainLayoutCore.razor` 内で `<StyleLoader>` コンポーネントにより `<style>` タグとして DOM に注入される
- アプリケーション全体に適用される（グローバルスコープ）
- デザイナーでの変更は即座にプレビューに反映される

---

## 2. ClassName プロパティの活用

### ClassName を持つデザインクラス

| クラス | 適用先 | 用途 |
|---|---|---|
| `FieldLayoutDesign.ClassName` | フィールドラッパー div | 特定フィールドのスタイル |
| `DetailLayoutDesign.ClassName` | モジュール表示のルート div | モジュール全体のスタイル |
| `ListElement.ClassName` | 一覧の列（td/th） | 特定列のスタイル |

### FieldLayoutDesign.ClassName の設定例

```json
{
  "Layout": {
    "FieldName": "Price",
    "ClassName": "price-highlight",
    "Name": "",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
  }
}
```

### DetailLayoutDesign.ClassName の設定例

```json
"DetailLayouts": {
  "": {
    "ClassName": "product-form",
    "Layout": { ... }
  }
}
```

### ListElement.ClassName の設定例

```json
"Elements": [
  [{ "FieldName": "Price", "Label": "価格", "ClassName": "text-end price-column" }]
]
```

### app.css での使用

```css
/* フィールド単位のスタイル */
.price-highlight .form-control {
  text-align: right;
  font-weight: bold;
  color: #d63384;
}

/* モジュール全体のスタイル */
.product-form .grid-row {
  border-bottom: 1px solid #eee;
  padding-bottom: 0.5rem;
}

/* 一覧列のスタイル */
.price-column {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
```

---

## 3. ページレベルの DOM 構造と HTML 属性

ページ全体のレイアウトやモジュール単位でCSSを適用するための属性とDOM構造。

### 全体レイアウト構造 (MainLayoutCore)

```html
<div class="page" data-pageframe="ページフレーム名">
  <!-- ヘッダー（トップバー） -->
  <header class="topbar" data-system="topbar">
    <!-- Topbar コンポーネント（内部のnavにstyleが付与される） -->
  </header>

  <!-- モバイルトップバー（サイドバーが1つだけの場合に表示） -->
  <header class="topbar mobile d-block d-sm-none"
          style="color:...; --background-start:...; --background-end:...; width:...px; min-width:...px"
          data-system="mobile_topbar">
    <!-- MobileTopBar コンポーネント -->
  </header>

  <div class="content">
    <!-- 左サイドバー -->
    <div class="sidebar"
         style="color:...; --background-start:...; --background-end:...; width:...px; min-width:...px"
         data-system="sidebar" data-system-placement="left">
      <!-- Sidebar コンポーネント -->
    </div>

    <!-- メインコンテンツ -->
    <main style="background:...; color:...">
      <header><!-- user-header セクション --></header>
      <article>
        <!-- Loading オーバーレイ -->
        <!-- ページコンテンツ（DetailPage or ListPage） -->
      </article>
      <footer><!-- user-footer セクション --></footer>
    </main>

    <!-- 右サイドバー -->
    <div class="sidebar" style="..." data-system="sidebar" data-system-placement="right">
      <!-- Sidebar コンポーネント -->
    </div>
  </div>
</div>
```

### 詳細ページ構造 (DetailPageComponent)

```html
<div class="lowcode-page" data-module="モジュール名" style="...">
  <!-- ModuleRenderer（モジュールのレイアウト内容） -->
</div>
```

### 一覧ページ構造 (ListPageComponent)

```html
<div list-module="モジュール名">
  <h4 class="row">
    <div class="col"><!-- ヘッダータイトル --></div>
    <div class="col text-end">
      <button data-system="bulk-download"><!-- 一括ダウンロード --></button>
      <label data-system="bulk-upload"><!-- 一括アップロード --></label>
    </div>
  </h4>

  <div data-system="search-field" class="mb-3">
    <!-- SearchFieldComponent -->
  </div>

  <div class="text-end mb-2">
    <a data-system="create" class="btn btn-outline-primary"><!-- 新規作成ボタン --></a>
  </div>

  <div data-system="list-field">
    <!-- ListField / TileListField コンポーネント -->
  </div>
</div>
```

### モジュールレベルの HTML 属性一覧

| 属性 | 付与される要素 | 値 | 用途 |
|---|---|---|---|
| `data-module` | 詳細ページのルート div | モジュール名 | 特定モジュールの詳細画面をCSS指定 |
| `list-module` | 一覧ページのルート div | モジュール名 | 特定モジュールの一覧画面をCSS指定 |
| `data-pageframe` | ページ全体のルート div | ページフレーム名 | 特定ページフレームをCSS指定 |
| `data-module-design` | ModuleDialog / ModulePanel | モジュール名 | ダイアログ/パネル内のモジュールをCSS指定 |

### data-system 属性一覧

ページ構造やシステムUI要素を特定するための属性。

| data-system 値 | 要素 | 説明 |
|---|---|---|
| `topbar` | `<header>` | ヘッダー（トップバー）全体 |
| `mobile_topbar` | `<header>` | モバイル用トップバー |
| `sidebar` | `<div>` | サイドバー（左右は `data-system-placement` で区別） |
| `sidebar-brand` | `<div>` | サイドバーのブランド/ホーム部分 |
| `topbarBrand` | `<a>` | トップバーのブランドリンク |
| `mobileTopbarBrand` | `<a>` | モバイルトップバーのブランドリンク |
| `mobileTopbarToggle` | `<button>` | モバイルトップバーのハンバーガーメニュー |
| `logout` | `<NavLink>` | ログアウトリンク |
| `search-field` | `<div>` | 一覧ページの検索フォーム領域 |
| `list-field` | `<div>` | 一覧ページのリスト表示領域 |
| `create` | `<a>` / `<button>` | 新規作成ボタン |
| `delete` | `<button>` | 削除ボタン（ListField, DetailListField, TileListField内） |
| `bulk-download` | `<button>` | 一括データダウンロードボタン |
| `bulk-upload` | `<label>` | 一括データアップロードボタン |
| `move-up` | `<button>` | 行を上に移動（ListField, DetailListField内） |
| `move-down` | `<button>` | 行を下に移動（ListField, DetailListField内） |
| `tab` | `<ul>` | タブのナビゲーション部分 |
| `search` | `<button>` | 検索ボタン |
| `clear` | `<button>` | 検索条件クリアボタン |
| `expander` | `<button>` | 折りたたみ/展開ボタン |
| `module-dialog` | `<div>` | モジュールダイアログ / パネル |
| `message-box` | `<div>` | メッセージボックス |
| `ok-button` | `<button>` | OKボタン（リンクテーブル選択等） |
| `cancel-button` | `<button>` | キャンセルボタン |

### サイドバー内部構造

```html
<div class="top-row ps-3 navbar" style="font-family:...; font-size:...px"
     data-system="sidebar-brand">
  <div class="container-fluid">
    <a class="navbar-brand" href="...">ブランドテキスト / 画像</a>
  </div>
</div>
<nav class="sidebar-nav flex-column" style="font-family:...; font-size:...px">
  <!-- 各メニュー項目 -->
  <div class="nav-item px-3">
    <a class="nav-link" data-title="メニュータイトル">...</a>
    <!-- 子メニュー（展開時） -->
    <nav class="nav-children flex-column" style="--sidebar-item-depth: N">
      <!-- 再帰的なSidebarItem -->
    </nav>
  </div>
</nav>
```

### トップバー内部構造

```html
<nav class="navbar navbar-expand navbar-[light|dark] cl-navbar-top"
     style="color:...; background-color:...; height:...px; font-family:...; font-size:...px">
  <div class="container-fluid">
    <a data-system="topbarBrand" class="navbar-brand" href="...">ブランド</a>
    <div class="navbar-collapse">
      <ul class="navbar-nav me-auto">
        <!-- メニュー項目 -->
        <li class="nav-item">
          <a class="nav-link" data-title="メニュータイトル">...</a>
        </li>
        <!-- ドロップダウンメニュー項目 -->
        <li class="nav-item dropdown">
          <a class="nav-link dropdown-toggle" data-title="メニュータイトル">...</a>
          <div class="dropdown-menu show">
            <a class="dropdown-item">子メニュー</a>
          </div>
        </li>
      </ul>
      <ul class="navbar-nav ms-auto">
        <!-- ユーザー名 / ログアウト -->
      </ul>
    </div>
  </div>
</nav>
```

### ページレベルの CSS セレクタ例

```css
/* 特定モジュールの詳細画面のみスタイル適用 */
[data-module="Product"] .form-control {
  border-radius: 0;
}

[data-module="Product"] .grid-row {
  --default-row-margin-bottom: 0.5rem;
}

/* 特定モジュールの一覧画面のみスタイル適用 */
[list-module="Product"] table th {
  background-color: #2c3e50;
  color: white;
}

[list-module="Order"] [data-system="create"] {
  background-color: #28a745;
  border-color: #28a745;
}

/* ページフレームごとのスタイル */
[data-pageframe="AdminFrame"] .sidebar {
  width: 280px;
  min-width: 280px;
}

/* 左サイドバーのみスタイル適用 */
[data-system="sidebar"][data-system-placement="left"] {
  border-right: 2px solid #dee2e6;
}

/* 右サイドバーのみスタイル適用 */
[data-system="sidebar"][data-system-placement="right"] {
  border-left: 2px solid #dee2e6;
}

/* サイドバーのブランド部分 */
[data-system="sidebar-brand"] .navbar-brand {
  font-size: 1.4rem;
  font-weight: 700;
}

/* トップバーのブランド部分 */
[data-system="topbarBrand"] {
  font-weight: bold;
}

/* 一覧ページの新規作成ボタン */
[data-system="create"] {
  font-weight: 600;
}

/* 一覧ページの検索フォーム領域 */
[data-system="search-field"] {
  background-color: #f8f9fa;
  padding: 1rem;
  border-radius: 0.5rem;
}

/* モジュールダイアログのスタイル */
[data-module-design="OrderDetail"] .modal-dialog {
  max-width: 900px;
}

/* タブのスタイル */
[data-system="tab"] .nav-link.active {
  font-weight: bold;
  border-bottom: 3px solid #0d6efd;
}

/* メニューのタイトルを使ったスタイル */
[data-title="設定"] {
  font-weight: bold;
}

/* ログアウトリンク */
[data-system="logout"] {
  color: #dc3545 !important;
}

/* 一覧内の削除ボタン */
[data-system="delete"] {
  opacity: 0.6;
}
[data-system="delete"]:hover {
  opacity: 1;
}
```

### ページレベルの CSS カスタムプロパティ

ヘッダー・サイドバーの色・フォント・サイズは直接CSSプロパティ（`color`, `background-color`, `height`, `width`, `font-family`, `font-size`）としてインラインで出力される。CSS変数としては以下のみが残る:

| CSS変数 | 対象 | 設定元 |
|---|---|---|
| `--background-start` | サイドバー | PageFrame の SideBar.BackgroundColorStart |
| `--background-end` | サイドバー | PageFrame の SideBar.BackgroundColorEnd |
| `--sidebar-item-depth` | サイドバーの子メニュー | ネスト階層（自動計算） |

---

## 4. フィールドラッパーの共通 DOM 構造

全フィールドは以下のラッパー div で囲まれる:

```html
<div class="field-layout [ClassName]"
     style="[font-family/font-size/font-weight/font-style/color の直接プロパティ]"
     data-name="フィールド名">
  <!-- フィールドコンポーネントの内容 -->
</div>
```

**共通CSSクラス:**
- `field-layout` - 全フィールドに付与
- `overflow-auto list-layout` - 一覧系フィールド（ListField, DetailListField, TileListField）
- `d-grid` - 垂直方向 Stretch 時

**インラインスタイル:**
- `font-family`, `font-size`, `font-weight`, `font-style`, `color` が `FieldLayoutDesign` の設定値に応じて直接出力される
- CSS変数は使わず、標準のCSSプロパティをそのまま出力する

**data属性:**
- `data-name="フィールド名"` - フィールド名で特定のフィールドをCSS指定可能

**特定フィールドのセレクタ例:**
```css
/* data-name でフィールドを特定 */
[data-name="ProductName"] .form-control {
  font-size: 1.2rem;
  font-weight: bold;
}

[data-name="Price"] .form-control {
  text-align: right;
}
```

---

## 5. フィールド型別 DOM 構造

各フィールドの DOM 構造（HTML出力とCSSクラス）は、個別のフィールドドキュメントの「DOM構造（CSS用）」セクションを参照。

→ [Fields/](Fields/) 以下の各フィールドドキュメント

---

## 6. レイアウト DOM 構造

### GridLayoutDesign

```html
<div data-name="レイアウト名" data-layout="grid">
  <div class="[card|border-0]" style="background-color: ...">
    <div class="[card-body grid-bordered|p-0]">
      <!-- 各行 -->
      <div class="grid-row [as-header|as-footer]"
           data-row="[wrap] [border] [mb] [mt]"
           style="[margin/height/background-color]">
        <!-- 各列 -->
        <div class="grid-column"
             data-border="[t r b l]"
             data-ha="[Start|Center|End|Stretch]"
             data-va="[Top|Middle|Bottom|Stretch]"
             style="[width/padding/background-color]">
          <!-- FieldLayoutDesign or ネストされたレイアウト -->
          <div class="field-layout [ClassName]" data-name="フィールド名"
               style="[font-family/font-size/color等]">
            <!-- フィールドコンポーネント -->
          </div>
        </div>
      </div>
    </div>
  </div>
</div>
```

**data-row 属性:** 行の状態を空白区切りで制御する。
- `wrap` - `IsWrap: true` の場合（折り返しあり）
- `border` - 行内にボーダー付き列がある場合
- `mb` - ボーダーなし行で末尾でない場合（行間マージン下）
- `mt` - 前行がボーダー付きで自行はボーダーなしの場合（行間マージン上）

**grid-column:** `display: grid` と `position: relative` はCSSで定義済み。クラス名に追加不要。

**重要なCSS変数（`:where()` で詳細度低く定義済み、app.css から上書き可能）:**
```css
/* grid-column のデフォルトパディング */
:where(.grid-column) {
  --default-column-padding-left: 0.375rem;
  --default-column-padding-right: 0.375rem;
  padding-left: var(--default-column-padding-left);
  padding-right: var(--default-column-padding-right);
}

/* grid-row のデフォルトマージン */
:where(.grid-row) {
  --default-row-margin-top: 1rem;
  --default-row-margin-bottom: 1rem;
}

/* ボーダー付きグリッドのパディング */
:where(.grid-bordered) {
  --default-card-padding-left: 0.625rem;
  --default-card-padding-right: 0.625rem;
  --default-card-padding-top: 1rem;
  --default-card-padding-bottom: 1rem;
  padding: var(--default-card-padding-top) var(--default-card-padding-right)
           var(--default-card-padding-bottom) var(--default-card-padding-left);
}
```

### TabLayoutDesign

```html
<div class="tab-host" data-name="タブ名" data-layout="tab">
  <ul class="nav nav-tabs" data-system="tab">
    <li class="nav-item">
      <a role="button" class="nav-link [active]">タブ名</a>
    </li>
  </ul>
  <div class="[tab-bordered]" style="padding: ...">
    <!-- 選択中タブのレイアウト内容 -->
  </div>
</div>
```

**タブコンテンツ領域のCSS変数:**
```css
:where(.tab-host > div) {
  --default-tab-padding-left: 1rem;
  --default-tab-padding-right: 1rem;
  --default-tab-padding-top: 0.625rem;
  --default-tab-padding-bottom: 0.625rem;
  padding-left: var(--default-tab-padding-left);
  padding-right: var(--default-tab-padding-right);
  padding-top: var(--default-tab-padding-top);
  padding-bottom: var(--default-tab-padding-bottom);
}
```

### CanvasLayoutDesign

```html
<div class="[card|border-0]" style="...">
  <div class="[card-body p-0|p-0 h-100]">
    <div class="canvas-root" style="[overflow設定]">
      <!-- 各要素（絶対位置） -->
      <div class="field-layout d-inline [ClassName]"
           style="position: absolute; top: Xpx; left: Ypx; width: Wpx; height: Hpx; z-index: Z"
           data-name="フィールド名">
        <!-- フィールドコンポーネント -->
      </div>
    </div>
  </div>
</div>
```

### FlowLayoutDesign（GridLayoutDesign の IsFlowLayout: true）

```html
<div class="mb-3 [card|border-0]" style="...">
  <div class="[card-body grid-bordered|p-0]">
    <div class="flow">
      <div class="field-layout d-inline flow-column [ClassName]"
           style="padding: ..."
           data-name="フィールド名">
        <!-- フィールドコンポーネント -->
      </div>
    </div>
  </div>
</div>
```

---

## 7. CSS セレクタパターン集

### 特定フィールドのスタイル（data-name属性）

```css
/* フィールド名で直接指定 */
[data-name="ProductName"] .form-control {
  font-size: 1.25rem;
}

/* 表示モードのテキスト */
[data-name="Status"] .text {
  font-weight: bold;
}
```

### ClassName との組み合わせ

```css
/* FieldLayoutDesign.ClassName = "required-field" */
.required-field .form-control {
  border-left: 3px solid #dc3545;
}

/* DetailLayoutDesign.ClassName = "compact-form" */
.compact-form .grid-row {
  --default-row-margin-bottom: 0.25rem;
}
.compact-form .grid-column {
  --default-column-padding-left: 0.25rem;
  --default-column-padding-right: 0.25rem;
}
```

### CSS変数の上書き

フレームワークは `:where()` セレクタ（詳細度0）で変数を定義しているため、app.css から簡単に上書き可能。

```css
/* グリッド行の間隔を調整 */
.grid-row {
  --default-row-margin-bottom: 0.5rem;
  --default-row-margin-top: 0.5rem;
}

/* グリッド列のパディングを調整 */
.grid-column {
  --default-column-padding-left: 1rem;
  --default-column-padding-right: 1rem;
}

/* ボーダー付きグリッドのパディング */
.grid-bordered {
  --default-card-padding-top: 1rem;
  --default-card-padding-bottom: 1rem;
  --default-card-padding-left: 1rem;
  --default-card-padding-right: 1rem;
}

/* タブコンテンツ領域のパディング */
.tab-host > div {
  --default-tab-padding-top: 1rem;
  --default-tab-padding-bottom: 1rem;
  --default-tab-padding-left: 1.5rem;
  --default-tab-padding-right: 1.5rem;
}
```

### Bootstrap クラスのカスタマイズ

```css
/* フォームコントロール全体のスタイル */
.form-control {
  border-radius: 0;
}

/* ボタンの角丸を変更 */
.btn {
  border-radius: 2px;
}

/* テーブルの背景色 */
.table-light {
  --bs-table-bg: #e9ecef;
}
```

### input/select等のフォント継承

`FieldLayoutDesign.FontFamily` / `FontSize` / `FontWeight` / `FontStyle` / `Color` を設定した場合、
配下の `<input>` / `<textarea>` / `<select>` / `<span class="input-group-text">` まで継承されるよう、
各フィールドコンポーネントの scoped CSS で次のルールが定義されている:

```css
input,
input:focus,
textarea,
textarea:focus,
select,
select:focus,
.input-group-text {
  font-size: inherit;
  font-weight: inherit;
  font-style: inherit;
  color: inherit;
}
```

これは Bootstrap の `.form-control` / `.form-select` / `.input-group-text` / `.form-control:focus` が
これらのプロパティを固定値で上書きするのを打ち消すため。
`font-family` は Bootstrap reboot の `input { font-family: inherit }` で既に継承されるため不要。

### バリデーション状態のスタイル

```css
/* バリデーションエラー時のスタイル */
.form-control.is-invalid {
  border-color: #dc3545;
  background-color: #fff3f3;
}

/* エラーメッセージのスタイル */
.invalid-feedback {
  font-size: 0.85em;
}
```

### `.card` の背景は透過がデフォルト

`IsBordered: true` のグリッドは Bootstrap の `.card` クラスでレンダリングされるが、
背景色は **デフォルトで透過** にされている (`background-color: transparent`)。

これにより:
- 親要素 (DetailList の選択状態 `.can-select.selected` の青背景など) が透けて見える
- `GridLayoutDesign.BackgroundColor` を明示的に設定したときだけインラインで色が出る

カードに固定の背景色を持たせたい場合は、`GridLayoutDesign.BackgroundColor` で設定するか、
app.css で `.card { background-color: ...; }` を上書きする。

### テーブル一覧のスタイル

```css
/* 選択可能行のホバー */
table tr.can-select:hover {
  background-color: #f0f7ff;
}

/* 選択中の行 */
table tr.can-select.selected {
  --bs-table-bg: #d9ecff;
}

/* DetailList の選択中の行 (透過カードを通して見える) */
div.can-select.selected {
  background: #d9ecff;
}

/* テーブルヘッダー */
thead.table-light th {
  font-weight: 600;
  white-space: nowrap;
}

/* テーブルセル内のテキスト */
table td > div > span {
  padding: 0.5em;
}
```

### タイル一覧のスタイル

```css
/* タイルコンテナのギャップ */
.tile-container {
  gap: 1rem;
}

/* 各タイルカード */
.tile {
  border: 1px solid #dee2e6;
  border-radius: 0.5rem;
  padding: 1rem;
  transition: box-shadow 0.2s;
}

.tile:hover {
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.tile.selected {
  border-color: #0d6efd;
  box-shadow: 0 0 0 2px rgba(13, 110, 253, 0.25);
}
```

### サイドバー・ヘッダーのスタイル

```css
/* サイドバーのナビゲーションリンク */
.nav-link {
  padding: 0.5rem 1rem;
  color: rgba(255, 255, 255, 0.8);
}

.nav-link.active {
  color: #fff;
  background-color: rgba(255, 255, 255, 0.1);
}

/* ヘッダーのブランド */
.navbar-brand {
  font-weight: bold;
  font-size: 1.2rem;
}
```

---

## 8. 実用的な app.css の例

### フォームのカスタマイズ例

```css
/* フォーム全体をコンパクトに */
.grid-row {
  --default-row-margin-bottom: 0.5rem;
}

/* ラベルを太字に */
label.form-label {
  font-weight: 600;
  color: #495057;
}

/* 入力フォーカス時のスタイル */
.form-control:focus,
.form-select:focus {
  border-color: #80bdff;
  box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
}

/* 必須マーク */
.text-danger {
  margin-left: 0.25rem;
}
```

### テーブルのカスタマイズ例

```css
/* テーブルヘッダーのスタイル */
.table thead th {
  background-color: #2c3e50;
  color: white;
  font-weight: 500;
  text-transform: uppercase;
  font-size: 0.85rem;
  letter-spacing: 0.05em;
}

/* 偶数行のストライプ */
.table-striped > tbody > tr:nth-of-type(even) > * {
  --bs-table-striped-bg: #f8f9fa;
}

/* 行ホバー */
.table > tbody > tr:hover > * {
  --bs-table-bg-state: #e2e6ea;
}
```

### ClassName との組み合わせ例

モジュール定義:
```json
{
  "DetailLayouts": {
    "": {
      "ClassName": "order-form",
      "Layout": {
        "Rows": [
          {
            "Columns": [
              {
                "Layout": {
                  "FieldName": "TotalAmount",
                  "ClassName": "amount-display",
                  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
                }
              }
            ]
          }
        ],
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
      }
    }
  }
}
```

app.css:
```css
/* 注文フォーム専用スタイル */
.order-form {
  max-width: 800px;
  margin: 0 auto;
}

.order-form .grid-row {
  --default-row-margin-bottom: 0.75rem;
}

/* 金額表示のスタイル */
.amount-display .text {
  font-size: 1.5rem;
  font-weight: bold;
  color: #dc3545;
  text-align: right;
}

.amount-display .form-control {
  font-size: 1.25rem;
  text-align: right;
  font-variant-numeric: tabular-nums;
}
```

### レスポンシブ対応の例

```css
/* モバイル対応 */
@media (max-width: 768px) {
  .grid-row {
    flex-wrap: wrap !important;
  }

  .grid-column {
    width: 100% !important;
    flex: 0 0 100% !important;
  }

  /* テーブルのスクロール */
  .table {
    font-size: 0.875rem;
  }

  /* タイルを1列に */
  .tile-container {
    grid-template-columns: 1fr !important;
  }
}
```

---

## 9. フォント外観システム

フレームワークはフィールドごとのフォント設定を、CSS変数ではなく直接CSSプロパティとしてインラインスタイルに出力する。

`FieldLayoutDesign` の `FontFamily`, `FontSize`, `FontWeight`, `FontStyle`, `Color` が設定されると、
フィールドラッパー div の `style` 属性に対応するCSSプロパティが直接出力される:

| FieldLayoutDesign プロパティ | 出力されるCSSプロパティ | 例 |
|---|---|---|
| `FontFamily` | `font-family` | `font-family: "Noto Sans JP";` |
| `FontSize` | `font-size` | `font-size: 16px;` |
| `FontWeight` | `font-weight` | `font-weight: 700;` |
| `FontStyle` | `font-style` | `font-style: Italic;` |
| `Color` | `color` | `color: #333333;` |

**出力例:**
```html
<div class="field-layout"
     style="font-family: Noto Sans JP; font-size: 16px; font-weight: 700; color: #333333;"
     data-name="Title">
  <!-- フィールドコンポーネント -->
</div>
```

**備考:**
- `appearance-*` クラスやCSS変数（`--font-family`, `--font-size`, `--text-color` 等）は使われない
- インラインスタイルは CSS の詳細度が高いため、app.css から上書きする場合は `!important` が必要
- スクリプトからフィールドの `Color`, `FontFamily`, `FontSize` を動的に変更した場合も同じ仕組みで出力される

app.css からのスタイル指定例:
```css
/* data-name で特定フィールドのスタイルを指定 */
[data-name="Title"] {
  font-size: 24px !important;
  font-weight: 700 !important;
}
```
