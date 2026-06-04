# リスト系フィールドの使い分け

複数レコードを表示する 3 種類のフィールド (`ListField` / `DetailListField` / `TileListField`) の使い分けと特性。あわせて、ListField のセルに詳細レイアウトを埋め込む応用を紹介する。

---

## ListField (表形式)

<!-- 画像参照: Manual の Image/web/patterns/list_field.png (ここではコメントアウト) -->

1 行 1 レコードを **テーブル (表) 形式** で表示。最も基本的なリスト表示で、フィールド数が多い・件数が多い場合に向く。各セルは標準でテキスト表示、編集を許すと行内編集も可能。

**標準パターン集の対応**: サイドバー **`リスト/ListField 解説 → `ListFieldOverview``**

---
## DetailListField (フォーム並び)

<!-- 画像参照: Manual の Image/web/patterns/list_detail.png (ここではコメントアウト) -->

1 行 = 子モジュールの DetailLayout 1 枚をまるごと表示。表形式じゃ収まらない複雑な入力フォームを縦に並べたい場合に使います。子モジュールの `DetailLayouts[""]` でカード化 (`IsBordered: true`) 推奨。

**標準パターン集の対応**: サイドバー **`リスト/DetailListField → `DetailListFieldOverview``**

---
## TileListField (タイル並び)

<!-- 画像参照: Manual の Image/web/patterns/list_tile.png (ここではコメントアウト) -->

1 行 = 1 タイル (カード) として **横並び + 折り返し**で配置。商品カタログ・ギャラリー風の見せ方に。`TileWidth` でタイル幅を明示しないと中身がはみ出すケースがあるので、中身の幅合計を計算してから指定する。

**標準パターン集の対応**: サイドバー **`リスト/TileListField 解説 → `TileListFieldOverview``**

---
## セルに詳細レイアウト (ListField の応用)

<!-- 画像参照: Manual の Image/web/patterns/list_detail_in_cell.png (ここではコメントアウト) -->

ListField の一覧レイアウト (ListLayout) の列の **詳細レイアウト (`DetailLayoutName`)** に、行モジュールの DetailLayout 名を指定すると、そのセルの中に詳細レイアウトがまるごと描画される。表の 1 列には収まらない複数フィールドの組み合わせ (状態+期限+担当者+メモ 等) を「詳細」列として 1 列に集約したいときに使う。

- セル用にはコンパクトな専用 DetailLayout を別途用意する (詳細画面用のレイアウトをそのまま指定すると大きすぎることが多い)
- 詳細レイアウトを指定する列ではフィールド名は指定しない。通常の列 (フィールド名指定) と混在できる
- 列の `IsViewOnly` でセル内レイアウトの表示専用/編集可能を個別に制御できる (未指定なら ListField 側の設定に従う)。編集可能にすればセル内の入力欄をそのまま編集して保存できる

**標準パターン集の対応**: サイドバー **`リスト/セル内詳細 → `DetailInListSample``** (行モジュール側は `ListSampleItems` の `DetailLayouts["InListCell"]` + `ListLayouts["DetailInList"]`)

---

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../ModuleDesign.md)
- [Field リファレンス](../Fields/)
