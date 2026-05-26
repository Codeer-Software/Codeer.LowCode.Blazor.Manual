# ツリー (カテゴリ階層のような自己参照)

**いつ使う**: カテゴリの階層 (家電 → スマホ → スマホアクセサリ)、組織図 (会社 → 部 → 課)、コメントスレッド (返信のさらに返信) など、**同じ種類のレコード同士で親子関係を表現**する。

## アプリの作り

- 部署一覧で各レコードに「親部署」フィールド
- 親部署は同じ部署マスタから選ぶ (自分自身も部署、相手も部署)
- ルート部署は親部署が空 (NULL)

## 支えるデータ構造

```
departments
├── id          PK
├── name        TEXT
└── parent_id   FK → departments.id (自己参照、NULL 許容)
```

同じテーブルが自分自身を参照する。ルートは `parent_id` が NULL。

## モジュールとテーブルの対応

| モジュール | テーブル | 主な参照 |
|---|---|---|
| `Department` | `departments` | `Parent` (`LinkField` → 同じ `Department` モジュール) で自己参照 |

## CLB ではこう作る

`LinkField` の `SearchCondition.ModuleName` に **自モジュール名 (自分自身)** を指定。`parent_id` が NULL を許容する設計に。

## 標準パターン集の対応

サイドバー **`データ操作/階層構造`** → `Department`

## 落とし穴

- ツリービュー表示は標準ビルトインでは無いので、Query フィールド + 再帰 CTE などで階層を組み立てる
- 自分が自分の祖先になる循環を防ぐバリデーションはアプリ側で書く

## 関連ドキュメント

- [アプリ作成パターン入口](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../module/module.md)
- [Field リファレンス](../fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
