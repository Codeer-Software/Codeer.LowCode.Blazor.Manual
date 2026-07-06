# 一覧ページ・カスタム一覧パターン集

`ListPageDesign` (PageFrame Link 配下) と「自前 SearchField+ListField」を組み合わせて、一覧画面を組み立てるためのレシピ集。パターン名を指示されたらそのまま組めるように、JSON 例とレイアウト上の注意を併記する。

## 用語

- **自動一覧ページ** : PageFrame Link で `ModulePageType: "Auto"` を指定すると、CLB が「検索フォーム → 一覧 → 行クリックで詳細」の 3 画面を自動生成する。Module の `SearchLayouts` / `ListLayouts` / `DetailLayouts` を組み合わせて成り立つ。
- **ListPageDesign** : Link の `ListPageDesign` で挙動を細かく制御 (検索レイアウト切替、Excel 入出力、行操作 UI 等)。

## 共通の落とし穴

- **`SearchLayouts[""]` (デフォルト) は必ず存在させる**。空辞書 (`{}`) は NG (`#39` 参照)。
- **検索レイアウトに Field 参照 (FieldLayoutDesign) が 1 つでも入っていないと、検索エリアそのものが非表示** (`GetDescendantFields().Any()` で判定)。ラベルだけの解説検索レイアウトでも、`InfoLabel` への `FieldLayoutDesign` を Row に置けば描画される。
- **PageFrame Link を複数置く同一 Module は `ModuleUrlSegment` で URL を分離**。同一 URL で複数 Link を置くと、ルーティングは最初の Link しかヒットせず後続 Link の `ListPageDesign` が無視される (`GetModulePageDesignIgnoreCase` 仕様)。
- **`FieldBase.LayoutName` と `*FieldDesign.LayoutName` を混同しない** (`#50` 参照)。
- **列幅は 1 列だけ可変にする**。`ListLayout` の Elements 全列に `Width` を入れると、行末のシステム列 (`>`/`🗑`) が右に伸びてしまう。商品名等の主要列は `Width` を指定せず、副次列だけ固定する。

---

## パターン 1: 基本の一覧ページ

「検索 → 一覧 → 詳細」最小構成。`ModulePageType: "Auto"` のデフォルト動作。

### Link 設定
```json
{
  "Title": "商品/一覧",
  "ModulePageType": "Auto",
  "ModuleUrlSegment": "product-list",
  "Module": "Product",
  "ListPageDesign": {
    "SearchLayoutName": "",
    "UseSubmitButton": false,
    "UseNavigateToCreate": true,
    "CanBulkDataUpdate": false,
    "CanBulkDataDownload": false,
    "ListFieldDesign": {
      "CanCreate": false, "CanUpdate": false, "CanDelete": true,
      "CanUserSort": true, "CanNavigateToDetail": true,
      "SearchCondition": { "LimitCount": 50, "ModuleName": "Product",
        "Condition": {"IsOrMatch": false, "IsNot": false, "Children": [], "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"} },
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
    }
  }
}
```

### レイアウトのポイント
- 一覧の列幅: 主要列 (商品名等) は `Width` 指定なし、副次列 (価格・在庫等) は `Width: 80` 等で固定。行末の `>` / `🗑` が伸びない。
- 一覧の並び順は列ヘッダクリック (`CanUserSort: true` がデフォルト) で動く。デフォルト並び順を固定したい場合はパターン 4 を参照。

---

## パターン 2: 一括処理 (Excel ダウンロード/アップロード)

検索結果を Excel ダウンロード + テンプレート Excel をアップロードで一括 INSERT/UPDATE。`CanBulkDataDownload` と `CanBulkDataUpdate` の両方 (`true`) を併用する。

### Link 設定 (差分のみ)
```json
"ListPageDesign": {
  "CanBulkDataUpdate": true,
  "CanBulkDataDownload": true,
  "ListFieldDesign": { "CanDelete": true, ... }
}
```

### 動作
- 一覧右上にダウンロード ⬇ / アップロード ⬆ ボタンが出る
- ダウンロードは検索結果範囲、アップロードは Excel ファイルから INSERT/UPDATE
- CSV が必要なら Excel をユーザー側で変換 (フレームワークは Excel 出力のみ)

---

