# 可視化・ダッシュボード (ガント / タスクボード / グラフ)

**いつ使う**: 業務データを表ではなく、ガントチャートのバー・カンバンのカード・グラフで見せたい。

- プロジェクトのタスクをガントチャートで見せ、バーのドラッグで日程を変える
- タスクをステータス列のカードで並べ、ドラッグでステータスを変える
- ホーム画面に月別推移・ステータス別件数などのグラフを並べる

使うフィールドはどれも拡張ライブラリのものです (アプリテンプレートには最初から組み込まれています)。

| フィールド | ライブラリ | ドキュメント |
|---|---|---|
| GanttField (ガントチャート) | Codeer.LowCode.Blazor.Extras | [GanttField](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/GanttField.md) |
| TaskBoardField (タスクボード) | Codeer.LowCode.Blazor.Extras | [TaskBoardField](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/TaskBoardField.md) |
| チャート / 横棒チャート / ラジアルチャート | Codeer.LowCode.Bindings.Blazor-ApexCharts | [チャート](https://github.com/Codeer-Software/Codeer.LowCode.Bindings.Blazor-ApexCharts/blob/main/docs/ApexChart.md) / [横棒チャート](https://github.com/Codeer-Software/Codeer.LowCode.Bindings.Blazor-ApexCharts/blob/main/docs/ApexHBarChart.md) / [ラジアルチャート](https://github.com/Codeer-Software/Codeer.LowCode.Bindings.Blazor-ApexCharts/blob/main/docs/ApexRadialChart.md) |

実物は [プロジェクト管理テンプレート](../templates/project_management.md) で確認できます (プロジェクト詳細にガントとタスクボード、ホームにグラフ)。

---

## 1. ガント / タスクボードを親レコードの詳細に埋め込む

いちばん確実な使い方です。[ヘッダ詳細 (1:N)](header_detail.md) と同じ親子構造で、明細を「表」ではなく「バー」や「カード」で見せる版と考えます。

### 支えるデータ構造

```
projects                 tasks
├── id  PK  ◀─────────── ├── project_id  FK
└── name                 ├── id  PK
                         ├── name
                         ├── start_date / end_date
                         ├── progress
                         ├── status
                         └── sort_index
```

### CLB ではこう作る

- 親モジュール (`Project`) の詳細レイアウトに GanttField / TaskBoardField を置く
- 検索条件の対象モジュールに子モジュール (`Task`) を指定し、条件で「子の FK = 親の `Id.Value`」に絞る (ListField の明細と同じ設定)
- 子モジュールには親への FK フィールド (IdField か LinkField) を持たせる。バー・カードを追加すると、この条件から親の値が FK に入る
- ガントとボードは値を持たないフィールドで、子レコードの変更はフィールドの中に保持され、**親モジュールの保存でまとめて保存**される (ListField と同じ)
- バー・カードをクリックしたときの編集ダイアログには、子モジュールの詳細レイアウトを指定する (GanttField は `DetailLayoutName`、TaskBoardField は `PopupLayoutName`)
  - このレイアウトには保存ボタンを置かない。ダイアログの OK で確定した内容が、親の保存で DB に書かれる
  - 親への FK と並び順は画面に出さず、データとして保持するフィールドにする

### タスクボードのステータス列

- ステータス列は `Statuses` に並べる。各列の `Value` と、`StatusField` (SelectField か TextField) の値の文字列が一致したカードがその列に入る
- `StatusField` に SelectField を使う場合は、選択肢の値と各列の `Value` を揃える
- 並べ替えを保存するには `SortIndexField` (NumberField) を設定する

## 2. 高さの出し方

- ガント・タスクボードは、置いた列の直上のレイアウトを `IsFillAvailable` にして最終行に置くと、画面の残りの高さを使い切る
- 1 画面にガントとボードを縦に並べるなど、高さを使い切るフィールドを複数置くときは、それぞれの行に固定の高さを指定する
- グラフは配置したセルの高さいっぱいに描画されるので、グラフを置く行に十分な高さを指定する (タブやカードの中に置くときは特に)

詳しくは [画面レイアウトのパターン](layout_patterns.md) を参照してください。

## 3. 全件を 1 枚で見る横断ビューは閲覧専用にする

「全プロジェクトのタスクを 1 枚のガントで」のような、DB テーブルを持たない画面専用モジュールに埋め込んだガント・ボードは、**ドラッグで動かしても保存されません** (保存は埋め込み先の親モジュールの保存に乗るため)。
横断ビューは閲覧専用と割り切り、編集は親レコードの詳細 (1 の形) で行います。

## 4. ダッシュボード (集計クエリ + グラフ)

グラフは「取得したレコードの項目をそのまま系列にする」フィールドです。月別推移・ステータス別件数などは、生のテーブルを直接渡さず、**SQL で集計した読み取り専用のモジュールを用意して、それをグラフのデータ元にします**。

### モジュール構成 (2 層)

| モジュール | テーブル | 主な設定 |
|---|---|---|
| 集計クエリモジュール (例: `MonthlyProduction`)。グラフ 1 つにつき 1 つ | なし | [QueryField](../db/query_field.md) に `GROUP BY` の SELECT を書き、出力列に対応するフィールド (Text / Number など) を定義する |
| ダッシュボード (例: `Home`) | なし | グラフのフィールドを並べ、それぞれの検索条件の対象モジュールに集計クエリモジュールを指定する |

```sql
-- 集計クエリの例 (月別の計画数量・実績数量)
SELECT month AS 月, SUM(planned) AS 計画数量, SUM(actual) AS 実績数量
FROM production
GROUP BY month
ORDER BY month
```

### グラフの設定

| グラフ | カテゴリ (軸・ラベル) | 値 |
|---|---|---|
| チャート (棒 / 折れ線 / 面 など) | カテゴリフィールド | 系列 (複数可。系列ごとに種類・色を指定) |
| 横棒チャート | カテゴリフィールド | 系列 (複数可) |
| ラジアルチャート (ドーナツ / 円 / ポーラエリア) | カテゴリフィールド | 系列フィールド (1 つ)。系列種別を必ず選ぶ (既定のままだと縦棒で描画される) |

- カテゴリフィールド・系列が指すのは、集計クエリモジュールの**フィールド名** (DB の列名ではない)
- 保存などでデータが変わった直後にグラフを更新するには、スクリプトからグラフの `Reload()` を呼ぶ ([スクリプト API](https://github.com/Codeer-Software/Codeer.LowCode.Bindings.Blazor-ApexCharts/blob/main/docs/Scripting.md))

## 落とし穴

- ガント・ボードの子モジュールにはデータソースの指定が必要 (無いと何も読み込まれない)
- ガント・ボードの編集ダイアログのレイアウトに保存ボタンを置かない
- DB テーブルを持たない画面に埋め込んだガント・ボードの編集は保存されない
- グラフに生のテーブルを直接渡すと集計されない。集計はクエリで行う

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [ヘッダ詳細 (1:N)](header_detail.md) / [多段ネスト](multi_nested.md)
- [Query フィールド](../db/query_field.md)
- [プロジェクト管理テンプレート](../templates/project_management.md)
- [ApexCharts バインディング](https://github.com/Codeer-Software/Codeer.LowCode.Bindings.Blazor-ApexCharts)
