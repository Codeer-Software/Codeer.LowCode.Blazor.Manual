# 多段ネスト (プロジェクト → フェーズ → タスクのような 3 段以上)

**いつ使う**: プロジェクト管理 (Project → Phase → Task)、組織図 (会社 → 部 → 課 → 社員)、ECサイト (注文 → 配送 → 配送アイテム) など、**親→子→孫と 3 段以上の階層**でレコードが連なる構造。

## アプリの作り

<!-- 画像参照: Manual の Image/web/patterns/multi_nested.png (ここではコメントアウト) -->

- プロジェクトの詳細画面を開くと、フェーズの一覧が見える
- フェーズの行 (or 詳細) を開くと、そのフェーズに紐づくタスクが見える
- プロジェクト → フェーズ → タスクの順に深掘りできる

## 支えるデータ構造

```
projects                phases                          tasks
├── id    PK ←─────────┤ ├── id         PK ←──────────┤ ├── id           PK
├── name                │ ├── project_id  FK             │ ├── phase_id     FK
└── owner               ├ ├── phase_name                 ├ ├── title
                        ├ ├── start_date                 ├ ├── assignee
                        └ ├── end_date                   └ ├── is_done
                                                            └ ...
```

3 段ネスト: `projects.id` ← `phases.project_id`、`phases.id` ← `tasks.phase_id`。

## モジュールとテーブルの対応

| モジュール | テーブル | 役割 | 主な参照 |
|---|---|---|---|
| `Project` | `Projects` | 親 (最上位) | `Phases` (`ListField` → `Phase`) で子を逆引き |
| `Phase` | `Phases` | 子 (中間) | `ProjectId` で親を指す + `Tasks` (`ListField` → `Task`) で孫を逆引き |
| `Task` | `Tasks` | 孫 (最深) | `PhaseId` で親を指す |

## CLB ではこう作る

各層に `ListField` を置いて、その下の層を `SearchCondition` で逆引き表示する:

- `Project.Phases` の `SearchCondition`: `Phase.ProjectId == this.Id.Value` で絞り込み
- `Phase.Tasks` の `SearchCondition`: `Task.PhaseId == this.Id.Value` で絞り込み

各層は通常の CRUD モジュール。詳細画面に下層の `ListField` を置くことで、ドリルダウンの形になる。

## 標準パターン集の対応

サイドバー **`データ操作/多段ネスト`** → `Project` + `Phase` + `Task`

## 落とし穴

- 各階層の Submit は基本的にその階層単独 (孫を含む全 Submit ではない)。トランザクション境界を意識する
- 多段でネストが深くなると画面が縦長になりがち。表示形式は**子の見え方**で選ぶ: 列の揃った表は `ListField` (この階層パターンの標準。`CanNavigateToDetail` で行から孫の編集画面へ遷移も可)、各行を 1 枚のフォーム/カードにしたいときだけ `DetailListField`。「明細だから DetailListField」と短絡しない → [CommonMistakes #53](../CommonMistakes.md)

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [ヘッダ詳細 (1:N)](header_detail.md) ─ 2 段の親子関係 (多段ネストの基本形)
- [モジュール定義の全体構造](../ModuleDesign.md)
- [Field リファレンス](../Fields/) ─ ListField / DetailListField の詳細
