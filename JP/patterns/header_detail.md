# ヘッダ詳細 (注文+明細のような 1:N)

**いつ使う**: 注文画面で「注文情報 (1件) + 明細行 (複数)」を一緒に編集したい。請求書、見積書、プロジェクト+タスクなども同様。

## アプリの作り

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
| `Order` | `orders` | 親 (ヘッダ) | `Details` (`DetailListField` → `OrderDetail`) で子を内包 |
| `OrderDetail` | `order_details` | 子 (明細) | `OrderId` (`LinkField` → `Order`) で親を参照 |

## CLB ではこう作る

- 親 (`Order`) の Detail Layout に **`DetailListField`** を置く。`SearchCondition` で「`OrderId.Value == this.Id.Value`」の条件を書いて子を逆引き
- 子 (`OrderDetail`) には `LinkField` (DbColumn=`order_id`) で親への FK を持たせる
- 親の Submit で **子の Add/Update/Delete を 1 トランザクションでまとめて保存** (CLB が自動処理)

## 標準パターン集の対応

- サイドバー **`データ操作/親子 (1:N)`** → `Order` + `OrderDetail`
- サイドバー **`データ操作/多段ネスト`** → `Project` + `Phase` + `Task` (3 段ネスト)

## 落とし穴

- 子の `DetailListField.DeleteTogether = true` で親レコード削除時に子も物理削除される
- 子の `SearchCondition.LimitCount` は親詳細では `null` (全件)、一覧ページでは `50` 等が定石

## 関連ドキュメント

- [アプリ作成パターン入口](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../module/module.md)
- [Field リファレンス](../fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
