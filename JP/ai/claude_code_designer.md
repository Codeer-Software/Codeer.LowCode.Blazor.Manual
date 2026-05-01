# Claude Code でデザインプロジェクトを編集する

[Claude Code](https://docs.claude.com/ja/docs/claude-code/overview) は Anthropic が提供する CLI ベースの AI 開発支援ツールです。Codeer.LowCode.Blazor のデザインファイル（`*.mod.json` / `*.mod.cs` / `*.frm.json` / SQL / app.css 等）はすべてテキストベースのため、Claude Code から直接読み書きできます。

「自然言語で指示 → モジュール／レイアウト／スクリプトを作成・編集」する流れで、デザイナでの操作と組み合わせて使うことを想定しています。

リファレンスとなる仕様書は本マニュアルの GitHub リポジトリに公開されているので、ローカルにダウンロードする必要はありません。Claude Code が必要に応じて GitHub から直接取得します。

## 前提

- Codeer.LowCode.Blazor のデザイナがインストール済みであること（[入手とライセンス](../introduction/installation.md)）
- Claude Code がインストール済みであること（[公式インストール手順](https://docs.claude.com/ja/docs/claude-code/setup)）
- Anthropic API キーまたは Claude Pro/Max のサブスクリプション

## 手順

### 1. 作業フォルダを作成する

任意の場所に作業フォルダを作成します。本マニュアルでは `C:\work\Test` を例として使用します。

### 2. デザイナでデザインプロジェクトを作成する

デザイナを起動し、`File` → `New Project` を選択してプロジェクトを作成します。

- **Name**: 任意（例: `Design`）
- **Folder**: 1. で作った作業フォルダ（例: `C:\work\Test`）
- **Template**: 任意（例: `Empty`）

<img width="500" src="../../Image/claude_code_designer_create_project.png">

### 3. 作業フォルダで Claude Code を起動する

```bash
cd C:\work\Test
claude
```

### 4. 起動直後に下記プロンプトを送る

リファレンスの場所を Claude Code に伝えるため、最初のメッセージで下記を貼り付けて送信します（コピペで OK）。

```
このフォルダは Codeer.LowCode.Blazor のデザインプロジェクトです。
デザインファイル編集の指示書が下記にあるので WebFetch で取得して内容に従ってください。

- プロジェクトガイド: https://raw.githubusercontent.com/Codeer-Software/Codeer.LowCode.Blazor.Manual/main/Claude/CLAUDE.md
- 編集の詳細指示: https://raw.githubusercontent.com/Codeer-Software/Codeer.LowCode.Blazor.Manual/main/Claude/ClaudeCodeForDesigner/CLAUDE.md

詳細リファレンス（Docs/ 以下: ModuleDesign / Layouts / Fields / PageFrame / Scripts / Enums / AppCss 等）は必要に応じて以下から取得してください:

ベース URL: https://raw.githubusercontent.com/Codeer-Software/Codeer.LowCode.Blazor.Manual/main/Claude/ClaudeCodeForDesigner/Docs/
ファイル一覧: https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Manual/tree/main/Claude/ClaudeCodeForDesigner/Docs
```

このプロンプトをスニペットとして保存しておき、毎セッションの最初に貼り付けると効率的です。

## 使い方の例

準備が整ったら自然言語で指示を出します。

- 「商品マスタのモジュールを作って。フィールドは商品コード、商品名、価格、在庫数」
- 「`Customer` モジュールに `登録日` の Date フィールドを追加して」
- 「`Order` 一覧で `合計金額` が 10000 円以上の行を赤背景にするスクリプトを書いて」
- 「サイドバーに `売上集計` ページを追加して、新しい `SalesSummary` モジュールにナビゲートさせて」

Claude Code はデザインファイルを直接編集します。デザイナのホットリロードが有効な状態であれば、保存と同時に Web アプリ側に反映されます。

## データベース

### テーブル作成も Claude Code に任せられる

モジュールが必要とするテーブルの DDL や、初期データ投入の INSERT 文も自然言語で指示すれば Claude Code が生成・実行してくれます。例:

- 「商品マスタモジュールに対応するテーブルを SQLite に作って。サンプルデータも 5 件入れて」
- 「`Order` モジュールに `Status` 列を追加して既存テーブルにマイグレーションして」
- 「`Customer` テーブルから 100 件のテストデータを生成して INSERT 文を書いて」

モジュール定義（`*.mod.json`）と DB スキーマを両方触れるので、フィールド追加 → 列追加までを一気に指示できます。

### サポート DB

新規プロジェクトはテンプレートに同梱されているサンプルの **SQLite** に接続された状態で立ち上がりますが、以下のデータベースをすべてサポートしています。

- Microsoft SQL Server
- MySQL
- Oracle Database
- PostgreSQL
- SQLite

接続先の切り替えも Claude Code に指示できます。例:

- 「DataSource を SQL Server に切り替えて。接続文字列は `designer.settings.Development.json` に置いて」
- 「PostgreSQL 用のサンプルデータベースに接続するように変更して」

`designer.settings.json` / `designer.settings.Development.json` / サーバープロジェクトの `appsettings.json` の編集と、必要に応じた DDL 方言の調整までやってくれます。

> 設定ファイルの仕様詳細は [designer.settings](../designer/designer_settings.md) を参照（Claude Code は同等の仕様を `Claude/ClaudeCodeForDesigner/Docs/ProjectSettings.md` から自動取得）。

## ヒント

- **デザイナと並行して使う**: ビジュアルで配置を細かく調整するのはデザイナ、フィールド・スクリプトの一括追加や繰り返し作業は Claude Code、と使い分けると効率的です。
- **指示は具体的に**: 「いい感じに」より「Title フィールドを追加し、ListLayout にも表示」など、対象モジュール・フィールド名・配置先を明示するほうが意図通りに動きます。
- **生成結果は必ず確認**: スクリプトや SQL は実行前にデザイナでプレビューする / バージョン管理にコミットして差分を確認する運用を推奨します。
- **オンライン参照のメリット**: リファレンスが更新されると自動で最新を参照できます。逆にオフライン環境では使えないので、必要なら `Docs/` を手動でダウンロードしておいてください（[GitHub のフォルダから zip ダウンロード](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Manual/tree/main/Claude/ClaudeCodeForDesigner)）。

## 関連情報

- [Codeer.LowCode.Blazor とは](../introduction/what_is_lowcode.md)
- [デザイナ概要](../designer/designer.md)
- [AI 概要](ai_overview.md)
- [AI でモジュールを作成（デザイナ内蔵 AI）](ai_modules.md)
- [GitHub: ClaudeCodeForDesigner](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Manual/tree/main/Claude/ClaudeCodeForDesigner)
