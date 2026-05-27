# 自動保存 (編集中に勝手に保存)

**いつ使う**: メモアプリ、議事録、入力フォームの一時保存など、**ユーザーが保存ボタンを押さなくても編集が自動的に DB に反映される** UI が欲しいとき。

## アプリの作り

<!-- 画像参照: Manual の Image/web/patterns/auto_save.png (ここではコメントアウト) -->

- 通常のフォーム画面に保存ボタンが**ない**
- タイトル / 本文を入力してフィールドからフォーカスが外れた瞬間 (or 一定時間後) に勝手に保存される
- 「保存しました」表示も出ない (ユーザーは保存を意識する必要なし)

## 支えるデータ構造

```
auto_save_memos
├── id           PK
├── title        TEXT
└── body         TEXT
```

データ的には普通の CRUD。違いはモジュール側の保存処理が UI 操作なしで走る点。

## モジュールとテーブルの対応

| モジュール | テーブル | 主なフィールド |
|---|---|---|
| `AutoSaveMemo` | `auto_save_memos` | `Title` / `Body` などの通常フィールド + `AutoSubmitField` (UI に表示しない裏方フィールド) |

## CLB ではこう作る

- 通常の CRUD モジュールとして定義
- **`AutoSubmitFieldDesign`** をフィールドに 1 つ追加。これがあるとフィールド変更時の自動 Submit が有効になる
- DetailLayout には配置しない (UI 表示用ではなく、自動 Submit を有効化する宣言として置く)
- 通常の SubmitButton は不要

## 標準パターン集の対応

サイドバー **`データ操作/自動保存`** → `AutoSaveMemo`

## 落とし穴

- 楽観ロック (`OptimisticLockingField`) と **自動 Submit (`AutoSubmitField`) は併用不可**。`OptimisticLockingNotSupportedWithAutoSubmit` エラーになる
- 入力中も毎回 DB 通信が走るので、フィールドが多いフォームでは UX/負荷に注意

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../ModuleDesign.md)
- [Field リファレンス](../Fields/) ─ AutoSubmitField の詳細
