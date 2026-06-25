# レイアウトガイドライン

レイアウト定義を作成する際の推奨ルール。好みに応じて変更可能。

---

## 詳細レイアウトのパターン（チャットで名前指定できる）

パターン名は **`やること-詳細1-詳細2-…`** という階層命名にする（ハイフン区切り、左から解釈）。将来パターンを増やすときもこの規則に従う。

### やること: `全体配置`（フォーム全体を組み直す）

`全体配置-<ラベル位置>[-<1行あたりの項目数>]`

- **詳細1＝ラベル位置**: `ラベル左`（ラベルを入力の左）／ `ラベル上`（ラベルを入力の上＝ネストGridでラベル行→入力行）
- **詳細2＝1行あたりの項目数（任意・数字）**: 省略時は **1**（1行に1項目）。`2`なら1行に2項目、`3`なら3項目…を横に並べる

| パターン名 | 内容 |
|---|---|
| **全体配置-ラベル左**（既定・迷ったらこれ） | 1行に1項目。ラベル左＋入力右（ラベル列 `Width:<最長ラベルから推定。下限96px>`＋`VerticalAlignment:"Middle"`、入力列 `Width:null`）。 |
| **全体配置-ラベル左-2** | ラベル左のまま、1行に2項目ずつ横に並べる。 |
| **全体配置-ラベル上** | 1行に1項目。ラベルを入力の上に置く（ネストGrid: ラベル行→入力行）。 |
| **全体配置-ラベル上-3** | ラベル上のまま、1行に3項目ずつ横に並べる。 |

数字は他の項目数にもできる（`全体配置-ラベル左-3` など）。

### 併用できる指定

- **カード分け**: 関連項目を枠線カード（`IsBordered:true` のネストGrid）でセクション化。`全体配置-*` と併用可。
- 自由形式の指示（この行を全幅に・罫線を引く・Notes を一番下に 等）にも従う。

### 既定（指定が無いとき）

**名前の指定が無く「いい感じに/見やすく並べて」だけのときは、必ず `全体配置-ラベル左`（1行1項目・ラベル左）を使う**（ファーストチョイス）。

- **ラベル左 = ラベルと入力を「同じ行」に左右で置く**（同じ Row 内の左 Column にラベル、右 Column に入力）。
- **ラベルを入力の「上の行」に置く（ラベル上／縦置き）にするのは、ユーザーが明示的に `ラベル上`・「縦置き」・「ラベルを上に」等と指定したときだけ。** 「いい感じに」だけのときにラベル上にしない（迷ったらラベル左）。

### どのパターンでも共通

システムフィールドは置かない / BooleanField はラベル無しで単独配置 / 全幅フィールド（複数行テキスト・File・List/DetailList）はラベル左にせずラベル上か省略。各パターンの詳細は下記レシピと各節を参照。

---

## おすすめ詳細レイアウトの作り方（「いい感じに配置して」の既定レシピ）

具体的な指定が無く「いい感じに/見やすく配置して」と言われたら、次の手順で組む。**並べ方は既定の「ラベル左」（1行1項目）をファーストチョイスにする。** パターン名や具体指定があればそれを優先。

1. **先頭行（タイトル＋戻るボタン）**: 一番上に、戻るボタン(AnchorTagField, `Target: HistoryBack`)とページタイトル(Label, `Style: H4`、Text=モジュール名等)を置く。標準は3カラム行（`Width:60` / 未指定 / `Width:60`）で「戻る｜タイトル(中央)｜空セル」。一から全体配置するときはタイトルと戻るボタンを新規作成して置く。
   - **戻るボタンはアイコンのみ**にする: `AnchorTagFieldDesign` の `TitleText` を `""`(空)、`Style: Text`、`Icon: "bi bi-arrow-left-circle-fill"`。大きさは**配置側の `FieldLayoutDesign.FontSize` を `30`** にして出す(フィールド側ではなく配置側)。
   - 一番下に**サブミットボタン**(SubmitButtonField)を置く。1カラムだけの行なら**列の `HorizontalAlignment: "Center"`** で左右中央に。一から全体配置するときは新規作成して置く。
