# Claude Code でデザインプロジェクトを編集する

[Claude Code](https://docs.claude.com/ja/docs/claude-code/overview) は Anthropic が提供する CLI ベースの AI 開発支援ツールです。Codeer.LowCode.Blazor のデザインファイル（`*.mod.json` / `*.mod.cs` / `*.frm.json` / SQL / app.css 等）はすべてテキストベースのため、Claude Code から直接読み書きできます。

「自然言語で指示 → モジュール／レイアウト／スクリプトを作成・編集」する流れで、デザイナでの操作と組み合わせて使うことを想定しています。

リファレンス一式は **ワークスペース**（`Designer` フォルダ）として ZIP で配布しています。これを展開したフォルダの中でデザインプロジェクトを作り、そこで Claude Code を起動します。ワークスペースには、Claude Code 向けの作業ルール（`CLAUDE.md`）・CLB の仕様リファレンス（`ClaudeCodeForDesigner/`）・プロジェクト固有ルールのひな形（`Project.md.sample`）などが含まれます。

## 手順

### 1. 作業フォルダを作成する

任意の場所に作業フォルダを作成します。本マニュアルでは `C:\work\Test` を例として使用します。

### 2. リファレンス ZIP をダウンロードしてワークスペースを展開する

#### 2-1. ZIP をダウンロード

[GitHub の Samples/Designer.zip ページ](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Manual/blob/main/Samples/Designer.zip) を開き、右上の **ダウンロードアイコン**（赤枠）をクリックします。

<img width="700" src="../../Image/claude_code_designer_download.png">

> 上のリンクから [直接ダウンロード](../../Samples/Designer.zip) も可能です（マニュアルをローカルクローンしている場合）。

#### 2-2. ZIP のブロックを解除

ダウンロードした ZIP は **インターネットからのファイル**としてブロックされた状態になっています（Windows の Mark of the Web）。展開前に **必ずブロック解除** してください。

1. エクスプローラで `Designer.zip` を **右クリック → プロパティ**
2. 「全般」タブ下部の **「セキュリティ: このファイルは他のコンピュータから取得したものです…」**
3. **「許可する」のチェックボックス**（赤枠）にチェック
4. OK で閉じる

<img width="450" src="../../Image/claude_code_designer_unblock.png">

> 解除しないまま展開すると、中の各ファイルがすべてブロック扱いになり、Claude Code がファイル読み込み時に警告を出すなどの不具合の原因になります。

#### 2-3. 作業フォルダに展開

ブロック解除後、`Designer.zip` を **右クリック →「すべて展開」** で作業フォルダ（例: `C:\work\Test`）に展開します。ZIP 名にあわせて `Designer` という **ワークスペースフォルダ**が作られ、その中に一式が入ります（例: `C:\work\Test\Designer`）。このフォルダ名は自由に変更してかまいません（例: プロジェクト名に合わせる）。本マニュアルでは `Designer` のまま進めます。

```
C:\work\Test\
└── Designer\                      ← ZIP から展開したワークスペース（この中で Claude Code を起動する）
    ├── CLAUDE.md                  ← Claude Code が起動時に自動読み込み（作業ルール）
    ├── ClaudeCodeForDesigner\     ← CLB の仕様リファレンス
    │   ├── CLAUDE.md
    │   ├── Docs\                  ← モジュール / レイアウト / フィールド / スクリプト等の詳細
    │   ├── Defaults\              ← 各フィールド・レイアウトの既定 JSON（コピー元）
    │   └── Samples\               ← 実装サンプル一式
    ├── Project.md.sample          ← Project.md にコピーしてプロジェクト固有ルールを書く（後述）
    ├── .claude\                   ← Claude Code 用の許可設定
    ├── ddl\                       ← テーブル作成用 SQL（DDL）を置く場所
    └── temporary\                 ← 作業中の一時ファイルを置く場所
```

`CLAUDE.md` は Claude Code への作業ルールで、`ClaudeCodeForDesigner/` 配下の仕様リファレンスと `Project.md`（後述）を参照すべき場所として Claude Code に伝える役割を持ちます。

### 3. デザイナでデザインプロジェクトを作成する

デザイナを起動し、`File` → `New Project` を選択して、**ワークスペースフォルダの中**にデザインプロジェクトを作成します。

- **Name**: `Design`（ワークスペース直下に `Design\` フォルダが作られます）
- **Folder**: 2. で展開したワークスペースフォルダ（例: `C:\work\Test\Designer`）
- **Template**: 任意（例: `Empty`）

<img width="500" src="../../Image/claude_code_designer_create_project.png">

作成すると、ワークスペースの中にデザインプロジェクトが並びます。

```
C:\work\Test\Designer\
├── CLAUDE.md
├── ClaudeCodeForDesigner\
├── Project.md.sample
├── .claude\
├── ddl\
├── temporary\
└── Design\                        ← 3. で作ったデザインプロジェクト
    ├── app.clprj
    ├── Modules\
    ├── PageFrames\
    └── Resources\
```

### 4. プロジェクト固有の情報を `Project.md` に蓄積する

ワークスペースには `Project.md.sample` というひな形が同梱されています。これを **同じフォルダに `Project.md` という名前でコピー**し、そのプロジェクト固有の前提・規約を書いておくと、Claude Code が作業時に読み取って参照してくれます。Claude Code と作業する中で気づいた都度、口頭で「これも `./Project.md` に追記して」と伝えると蓄積できます。

書くと有効な例:

- **接続先 DB**: 「このプロジェクトは PostgreSQL に接続。テーブル・カラム名は `snake_case`」
- **命名規約**: 「モジュール名は `Pascal` ＋単数形、フィールド名はキャメルケース」
- **業務ルール**: 「`Order.Status` は 0=新規, 1=処理中, 2=完了, 9=キャンセル」
- **共有ライブラリ**: 「日付フォーマットは常に `yyyy-MM-dd HH:mm`」
- **既存資産**: 「`SharedComponents/` 以下に共通コンポーネントあり、新規作成前に確認」
- **多言語対応**: 「ja/en の Resources を必ず両方更新」

これらを蓄積していくと、毎回の指示が短くて済み、Claude Code の出力が一貫します。コピーして作った `Project.md` はリファレンス更新（ZIP 再ダウンロード→上書き）の対象外（同梱されるのは `Project.md.sample` の方）なので、更新で消える事故を防げます。

> Claude Code 内蔵のメモリ機能（`#` で始まる発言や `/memory` コマンド）でも永続化できます。詳しくは [公式ドキュメント](https://docs.claude.com/ja/docs/claude-code/memory) を参照。

### 5. ワークスペースで Claude Code を起動する

```bash
cd C:\work\Test\Designer
claude
```

`CLAUDE.md` がワークスペース直下にあるので、Claude Code は起動時に自動で読み込み、`ClaudeCodeForDesigner/` 以下の仕様リファレンスや `Project.md` を参照できる状態になります。あとは自然言語で指示を出せば、`Design/` 配下のデザインファイルを作成・編集してくれます。

> ワークスペースの `CLAUDE.md` には、作成・編集したデザイン定義を検証する「デザインチェック」や、DB の中身確認・テストデータ投入を Claude Code から行うための手順も含まれています。これらを使うには、`.claude/settings.local.json.sample` を同じフォルダに `settings.local.json` としてコピーし、デザイナ exe のパスを記入します（マシン固有の設定なので、このファイルは配布・共有しません）。

## 使い方の例

準備が整ったら自然言語で指示を出します。

- 「商品マスタのモジュールを作って。フィールドは商品コード、商品名、価格、在庫数」
- 「`Customer` モジュールに `登録日` の Date フィールドを追加して」
- 「`Order` 一覧で `合計金額` が 10000 円以上の行を赤背景にするスクリプトを書いて」
- 「サイドバーに `売上集計` ページを追加して、新しい `SalesSummary` モジュールにナビゲートさせて」

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

> 設定ファイルの仕様詳細は [designer.settings](../designer/designer_settings.md) を参照（Claude Code は同等の仕様を `ClaudeCodeForDesigner/Docs/ProjectSettings.md` から自動取得）。

## ヒント

- **デザイナと並行して使う**: ビジュアルで配置を細かく調整するのはデザイナ、フィールド・スクリプトの一括追加や繰り返し作業は Claude Code、と使い分けると効率的です。
- **指示は具体的に**: 「いい感じに」より「Title フィールドを追加し、ListLayout にも表示」など、対象モジュール・フィールド名・配置先を明示するほうが意図通りに動きます。
- **生成結果は必ず確認**: スクリプトや SQL は実行前にデザイナでプレビューする / バージョン管理にコミットして差分を確認する運用を推奨します。
- **リファレンスを最新に保つ**: フレームワーク／マニュアルが更新されたら ZIP を再ダウンロードし、ワークスペースの `CLAUDE.md` と `ClaudeCodeForDesigner/`（必要なら `Project.md.sample`）を上書きしてください。自分でコピーして作った `Project.md` と `Design/` のデザインプロジェクトは上書き対象外なので、プロジェクト固有の情報は維持されます。

## 関連情報

- [Codeer.LowCode.Blazor とは](../introduction/what_is_lowcode.md)
- [デザイナ概要](../designer/designer.md)
- [AI 概要](ai_overview.md)
- [GitHub: ClaudeCodeForDesigner](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Manual/tree/main/ClaudeCode/Designer/ClaudeCodeForDesigner)
