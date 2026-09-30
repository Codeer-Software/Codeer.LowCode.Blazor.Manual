# AI
Codeer.LowCode.Blazorは生成AI機能をシームレスに取り込むことがだき、さまざまな使用シーンで効率を上げ、ユーザに目新しい機能と高い利便性を提供します。

例えば以下のようにAIを活用することができます：
- 写真あるいはテキストを取り込み、画面の項目に合わせてデータを反映：例えば名刺や請求書等の書類取り込み機能
- 開発サポート：例えばクエリやSQL文の生成等

AIの使用方法はAIカテゴリの中の各ページをご参照ください。

## アプリに組み込むAI機能 (Extras)

アプリの画面に置くAI機能は、拡張ライブラリ [Codeer.LowCode.Blazor.Extras](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras) のFieldとして提供しています。アプリテンプレートには最初から組み込まれています。

| Field | 内容 |
|---|---|
| [AITextAnalyzerField](AITextAnalyzerField.md) | 帳票ファイルや自由テキストをAIで解析し、モジュールのフィールドへ自動入力する |
| [AIChatField](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/AIChatField.md) | AIとのチャット画面。標準のAgentはアプリのデータベースを読んで、集計やグラフで答える |
| [SemanticSearchField](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/SemanticSearchField.md) | 行を「内容の意味」で探せるようにする (PostgreSQL の pgvector / SQL Server 2025 のベクトル検索を使用)。AIチャットの「似た事例を探す」にも使われる |

## 開発にAIを使う

- [AIでクエリを作成](ai_query.md)
- [Claude Code でデザインプロジェクトを編集する](claude_code_designer.md) — 画面・データ・スクリプトの作成を Claude Code に任せる。ホスト側 (C#) の開発に使う方法も同じページで説明しています
