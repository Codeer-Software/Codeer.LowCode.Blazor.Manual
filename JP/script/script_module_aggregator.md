# ModuleAggregator / BatchAggregator

スクリプトから、SQL を書かずにモジュールのデータを集計するためのクラスです。
件数・合計・平均・最小・最大・重複を除いた件数を、項目や日付の単位 (年・四半期・月・週・日・時) でまとめます。

| クラス | 用途 |
|---|---|
| `ModuleAggregator<T>` | 1 モジュールを集計 |
| `ModuleAggregator` | ジェネリックなしの版 (モジュール名を文字列で指定) |
| `BatchAggregator` | 複数の `ModuleAggregator` を 1 回のリクエストで実行 |

- 集計は DB 側 (GROUP BY) で行います。行を全部読み込まないので、件数が多くても軽く動きます
- 権限は一覧と同じです (読めない行は数えない・読めない項目を使うとエラー)
- 対象は表を持つモジュールだけです (QueryField のモジュールは集計できません)
- アプリ (ホスト) が集計 API に対応している必要があります。1.3.39 より前のテンプレートで作ったアプリは [集計 API の結線](../user_code/aggregate_api.md) を行ってください

---

## 1. 基本

```csharp
// 受注を 状態 × 受注月 でまとめ、金額の合計と件数を取る
var searcher = new ModuleSearcher<Order>();
searcher.AddGreaterThanOrEqual(e => e.OrderedOn.Value, new DateOnly(2026, 4, 1));

var agg = new ModuleAggregator<Order>();
agg.Where(searcher);                 // 条件 (省略可。ModuleSearcher の条件だけを使う。並び・件数は使わない)
agg.GroupBy(e => e.Status);          // まとめる項目 (.Value は省略できる)
agg.GroupByMonth(e => e.OrderedOn);  // 日付・日時は単位を選ぶ
var amount = agg.Sum(e => e.Amount); // 集計値。戻り値は値の番号 (0 始まり)
var count = agg.Count("件数");
agg.OrderByMeasureDescending(amount);
agg.Limit(50);
var result = agg.Execute();          // AggregateResult

foreach (var row in result.Rows)
{
    var status = row.KeyText(0);     // まとめた項目の表示 (選択・リンクは表示名)
    var month = row.Key(1);          // まとめた項目の値 (月でまとめた日付は期間の開始日)
    var total = row.Number(amount);  // 集計値 (decimal?)
}
if (result.IsLimited) Logger.Info($"{result.GroupCount} 件中 {result.Rows.Count} 件だけ表示");
```

## 2. メソッド

| メソッド | 説明 |
|---|---|
| `Where(searcher)` | 条件。`ModuleSearcher` の条件だけを使う |
| `GroupBy(e => e.Field)` | まとめる項目。複数呼べる。リンク越し (`e => e.Customer.Region`) も可 |
| `GroupByYear` / `GroupByQuarter` / `GroupByMonth` / `GroupByWeek` / `GroupByDay` / `GroupByHour` | 日付・日時の項目を単位でまとめる。週は月曜始まり |
| `GroupByYear(e => e.Field, 4)` / `GroupByQuarter(e => e.Field, 4)` | 年度で区切る。第 2 引数は年度の開始月 (4 なら 4 月〜翌 3 月が 1 年度) |
| `Count()` / `Count(name)` | 件数。戻り値は値の番号 |
| `CountDistinct(e => e.Field)` | 重複を除いた件数 |
| `Sum` / `Avg` (数値) / `Min` / `Max` (数値・日付・日時・時刻・文字) | 集計値。戻り値は値の番号 |
| `HavingGreaterThan(値の番号, 数値)` など | 集計した後の絞り込み (`HavingEquals` / `HavingNotEqual` / `HavingLessThan` / `HavingLessThanOrEqual` / `HavingGreaterThanOrEqual`) |
| `OrderByGroup(番号)` / `OrderByGroupDescending` / `OrderByMeasure(値の番号)` / `OrderByMeasureDescending` | 並び。指定が無ければ まとめた項目の昇順 (選択は候補の順・リンクは表示名の順・空値は最後) |
| `Limit(count)` | 返すまとまりの数の上限。超えたら結果の `IsLimited` が true |
| `Execute()` | 実行して `AggregateResult` を返す |

## 3. 結果 (AggregateResult)

| メンバー | 説明 |
|---|---|
| `Rows` | まとめた値の組み合わせ 1 つにつき 1 行 |
| `Rows[i].Key(n)` / `KeyText(n)` | n 番目にまとめた項目の値 / 表示 (空値は null / 空文字) |
| `Rows[i].Value(n)` / `Number(n)` | n 番目の集計値 (object / decimal?) |
| `TotalCount` | 条件に合った行数 (まとめる前) |
| `GroupCount` | まとまりの総数 (上限で切る前) |
| `IsLimited` | `Limit` で切られた |

- 空値も 1 つのまとまりになります (黙って除きません)
- UTC で保存した日時 (登録日時・更新日時など) は、見ている人のブラウザのタイムゾーンで日・月・年度に区切ります

## 4. BatchAggregator — 複数の集計を一括

条件やまとめる項目が違う複数の集計を、1 回のリクエストにまとめます。
同じ条件・同じ項目なら、1 つの集計に `Count` / `Sum` などを並べる方が効率的です。

```csharp
var byStatus = new ModuleAggregator<Order>();
byStatus.GroupBy(e => e.Status);
byStatus.Count();

var byMonth = new ModuleAggregator<Order>();
byMonth.GroupByMonth(e => e.OrderedOn);
byMonth.Sum(e => e.Amount);

var response = BatchAggregator.Execute(byStatus, byMonth);
var r1 = response.GetAt(0);        // AggregateResult
var r2 = response.GetBy(byMonth);  // AggregateResult
```

| メソッド | 説明 |
|---|---|
| `GetAt(index)` | 渡した順番 (0 始まり) で結果を取得 |
| `GetBy(aggregator)` | 渡した `ModuleAggregator` を指定して結果を取得 (バッチに渡していないものは例外) |

検索 (`ModuleSearcher`) と集計を混ぜて 1 回にはできません (`BatchSearcher` と `BatchAggregator` を別々に呼びます)。

## 5. 表やグラフに出す

組み立てた集計は、そのまま表やグラフに渡せます。

| 渡し先 | 書き方 | ドキュメント |
|---|---|---|
| クロス集計フィールド (Extras) | `CrossTab1.Show(agg, 1)` (先頭 1 個のまとめた項目が行、残りが列) | [CrossTabField](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/CrossTabField.md) |
| 集計チャート (ApexCharts) | `Chart1.Show(agg, 1)` (先頭 1 個がカテゴリ、残りが系列の分割) | [集計チャート](https://github.com/Codeer-Software/Codeer.LowCode.Bindings.Blazor-ApexCharts/blob/main/docs/ApexAggregateChart.md) |

画面に置くだけなら、どちらもデザイナの設定だけで集計できます (スクリプトは不要)。スクリプトは、条件を組み立てて動的に集計を変えたいときに使います。

---

## 関連項目

- [ModuleSearcher / BatchSearcher](script_module_searcher.md)
- [可視化・ダッシュボード](../patterns/visualization_dashboard.md)
- [集計 API の結線 (既存アプリ)](../user_code/aggregate_api.md)
