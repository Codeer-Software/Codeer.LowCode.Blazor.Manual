# ヘッダ詳細 (注文+明細のような 1:N)

**いつ使う**: 注文画面で「注文情報 (1件) + 明細行 (複数)」を一緒に編集したい。請求書、見積書、プロジェクト+タスクなども同様。

## アプリの作り


<!-- 画像参照: Manual の Image/web/patterns/header_detail.png (ここではコメントアウト) -->
- 注文の詳細画面に「明細リスト」がインラインで並んでいる
- 「+ 明細追加」ボタンで行追加、各行で商品/数量/単価を入力
- 親の「保存」ボタン 1 回で **注文 + 全明細が同時保存** される

## 支えるデータ構造

```
orders                     order_details
├── id        PK ←─────────┤ ├── id          PK
├── customer  TEXT          │ ├── order_id    FK → orders.id
├── order_date DATE         └ ├── product     TEXT
└── ...                       ├── qty         INT
                              └── price       NUMBER
```

子側 (`order_details`) が親への FK (`order_id`) を持つ。親側は FK 列を持たない。

## モジュールとテーブルの対応

| モジュール | テーブル | 役割 | 主な参照 |
|---|---|---|---|
| `Order` | `orders` | 親 (ヘッダ) | `Details` (**`ListField`** → `OrderDetail`) で子を内包 |
| `OrderDetail` | `order_details` | 子 (明細) | `OrderId` (**`IdField`**, `IsManualInput:false`) で親 FK を保持 |

> **明細表は `ListField` を使う。`DetailListField` ではない (致命的な誤りやすさ)。** 名前が「明細」っぽいのは `DetailListField` だが、用途は逆。日付・科目・金額… を**列の揃った表**で並べる一般的な明細は `ListField`。詳しくは下の「使い分け」と [CommonMistakes #53](../CommonMistakes.md) を必ず読む。

## CLB ではこう作る

- 親 (`Order`) の Detail Layout に **`ListField`** を置く。`SearchCondition.Condition` に `FieldVariableMatchCondition` (`SearchTargetVariable: "OrderId.Value"` = 子FK / `Variable: "Id.Value"` = 親PK / `Comparison: "Equal"`) を書いて子を逆引き。明細の**列定義は子モジュール (`OrderDetail`) 側の `ListLayouts[""].Elements`** に書く (親ではない)
- 子 (`OrderDetail`) の親 FK は **`IdFieldDesign`** (`IsManualInput: false`、DbColumn=`order_id`)。CLB が親保存時に親 Id を自動でセットする。`NumberField` は不可 ([CommonMistakes #23](../CommonMistakes.md))。候補から選ばせたい設計なら `LinkField` でもよいが、ヘッダ＋明細の正典は `IdField`
- 親の Submit で **子の Add/Update/Delete を 1 トランザクションでまとめて保存** (CLB が自動処理)

### `ListField` と `DetailListField` の使い分け (明細でここを間違えない)

| 子レコードの見え方 | 使うフィールド |
|---|---|
| **列の揃った均一な表** (日付・科目・金額… を 1 行 1 レコードで並べる) ← **ヘッダ＋明細はこれ** | **`ListField`** (子の `ListLayouts` の `Elements` で列定義。列ヘッダー 1 回・行ごとのラベル重複なし) |
| 1 レコードが**複雑なフォーム** (項目が多い・縦組み・項目ごとにラベルが要る) | `DetailListField` (子の `DetailLayouts` で各行をフォーム描画。子は `IsBordered:true` でカード化必須) |
| カード／タイルを**グリッド状**に並べたい | `TileListField` |

> **迷ったら `ListField`。「明細だから DetailListField」と短絡しない。** `DetailListField` は「各行を 1 枚のフォーム/カードにしたい」ときだけ。均一な明細表に使うと行ごとにラベルが重複し冗長になる。正典サンプルでも `Order.Details` は `ListFieldDesign`。

## 標準パターン集の対応

- サイドバー **`データ操作/親子 (1:N)`** → `Order` + `OrderDetail`
- サイドバー **`データ操作/多段ネスト`** → `Project` + `Phase` + `Task` (3 段ネスト)

## 落とし穴

- **明細表に `DetailListField` を選ばない** (この誤りが最頻出)。ヘッダ＋明細の標準は `ListField`。上の「使い分け」を参照
- 親の明細フィールド (`ListField`) の `DeleteTogether = true` で親レコード削除時に子も物理削除される
- 明細の `SearchCondition.LimitCount` は親詳細では `null` (全件)、一覧ページでは `50` 等が定石。`0` は 0 件表示になり明細が消える

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../ModuleDesign.md)
- [Field リファレンス](../Fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
