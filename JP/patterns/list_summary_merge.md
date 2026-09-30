# グループ集計表 (小計・合計行 + セル結合)

**いつ使う**: 一覧を Excel 帳票のような「グループ化された集計表」で見せたい。

- 売上一覧をカテゴリでまとめ、同じカテゴリ名の繰り返しを縦 1 セルにまとめて見せたい
- グループの区切りに小計、末尾に総合計を出したい
- 「単価・数量・金額」の 3 列の上に「金額情報」のようなグループ見出しを付けたい

## アプリの作り

- 一覧 (ListField) はグループにする列の順に並んでいる
- 同じカテゴリが続くセルは縦に結合され、1 セルで表示される
- 各グループの最後の行の下に小計行、表の下端に合計行が表示される
- 列見出しの上段に、複数列をまとめたグループ見出しが表示される

## 支えるデータ構造

```
sales
├── id            PK
├── category      TEXT
├── sub_category  TEXT
├── unit_price    NUMBER
├── quantity      NUMBER
└── amount        NUMBER
```

通常のテーブル。小計・合計は DB には持たず、画面上でスクリプトが計算して表示する。

## モジュールとテーブルの対応

| モジュール | テーブル | 主な設定 |
|---|---|---|
| `Sales` | `sales` | 通常の CRUD モジュール |
| `SalesReport` (画面) | なし | `Items` (ListField → `Sales`)。検索条件の並び順で `Category` → `SubCategory` の順にソート |

## CLB ではこう作る

デザイン側は通常の ListField と List レイアウトだけで、小計・合計行とセル結合は**すべてスクリプトで設定**します。
製品が用意するのは「列に揃った表示専用の行」と「セル結合」で、集計値の計算はスクリプトで行います。
使える API の一覧は [ListField の「サマリー行・セル結合・ヘッダー文字」](../fields/List.md#サマリー行セル結合ヘッダー文字) を参照してください。

- 一覧はグループにする列でソートしておく (同じ値が隣り合うことで、縦結合が「グループのまとまり」に見える)
- 縦結合したい列は、List レイアウトの列で閲覧表示 (`IsViewOnly`) にする
- ヘッダーの結合は画面を開いたときに 1 回、小計・合計行と縦結合はデータが変わるたびに作り直す

```csharp
// 詳細レイアウトの OnAfterInitialization に設定
void InitView()
{
    // ヘッダー構造は変わらないので 1 回だけ。見出しを省略すると起点列のラベルを表示
    Items.MergeHeaderColumns("Category", 2, "分類");      // Category〜SubCategory の 2 列を「分類」にまとめる
    Items.MergeHeaderColumns("UnitPrice", 3, "金額情報");
    RebuildView();
}

// ListField (Items) の OnDataChanged に設定
void RebuildView()
{
    // 並びが変わるたびに全部作り直す
    Items.ClearSummaryRows();
    Items.ClearRowMerges();

    var rows = Items.Rows;
    var i = 0;
    decimal sub = 0;
    decimal total = 0;
    foreach (var row in rows)
    {
        if (row.Amount.Value != null)
        {
            sub = sub + row.Amount.Value;
            total = total + row.Amount.Value;
        }
        // グループの最後 (次の行が別カテゴリ、または最終行) の直後に小計
        var isGroupEnd = i == rows.Count - 1 || rows[i + 1].Category.Value != row.Category.Value;
        if (isGroupEnd)
        {
            var s = Items.InsertSummaryRow(i);
            s.MergeColumns("Category", 2);
            s.SetText("Category", row.Category.Value + " 小計");
            s.SetText("Amount", sub.ToString("#,0"));
            s.BackgroundColor = "#E3F2FD";
            sub = 0;
        }
        i = i + 1;
    }

    var sum = Items.AddSummaryRow();
    sum.SetText("Category", "合計");
    sum.SetText("Amount", total.ToString("#,0"));
    sum.BackgroundColor = "#FFF8E1";
    sum.SetColor("Amount", "#D32F2F");
    sum.SetHorizontalAlignment("Category", HorizontalAlignment.Center);
    sum.SetHorizontalAlignment("Amount", HorizontalAlignment.End);

    // 同じ値が続くセルを縦に結合。小計行の位置では自動で分かれる
    Items.MergeSameRows("Category");
    Items.MergeSameRows("SubCategory");
}
```

- 列固定 (`FixedColumnCount`) と併用できる
- 任意の範囲を結合したいときは `MergeRows("Category", 0, 4)` (開始行, 行数) を使う

## 落とし穴

- 条件を満たさないと、エラーにならずに通常の表示になる。効かないときは、List レイアウトが 1 行構成 (セル結合なし) か、縦結合の列が閲覧表示か、結合が列固定の境界をまたいでいないかを確認する
- ソート・ページ切り替え・検索・行の追加削除には自動で追従しない。`OnDataChanged` (検索条件側は `OnSearchDataChanged`) で作り直す
- 結合したヘッダーはソート・列幅変更・列カスタマイズの操作対象にならない。ユーザーにソートさせたい列はヘッダー結合に含めない
- 小計・合計行は表示専用で、保存されるデータには入らない。集計は DB に問い合わせ直さず、画面上の `Rows` から計算する

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [リスト系フィールドの使い分け](list_patterns.md)
- [ListField](../fields/List.md)