2. **入力の並べ方（既定は `全体配置-ラベル左`・1行1項目）**: **通常サイズの入力フィールドには見出しラベルを付ける**（ラベルの無い素並びにしない＝フォームの基本）。既定は「ラベル左＋入力右」で**1行に1項目**＝ラベル列 `Width:<推定幅>`＋`VerticalAlignment:"Middle"`、入力列 `Width:null`の2カラム行を、項目ごとに縦に積む（横に複数項目を詰めない）。ラベルは NewLabels で `Text:""`＋`RelativeField:"対象フィールド名"`で作り表示名に追従させる。1行に複数項目を詰めるのは**項目数（`-2`/`-3` 等）を指定されたとき**だけ。（後述「レイアウト最小化の原則」は上級者向けの削減ガイドで、明確な条件を満たすときだけラベルを省く。基本のフォームでは省かない。）
   - **ラベル列の幅は文字数から推定する（重要）**: そのフォーム内の見出しラベル（各入力の表示名）の中で**最も長いもの**を基準に、ラベルが折り返さず収まる幅にする。目安は **`最長ラベルの文字数 × 18px + 余白 40px`**（全角文字を約18pxとみなす）。例: 最長が「顧客コード」(5文字)なら 5×18+40 ≈ 130px、「電話番号」(4文字)なら 4×18+40 ≈ 110px。**下限は 96px（50px のように狭くしない）**、上限の目安は 240px。**ラベル列はフォーム内で全て同じ幅に揃える**（行ごとに変えると左端が不揃いになる）。ラベルが極端に長い1項目だけは、その項目をラベル上にして全幅にしてもよい。
   - **この「ラベル左」の対象は通常サイズの入力だけ**。**全幅にする大きいフィールド（複数行テキスト/File/List/DetailList、後述4）と BooleanField（後述）は、ラベル左の2カラムにしない**（全幅フィールドはラベルを上か省略、Boolean はラベル省略）。これらをラベル左にすると入力が全幅にならない/ラベルが重複する。
   - **ラベルを付けないフィールド**: **`BooleanField`（チェックボックス/スイッチ/トグル）には見出しラベルを付けない**。Boolean はフィールド自身がチェックボックスの横に表示名を描画するため、別の見出しラベルは重複になる。Boolean は1セルに単独で置く（ラベル左の2カラムにもラベル上のネストGridにもしない）。同様に Button/SubmitButton/AnchorTag/Label などラベル不要の要素にも見出しラベルは付けない。
   - **`全体配置-ラベル上`（ラベルを上）と指示された場合は別ルール**（後述「ラベルを上に配置する場合」）: **ラベル列にも入力列にも `Width` を付けない**（左ラベル用の `Width:120` を流用しない）。**1行あたりの項目数も既定は1**（`全体配置-ラベル上`なら1項目1行）。項目数（`-2`/`-3` 等）が指定されたら、その数だけ1つの行に横に並べる（各項目はラベル上のネストGrid）。長文/File/List は単独で全幅。
   - **レイアウト方式を切り替えるとき（縦↔横、ラベル左↔ラベル上 等）もラベルを必ず維持する**: 方式変更は配置の組み替えであってラベルの削除ではない。既に見出しラベル（`XxxLabel` 等）がフィールド一覧にあるなら、新しい構造の中でそれらを `FieldLayoutDesign.FieldName` で**必ず再配置**する（落とさない・素並びに戻さない）。NewLabels は新規ラベルを足すときだけで、既存ラベルの再利用に NewLabels は不要。
3. **関連フィールドはセクションにまとめる**: グループごとに**入れ子の GridLayoutDesign を `IsBordered:true` のカード**にし、必要なら見出しの Label を1行目に置く。
4. **全幅にするもの**: 複数行テキスト・File・List/DetailList・MarkupString は1カラム行で全幅に。**全幅フィールドの見出しラベルはラベル左（同じ行に2カラム）にしない**——入力が全幅にならないため。ラベルが要るなら入力行の**上**に別の行でラベルを置く（入力行は1カラムのまま）。ラベルが不要なら省いてよい（テーブルヘッダや placeholder で自明な場合）。
5. **出さないフィールド**: `Id` / `LogicalDelete` / `OptimisticLocking` / `CreatedAt` / `UpdatedAt` / `Creator` / `Updater` は配置しない（業務的に不要 or 自動）。
6. **行は必ず1列以上**（空行を作らない）。空けたいセルは `Layout` を省略した空セルにする（`FieldName` 空の FieldLayoutDesign は禁止）。

> 罫線（IsBordered と セル BorderStyle の違い・スコープ）は後述の「罫線」節を参照。

---

## レイアウト最小化の原則（上級者向け・既定では適用しない）

> これは「もっと洗練させたい」ときの上級ガイドです。**単に「いい感じに並べて」と言われた基本のフォーム作成では適用せず、各入力にラベルを付ける既定（上記レシピ）に従ってください。** 下記の削減は、**その削減候補ごとの明確な条件を満たすとき**だけ行います。条件が曖昧なら削らない（ラベルは付けたまま）。

**「削れるだけ削る」がデザインの基本**。ラベル・ページャー・ヘッダー等は「あって当然」と思い込まずに、削っても情報が失われないかチェックする。ただし**入力欄の見出しラベルを一律に省くのは禁止**——下表の条件（placeholder で代用できる等）を満たす個別のラベルだけを削る。

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