## パターン 3: 一覧表示を DetailListField / TileListField に差し替え

`ListPageDesign.ListFieldDesign` は `FieldDesignBase` 型なので、ListField 以外に DetailListField や TileListField を入れることもできる。「フォーム形式の一覧」や「タイル形式の一覧」がそのまま作れる。

### Link 設定 (DetailListField 版)
```json
"ListPageDesign": {
  "ListFieldDesign": {
    "LayoutName": "",
    "CanNavigateToDetail": true,
    "CanDelete": true,
    "SearchCondition": { "ModuleName": "Product", ... },
    "PagerPosition": "Bottom",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DetailListFieldDesign"
  }
}
```

`TileListFieldDesign` に変えれば同様にタイル形式に切り替わる (`TileWidth` でタイル幅指定)。

### 絶対常識: 対象 Module の DetailLayout はカード化する
DetailListField / TileListField で各レコードを並べる場合、**行 Module の `DetailLayouts[""].Layout.IsBordered: true` を必ず指定**。ListField (表形式) は表自体が罫線で区切るので不要だが、DetailListField / TileListField はフォーム/タイルがただ並ぶだけで境界が無いと 1 レコード = 1 ブロックに見えない。

```json
"DetailLayouts": {
  "": {
    "Layout": {
      "IsBordered": true,
      ...
    }
  }
}
```

詳細は [DetailListField](temporary/_field_catalog.md) のカード化セクション参照。

---

## パターン 4: 既定条件 (固定フィルタ + デフォルトソート)

「ある Module を毎回○○の条件で絞り込んで価格降順で見せる」みたいに、開発者がコードで固定する条件。`ListPageDesign.ListFieldDesign.SearchCondition` の `SortConditions` と `Condition` に書く。**ユーザーが画面で入れた検索条件と AND で結合される** (ユーザー条件で上書きされない)。

### Link 設定 (差分のみ: 在庫 50 以上を価格高い順)
```json
"ListPageDesign": {
  "ListFieldDesign": {
    "SearchCondition": {
      "LimitCount": 50, "ModuleName": "Product",
      "SortConditions": [
        { "Variable": "Price.Value", "IsDescending": true }
      ],
      "Condition": {
        "IsOrMatch": false, "IsNot": false,
        "Children": [
          {
            "SearchTargetVariable": "Stock.Value",
            "Comparison": "GreaterThanOrEqual",
            "Value": { "Value": 50, "TypeFullName": "Codeer.LowCode.Blazor.Repository.DecimalValue" },
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
          }
        ],
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
      }
    }
  }
}
```

### 使いどころ
- 同じ Module を「全件」「アクティブのみ」など、複数の Link で違う見せ方にする
- 一覧のデフォルト並び (新着順固定など) を指定する ← これはほぼ全 Link で使う
- 検索条件の常時固定は稀だが、テナント分離・ロール別画面で使う

### 注意
- 列ヘッダクリックで `SortConditions` は上書きされる。ユーザーがソート列を変えたら既定ソートは消える (リロードで戻る)。
- ユーザー条件と AND 結合される性質を活かして「ベースフィルタ」として使う。OR にしたい場合は別途 `IsOrMatch: true` の MultiMatchCondition を使う。

---

## パターン 5: カスタム一覧 (自前検索 + サマリ + List)

`ModulePageType: "Auto"` の自動一覧ページは「検索→一覧→詳細」固定だが、それでは届かない要件 (検索結果のサマリ表示、ボタンや図と一緒に配置、複数の ListField 並べ等) がある場合は、**Module の DetailLayout に SearchField + ListField + α を自分で配置する** 「カスタム一覧」パターン。

### 構成 (`DbTable: ""` の表示専用モジュール)

| Field | 役割 |
|---|---|
| `PageTitle` (LabelField, H4) | 画面タイトル |
| `Description` (LabelField) | 解説テキスト |
| `Search` (SearchFieldDesign) | 検索フォーム |
| `SummaryText` (LabelField) | 検索結果のサマリ (件数・合計値・平均等) |
| `ResultsLabel` (LabelField) | 「結果」見出し |
| `Results` (ListFieldDesign) | 検索結果一覧 |

### SearchField の設定
```json
{
  "ResultsViewFieldName": "Results",
  "LayoutName": "Custom",
  "OnSearched": "Search_OnSearched",
  "Name": "Search",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchFieldDesign"
}
```

