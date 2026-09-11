# Project.md（プロジェクト固有ルール）

このデザインプロジェクト固有の前提を書く（ユーザー所有・ワークスペース更新で上書きされない）。Claude Code は毎回これを読んで従う。`ClaudeCodeForDesigner/` の汎用ルールはここには書かない（プロジェクト固有のことだけ）。

## 接続先 DB / データソース

- `Main` = Azure Database for PostgreSQL の `lowcode_demo`（LowCodeSamples のデモ DB と共用。デモ用なので `AllowCliSqlAccess: true`。ただし LowCodeSamples が使う既存テーブルは変更しない）

## デプロイ先 / 動作確認

- `local` = `Local/Designs`（FileSystem、`AllowCliDeploy: true`。ローカルの `dotnet run` サーバーが読む）
- `azure` = App Service（FTPS）。CLI からは送らない。デザイナの「送信」で App.zip を置き、`UseHotReload=false` なので App Service を再起動する
- （動作確認サーバーの起動方法。ホスト同居なら `dotnet run` の対象プロジェクト。URL 自体はマシン固有なので `LocalEnvironment.md` の `ServerUrl:` に書く）

## 命名規約

- （テーブル / 列 / モジュール / フィールドの命名ルール。`ClaudeCodeForDesigner/Docs/DatabaseGuidelines.md` の標準と違う点があれば明記）

## 業務ルール

- iPad Air（横 1180×820 / 縦 820×1180）で MAUI アプリとして見せる営業支援（SFA）デモ。ナビはサイドバーではなく**ヘッダ型**（`Main.frm.json` の `Header.IsVisible: true` / `Left.IsVisible: false`）
- モジュールは SFA テンプレート（`ClaudeCodeForDesigner/_samples/SFA`）由来。データソースは `Main`、クエリ SQL は PostgreSQL 版（`../LowCodeSamples/Design/Modules/SFA/クエリ`）に差し替え済み。`Home.mod.cs` の対象年月は `yyyy-MM` 形式（PostgreSQL の `to_char` に合わせる）
- ダッシュボード（Home）の 1〜2 行目と商談詳細の 1 行目は縦向きで折り返す（`IsWrap` + `MinWidth`）ように変更済み

## 既存資産

- ログインユーザーは `AppUser` モジュール → `Main` の `native_app_users`（id SERIAL / user_name / name / hash / salt）。2026-09-11 に作成。`app_users`（id / name / email）は LowCodeSamples の承認サンプル用で別物なので使わない
- ユーザーが 0 件のときサーバー起動時に `admin` / `admin` が自動作成される（`Server/CookieAuthentication.cs`）

## 作業中に得た知見（追記していく）

- 2026-09-11: 当月のダッシュボードを埋めるため `deal` に 12 件・`activity` に 20 件のデモデータを追加（`deal.memo` が `[iPadデモ]` で始まる行。消すときはこの条件で削除）
