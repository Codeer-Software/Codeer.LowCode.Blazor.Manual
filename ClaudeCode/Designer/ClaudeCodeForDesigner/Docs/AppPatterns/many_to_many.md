# 多対多 (記事とタグのような関係)

**いつ使う**: 記事に複数のタグを付ける、ユーザーに複数のロールを割り当てる、商品に複数のキャンペーンを紐付けるなど、**両側が複数のレコードと関連する**操作。

## アプリの作り


<!-- 画像参照: Manual の Image/web/patterns/many_to_many.png (ここではコメントアウト) -->
- 記事の詳細画面で「タグ追加」ボタンを押す
- タグ検索ダイアログから 1 件選んで追加
- 記事画面に紐付いたタグがリストで並ぶ。不要なタグは削除可能
- 親の保存ボタンで紐付け情報も一緒に保存

## 支えるデータ構造

```
articles            article_tags                tags
├── id    PK ←─────┤├── id          PK         ├── id    PK
├── title           ├── article_id   FK → articles.id    ├── name
└── ...             ├── tag_id       FK → tags.id ───────┘
                    └── ...
```

中間テーブル (`article_tags`) が両側への FK を持つ。

## モジュールとテーブルの対応

| モジュール | テーブル | 役割 | 主な参照 |
|---|---|---|---|
| `Article` | `articles` | 本体 | `Tags` (**`ListField`** → `ArticleTag`) で中間モジュールを内包 |
| `ArticleTag` | `article_tags` | 中間 (紐付け) | `ArticleId` (**`IdField`** ← 本体 FK) + `TagLink` (**`LinkField`** → `Tag`) |
| `Tag` | `tags` | 相手 (マスタ) | 通常の CRUD モジュール |

## CLB ではこう作る

- 中間モジュール (`ArticleTag`) は、本体への FK を **`IdFieldDesign`** (`ArticleId`, `IsManualInput:false`)、相手マスタへの参照を **`LinkFieldDesign`** (`TagLink`) で持つ。本体保存時に `ArticleId` は CLB が自動セット
- 本体 (`Article`) の Detail に **`ListField`** (表形式) で中間を内包。`SearchCondition.Condition` の `FieldVariableMatchCondition` (`SearchTargetVariable: "ArticleId.Value"` / `Variable: "Id.Value"` / `Equal`) で逆引き。**`DetailListField` ではない** ([CommonMistakes #53](../CommonMistakes.md))
- ユーザーは各行の Tag リンク (`TagLink`) で紐付け先を選択
- 正典: `Samples/PatternShowcase/App/Modules/Article.mod.json` (`Tags`＝`ListFieldDesign`) + `ArticleTag.mod.json` (`ArticleId`＝`IdFieldDesign` / `TagLink`＝`LinkFieldDesign`)

## 標準パターン集の対応

サイドバー **`データ操作/多対多`** → `Article` + `ArticleTag` + `Tag`

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../ModuleDesign.md)
- [Field リファレンス](../Fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
