# 多対多 (記事とタグのような関係)

**いつ使う**: 記事に複数のタグを付ける、ユーザーに複数のロールを割り当てる、商品に複数のキャンペーンを紐付けるなど、**両側が複数のレコードと関連する**操作。

## アプリの作り

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
| `Article` | `articles` | 本体 | `Tags` (`DetailListField` → `ArticleTag`) で中間モジュールを内包 |
| `ArticleTag` | `article_tags` | 中間 (紐付け) | `ArticleId` (Link → `Article`) + `TagId` (Link → `Tag`) |
| `Tag` | `tags` | 相手 (マスタ) | 通常の CRUD モジュール |

## CLB ではこう作る

- 中間モジュール (`ArticleTag`) に **`LinkField` を2つ** (両側への FK)
- 本体 (`Article`) の Detail に `DetailListField` で中間を内包。`SearchCondition` で「`ArticleTag.ArticleId == this.Id.Value`」で逆引き
- ユーザーは中間の Tag フィールドで紐付け先を選択

## PatternShowcase の対応

サイドバー **`データ操作/多対多`** → `Article` + `ArticleTag` + `Tag`

## 関連ドキュメント

- [アプリ作成パターン入口](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../module/module.md)
- [Field リファレンス](../fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