- `LayoutName`: 検索対象 Module (Results の SearchCondition.ModuleName) の SearchLayouts から取得するキー名。専用キー ("Custom" 等) を **検索対象 Module 側に** 作っておき、必要な検索 Field だけを置く
- `ResultsViewFieldName`: 検索結果を表示する ListField の Name
- `OnSearched`: 検索完了時のスクリプト (サマリ更新用)

### サマリスクリプト (mod.cs)
```csharp
void Search_OnSearched()
{
    int count = Results.RowCount;
    decimal totalStock = 0;
    decimal totalPrice = 0;
    foreach (var row in Results.Rows)
    {
        totalStock += row.Stock.Value;
        totalPrice += row.Price.Value;
    }
    decimal avg = count > 0 ? totalPrice / count : 0;
    SummaryText.Text = "件数: " + count + "件　合計在庫: " + totalStock + "　平均価格: " + (int)avg + " 円";
}
```

- `Results.Rows` は `List<Module>`、`Results.RowCount` は `int`
- 各 Row は子 Module なので `row.Stock.Value` のように Field 名でアクセス
- スクリプトに `await` は付けない (`#21`)

### Module 側の準備
- 検索対象 Module (例: ListPageDemo) の `SearchLayouts` に専用キー (例: "Custom") を追加し、絞込みに使う Field の参照を入れる
  ```json
  "SearchLayouts": {
    "": { ... },
    "Custom": {
      "OnSearchInitialization": "", "ShowDefaultSearchButtons": true,
      "Layout": {
        "Rows": [
          { "Columns": [{ "Layout": { "FieldName": "Name", "TypeFullName": "..FieldLayoutDesign" }, ... }] }
        ],
        "TypeFullName": "..SearchGridLayoutDesign"
      }
    }
  }
  ```

### 使いどころ
- 検索結果のサマリ (件数・合計・平均) を表示したい
- ListField の上下にカード・ボタン・図・グラフ等を並べたい
- 1 ページに複数の ListField を置きたい (例: カテゴリ別 3 リスト並列)

### 注意
- 自動一覧ページの便利機能 (`UseSubmitButton` / `CanBulkDataUpdate` / 一覧編集) はカスタム一覧では使えない → 自前で SubmitButton / 上下ボタン等を配置する
- 検索結果の URL クエリ連動が要るなら `Search.UserUrlParameter: true` にする

---

## レイアウト位置決めのポイント (一覧系全般)

- **詳細ページの 1 行目** は「戻る / タイトル / 空」の 3 列。Col[0]=Width:60、Col[1]=flex (Width 未指定) + `HorizontalAlignment: "Center"`、Col[2]=Width:60。詳細遷移先には必ず `AnchorTagField` (`Target: "HistoryBack"`、`Icon: "bi bi-arrow-left-circle-fill"`、`FontSize: 30`) を Col[0] に置く (`#36`)。
- **検索フォームの列** : SearchLayout 内の Row の Column は `Width` 指定無しで OK (検索 Field が自分でサイズ調整)。
- **ListLayout の列幅** : 1 列だけ可変、残りは固定。例: `Name` (Width 無し) / `Price` (Width:100) / `Category` (Width:100) / `Stock` (Width:80)。`>` / `🗑` のシステム列はフレームワーク側で生成される。
- **カスタム一覧の縦方向** : `PageTitle (H4) → Description → Search → SummaryText → ResultsLabel → Results`。Row 間の `Margin.Bottom: 4` か `8` で揃える (12 以上はバラつきの元)。

## パターン早見表

| 指示 | 適用パターン |
|---|---|
| 「一覧ページ作って」 | パターン 1 (基本) |
| 「Excel で一括処理したい」 | パターン 2 (CanBulkDataUpdate/Download) |
| 「フォーム形式で並べたい」 | パターン 3 (DetailListField + IsBordered) |
| 「タイル形式で並べたい」 | パターン 3 (TileListField + IsBordered) |
| 「並び順固定」「○○のレコードだけ表示」 | パターン 4 (既定条件) |
| 「検索結果のサマリ出したい」「ListField の上下に何か配置したい」 | パターン 5 (カスタム一覧) |
