# 双方向 ID 持ち合い (会社↔代表者・申請↔承認フローのような相互参照)

**いつ使う**: 1:1 で対応する2つのレコードが **互いを参照し合う** 関係。

具体例:
- 会社 ↔ 代表者
- 注文 ↔ 領収書
- ユーザー ↔ 拡張プロフィール
- 申請 ↔ 承認フロー

純粋な DB 設計だけ見ると 1:1 は片方向 FK が定石ですが、CLB の `LinkField` / `ModuleField` が **単方向 FK 前提**で動くため、**両側から相手を参照したい**ケースでは双方向 FK にしておく方が CLB の機能を素直に活かせます。

## アプリの作り

申請 ↔ 承認フローを例にすると:
- 申請の詳細画面に **承認フローが ModuleField で内包**されている (申請 → 承認フロー方向)
- 承認待ち一覧から「申請を開く」ボタンで申請画面に戻れる (承認フロー → 申請方向)
- 両方の画面でお互いのフィールドが見える

## 支えるデータ構造

```
mutual_main                       mutual_sub
├── id        PK ←───────────────┤ ├── id        PK
├── name                          ├── name
└── sub_id    FK → mutual_sub.id  └── main_id    FK → mutual_main.id  ←┐
                                                                        │
        (mutual_main.sub_id) ───────────────────────────────────────────┘
```

両側に相手への FK 列がある。

## モジュールとテーブルの対応

| モジュール | テーブル | 役割 | 主な参照 |
|---|---|---|---|
| `MutualMain` | `mutual_main` | 親 | `SubSlot` (`ModuleField` → `MutualSub`, `DbColumn=sub_id`) で子を内包 |
| `MutualSub` | `mutual_sub` | 子 | `Main` (`LinkField` → `MutualMain`, `DbColumn=main_id`) で親を逆参照。`DataOnlyFields` に入れて UI には出さない |

## CLB ではこう作る

- 親に `ModuleField`、子に `LinkField` を置いて両側に FK 列を持たせる
- 親の `.mod.cs` の `OnAfterInitialization` で **新規時に子の親参照に自分のテンポラリ ID をセット**:

```csharp
void OnAfterInitialization()
{
    if (IsNewData)
    {
        SubSlot.ChildModule.Main.Value = this.Id.Value;
    }
}
```

- 親の Submit を 1 回押すと、CLB が **片方を NULL Insert → 後追い UPDATE で実 ID を埋める**ことで双方向サイクルを自動解決。アプリ側コードに特別な配慮は不要

## DB の前提条件

- 双方向の FK 列のどちらか (もしくは両方) が **NULL 許容**であること (片方を NULL Insert するため)。NOT NULL 制約が両方に付いてると CLB の解決ロジックでも DB エラーになる

## 標準パターン集の対応

サイドバー **`データ操作/双方向ID (1:1)`** → `MutualMain` + `MutualSub`

## 落とし穴

- 「親 (Main) の Submit ボタン」だけで両方保存される。子側に Submit ボタンを置く必要なし
- 子の `Main` フィールドを `DataOnlyFields` に入れず UI に出してしまうと、ユーザーが手動で親を選ぶ必要があるように見えて混乱しやすい

## 関連ドキュメント

- [アプリ作成パターン入口](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../module/module.md)
- [Field リファレンス](../fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
