# マスタ参照 (店舗に業態を紐付けるような 多対1)

**いつ使う**: 商品にカテゴリを紐付け、注文に顧客を紐付け、店舗に業態を紐付けるなど、**他のマスタから 1 件選んで参照する**操作。

## アプリの作り

<!-- 画像参照: Manual の Image/web/patterns/lookup.png (ここではコメントアウト) -->

- 店舗の詳細画面で「業態」フィールドの 🔍 ボタンを押すと検索ダイアログ
- ダイアログで業態マスタから 1 件選択 (例: 「飲食店」)
- 店舗レコードにその業態の ID が保存され、画面には業態名が表示される

## 支えるデータ構造

```
shops                          shop_types
├── id                         ├── id        PK
├── name                       ├── name      TEXT
├── shop_type_id  FK ─────────→└── ...
└── ...
```

参照する側 (`shops`) が FK 列 (`shop_type_id`) を持つ。

## モジュールとテーブルの対応

| モジュール | テーブル | 役割 | 主な参照 |
|---|---|---|---|
| `Shop` | `shops` | 参照する側 | `ShopTypeRef` (`LinkField` → `ShopType`) で FK を保持 |
| `ShopType` | `shop_types` | 参照される側 (マスタ) | 通常の CRUD モジュール |

## CLB ではこう作る

参照する側 (`Shop`) に `LinkField` を1つ追加:
- `DbColumn`: 外部キー列名 (`shop_type_id`)
- `SearchCondition.ModuleName`: 参照先モジュール名 (`ShopType`)
- `ValueVariable`: `Id.Value`
- `DisplayTextVariable`: `Name.Value` (画面表示用)

## 標準パターン集の対応

サイドバー **`データ操作/マスタ参照`** → `Shop` + `ShopType`

## 落とし穴

- 参照先モジュールが PageFrame に登録されてないと、検索ダイアログで開けないことがある
- 表示用に参照先の他フィールドを引きたいとき (例: 業態名 + 業態の説明) は、参照する側モジュールの `LinkFieldNames` にパス追加 + レイアウトで参照する

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../ModuleDesign.md)
- [Field リファレンス](../Fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
