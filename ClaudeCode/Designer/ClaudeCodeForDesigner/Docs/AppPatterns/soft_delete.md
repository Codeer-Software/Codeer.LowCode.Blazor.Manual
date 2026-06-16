# 論理削除 (削除フラグで非表示にする)

**いつ使う**: 「削除」ボタンを押しても物理的には消さず、削除フラグを立てるだけ。後から復活できる、または監査要件で履歴が必要な場合。

## アプリの作り


<!-- 画像参照: Manual の Image/web/patterns/soft_delete.png (ここではコメントアウト) -->
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
| `DeletedAtItem` / `DeletedAtItemAdmin` | `deleted_at_items` | 削除時刻 (`DeletedAt`) を自動記録するバリエーション (標準パターン集)。下記参照 |
| `AuditDeleteItem` / `AuditDeleteItemAdmin` | `audit_delete_items` | 削除者 + 削除時刻 (`Deleter` + `DeletedAt`) を自動記録する監査バリエーション (認証パターン集)。下記参照 |

## CLB ではこう作る

- `BooleanFieldDesign` を作って **`Name` を必ず `"LogicalDelete"`** にする (CLB の予約名)
- これだけで CLB の削除ボタンが **`UPDATE SET LogicalDelete = true`** に変わる
- 一覧表示時も `LogicalDelete = false` で自動フィルタされる
- UI には出さず Fields にだけ定義 (一覧で空セルになるので)

## バリエーション: 削除時刻・削除者を自動記録する (DeletedAt / Deleter)

`LogicalDelete` (Boolean) の代わりに、削除の **時刻** と **実行者** を記録する予約名フィールドを置くと、CLB が論理削除時に自動でセットする。Boolean フラグは無くてもよい (`DeletedAt` か `Deleter` のいずれかがあれば論理削除扱いになり、`DeletedAt`／`Deleter` が値を持つ行＝削除済みとして一覧から自動除外される)。

| フィールド名 (予約名) | 型 | 論理削除時の自動動作 |
|---|---|---|
| `DeletedAt` | DateTime | 削除時刻を自動セット |
| `Deleter` | Link → ユーザー (AppUser) | 削除実行者 (ログインユーザー) を自動セット |

- **`DeletedAt` は認証不要**。時刻だけなのでログインの有無に関係なく動く → 標準パターン集に収録
- **`Deleter` は認証が前提**。ログインユーザー (`CurrentUser`) を `AppUser` へのリンクとして記録するため、認証パターン集に収録 (非認証アプリでは実行者を特定できず空になる)
- 管理画面は `LogicalDelete` 版と同じく **別モジュール (同じテーブル) + フィールド名を予約名以外** (`DeletedAtView` / `DeleterView`) にして自動フィルタを回避し、削除済みの「いつ・誰が」を表示する。`DeletedAtView` を空にして更新すると復活する

## 標準パターン集の対応

- サイドバー **`データ操作/論理削除/フラグ`** → `SoftDeleteItem` (一般画面)
- サイドバー **`データ操作/論理削除/フラグ管理`** → `SoftDeleteItemAdmin` (削除済も見える管理画面)
- サイドバー **`データ操作/論理削除/時刻`** → `DeletedAtItem` (一般画面、`DeletedAt` で削除時刻を自動記録)
- サイドバー **`データ操作/論理削除/時刻管理`** → `DeletedAtItemAdmin` (削除済の削除時刻を表示する管理画面)

## 認証パターン集の対応

- サイドバー **`削除監査`** → `AuditDeleteItem` (一般画面、`Deleter` + `DeletedAt` で削除者・時刻を自動記録)
- 管理画面 (`AdminFrame`) サイドバー **`削除監査 (管理)`** → `AuditDeleteItemAdmin` (削除済の「いつ・誰が」を表示)

## 落とし穴

- フィールド名は必ず `LogicalDelete` の綴り。`IsDeleted` 等の任意名だと CLB の自動動作が効かない
- 管理画面用は **別モジュール (同じテーブルを参照)** + Boolean 名を予約名以外にして自動フィルタを回避
- `DeletedAt` は必ず **DateTime**、`Deleter` は必ず **Link (→ AppUser)** で定義する。型が違うと CLB が削除マーカーとして認識せず自動セットされない
- `Deleter` / `DeletedAt` は CLB が論理削除時に自動でセットするので、スクリプトで代入する必要はない (`Creator`/`CreatedAt` と同じ扱い)

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../ModuleDesign.md)
- [Field リファレンス](../Fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
