# レイアウトガイドライン

レイアウト定義を作成する際の推奨ルール。好みに応じて変更可能。

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
レイアウトのバランスを取るため3カラム構成にする。

**フィールド定義:**
- `BackButton` - AnchorTagFieldDesign, Target: `HistoryBack`, Icon: `bi bi-arrow-left-circle-fill`
- `TitleLabel` - LabelFieldDesign, Style: `H2`, テキストはページの内容に応じた名称
- `Spacer` - LabelFieldDesign, Text: `""` (空文字、右側バランス用)

**レイアウト (3カラム、幅指定なし):**

| カラム1 | カラム2 | カラム3 |
|---|---|---|
| BackButton (FontSize: 30) | TitleLabel (HorizontalAlignment: Center) | Spacer |

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

## DetailLayout - ラベルを上に配置する場合

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
