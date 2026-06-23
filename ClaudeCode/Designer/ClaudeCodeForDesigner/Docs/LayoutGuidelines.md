# レイアウトガイドライン

レイアウト定義を作成する際の推奨ルール。好みに応じて変更可能。

---

## レイアウト最小化の原則

**「削れるだけ削る」がデザインの基本**。ラベル・ページャー・ヘッダー等は「あって当然」と思い込まずに、削っても情報が失われないかチェックする。

### 削れる候補チェックリスト

| 削除候補 | 削れる条件 | 代替・備考 |
|---|---|---|
| **「○○状況」「○○情報」「明細」のような囲みラベル** | カードや表で囲まれていて、見た目で何のセクションか自明 | カード化 + 内容で十分伝わる |
| **入力欄の「コメント」「メモ」等のラベル** | placeholder で代用できる | `Placeholder: "コメント"` |
| **DetailListField / ListField の DisplayName ラベル** | テーブルヘッダー (列ラベル) で何のリストか自明 | ListField の DisplayName 空にする |
| **ページャー** | 件数が少なく (~20件) 全件 1 画面で見たい用途 | `PagerPosition: "None"` + `SearchCondition.LimitCount: null` |
| **ユーザーソート (列ヘッダクリック)** | 表示順が業務上固定 (時系列・優先順位等) | `CanUserSort: false` + `SortConditions: [{Variable: "...Value", IsDescending: ...}]` で固定順設定 |
| **「状態」「現在の担当」のような単独表示** | 他の情報と統合して 1 行で表現可能 | テキスト連結 (`状態:進行中  A→▶B→C`) |
| **申請者・申請日時の Detail 表示** | 履歴の最古エントリで分かる | 一覧 (List) には残し、Detail から削る |
| **連番 Id 列** | 業務的に無意味 | ListLayout / DetailLayout どちらにも出さない (cf. [#45](../CLAUDE.md)) |
| **承認/却下のような状態依存ボタン** | 該当状態じゃないとき | スクリプトで `IsVisible` 制御 |

### 削減の前にやる確認

- そのフィールド/ラベル/UI 要素を削ったとき、ユーザーが「何の情報か分からなくなる」か?
- 同じ情報を他の場所 (履歴、テーブルヘッダ、placeholder) で代用できるか?
- 「念のため出しておく」のような曖昧な理由なら削る候補

### 統合の発想

複数のフィールドを 1 行に圧縮すると省スペースになる。例:

```
[Before]
状態         : 進行中
現在の承認者 : アリス
承認順番     :
 順番 1  状態 進行中
 アリス [必須] 待ち

[After]
状態:進行中  ✓アリス→▶アリス
```

スクリプトで TextField (`IsViewOnly: true`) に動的に文字列を組み立てて表示するパターン。マーカー記号 (`✓` / `✗` / `▶` / `—`) で状態を表現する。

---

## DetailLayout - GridColumn の HorizontalAlignment

| フィールド種別 | HorizontalAlignment | 備考 |
|---|---|---|
| 入力フィールド（TextField, NumberField, DateField, DateTimeField, TimeField, SelectField, LinkField, BooleanField 等） | **設定しない（省略）** | 設定すると入力欄の横幅が崩れる |
| ラベル（LabelFieldDesign） | 必要に応じて設定OK（例: `"Left"`） | |
| ボタン（SubmitButton, Button等）が行に1つだけの場合 | `"Center"` | |

---

## DetailLayout - 戻るボタンとタイトル行

詳細画面の先頭行には、戻るボタンとページタイトルを同じ行に配置する。
レイアウトのバランスを取るため**3カラム構成**にし、**両端のカラムに固定 Width を設定して、真ん中 (タイトル) が残り全幅で伸びる**ようにする。

**フィールド定義:**
- `BackButton` - AnchorTagFieldDesign, Target: `HistoryBack`, Icon: `bi bi-arrow-left-circle-fill`
- `TitleLabel` (or `PageTitle`) - LabelFieldDesign, Style: `H4` 以上, テキストはページの内容に応じた名称
- 3つ目のカラムは **Layout 省略の空セル**

**レイアウト (3カラム、両端固定/真ん中 flex):**

| カラム1 (Width: 60) | カラム2 (Width 未指定) | カラム3 (Width: 60) |
|---|---|---|
| BackButton (FontSize: 30) | TitleLabel (HorizontalAlignment: Center) | 空セル (Layout 省略) |

**ポイント:**
- カラム1とカラム3に **`Width: 60`** を入れる (戻るアイコンの自然サイズに合わせた値)。両端を等幅にすることで視覚的バランスが取れる
- カラム2 (タイトル) は **Width 未指定** にして flex で残り全幅を占有させる
- カラム3 は `Layout` プロパティ自体を**省略** (空セル)。`HorizontalAlignment: Center` のカラム2でタイトルが中央寄せされる
- 両端の Width を入れないと、カラム2 のコンテンツサイズに依存して挙動が変わるので、明示的に固定する

**JSON 例:**

```json
{
  "Columns": [
    {
      "Layout": {
        "FieldName": "BackButton",
        "FontSize": 30,
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
      },
      "Width": 60,
      "BorderStyle": { "LeftColor": "", "TopColor": "", "RightColor": "", "BottomColor": "" },
      "Border": "None"
    },
    {
      "Layout": {
        "FieldName": "PageTitle",
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
      },
      "HorizontalAlignment": "Center",
      "BorderStyle": { "LeftColor": "", "TopColor": "", "RightColor": "", "BottomColor": "" },
      "Border": "None"
    },
    {
      "Width": 60,
      "BorderStyle": { "LeftColor": "", "TopColor": "", "RightColor": "", "BottomColor": "" },
      "Border": "None"
    }
  ]
}
```

---

## Grid 中央配置パターン（空セルで挟む）

GridLayout で要素を中央・左寄せ・右寄せに配置するには、**Layout が null の空セル（`"Layout": null`）** を配置する特殊パターンが使える。レンダラー側で自動検出され、中身セルがコンテンツサイズに、空セルが残りスペースを埋めるように挙動する。

### パターン一覧

| 行構成 | 挙動 |
|---|---|
| `[空 \| content \| 空]` | **中央配置** — 中身は内容サイズ、左右の空セルが均等に余白を作る |
| `[空 \| 空 \| content \| 空 \| 空]` | **中央配置** — 4個の空セルで均等分割 |
| `[content \| 空]` | **左寄せ** — 中身は内容サイズ、右側の空セルが残りを埋める |
| `[空 \| content]` | **右寄せ** — 左側の空セルが残りを埋め、中身は内容サイズ |
| `[field \| content \| field]` | **通常 flex:1 均等** — 空セルが存在しないので通常の分割になる |

### 検出条件

レンダラーは以下の条件で「センタリングパターン」と判定する:

- 行が **2列以上**
- **中身ありのセルが1つだけ**（`Layout != null`）
- **残りのセルは全て空**（`Layout == null`）

自分以外の列全てが空セル、という条件なので、行に他のフィールドを混ぜると効かない。

### JSON 例（中央配置）

```json
{
  "Columns": [
    { "Layout": null },
    {
      "Layout": {
        "FieldName": "CenteredButton",
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
      }
    },
    { "Layout": null }
  ],
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridRowDesign"
}
```

### 注意点

- **狭い親の中でも動く**（例: 280px 幅のサイドバー内）。CSS Grid の `grid-template-columns: 100%` を inline で `auto` に上書きするため、intrinsic size が正しく効く
- 空セルに誤ってフィールドを入れると中央配置にならない。`"Layout": null` を正確に
- `HorizontalAlignment: "Center"` だけだと中身がカラム全幅に広がってしまう場合があるため、中身の大きさに合わせて中央配置したいときはこのパターンを使う

---

## DetailLayout - ラベルを上に配置する場合 (縦並び)

ラベルを入力フィールドの上に縦に並べるときは、外側の行に直接2行で並べるのではなく、**カラム内にネストした GridLayoutDesign** を使う。ラベル行の `Margin.Bottom` を `0` にしてラベルと入力欄の間隔を詰める。

**構造:**
```
外側の Row
  └─ Column
       └─ GridLayoutDesign (ネスト)
            ├─ Row (Margin: { Bottom: 0 }) ← ラベル行
            │    └─ Column: FieldLayout (ラベル, VerticalAlignment: Bottom)
            └─ Row ← 入力フィールド行
                 └─ Column: FieldLayout (入力フィールド)
```

**ポイント:**
- ラベル行の `Margin.Bottom` を `0` に設定し、ラベルと入力欄の間の余白を除去する
- ラベルの `VerticalAlignment` は `"Bottom"` にする
- 横並びの場合は、1つの外側 Row に複数の Column を配置し、各 Column 内にそれぞれネスト Grid を持たせる
- `IsRowMarginRemoved: true` は **非推奨**。`Margin.Bottom: 0` だけで詰める

---

## DetailLayout - ラベルとフィールドを一行に並べる場合 (横並び)

ラベルを入力フィールドの**左**に並べるときは、外側 Row の中に2列 (ラベル列・フィールド列) を置く。

```
Row
  ├─ Column[0]: ラベル用 LabelField (Width: 130 等で固定, VerticalAlignment: "Middle")
  └─ Column[1]: 入力フィールド
```

### 鉄則: ラベル列には「新しく作った LabelField」を置く。データフィールドを2回置かない

左のラベル列に入れるのは、入力フィールドとは**別に新規作成した `LabelFieldDesign`**（例: `CustomerCodeLabel`）。
1つの入力フィールドにつき、ラベル用 LabelField を1つ `Fields` に追加し、左列でそれを参照する。

> ⚠️ **同じフィールド名 (`FieldName`) を、同一レイアウト内の複数の列・行に置いてはいけない。**
> 入力フィールドを左列と右列の両方に置くと、同じ入力欄が2つ描画されてレイアウトが壊れる。
> 「ラベル列にとりあえず同じフィールドを置く」は誤り。ラベルは必ず専用の LabelField を作る。

```json
// ❌ 絶対ダメ: 同じ CustomerCode を2列に置く → 入力欄が2個出て壊れる
"Columns": [
  { "Layout": { "FieldName": "CustomerCode", "TypeFullName": "...FieldLayoutDesign" }, "Width": 140 },
  { "Layout": { "FieldName": "CustomerCode", "TypeFullName": "...FieldLayoutDesign" } }
]

// ✅ 正しい: 左 = ラベル用 LabelField、右 = 入力フィールド
"Columns": [
  { "Layout": { "FieldName": "CustomerCodeLabel", "TypeFullName": "...FieldLayoutDesign" }, "Width": 140, "VerticalAlignment": "Middle" },
  { "Layout": { "FieldName": "CustomerCode", "TypeFullName": "...FieldLayoutDesign" } }
]
```

ラベル用 LabelField は `Fields` に追加する。ラベル文字列の指定方法は2通り:

```json
// 方法A: Text にラベル文字列を直接書く (固定文字。対象フィールドの表示名とは連動しない)
{
  "Name": "CustomerCodeLabel",
  "Text": "顧客コード",
  "RelativeField": "CustomerCode",
  "Style": "Default",
  "Icon": "",
  "IsHtml": false,
  "OnClick": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LabelFieldDesign"
}

// 方法B: Text を空にして RelativeField で対象フィールドを指す (対象の表示名 DisplayName がラベルになり、表示名変更にも追従)
//   ※ Text のデフォルトは "Label"。空にしないと "Label" と表示されるので必ず "" を明記する
{
  "Name": "CustomerCodeLabel",
  "Text": "",
  "RelativeField": "CustomerCode",
  "Style": "Default",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LabelFieldDesign"
}
```

**ポイント:**
- ラベル列に `Width` (例: 130px) を固定値で指定し、フィールド列は自動幅
- ラベル列の `VerticalAlignment: "Middle"` を必ず設定 (デフォルトの Top だと、入力欄の中央高さと揃わない)
- `RelativeField` に対象フィールド名を入れると、ラベルクリックで入力欄にフォーカスが移る (for 連動)。`Text` を空にすればその Field の `DisplayName` がラベルになる (Text のデフォルト "Label" に注意)

---

## SearchLayout - 検索フィールドのラベル/入力縦並び

検索レイアウトでは、**ラベルを入力フィールドの上**に配置するのが標準的。1行に複数の検索条件を横並びに置き、各列でラベル/入力を縦に積む構造になる。

実装は **「DetailLayout - ラベルを上に配置する場合」と同じネスト Grid パターン**を使う。外側は `SearchGridLayoutDesign`、各列で `GridLayoutDesign` を入れ子にして 2 行 (ラベル/入力) に分ける。

```
SearchGridLayoutDesign
  └─ Row 0
       ├─ Column[0]: ネスト GridLayoutDesign  ← 検索条件1
       │    ├─ Row (Margin: { Bottom: 0 }): ラベル (VerticalAlignment: Bottom)
       │    └─ Row: 入力フィールド
       ├─ Column[1]: ネスト GridLayoutDesign  ← 検索条件2
       │    └─ (同様)
       └─ Column[N]: 空セル (Layout 省略, スペーサー)
```

**ポイント:**
- 検索条件はラベルを下揃え (`VerticalAlignment: "Bottom"`)、入力欄を上揃えで詰める
- 行内の最後に**空列** (Layout が空 or 省略) を1つ置くと、検索ボックスが画面幅いっぱいに広がるのを防げる
- 折りたたみ可能にする場合は外側に `IsExpandable: true` の Grid を被せる ([このファイル下部の SearchLayout セクション](#searchlayout---検索画面は折りたたみ可能にする)を参照)

---

## DetailLayout - DetailListField に入れるモジュールは IsBordered: true にする

DetailListField（明細リスト）の子モジュールの DetailLayout では、`GridLayoutDesign` の `IsBordered` を `true` に設定する。

**理由:**
- 枠がないと、先頭フィールドにフォーカスを当てたときにハイライトの上部が見切れる（上方向のパディングが不足するため）
- カード状の枠があることで、各明細行の区切りが視覚的に明確になる

**設定箇所:** 子モジュールの `DetailLayouts` → `""` → `Layout` → `IsBordered`

```json
"Layout": {
  "IsBordered": true,
  ...
}
```

---

## ListLayout - Id 列を Elements に入れない (慣行)

`IdFieldDesign` の `Id` フィールドは **ListLayout の Elements に入れない**のが CLB の慣行。Id を入れると空セルが描画されてしまう (IdField は UI 上で値を出さない仕様)。

行番号が欲しいときは `ListNumberFieldDesign` を Fields に追加し、ListLayout の先頭列に置く。

```json
// ❌ 悪い: Id 列が空セルになる
"Elements": [[
  { "FieldName": "Id", "Width": 80 },
  { "FieldName": "Name", "Width": 240 }
]]

// ✅ 良い: Id 列を入れない
"Elements": [[
  { "FieldName": "Name", "Width": 240 }
]]

// ✅ 行番号が欲しい場合: ListNumberField を使う
"Elements": [[
  { "FieldName": "RowNo", "Width": 60 },   // ListNumberFieldDesign
  { "FieldName": "Name", "Width": 240 }
]]
```

---

## ListLayout - Elements の構造

`Elements` は行の配列（通常1行）。各行内の配列が横に並ぶ列を定義する。

| パターン | 意味 | 結果 |
|---|---|---|
| `[[Col1, Col2, Col3]]` | 1行ヘッダー、3列 | 列が横に並ぶ（正しい） |
| `[[Col1], [Col2], [Col3]]` | 3行ヘッダー、各1列 | 列が縦に並ぶ（意図しない） |

通常は `[[Col1, Col2, Col3]]` のように全列を1つの内側配列にまとめること。
複数行ヘッダーが必要な場合のみ外側の配列に行を追加し、`RowSpan` / `ColumnSpan` で制御する。

---

## SearchLayout - 検索画面は折りたたみ可能にする

検索フォームのレイアウトは `SearchGridLayoutDesign` で、以下の設定を推奨:

- `IsBordered: true` — 枠線で検索フォーム領域を明示
- `IsExpandable: true` — 折りたたみ可能にする
- `ExpanderLabel: "検索条件"` — 折りたたみ時のラベル
- `IsExpanderDefaultOpened: false` — 初期状態は閉じる（一覧を多く表示）

```json
"SearchLayouts": {
  "": {
    "Layout": {
      "Operator": "And",
      "IsBordered": true,
      "IsExpandable": true,
      "ExpanderLabel": "検索条件",
      "IsExpanderDefaultOpened": false,
      "Rows": [...],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign"
    }
  }
}
```

---

## 全般 - 表示専用モジュールの ListLayout にフィールドを入れない

DBと結びつかない表示専用モジュール（チャートダッシュボード、ダイアログ等）では `ListLayouts` の Elements にフィールドを入れないこと。

**理由:**
- `ListLayouts` の Elements にフィールドが入っていると、一覧表示モジュールと認識されてしまう
- 表示専用モジュールはデータの一覧表示が不要であり、詳細画面のみで完結する
- 同様に `IdFieldDesign` や `SubmitButtonFieldDesign` も不要

---

## 全般 - 一覧専用モジュールは DetailLayouts を作らない

親の `ListField`（表形式）にインライン表示するだけ、かつ詳細ページへ遷移しない（`CanNavigateToDetail: false`）子明細モジュールは、**`DetailLayouts` を空 `{}` にする**。詳細デザインを作り込まない。

**理由:**
- `ListField` の行は対象モジュールの **`ListLayouts[""]` の Elements** で描画され、`DetailLayouts` は使われない
- 遷移しないなら `DetailLayouts` は完全に未使用。作り込むと使われないラベル・カードが増えて冗長
- `DetailLayouts` の C# 既定値は `{}`（空）なので、空でも読み込み・インライン編集に支障はない

**逆に `DetailLayouts[""]` が必要なのは:** 詳細ページへ遷移するモジュール／`DetailListField`・`TileListField` でフォーム・タイル表示するモジュール（後者はカード化 `IsBordered: true` も必須 → [Fields/DetailListField.md](Fields/DetailListField.md)）。

---

## 全般 - FieldLayout の Name プロパティ

レイアウト内の FieldLayoutDesign や GridLayoutDesign に `Name` プロパティを設定すると、スクリプトからその名前でレイアウト要素にアクセスして表示制御できる。

```json
{
  "Name": "EditSection",
  "Rows": [...],
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
}
```

```csharp
// スクリプトから表示制御
EditSection.IsVisible = false;
EditSection.IsViewOnly = true;
EditSection.IsEnabled = false;
```

名前が不要な場合は空文字 `""` で良い。

---

## 全般 - DataOnlyFields の活用

DetailLayout / ListLayout の `DataOnlyFields` に指定したフィールドは、データとしてロードされるが画面には表示されない。スクリプトで内部計算に使うフィールドや、SearchCondition でのみ使うフィールドに活用する。

```json
"DetailLayouts": {
  "": {
    "DataOnlyFields": ["HiddenCalcField", "InternalFlag"],
    "Layout": { ... }
  }
}
```

---

## 読み取り専用・計算結果フィールドの正攻法

「合計金額」「作成日時」など**ユーザーに編集させず表示だけしたい**フィールドは、代理のラベルで誤魔化さず、フィールド本体を読み取り専用で置く。

### 計算結果フィールド（合計・小計など）

- 専用のフィールド（`NumberFieldDesign` 等、桁区切りは `Format: "#,0"`）を**そのままレイアウトに置く**。値はスクリプト（`OnDataChanged` 等）でそのフィールドに代入する（明細の集計は [ScriptGuidelines.md](ScriptGuidelines.md) の `Rows` を使う方法）。
- ❌ `LabelFieldDesign` + `RelativeField` で代理表示しない。`Format`（桁区切り等）が効かず、フィールドが二重定義になる。

### 入力させたくない（読み取り専用表示）

| 対象 | 設定 |
|---|---|
| DetailLayout の 1 フィールド | その列の `FieldLayoutDesign` に **`"IsViewOnly": true`**（design-time。スクリプト不要で入力欄が表示専用になる） |
| ListLayout の 1 列 | 列要素（`ListElement`）に **`"IsViewOnly": true`** |
| セクション全体 | 囲っている `GridLayoutDesign` に **`"IsViewOnly": true`**（`LayoutDesignBase` 共通プロパティ） |
| 画面に出す必要すらない（計算・検索条件専用） | `DataOnlyFields` に入れて非表示にする |

### `IsUpdateProtected` は読み取り専用化ではない

`IsUpdateProtected: true` は「**既存レコードの更新時にその列を上書きしない**」ための設定。**入力欄を編集不可にする効果も、新規作成時の手入力を防ぐ効果も無い**。編集させたくないなら上記の `IsViewOnly` を使う。

---

## レイアウト共通 - 同じフィールドは1箇所だけ（配置＝移動）

レイアウト内で同じ `FieldName` の `FieldLayoutDesign` は**1箇所にしか置けない**。すでに配置済みのフィールドを別の場所に「配置」「追加」「入れる」のは**移動**を意味する。

- 新しい位置に `FieldLayoutDesign` を置く
- **元の位置からは必ず削除する**（その `GridColumn` の `Layout` を出力しない＝空セルにする）
- 同じフィールドが複数箇所に出現するレイアウトは不正。

## TabLayout - Tabs と Layouts の整合性

`TabLayoutDesign` は `Tabs`（タブ名）と `Layouts`（各タブの中身）を持つ。

- **`Tabs.Count == Layouts.Count` を必ず一致させる**。タブを1つ追加したら、同じインデックスに対応する `Layouts` も1つ追加する（既存 Layouts は変更しない）。
- タブ追加時に中身の指定が無ければ、その `Layouts` 要素には「1行×4列・全列が空（Layout 省略）の `GridLayoutDesign`」を初期形として入れる。
- 既存タブを配置場所に使うときは「空の `GridColumn`（Layout 省略）」を優先し、無ければ最小限 Column/Row を足す。既存 Column の幅・パディング等は変えない。

## ListLayout - 行追加と結合は別の操作

- **「段（行）を増やす」**＝既存行と同じ列数の**空行を追加**する。各セルは `{ "FieldName": "", "ColumnSpan": 1, "RowSpan": 1 }`。既存セルは一切変えない（`RowSpan` 変更もフィールド移動もしない）。
- **「結合する／またがる」**＝`RowSpan` / `ColumnSpan` を使う。**明示的に結合を指示されたときだけ**。
- 結合を指示されていないのに `RowSpan` を 2 以上にしたり、結合用プレースホルダーを作ったりしない。
- 複数行ヘッダーで `FieldName: ""` のセルは、`RowSpan` で上のセルに覆われた位置のプレースホルダー専用。データ表示セルには必ず `FieldName` を設定する。