## DetailLayout - 行(GridRow)には必ず列(GridColumn)を入れる

`GridRow` には**最低1つの `GridColumn`** が必要。`Columns` が空配列の行は画面に何も描画されず不具合になる。
行を追加するときは、その行の `Columns` に `GridColumn` を作ること。フィールドを置く行は、その `GridColumn.Layout` に
`FieldLayoutDesign` を設定する（例: ラベル列＋入力列なら `GridColumn` を2つ）。

```jsonc
// ❌ 悪い: 列の無い行（何も表示されない）
{ "Columns": [] }

// ✅ 良い: 列を入れ、フィールドを置く列には Layout を設定
{ "Columns": [
  { "Layout": { "TypeFullName": "...FieldLayoutDesign", "FieldName": "OrderDate" } }
] }
```

※「列の `Layout` を省略した空セル」(`{ }`) は**有効**（意図的な余白セル）。禁止なのは「列が1つも無い行」。

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

**骨組み（この構造をコピーして使う。1項目＝ネストGridで「ラベル行→入力行」。ラベルと入力を同じ行に横並びにしない）:**
```jsonc
// 既定は1行1ブロック。項目数(-2/-3 等)を指定されたら、その数だけ「項目ブロック」を1つの外側Rowに横に並べる(各 Column が1ブロック)。下は2個並べる例
{
  "Columns": [
    { "Layout": {                                  // ← ブロック1
        "Rows": [
          { "Margin": { "Bottom": 0 },             // ラベル行(下マージン0)
            "Columns": [ { "Layout": { "FieldName": "CustomerCodeLabel", "VerticalAlignment": "Bottom", "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign" } } ] },
          { "Columns": [ { "Layout": { "FieldName": "CustomerCode", "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign" } } ] }
        ],
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign" } },
    { "Layout": { /* ← ブロック2 (CustomerNameLabel / CustomerName を同じ構造で) */
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign" } }
  ]
}
```
ラベル行の `CustomerCodeLabel` は入力フィールドではなく、その入力用に用意した**別のラベルフィールド**(`LabelFieldDesign`、`Text` は `""`、`RelativeField` に入力名を設定):
```jsonc
// 各入力に対応するラベルフィールドを用意し、上のラベル行から FieldName で参照する
{ "Name": "CustomerCodeLabel", "Text": "", "RelativeField": "CustomerCode", "VerticalAlignment": "Bottom", "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LabelFieldDesign" }
```

**ポイント:**
- **ラベルと入力は必ず別の行**(ラベル行が上、入力行が下)。同じ行に `[ラベル, 入力]` の2カラムで置くのは「ラベル左」であって縦置きではない。**ラベルを入力の左に置かない。**
- **ラベル行に置くのは入力フィールドではなく上記の `LabelFieldDesign`**(`Text:""` + `RelativeField`)。**入力フィールド(CustomerCode)を上の行にもう一度置いて見出し代わりにしない**(同じフィールドを2箇所に配置するのは不具合)。
- 各項目ブロックは必ず**ネストした `GridLayoutDesign`**(2行: ラベル行→入力行)。用意したラベルフィールドは**必ずレイアウトのラベル行に配置**する(作っただけで置き忘れない)。
- ラベル行の `Margin.Bottom` を `0`、ラベルの `VerticalAlignment` は `"Bottom"`。
- **ラベル列・入力列に `Width` を付けない**(縦並びは幅指定不要。ラベル左用の `Width:120` を流用しない)。
- **1行あたりの項目数は指定に従う**: 既定は **1行1項目**（`全体配置-ラベル上`）。項目数（`-2`/`-3` 等）を指定されたら、上の骨組みのように1つの外側 Row に項目ブロックをその数だけ横に並べ、項目数に応じて行を増やす。長文(複数行)/File/List は単独で全幅。
- `IsRowMarginRemoved: true` は **非推奨**。`Margin.Bottom: 0` だけで詰める

---

## DetailLayout - ラベルとフィールドを一行に並べる場合 (横並び)

ラベルを入力フィールドの**左**に並べるときは、外側 Row の中に2列 (ラベル列・フィールド列) を置く。

```
Row
  ├─ Column[0]: ラベル用 LabelField (Width: 最長ラベルから推定. 文字数×18+40, 下限96px. VerticalAlignment: "Middle")
  └─ Column[1]: 入力フィールド
```

ラベル列の `Width` は**最長ラベルの文字数から推定**し、全行で**同じ幅に揃える**（目安 `文字数×18+40px`、下限 96px、狭すぎる 50px 等にしない）。

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
