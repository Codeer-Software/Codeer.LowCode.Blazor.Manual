# 単一 CRUD (1 画面で完結する基本アプリ)

**いつ使う**: 商品マスタ・社員マスタ・取引先一覧など、**1 種類のレコードを登録・閲覧・編集・削除する**基本形。

## アプリの作り


<img src="../../Image/web/patterns/single_crud.png" alt="商品一覧画面 (単一CRUDの例)" style="border: 1px solid #ccc;" width="800">
- サイドバーから「商品」を開くと一覧画面
- 一覧から行を開くと詳細画面 (登録/編集も同じ画面)
- 「新規」ボタンで新規登録、「保存」ボタンで保存、「削除」で削除

## 支えるデータ構造

```
products
├── id          PK
├── code        TEXT
├── name        TEXT
└── price       NUMBER
```

## モジュールとテーブルの対応

| モジュール | テーブル | 主なフィールド ↔ DB列 |
|---|---|---|
| `Product` | `products` | `Id` (IdField) ↔ `id` / `Code` (Text) ↔ `code` / `Name` (Text) ↔ `name` / `Price` (Number) ↔ `price` |

## CLB ではこう作る

1モジュール = 1テーブル。`DataSourceName` + `DbTable` を指定し、`IdFieldDesign` + 各カラムに対応する Field + `SubmitButton` を並べる。

## 標準パターン集の対応

サイドバー **`データ操作/CRUD`** → `Product` モジュール

## 関連ドキュメント

- [アプリ作成パターン入口](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../module/module.md)
- [Field リファレンス](../fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
