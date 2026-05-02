# Codeer.LowCode.Blazor デザインプロジェクト

このフォルダは Codeer.LowCode.Blazor のデザインプロジェクトです。デザイナと並行して、Claude Code でデザインファイル（JSON / C# スクリプト / SQL / CSS）を直接編集します。

## 参照すべきリファレンス

- **`./ClaudeCodeForDesigner/CLAUDE.md`** — デザインファイル編集の詳細指示書。`./ClaudeCodeForDesigner/Docs/` 配下のリファレンスへのインデックスを兼ねる
- **`./ClaudeCodeForDesigner/Docs/`** — 各仕様の詳細リファレンス（ModuleDesign / Layouts / Fields / PageFrame / Scripts / Enums / AppCss / ProjectSettings / DatabaseGuidelines / CommonMistakes 等）
- **`./Project.md`** — このプロジェクト固有のルール（接続先 DB、命名規約、業務ルール、既存資産等）

## やること

ユーザの自然言語の指示を受けて、以下を作成・編集する：

- モジュール（`*.mod.json`）
- スクリプト（`*.mod.cs`）
- ページフレーム（`*.frm.json`）
- DDL / SQL
- カスタム CSS（`app.css`）

## 守ること

- 編集前に、該当する `./ClaudeCodeForDesigner/Docs/` の仕様を確認する
- `./Project.md` のルールは絶対に守る
- DB 主キーは `INTEGER` (long) + 自動採番が原則。`TEXT`/`VARCHAR` で GUID を保存する設計にしないこと（詳細: `./ClaudeCodeForDesigner/Docs/DatabaseGuidelines.md` および `./ClaudeCodeForDesigner/Docs/CommonMistakes.md` の関連項）
- プロジェクト固有の知見（業務ルール・命名・接続先 DB 等）を作業中に得たら、ユーザに確認のうえ `./Project.md` に追記して蓄積する
