# 状態遷移を持つレコード (申請のステータス管理)

**いつ使う**: 申請の状態 (下書き → 申請中 → 承認済)、注文の進行状態 (受注 → 出荷 → 完了) など、**特定の状態しか取らず、状態間の遷移にルールがある**レコード。

## アプリの作り

- 詳細画面に「状態」フィールド (表示のみ)
- 現在状態に応じて操作ボタンが出し分けされる ("下書き"なら「申請」ボタン、"申請中"なら「承認」「却下」ボタン等)
- ボタンを押すと状態が変わって保存される

## 支えるデータ構造

```
leave_requests
├── id           PK
├── user
├── start_date
├── status       TEXT  ("Draft" / "Submitted" / "Approved" / "Rejected")
└── ...
```

`status` 列に有限の値だけを入れる。

## モジュールとテーブルの対応

| モジュール | テーブル | 主な参照 |
|---|---|---|
| `LeaveRequest` | `leave_requests` | `Status` (`SelectField`, Candidates で状態と値の対応を定義) + 状態遷移ボタン (`ButtonField`) |

## CLB ではこう作る

- `SelectFieldDesign` で `Candidates` に `"下書き,Draft"` / `"申請中,Submitted"` / `"承認済,Approved"` 等を定義
- 状態遷移ボタンの `OnClick` スクリプトで `Status.Value = "Submitted"` のように更新
- ボタンの `IsVisible` を現在状態によって出し分け、不正な遷移を UI レベルで防ぐ
- スクリプト側にも `if (Status.Value != "Draft") { return; }` のようなガードを入れる (UI 回避された場合の保険)

## 標準パターン集 / 認証付きパターン集の対応

標準パターン集には専用サンプルなし。承認フロー付きの完全例は **認証付きパターン集** テンプレート (内部名 `PatternShowcaseAuth`) の `LeaveRequest` / `ExpenseRequest` + `ApprovalFlow` を参照。

## 関連ドキュメント

- [アプリ作成パターン入口](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../module/module.md)
- [Field リファレンス](../fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
