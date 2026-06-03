# CSS

見た目は通常の Blazor アプリと同様に CSS で変更できます。フレームワークは標準の HTML/CSS で動いており、特殊な構文はありません。

このページは「フレームワーク標準の見栄え」を上書きするための方法をまとめています。

## どこに書くか

アプリ全体のスタイルは **`wwwroot/css/app.css`** に書きます。Bootstrap・このフレームワーク本体・テンプレートが用意した既定スタイルの**後**に読み込まれるので、ここに書いた CSS が最終的に勝ちます。

- 個別の見栄えを上書きしたい → `app.css` に普通の CSS / Bootstrap セレクタで上書き
- 特定の Module / Field だけスタイルを変えたい → 後述の [`data-` 属性セレクタ](#識別属性-data--でスタイルを当てる) や [ClassName プロパティ](#classname-プロパティでクラスを付ける)
- アプリ全体の余白・サイドバー色などの**既定値**を変えたい → 後述の [カスタマイズ可能な CSS 変数](#カスタマイズ可能な-css-変数) を `:root` で上書き
- **特定の PageFrame でだけ** 違う見栄えにしたい → 後述の [PageFrame ごとの CSS](#pageframe-ごとの-css) を使う

## PageFrame ごとの CSS

PageFrame 単位で独立した CSS を持たせることができます。**現在表示中の PageFrame に対応する CSS だけが動的に注入され**、別の PageFrame に切り替えるとその CSS が外れて切り替え先のものに入れ替わります。

「管理者画面と顧客画面で配色を全く変えたい」「ある画面群だけサイドバーを隠したい」といった、**PageFrame 単位で大きく見栄えを変えたい**場面で使います。

### 書き方

デザインプロジェクトの `PageFrames/` フォルダに、PageFrame と**同名の `.css` ファイル**を置くだけです。

```
App/
├── app.css                       ← アプリ全体に常に効く
└── PageFrames/
    ├── Main.frm.json             ← Main フレーム定義
    ├── Main.css                  ← Main フレーム表示中だけ効く CSS
    ├── AdminFrame.frm.json       ← AdminFrame フレーム定義
    └── AdminFrame.css            ← AdminFrame 表示中だけ効く CSS
```

デザイナの Solution Explorer で PageFrame を右クリックして「CSS ファイルを作成」を選ぶと、対応する `.css` ファイルが追加されます (既存ファイルがあればその項目は出ません)。PageFrame をリネーム / 削除すると、対応する CSS ファイルも追従します。

### 読み込み順序

CSS は次の順で読み込まれます。後勝ち (= 後ろが上書きする):

```
Bootstrap → フレームワーク既定 → app.css → PageFrame の CSS (現在表示中のものだけ)
```

つまり PageFrame ごとの CSS が一番強いので、`app.css` のスタイルを部分的に上書きする使い方ができます。

### 例: PageFrame ごとにテーマカラーを変える

`PageFrames/AdminFrame.css`:
```css
:root {
  --bs-primary: #b91c1c;        /* 管理者画面は赤系 */
  --background-start: #4a0303;
  --background-end: #1a0000;
}
```

`PageFrames/CustomerFrame.css`:
```css
:root {
  --bs-primary: #1d4ed8;        /* 顧客画面は青系 */
  --background-start: #052767;
  --background-end: #0a1f4d;
}
```

`app.css` 側でフレーム全体に共通の調整を入れたまま、PageFrame 別の差分だけ各 CSS に書ける構成にできます。

### `data-pageframe` セレクタとの使い分け

PageFrame ごとに見た目を変える方法は 2 つあります。

| 方法 | 適している場面 |
|---|---|
| **`PageFrames/{name}.css`** | PageFrame 単位で**たくさん**スタイルを差し替える時 (テーマ全体の差替など)。CSS が個別ファイルなので管理しやすい |
| **`[data-pageframe="..."]` セレクタ** | `app.css` 内で**少しだけ**フレーム別の調整を入れたい時 |

両者は併用できます。`app.css` で共通スタイル + `data-pageframe` で軽い分岐 + 大規模な差替は PageFrame 個別 CSS、という使い分けが現実的です。

---

## 識別属性 (`data-*`) でスタイルを当てる

フレームワークが描画する DOM には、CSS から狙えるように **識別用の `data-*` 属性**が組み込まれています。`app.css` でこれらをセレクタにすれば、特定のモジュール / フィールド / システム UI 部品に対してピンポイントでスタイルを当てられます。

### フィールド: `data-name`

レイアウトに置いた Field は、その Field の `Name` プロパティが `data-name` 属性として外側の `<div>` (List/DetailList の場合は `<td>`) に出ます。

```css
/* "Email" という名前のフィールド全体を赤くする */
[data-name="Email"] {
  background-color: #ffeaea;
}

/* "Email" フィールドの中の input だけスタイル変更 */
[data-name="Email"] input {
  border-color: #d33;
}
```

> 同じフィールド名が複数モジュールに存在する場合は、後述の `data-module` などと組み合わせて絞り込みます。

### モジュール: `data-module` / `data-module-design`

| 属性 | 出る場所 |
|---|---|
| `data-module="ModuleName"` | 詳細ページ (`/モジュール名/...` の URL で開く DetailPage) のルート `<div>` |
| `data-module-design="ModuleName"` | ダイアログ・パネル等で開いた埋め込みモジュールの外側 `<div>` |

```css
/* Customer モジュールの詳細画面だけ背景色を変える */
[data-module="Customer"] {
  background-color: #f8f9fa;
}

/* Customer モジュールの中の Email フィールドだけスタイル変更 */
[data-module="Customer"] [data-name="Email"] {
  font-weight: bold;
}

/* ダイアログで開いた CustomerEditDialog のヘッダー高さを変える */
[data-module-design="CustomerEditDialog"] .card-header {
  padding: 0.25rem 0.5rem;
}
```

### PageFrame: `data-pageframe`

アプリ全体を囲む `<div class="page">` には `data-pageframe="PageFrame名"` が付きます。サイドバー構成が異なる複数の PageFrame を持つアプリで、PageFrame ごとに見た目を切り替えたい時に使います。

```css
/* AdminFrame でのみサイドバーを濃いグレーに */
[data-pageframe="AdminFrame"] .sidebar {
  background: #2c2c2c !important;
}
```

### システム UI 部品: `data-system`

「ヘッダー」「サイドバー」「Submit ボタン」「検索バー」など、フレームワークが自動で配置する UI 部品には **`data-system="識別名"`** が付いています。これでスタイルを当てたり、不要な部品を非表示にしたりできます。

主な識別名:

| `data-system` の値 | 何の部品か |
|---|---|
| `topbar` / `mobile_topbar` | 上部ヘッダー (PC / モバイル) |
| `topbarBrand` / `mobileTopbarBrand` / `sidebar-brand` | ロゴ/タイトル領域 |
| `sidebar` | サイドバー (位置は `data-system-placement="left"` / `"right"` 併用) |
| `mobileTopbarToggle` | モバイル時のハンバーガーボタン |
| `logout` | サイドバー/ヘッダーのログアウトリンク |
| `tab` | TabLayout のタブ部分 |
| `search-field` | 一覧ページの検索バー領域 |
| `search-condition-field` | 検索レイアウト内の各検索フィールド枠 |
| `search-mode-toggle` / `search-mode-menu` / `search-mode-item` | 検索の「= / 範囲 / 空」モード切替 UI |
| `list-field` | 一覧ページの List フィールド領域 |
| `create` | 「新規」ボタン / 行追加ボタン |
| `delete` | 行削除ボタン |
| `move-up` / `move-down` | 行の並び替えボタン |
| `bulk-download` / `bulk-upload` | 一覧ページの Excel 一括ダウンロード/アップロード |
| `ok-button` / `cancel-button` | ダイアログの OK / キャンセル |
| `expander` | Collapse の展開トグル |

```css
/* 上部ヘッダーの高さを縮める */
[data-system="topbar"] {
  min-height: 2.5rem;
}

/* 左サイドバーの背景を変える (右サイドバーには影響しない) */
[data-system="sidebar"][data-system-placement="left"] {
  background: linear-gradient(180deg, #2563eb, #1e40af);
}

/* 一覧ページの Excel 一括ダウンロードボタンを隠す */
[data-system="bulk-download"] {
  display: none;
}

/* 行削除ボタンの色を変える */
[data-system="delete"] {
  color: #b91c1c;
}
```

> `data-system` の値は **フレームワークが内部で使う安定識別子**です。任意の文字列ではないので、上の表に出ていない値を勝手に作る必要はありません。

### レイアウト: `data-layout`

GridLayout / TabLayout の最外殻には `data-layout="grid"` / `data-layout="tab"` が付きます。

```css
/* Tab レイアウトのタブ全体の余白調整 */
div[data-layout="tab"] > ul.nav-tabs {
  margin-bottom: 1rem;
}
```

---

## `ClassName` プロパティでクラスを付ける

Field・Layout・Module は共通で **`ClassName`** プロパティを持ち、ここに任意の CSS クラス名を設定できます。`data-*` セレクタで届きにくい場面や、状態に応じて動的にクラスを切り替えたい場面で使います。

Field の場合、設定したクラスは Field を囲む `<div>` (List/DetailList の場合は `<td>`) に追加されます:

```html
<!-- 例: ClassName に "important-field" を設定 -->
<div class="field-layout important-field" data-name="Email">
  <!-- ここに実際のフィールドが描画される -->
</div>
```

```css
.important-field {
  background-color: #fff7c2;
  border: 1px solid #f5c518;
}
```

`ClassName` はスクリプトからも設定できるので、ユーザー操作に応じてスタイルを動的に切り替えられます:

```csharp
void Status_OnDataChanged()
{
    Email.ClassName = Status.Value == "Active" ? "active-field" : "inactive-field";
}
```

Layout (Grid / Canvas / Tab) や Module の `ClassName` も同じ要領でレイアウト要素・モジュール要素にクラスが追加されます。実際にどの要素に付くかは、ブラウザの開発者ツールで確認してください。

---

## よく使うパターン

### モジュール × フィールド名で絞り込む

```css
/* Customer モジュールの Email だけ赤背景。他モジュールの Email は影響なし */
[data-module="Customer"] [data-name="Email"] {
  background-color: #ffeaea;
}
```

### 一覧画面の特定の列だけスタイル変更

List/DetailList/TileList の各セルにも `data-name="フィールド名"` が `<td>` に付きます。

```css
/* 一覧表の Status 列だけ等幅フォントに */
td[data-name="Status"] {
  font-family: monospace;
}
```

### 検索フィールドだけ別スタイルにする

検索レイアウト配下の各検索フィールドは `data-system="search-condition-field"` 配下に置かれます。

```css
[data-system="search-condition-field"] [data-name="Email"] input {
  background: #f0f8ff;
}
```

### サイドバー全体を最小化する

```css
/* サイドバーの幅をデフォルトより狭く */
[data-system="sidebar"] {
  --sidebar-collapsed-width: 60px;
}
```

### 不要なシステム UI を隠す

```css
/* ロゴ領域を消す */
[data-system="topbarBrand"],
[data-system="sidebar-brand"] {
  display: none;
}

/* ログアウトボタンを消す (認証なしで運用したい場合等) */
[data-system="logout"] {
  display: none;
}
```

### モジュールごとにテーマを変える

```css
/* 管理者用モジュールは赤系、顧客用モジュールは青系 */
[data-module^="Admin"] {
  --bs-primary: #b91c1c;
}
[data-module^="Customer"] {
  --bs-primary: #1d4ed8;
}
```

### List の `IsEnabled=false` を従来の透過表示に戻す

List フィールドのセルで `IsEnabled=false` にした入力欄は、読み取り専用（ViewOnly）の透過表示と見分けられるよう、**無効状態のグレー背景**で表示されます。以前と同じ透過表示に戻したい場合は、`app.css` に次を貼り付けてください。

```css
/* List の IsEnabled=false の入力欄を透過表示に戻す */
#app table td input.form-control:disabled,
#app table td textarea.form-control:disabled {
  background: transparent !important;
}
```

製品側のルールは `!important` 付きのスコープ CSS なので、アプリのルート要素 `#app` を前置して詳細度を上げ、`!important` で上書きしています。

---

## カスタマイズ可能な CSS 変数

レイアウトの余白などはあらかじめ CSS 変数で定義されているので、`app.css` で値を上書きすればアプリ全体に反映できます。
個別の Grid・Row・Column・Tab の `Padding` / `Margin` プロパティを使えば**そのレイアウト単体**で上書きでき、CSS 変数を使えば**アプリ全体の既定値**を変更できます。

### レイアウト (Grid / Row / Column / Tab) の余白

| 変数 | 既定値 | 用途 |
|---|---|---|
| `--default-card-padding-top` | `1rem` | Grid `IsBordered` オン時のカード内側 上 |
| `--default-card-padding-right` | `0.625rem` | 同 右 |
| `--default-card-padding-bottom` | `1rem` | 同 下 |
| `--default-card-padding-left` | `0.625rem` | 同 左 |
| `--default-row-margin-top` | `1rem` | Row の上マージン |
| `--default-row-margin-bottom` | `1rem` | Row の下マージン |
| `--default-column-padding-left` | `0.375rem` | Column の左パディング |
| `--default-column-padding-right` | `0.375rem` | Column の右パディング |
| `--default-column-border-padding-top` | `0.5rem` | `BorderStyle` 指定ありの Column の上パディング |
| `--default-column-border-padding-bottom` | `0.5rem` | 同 下 |
| `--default-tab-padding-top` | `0.625rem` | Tab パネル内側 上 |
| `--default-tab-padding-right` | `1rem` | 同 右 |
| `--default-tab-padding-bottom` | `0.625rem` | 同 下 |
| `--default-tab-padding-left` | `1rem` | 同 左 |

`app.css` で `:root` または `body` に書けばアプリ全体に効きます。

```css
:root {
  --default-row-margin-top: 1.5rem;
  --default-row-margin-bottom: 1.5rem;
  --default-column-padding-left: 0.5rem;
  --default-column-padding-right: 0.5rem;
}
```

### サイドバー背景グラデーション

| 変数 | 既定値 | 用途 |
|---|---|---|
| `--background-start` | `rgb(5, 39, 103)` | サイドバー背景グラデーション 上 |
| `--background-end` | `#3a0647` | サイドバー背景グラデーション 下 |

```css
:root {
  --background-start: #1a1a2e;
  --background-end: #16213e;
}
```

### Bootstrap の CSS 変数

このフレームワークは Bootstrap をベースに描画しています。色や枠線の基本トーンは Bootstrap の CSS 変数 (`--bs-body-bg` / `--bs-body-color` / `--bs-border-color` / `--bs-primary` 等) でも調整できます。詳細は [Bootstrap 公式ドキュメント](https://getbootstrap.com/docs/5.3/customize/css-variables/) を参照してください。

---

## まとめ: どの方法を使うか

| やりたいこと | 推奨方法 |
|---|---|
| アプリ全体の色・余白の既定値を変える | **CSS 変数** を `:root` で上書き |
| 特定のモジュール / フィールド単位でスタイル変更 | **`data-module` × `data-name`** で絞り込み |
| 検索バー・サイドバー・行操作ボタン等のシステム UI 部品の見栄えを変える/隠す | **`data-system="..."`** セレクタ |
| **PageFrame ごとに大きく見栄えを変える** | **`PageFrames/{name}.css`** に書く |
| ユーザー操作に応じて動的に切り替え | **`ClassName` プロパティ** をスクリプトで設定 |
| 単純に Bootstrap テーマを差し替えたい | 下記 [サンプル](#サンプル-特殊例) を丸ごとコピー |

---

## サンプル (特殊例)

通常運用では上記の `data-*` セレクタ・`ClassName`・CSS 変数で十分ですが、Bootstrap のテーマを差し替えるような大規模カスタマイズの例も用意しています。

- [Material Design](material_design.md) — Bootstrap テーマを差し替える例
- [Fluent Design](fluent_design.md) — 同上
- [Custom Styles](custom_styles.md) — 自前で書くスタイル例

実物を見るには[デモサイト](https://lowcodedemo.azurewebsites.net) のサイドバー「CSS」項目が参考になります。
