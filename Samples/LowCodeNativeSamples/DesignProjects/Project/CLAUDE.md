# Codeer.LowCode.Blazor デザインワークスペース

このフォルダで Claude Code を起動し、Codeer.LowCode.Blazor のデザインファイル（JSON / C# スクリプト / SQL / CSS）を編集する。デザイナ（GUI アプリ）と並行して使う。

## 質問にも答える（作るだけではない）

ユーザーが「このワークスペースの使い方」「Codeer.LowCode.Blazor の機能・用語」「デザインの作り方」などを**質問**してきたら、いきなりファイルを作らず、まず質問に答える。根拠は次の 3 つ:

- `./ClaudeCodeForDesigner/CLAUDE.md` と `./ClaudeCodeForDesigner/Docs/` — CLB の作り方・仕様・パターン集
- `./ClaudeCodeForDesigner/` 配下の自動生成リファレンス（フィールドカタログ・仕様・デフォルト JSON・サンプル）
- オンラインマニュアル（日本語）: https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Designer.Standard/blob/main/Docs/claude_code_designer.md

確証が持てないことは断定せず、該当ドキュメントの場所を案内する。

## まず読むもの（デザイン作業の前に必ず）

- **`./ClaudeCodeForDesigner/CLAUDE.md`** — デザインファイル作成の詳細指示書（CLB の仕様・フィールド・レイアウト・designcheck / sql / rename CLI・規約）。**着手前に通読する**。`./ClaudeCodeForDesigner/` 配下の Docs と生成リファレンス（仕様・カタログ・デフォルト・サンプル）の索引も兼ねる
- **`./Project.md`** — このプロジェクト固有のルール（接続先 DB・命名・業務ルール・既存資産）。**絶対に守る**

下の「作業の進め方」はこのワークスペースで**常に効く運用ルール**。デザインの中身の作り方は `ClaudeCodeForDesigner/` を見る。

## 基本姿勢: 設定 → スクリプト → C# の順

