# 論理削除 (削除フラグで非表示にする)

**いつ使う**: 「削除」ボタンを押しても物理的には消さず、削除フラグを立てるだけ。後から復活できる、または監査要件で履歴が必要な場合。

## アプリの作り

- 一般ユーザー画面: 「削除」ボタンを押すとレコードが見えなくなる (一覧から消える)
- 管理画面: 削除済みレコードも表示。フラグを戻して復活できる

## 支えるデータ構造

```
soft_delete_items
├── id            PK
├── name          TEXT
└── LogicalDelete BOOLEAN  ← CLB 予約名 (この綴りでないと動かない)
```

## モジュールとテーブルの対応

| モジュール | テーブル | 役割 |
|---|---|---|
| `SoftDeleteItem` | `soft_delete_items` | 一般ユーザー用。`LogicalDelete` 予約名で論理削除自動動作 |
| `SoftDeleteItemAdmin` | `soft_delete_items` (同じテーブル) | 管理用。Boolean 名を予約名以外 (例: `DeletedFlag`) にして自動フィルタを回避 |

## CLB ではこう作る

- `BooleanFieldDesign` を作って **`Name` を必ず `"LogicalDelete"`** にする (CLB の予約名)
- これだけで CLB の削除ボタンが **`UPDATE SET LogicalDelete = true`** に変わる
- 一覧表示時も `LogicalDelete = false` で自動フィルタされる
- UI には出さず Fields にだけ定義 (一覧で空セルになるので)

## 標準パターン集の対応

- サイドバー **`データ操作/論理削除`** → `SoftDeleteItem` (一般画面)
- サイドバー **`データ操作/論理削除(管理)`** → `SoftDeleteItemAdmin` (削除済も見える管理画面)

## 落とし穴

- フィールド名は必ず `LogicalDelete` の綴り。`IsDeleted` 等の任意名だと CLB の自動動作が効かない
- 管理画面用は **別モジュール (同じテーブルを参照)** + Boolean 名を予約名以外にして自動フィルタを回避

## 関連ドキュメント

- [アプリ作成パターン入口](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../module/module.md)
- [Field リファレンス](../fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
