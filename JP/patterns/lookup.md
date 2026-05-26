# マスタ参照 (商品にカテゴリを紐付けるような 多対1)

**いつ使う**: 商品にカテゴリを紐付け、注文に顧客を紐付け、社員に部署を紐付けるなど、**他のマスタから 1 件選んで参照する**操作。

## アプリの作り

- 商品の詳細画面で「カテゴリ」フィールドの 🔍 ボタンを押すと検索ダイアログ
- ダイアログでカテゴリ一覧から 1 件選択
- 商品レコードにそのカテゴリの ID が保存され、画面にはカテゴリ名が表示される

## 支えるデータ構造

```
products                   categories
├── id                     ├── id        PK
├── name                   ├── name      TEXT
├── category_id  FK ─────→ └── ...
└── ...
```

参照する側 (`products`) が FK 列 (`category_id`) を持つ。

## モジュールとテーブルの対応

| モジュール | テーブル | 役割 | 主な参照 |
|---|---|---|---|
| `Product` | `products` | 参照する側 | `Category` (`LinkField` → `Category`) で FK を保持 |
| `Category` | `categories` | 参照される側 (マスタ) | 通常の CRUD モジュール |

## CLB ではこう作る

参照する側 (`Product`) に `LinkField` を1つ追加:
- `DbColumn`: 外部キー列名 (`category_id`)
- `SearchCondition.ModuleName`: 参照先モジュール名 (`Category`)
- `ValueVariable`: `Id.Value`
- `DisplayTextVariable`: `Name.Value` (画面表示用)

## 標準パターン集の対応

`LookupCustomer` などで `LinkField` の使用例。`Article` モジュールでも使われている。

## 落とし穴

- 参照先モジュールが PageFrame に登録されてないと、検索ダイアログで開けないことがある
- 表示用に参照先の他フィールドを引きたいとき (例: カテゴリ名 + カテゴリの説明) は、親モジュールの `LinkFieldNames` にパス追加 + レイアウトで参照する

## 関連ドキュメント

- [アプリ作成パターン入口](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../module/module.md)
- [Field リファレンス](../fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