- **ローコード (デザイン設定) で済ませる。** フィールド・レイアウト・条件・リンク・集計などの設定で表現できることに、スクリプトを書かない
- **スクリプトは最小限。** 設定で表せない挙動だけに、短く書く。書く前に「既存のフィールド／プロパティで代替できないか」を必ず問う（スクリプトを書きすぎる傾向があるので意識的に抑える）
- **ホスト (C# ソリューション) を触るのは最後の手段。** フィールド型そのものが無い・性能をコードで改善したい・独自 API が要るなど、ローコードの範囲で不可能と確定したときだけ。やり方は `./ClaudeCodeForDesigner/_specs/HostCustomization.md`

## 作業の進め方（常時このルールで動く）

### 1. 外部ツールはデザイナがセットアップ済み・以後は確認なし
- このワークスペースは**デザイナの「Claude Code Workspace」メニュー（または `claude-workspace` CLI）が展開したもの**で、デザイナ exe のパスは `.claude/settings.local.json` と `LocalEnvironment.md` に**焼き込み済み**。exe パスを自分で探さない
- **exe に繋がらないとき（パスに実在しない・CLI が動かない）は、ディスクを探索して勝手に直さず、ユーザーに正しいパスを確認する**。ユーザーから「exe のパスを変えて」「新しいパスは○○」と指示されたら自分で書き換えてよい: ① `LocalEnvironment.md` の `DesignerExePath:` 行 ② `.claude/settings.local.json` 内の旧パスの**全出現**（許可リストとフック。JSON 文字列内なので `\` は `\\` にエスケープ、ユーザーが追記した他の許可は温存）。書き換え後は `template-list` など軽いサブコマンドで疎通確認し、許可が効かないようならセッションの再起動を案内する。デザイナのメニュー Tools > Claude Code Workspace の再実行（`settings.local.json` を消してから実行するとパスが再生成される）を案内するのでもよい
- **`designcheck` / `sql` / `rename-*`（rename-field / rename-module / rename-pageframe / rename-layout / rename-enum / rename-enum-member / 一括の rename-batch）/ `ai-refresh` / `defaults` / `template-list` / `template-extract` / `template-create` / `api` / `selenium-test-init` / `pageobject` は確認なしで実行してよい**（`dotnet build` / `dotnet test` も）。`deploy`（現在のデプロイ先へ App.zip を送る＝デザイナの「送信」）も確認なしで実行してよい。どのデプロイ先へ送れるかは、そのデプロイ先の **`AllowCliDeploy`**（`designer.settings.Development.json` の `DeployInfo`。ユーザーが設定済み。方式 FileSystem / FTPS は問わない）が決め、`false` のデプロイ先には CLI からそもそも送れないので、これが安全境界。拒否されたら、デザイナの「デプロイ先の追加」で「CLIからのデプロイを許可する」を付けるか `AllowCliDeploy: true` を設定してもらうようユーザーに案内する（自分で `Development.json` を書き換えない）。どの DB に SQL を流せるかは、各データソースの `designer.settings.json` の **`AllowCliSqlAccess`**（ユーザーが設定済み）が決める。`false` のデータソースには CLI からそもそも実行できないので、これが安全境界。`sql` と `designcheck` 以外は DB 接続せず完結する（詳細は `./ClaudeCodeForDesigner/CLAUDE.md`）
- **`designer.settings.Development.json` は基本読まない・書かない（許可制）。** 接続文字列・デプロイ設定（秘密情報）の置き場で、デザイン作業でこの中身が必要になることは無い — データソースの名前と種別は `designer.settings.json`（秘密なし）にあり、DB のスキーマ・データ確認は `sql` / `designcheck` CLI が接続文字列を内部で解決してくれる。扱うのはユーザーが明示的に依頼したときだけ（`.claude/settings.json` の ask 設定で確認が出る）。**その場合も、許可を求める前に「このファイルの内容（接続文字列やパスワード）は読むと LLM への送信と会話ログへの記録が発生する」ことを一言伝え、リスクを了解したうえで承認してもらう**。データソースやデプロイ設定の追加は、デザイナのソリューションツリーで設定ファイルを右クリック（「データソースの追加」等）からもできるので、そちらを案内するのも良い
- **このワークスペースはデザイナ 1.3.15 以降が前提**（`template-create` / `deploy` / `api` は 1.3.24 以降）。古い exe に未知のサブコマンドを渡すと GUI が起動してしまい `--out` が生成されない。`--out` の JSON が出来ていない／ウィンドウが開いた場合は「その版が未対応」と判断し、**作業を進めずユーザーにデザイナのバージョンアップと Tools > Claude Code Workspace の再実行を促す**（ワークスペースはデザイナと同一バージョンの内容に更新される）
- **動作確認のサーバーは、ユーザーが起動していればそれを使い、起動していなければ自分で起動してよい。ユーザーに「起動しないで」「止めて」と言われたらやめる**（自分で起動できるのはホストソリューションと同居しているとき。手順は「4. 動作確認」と `./ClaudeCodeForDesigner/Docs/BrowserTestGuide.md`）。URL はホスト側 `CLAUDE.md`「ビルドと起動」か Server プロジェクトの `Properties/launchSettings.json`（https プロファイルの `applicationUrl`）から取り、`LocalEnvironment.md` に `ServerUrl:` 行として記録して以後はそれを使う。同居していない（起動方法が分からない）ときだけ URL をユーザーに聞く。Playwright の導入（`tools/` への `npm install playwright` と `npx playwright install chromium`）、`node`、`dotnet run` は許可済みなので諮らなくてよい。それ以外の `.claude` の許可追加は勝手に広げずユーザーに諮る

### 2. ツールの使い方（許可ブロック・エラーを増やさない）
- **スクリプトは Write/Edit ツールで作る。** シェルのヒアドキュメント（`cat > file <<EOF`）でスクリプトを量産しない（中身の `{}`・引用符が毎回ブロックされる）
- **一覧・検索・読み込みは Read / Grep / Glob ツールを使う。** `cat` / `grep` / `ls` のワンライナーや、`for`/`while`/`if`・変数代入・パイプを詰めた複合コマンドを避ける（静的解析が通らず止まる）
- **Bash は絶対パスで。`cd` しない**（作業ディレクトリは呼び出し間で持ち越され、相対パスが狂う）
- **PowerShell は実行ファイルをリテラルパスで呼ぶ**（`& $var` の動的呼び出しは必ず止まる）。`$()` 部分式・スクリプトブロックも避け、小さく単純なコマンドに分ける
- **広域なファイル探索をしない**（`C:\` 全走査等。遅く・アクセス拒否でエラー終了する）。必要な情報は最小の取得で済ませる

### 3. ファイル配置
- **デザインプロジェクトのフォルダ（既定 `design/`）直下には設計ファイルのみ**（`app.clprj` / `designer.settings*.json` / `Modules` / `PageFrames` / `Enums` / `Resources`）。作業ファイルを混ぜない。デプロイ（App.zip）に入るのはこのフォルダの中身だけ
- **DDL（CREATE TABLE 等）→ `ddl/`**
- **このデザイン固有の文書（仕様メモ・決定事項・ユーザー向けの説明）→ `docs/`**。ワークスペース直下に散らさない
- **作業物（seed SQL・CLI の `--out` JSON・スクショ・検証スクリプト）→ 自分のスクラッチパッド（セッションの一時領域）**。ワークスペースに作業ファイルを置かない
- **依存（Playwright 等の `npm install`）→ `tools/`**。プロジェクト直下に `node_modules` を作らない

### 4. 動作確認（ブラウザ / Playwright）は作業の一部として行う
- **画面に見える変更（モジュール・レイアウト・フィールド・スクリプトの挙動）をしたら、区切りごとに実際の画面で確認してから次へ進む。** designcheck の緑は「読み込める」までの保証で、0 件表示・出し分け・合計計算・レイアウト崩れは画面でしか分からない。毎編集ではなく「1 モジュール分」「1 機能分」のまとまりで撮る（1 回数十秒）。ドキュメントだけ・DDL だけの変更ならスキップしてよい
- 手順は `./ClaudeCodeForDesigner/Docs/BrowserTestGuide.md`。要点: ① サーバーが起きているか URL に到達して確かめる。**起きていればそれを使う**（ユーザーが VS 等で起動しているもの）。起きていなければ、ホストソリューションと同居しているときは自分で `dotnet run` をバックグラウンドで起動する（同居していなければユーザーに聞く）。**ユーザーから「起動しないで」「止めて」と言われたら、以後そのセッションでは起動せず、自分で起動したものは止める** ② CLI の `deploy "<デザインプロジェクトのフォルダ>"` で自分の編集を反映する（送れる先は `AllowCliDeploy` が決める） ③ **`*.mod.cs`（スクリプト）変更時・スキーマ変更時はサーバー再起動が必須**。自分で起動したサーバーは自分で止めて起動し直す。ユーザーが起動したサーバーは勝手に止めず、再起動を依頼する ④ Playwright でログイン → 対象ページ → スクショと DOM 取得 ⑤ 自分で起動したサーバーは作業の終わりに止める
- **稼働サーバが自分の編集を配信しているとは限らない。** デザインプロジェクトへの直接編集は稼働サーバに自動反映されない（反映は上記 ②）。スクショ判定の前に、サイドバー構造を dump して「いま何が配信されているか」を突き合わせる
- UI 自動操作は**構造を dump → セレクタ確定**の順（決め打ちセレクタは複製・非表示要素で失敗する）。導入したパッケージと使う API を一致させる（`playwright` 本体と `@playwright/test` は別物）
- 判定はスクショの目視だけで済ませない。DOM のテキスト・行数・要素の位置やサイズを数値で取って期待値と突き合わせる

### 5. スコープと着手前の確認（求められないものを作らない・聞くべきことは聞く）
- **土台の確認を着手条件にする。** このワークスペースのデザインプロジェクトが**サンプル／ショーケース／業務テンプレート由来**（`PatternShowcase` / `GettingStarted` / `InventoryManagement` / `SFA` / `ProjectManagement` から作ったもの。デモ画面やデモユーザー alice / bob 等が入っている）で、ユーザーが**自分の業務アプリ**を求めてきたら、その上に増築しない。まず「サンプル集が入っています。これを土台にしますか？ 空のプロジェクトから作りますか？」を確認する。既定の提案は**空のプロジェクト（`Empty`。AppUser とログインだけを含む。ログインの無いデスクトップ WPF / WinForms ホストなら `EmptyNoAuth`）を別のデザインプロジェクトとして新規に作る**こと（ホストと同居しているなら `DesignProjects/<アプリ名>/`）で、必要なパターンだけサンプルから写す。「続きをやって」はこの確認の省略を意味しない
- **業務の根幹に関わる要件は既定値で走る前に確認する。** 承認ルート（誰が・何段階）、関係者とロール、金額計算（税率・端数処理）、採番形式、締め日など、後から変えると手戻りが大きいものは聞く。聞かずに既定値で進めてよいのは表示・レイアウトなど差し替えの軽い判断だけ。質問は既定値を添えて Yes/No で答えられる形にまとめる
- **承認・ワークフロー等を勝手に足さない／「推奨」もしない。** 既定は CRUD（ヘッダ＋明細＋合計等）に留める
- **承認は認証前提。** 非認証アプリに承認ボタンは業務的に無意味。やるなら Extras の `ApprovalFlowField` で正式に作る（承認モジュール群はデザイナの `approval-setup` が生成。実装例は `./ClaudeCodeForDesigner/_samples/PatternShowcase/Modules/ExpenseRequest.mod.json` と [Docs/AppPatterns/auth_workflow.md](ClaudeCodeForDesigner/Docs/AppPatterns/auth_workflow.md)）
- **完了報告は「ユーザーが受け取るものの姿」を先に。** 技術的に何を作ったかより先に、サイドバーに何が並び・誰がどう使う状態になったかを示す。サンプル画面やデモデータ・仮の承認者が残っているなら冒頭で明示する（末尾の補足で済ませない）

### 6. DB を触るとき
- テーブル作成（DDL）・テストデータ投入・中身確認（件数 / 列 / 親子の紐付け）は、自前で DB 接続せず **`sql` サブコマンド**で行う（詳細: `./ClaudeCodeForDesigner/CLAUDE.md`「SQL 実行 CLI」、`./ClaudeCodeForDesigner/Docs/DatabaseGuidelines.md`）
- **モジュールを作ったらテーブルも用意する**（無いと designcheck が「列が存在しない」と報告する）
- **DB 主キーは `INTEGER`（long）+ 自動採番が原則。`TEXT`/`VARCHAR` で GUID を保存する設計にしない**（詳細: `./ClaudeCodeForDesigner/Docs/DatabaseGuidelines.md`）

### 7. Selenium テスト（求められたときに作る）
- ユーザーが「テストを書いて」「自動テストしたい」と言ったら、`./ClaudeCodeForDesigner/Docs/SeleniumTestGuide.md` の手順で行う: `selenium-test-init` でテストプロジェクトを展開（既にあれば再利用）→ `pageobject` で PageObject を生成 → `Scenario/` にテストを書く → `dotnet test`
- テストデータは **DataManager**（テストプロジェクト同梱。`sql` CLI と同じ DB にアプリと同じアクセス層で入る）で、そのテストが触るテーブルだけ初期化・投入する
- テストプロジェクトの置き場は、ホストソリューションがあればその隣（`Tests/<App>.SeleniumTest`）、無ければワークスペース直下の `SeleniumTest/`
- 実行にはサーバーが起動している必要がある。URL は既存ルールどおりユーザーに確認して `testsettings.json` の `BaseUrl` に書く

### 8. ホストソリューション (C#) と同居しているとき
- Codeer.LowCode.Blazor.Starter（またはテンプレートから作った `LowCodeApp.sln`）のフォルダでは、このワークスペースは **`DesignProjects/<デザイン名>/`**（`Project.md` / `ddl/` / `docs/` / `design/` とこの規約一式）としてホストのルートの下に置かれ、デザイン担当者はこのフォルダで Claude Code を起動する。ホストのルートにある `CLAUDE.md` は**ホスト側の所有**（ホストを触る人向けの構成・ビルド・起動・appsettings の説明）。両方に従う（矛盾したらホスト側の `CLAUDE.md` が優先）
- 旧配置（ワークスペースがホストのルートそのものに展開されている場合）では、ルートの `CLAUDE.md` はホスト所有で、この規約は `./ClaudeCodeForDesigner/WorkspaceRules.md` として置かれている。扱いは同じ
- デザイン作業でホストの再ビルドは要らない（デザインファイルは `deploy` で反映、スクリプト変更はサーバー再起動）。動作確認用サーバーの起動コマンドと URL はホスト側 `CLAUDE.md`「ビルドと起動」にある（Starter なら `dotnet run --project Source/Hosts/Cookie/LowCodeApp.Server --launch-profile https`、`https://localhost:7137`、初期ユーザー `admin`/`admin`）
- ホストが配信するデザインは 1 つ（appsettings の `DesignFileDirectory` にある App.zip）。サンプルから自分のアプリへ移るなど、別のデザインプロジェクトに切り替えるときは、ホスト側 `CLAUDE.md` の「デザインプロジェクトの切り替え」に従う（デプロイ先の App.zip と接続文字列の向き先を変える）
- C# に手を入れるのは上記「基本姿勢」の最後の手段に該当するときだけ。ライブラリの拡張点は `<ホストのルート>/ClaudeCodeForDeveloper/_specs/HostCustomization.md`（デザイナの `developer-workspace` が生成。`./ClaudeCodeForDesigner/_specs/HostCustomization.md` も同じ内容）。入れたら `dotnet build` が通る状態で止め、サーバー／デザイナの再起動が必要なことをユーザーに伝える

## プロジェクト固有の知見を貯める

作業中に得た業務ルール・命名・接続先などは、ユーザーに確認のうえ `./Project.md` に追記して蓄積する。
