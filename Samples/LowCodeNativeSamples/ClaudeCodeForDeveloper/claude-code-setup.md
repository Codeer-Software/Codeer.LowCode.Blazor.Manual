# Claude Code 向け: Codeer.LowCode.Blazor を動かすまでの実行手順

**この文書は Claude Code（AI）が読んで実行するための手順書です。**（利用者向けの注意: この手順は AI が実行します。
AI の動作は決定的ではなく、ソフトウェアのインストール・ファイルの変更・コマンド実行を伴います。内容を確認のうえ自己責任でご利用ください。
本リポジトリは LICENSE のとおり AS IS で提供され、AI の操作に起因する損害について Codeer は責任を負いません。） ユーザーが「この URL を見て指示に従って」と言って
渡してきたら、上から順に実行してください。
ゴール: ユーザーの作業フォルダに、Codeer.LowCode.Blazor のアプリ（サーバー / クライアント / デザイナ）がビルド済みで、
**ユーザーのアプリ用の空のデザインプロジェクト**（ログインとユーザー管理だけが入った状態）ができていて、**サーバーがブラウザで開き、デザイナも起動している**状態にする。
そのあと Claude Code が画面（デザイン）を追加・変更できるようにする（デザインの作業場は `<ROOT>\DesignProjects\Project\`、ホストの C# の作業場は ROOT）。
**頼まれていないサンプル画面を作らない。** サンプル集（`PatternShowcase`）は、ユーザーが希望したときだけ別のデザインプロジェクトとして追加する。

## 進め方の原則

- **一本道で進める。迷ったらこの文書の既定値を採用する。** ユーザーに聞くのは下の「聞いてよいこと」の 1 つだけ。
  それ以外（フォルダ名・テンプレート・バリアント・ポート・DB の種類）は聞かずに既定値で進める。**後から変えられることは聞かない**（デザインプロジェクトのフォルダ名はリネームで済む、サンプル集は後から展開できる）
- **難しいことを聞かない。** 「NuGet のソースをどうしますか」「証明書を信頼しますか」のような質問はしない。この文書に書いてある通りに処理する
- **失敗したら止まらず、この文書の「うまくいかないとき」を見て自分でリカバリする。** それでも無理なときだけ、ユーザーに「何を」「どう」してほしいか 1 行で頼む
- **各ステップの終わりに 1 行で進捗を報告する**（「.NET SDK 10 を確認しました」など）。長い説明はしない
- ユーザーへの応答はユーザーの言語で（日本語で話しかけられたら日本語）
- **Windows 専用。** macOS / Linux では「デザイナが Windows 専用のため、このセットアップは Windows で行ってください」と伝えて終了する
- 作業フォルダの外（`C:\` 直下や他のプロジェクト）には何も作らない

### 聞いてよいこと（この 1 つだけ・既定値を提示して Yes/No で答えられる形で）

1. 不足しているソフト（.NET SDK / VS Code）を **winget でインストールしてよいか**（不足しているものがあるときだけ聞く）

デザインプロジェクトは `DesignProjects\Project\` に **`Empty` から**作る（名前は聞かない。後でフォルダごとリネームできる）。
サンプル集（`PatternShowcase`）は作らない（ユーザーが後で「サンプル集を展開して」と言えば `CLAUDE.md` の「デザインプロジェクトの切り替え」で足せる）。

アプリはログイン画面のある `Cookie` ホスト 1 種類（認証なしの構成は作らない。前段で認証済みなどの事情で不要なら、セットアップ後に `CLAUDE.md` の「認証を外す」で外す）。

---

## Step 0. 前提の確認

PowerShell で確認する。すべて非対話で判定できる。

| 確認 | コマンド | 判定 |
|---|---|---|
| OS | `[System.Environment]::OSVersion.Platform` | `Win32NT` 以外なら終了（上記） |
| .NET SDK 10 | `dotnet --list-sdks` | `10.` で始まる行があれば OK |
| Visual Studio 2026 | `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationVersion` | `18.` 以上なら「VS あり」。vswhere 自体が無い／エラーなら「VS なし」 |
| VS Code | `code --version` | VS なしのときだけ気にする |
| Claude Code の作業フォルダ | `Get-ChildItem -Force` | **空でなければ** サブフォルダ `LowCodeNativeSamples` を作ってそこを作業フォルダにする |

以降、作業フォルダの絶対パスを **ROOT** と呼ぶ（例: `C:\Users\taro\work\LowCodeNativeSamples`）。パスに日本語や空白が入っていても動くが、
すべてのコマンドでパスは `"` で囲む。

## Step 1. 不足しているものを入れる

不足があれば、ここで **1 回だけ**まとめて聞く: 「.NET SDK 10 / VS Code を winget でインストールしてよいですか？」（Git は不要）
（不足しているものだけ列挙する。ユーザーが No なら、何を手でインストールすればよいかを 1 行ずつ伝えて、入ったら続きをやると言って待つ）

```powershell
winget install --id Microsoft.DotNet.SDK.10 -e --accept-source-agreements --accept-package-agreements
winget install --id Microsoft.VisualStudioCode -e --accept-source-agreements --accept-package-agreements   # VS が無いときだけ
```

- インストール後は **PATH がこのシェルに反映されない**ことがある。`dotnet` が見つからなければ新しい PowerShell を起こす代わりに
  `$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")` を実行してから再確認する
- **winget が無い環境**（`winget` コマンド自体が見つからない）では .NET SDK を公式スクリプトで入れる（管理者権限不要）:
  ```powershell
  Invoke-WebRequest https://dot.net/v1/dotnet-install.ps1 -OutFile "$env:TEMP\dotnet-install.ps1"
  & "$env:TEMP\dotnet-install.ps1" -Channel 10.0 -InstallDir "$env:LOCALAPPDATA\Microsoft\dotnet"
  [System.Environment]::SetEnvironmentVariable("Path", $env:Path + ";$env:LOCALAPPDATA\Microsoft\dotnet", "User")
  [System.Environment]::SetEnvironmentVariable("DOTNET_ROOT", "$env:LOCALAPPDATA\Microsoft\dotnet", "User")
  $env:Path += ";$env:LOCALAPPDATA\Microsoft\dotnet"; $env:DOTNET_ROOT = "$env:LOCALAPPDATA\Microsoft\dotnet"
  ```
  VS Code も winget 無しなら、ユーザーに https://code.visualstudio.com/ からのインストールを頼む（VS があるなら不要）

## Step 2. アプリのフォルダを ROOT に書き出す

このリポジトリ (Starter) は Codeer が保守するテンプレート群の正本で、ユーザーのアプリに要るのはそのうち **Cookie ホスト 1 つ**だけ。
そのため **リポジトリを ROOT に clone しない**。zip を一時フォルダに展開し、付属ツールの `export-app` で ROOT にアプリの形（`Source\LowCodeNativeSamples.sln` と 5 プロジェクト、
Claude Code 用の文書と設定）だけを書き出す。他のホスト・保守ツール・`.git` は ROOT に残さない。

```powershell
$tmp = Join-Path $env:TEMP ("starter_" + [Guid]::NewGuid().ToString("N"))
Invoke-WebRequest https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Starter/archive/refs/heads/main.zip -OutFile "$tmp.zip"
Expand-Archive "$tmp.zip" -DestinationPath $tmp -Force
$repo = Join-Path $tmp "Codeer.LowCode.Blazor.Starter-main"
dotnet run --project "$repo\Source\Tools\StarterTool" -- export-app "<ROOT>"      # スマホアプリ (Android/iOS) も要るなら末尾に --maui
Remove-Item $tmp -Recurse -Force; Remove-Item "$tmp.zip" -Force
```

書き出し後、`<ROOT>\Source\LowCodeNativeSamples.sln`、`<ROOT>\CLAUDE.md`、`<ROOT>\ClaudeCodeForDeveloper\claude-code-setup.md` があることを確認する。
**以降はこの手順書の ROOT にあるコピーを読んで続ける**（パスがアプリの配置に書き換わっている。Web 上の原本は Step 2 までの案内）。
**このフォルダが以後の作業場所。** Claude Code の知識源は 3 つ: ROOT の `CLAUDE.md`（ホスト側の説明）、`ClaudeCodeForDeveloper/`（この手順書と、Step 5 で
生成するホスト開発のリファレンス）、Step 7 で展開される `DesignProjects\Project\ClaudeCodeForDesigner/`（デザインの作り方）。

> ユーザー自身のリポジトリにしたい場合は `.git` を削除して `git init` すればよい（聞かれたときだけ案内する。聞かない）。

## Step 3. 使う値を固定する（質問はしない）

| 名前 | 値 |
|---|---|
| `<VARIANT>` | `Cookie`（ログイン画面のある Web アプリ。公開しているホストはこれと、その Android/iOS クライアントの `Maui` だけ） |
| `Project` | `Project`（デザインプロジェクトのフォルダ名。`DesignProjects\Project\`。後からリネームしてよい） |
| `<TEMPLATE>` | `Empty`（空のプロジェクト。AppUser モジュールとログインだけ。初期ユーザー `admin` / `admin`） |
| `<URL>` | `https://localhost:7137` |

ソリューションは `<ROOT>\Source\LowCodeNativeSamples.sln`。
## Step 4. ローカルの置き場を作り、appsettings を書き換える

サーバーの設定 `<ROOT>\Source\LowCodeNativeSamples.Server\appsettings.Development.json` は、パスが
`C:\Codeer.LowCode.Blazor.Local\...` という固定パスになっている。これを **ROOT 配下**に向ける。

1. フォルダを作る: `<ROOT>\Local\Data`、`<ROOT>\Local\Designs`、`<ROOT>\Local\Storages`（`<ROOT>\Local\Font` は Step 2 の export-app が作り、PDF 出力用の Noto Sans JP（`NotoSansJP.ttf` / `NotoSansJP#b.ttf`、SIL Open Font License）が入っている）
2. `appsettings.Development.json` を編集する（JSON として読んで書き戻す。`\` は JSON 内で `\\`）:
   - `ConnectionStrings` の各値: `Data Source=C:\\Codeer.LowCode.Blazor.Local\\Data\\<ファイル名>` → `Data Source=<ROOT>\\Local\\Data\\<ファイル名>`（ファイル名はそのまま）
   - `FileSystemStorages[*].Directory` → `<ROOT>\\Local\\Storages`
   - `DesignFileDirectory` → `<ROOT>\\Local\\Designs`
   - `FontFileDirectory` → `<ROOT>\\Local\\Font`
   - 他の項目は触らない

要するに、文字列 `C:\\Codeer.LowCode.Blazor.Local` を `<ROOT>\\Local` に置換する（`DesignFileDirectory` は全ホストで `...\\Designs`）。
`ConnectionStrings` には全テンプレート分（`SampleSQLite` / `PatternsSQLite` / `Inventory` / `Sfa` / `ProjectManagement`）が入っているので、どのテンプレートで
デザインプロジェクトを作ってもサーバー側の追加設定は要らない（`template-create --data-dir` が置く DB ファイル名と一致している）。
DB は SQLite（ファイル）なので DB サーバーのインストールは不要。PostgreSQL 等に変えたいという話が出たら、
セットアップ完了後に `CLAUDE.md` の「appsettings リファレンス」を見て対応する（今はやらない）。

## Step 5. ビルド

```powershell
dotnet build "<ROOT>\Source\LowCodeNativeSamples.sln"
```

- 初回は NuGet の復元で数分かかる。`Build succeeded` と `0 Error(s)` を確認する
- 警告（`warning`）は無視してよい
- 成功すると、デザイナの exe が `<ROOT>\Source\LowCodeNativeSamples.Designer\bin\Debug\net10.0-windows\LowCodeNativeSamples.Designer.exe` にできる。以降 **DESIGNER_EXE** と呼ぶ

続けて、ホスト（C#）を触るときに Claude Code が読むリファレンスを生成する。デザイナ exe には GUI を出さない CLI（headless）があり、
**GUI サブシステムの exe なので PowerShell の `&` では待たない。必ず `Start-Process -Wait` で呼び、結果は `--out` の JSON ファイルで受け取る**（以降の Step も同じ）:

```powershell
Start-Process -FilePath "<DESIGNER_EXE>" -Wait -ArgumentList @(
  "developer-workspace", "<ROOT>", "--out", "<ROOT>\Local\developer-workspace.json")
Get-Content "<ROOT>\Local\developer-workspace.json"
```

`<ROOT>\ClaudeCodeForDeveloper\_specs\HostCustomization.md` などができる（`_` で始まるものは生成物・gitignore 済み。この手順書などコミット済みの文書には触らない）。
同時に `<ROOT>\.claude\settings.local.json`（デザイナ exe のパスを焼き込んだ許可リストと、`DesignProjects\*\` の各ワークスペースを最新化するフック。既存なら触らない）ができる。
パッケージを更新したときは同じコマンドで作り直す。判定: JSON に `"error"` が無く `written` に `HostCustomization.md` が含まれる。

> `.claude/settings.local.json` は **Claude Code が次のセッションから読む**。今のセッションでは反映されないので、完了メッセージで
> 「一度 Claude Code を再起動（`/exit` して再度 `claude`）すると、デザイナのコマンドが許可済みになります」と伝える。

## Step 6. デザインプロジェクトをテンプレートから作る

デザインプロジェクトは **`<ROOT>\DesignProjects\<名前>\`** に 1 つずつ置く。`design\` がデザイン本体（デザイナが開く・デプロイされる範囲）で、
隣に `Project.md` / `ddl\` / `docs\`（Step 7 で生成）が並ぶ。ユーザーのアプリは `DesignProjects\Project\` に **`Empty` から**作る。

**実行前に 1 行予告する**: 「デザインプロジェクトを `DesignProjects\Project` に空のプロジェクトから作ります（ログインとユーザー管理だけの状態です）」。
サンプル集はここでは作らない。


```powershell
Start-Process -FilePath "<DESIGNER_EXE>" -Wait -ArgumentList @(
  "template-create", "--name", "<TEMPLATE>",
  "--out-dir", "<ROOT>\DesignProjects\Project\design",
  "--data-dir", "<ROOT>\Local\Data",
  "--deploy-dir", "<ROOT>\Local\Designs",
  "--out", "<ROOT>\Local\template-create.json")
Get-Content "<ROOT>\Local\template-create.json"
```

これ 1 回で次が済む: `<ROOT>\DesignProjects\Project\design` にデザインプロジェクト（`app.clprj` / `Modules` / `PageFrames` …）が展開され、
テンプレート付属の DB（SQLite。`app_users` テーブルと admin だけ入っている）が `<ROOT>\Local\Data` に置かれ、`design\designer.settings.Development.json` の接続文字列がそのパスに
書き換わり、デプロイ先が `<ROOT>\Local\Designs` に設定されて **`App.zip` がそこに出力**される（サーバーはこの zip を読む）。

判定: JSON に `"error"` が無く、`deploy.success` が `true`。`<ROOT>\Local\Designs\App.zip` が存在する。

## Step 7. Claude Code 用ワークスペースを展開する（デザインを AI で編集できるようにする）

ワークスペース＝デザインプロジェクト 1 つ分のフォルダ `<ROOT>\DesignProjects\Project`。その中の `design` がデザイン本体:

```powershell
Start-Process -FilePath "<DESIGNER_EXE>" -Wait -ArgumentList @(
  "claude-workspace", "<ROOT>\DesignProjects\Project", "--project", "design", "--out", "<ROOT>\Local\claude-workspace.json")
Get-Content "<ROOT>\Local\claude-workspace.json"
```

`<ROOT>\DesignProjects\Project\` に `CLAUDE.md`（デザイン作業の規約）、`ClaudeCodeForDesigner/`（デザインの作り方・仕様・カタログ。**自動生成、手で編集しない**）、
`Project.md`（このデザイン固有のメモ）、`ddl/`、`docs/`、`LocalEnvironment.md`、`.claude/settings.local.json`（デザイナ exe のパスを焼き込んだ許可リストとフック）ができる。
ROOT の `CLAUDE.md`（このリポジトリのもの）には触らない。

判定: JSON の `aiRefresh` が `ok`。`<ROOT>\DesignProjects\Project\ClaudeCodeForDesigner\CLAUDE.md` と `_field_catalog.md` が存在する。

> デザインだけを扱う人は **`<ROOT>\DesignProjects\Project` で Claude Code を起動**する（ホストのソースが視界に入らない）。ROOT で起動した Claude Code は
> ホスト（C#）とデザインの両方を扱える（Step 5 の `settings.local.json` に同じ許可とフックがある）。完了メッセージでこの使い分けを伝える。

## Step 8. HTTPS 開発証明書

```powershell
dotnet dev-certs https --check --trust
```

`A valid certificate was found` と `trusted` が出れば OK。信頼されていなければ `dotnet dev-certs https --trust` を実行する。
**Windows がセキュリティ警告のダイアログを出す**ので、ユーザーに「証明書のダイアログが出たら『はい』を押してください」と伝える。
押されなくても http（`<URL>` の代わりに `http://localhost:5085`）で動くので、そこで詰まらない。

## Step 9. サーバーとデザイナを起動する

サーバー（別プロセス・バックグラウンド。ブラウザは `launchSettings.json` の設定で自動で開く）:

```powershell
Start-Process -FilePath "dotnet" -WorkingDirectory "<ROOT>\Source\LowCodeNativeSamples.Server" -ArgumentList @(
  "run", "--project", "<ROOT>\Source\LowCodeNativeSamples.Server", "--launch-profile", "https", "--no-build")
```

デザイナ（デザインプロジェクトのフォルダを引数に渡すと、そのプロジェクトを開いた状態で起動する）:

```powershell
Start-Process -FilePath "<DESIGNER_EXE>" -ArgumentList @("<ROOT>\DesignProjects\Project\design")
```

起動確認: 20〜60 秒待ってから `Invoke-WebRequest <URL> -UseBasicParsing -SkipCertificateCheck` が 200 を返せば OK
（PowerShell 5.1 には `-SkipCertificateCheck` が無い。その場合は `http://localhost:<httpポート>` で確認する）。
ブラウザが開かなければ `Start-Process "<URL>"` で開く。

- ログイン画面が出る。`admin` / `admin`。ログイン後はホームと「AppUser」（ユーザー管理）だけの空のアプリ
- デザイナ右上に「トライアル」と表示されるのは正常（ライセンス登録は不要。試用できる）

**起動がうまくいかないときはユーザーにやってもらってよい**: 「Visual Studio で `Source\LowCodeNativeSamples.sln` を開き、
`LowCodeNativeSamples.Server` を F5 で起動してください（デザイナは `LowCodeNativeSamples.Designer` を右クリック > デバッグ > 新しいインスタンスを開始）」。
VS が無いときは VS Code（次項）。

## Step 10. VS 2026 が無いとき: VS Code で開ける状態にする

ROOT に `.vscode/`（`launch.json` / `tasks.json` / `extensions.json`）が同梱されている（このリポジトリの一部）。
`launch.json` の Designer 構成は `DesignProjects/Project/design` を開く（この手順の値と同じ。フォルダをリネームしたら `launch.json` も合わせる）。

1. 拡張機能: `code --install-extension ms-dotnettools.csdevkit`（VS Code があるとき。無ければ `extensions.json` の推奨が VS Code 側で提示される）
2. 開く: `code "<ROOT>"`
3. 使い方をユーザーに伝える: 「実行とデバッグ」ビューで **Server** を選んで F5 でサーバー、**Designer** でデザイナが起動する。
   `Ctrl+Shift+B` でビルド

## Step 11. 完了メッセージ

以下を短く伝える（この文書の URL や内部手順は説明しない）:

- アプリの URL（`<URL>`）とログイン情報（`admin` / `admin`）
- デザインプロジェクトの場所（`<ROOT>\DesignProjects\Project\design`）と、今はログインとユーザー管理だけの空の状態であること。デザイナで編集 → 「送信」でサーバーに反映されること（サーバーの再起動は不要。スクリプトを変えたときだけ再起動）
- **次にできること**: 「Claude Code を再起動して、作りたいアプリの内容を教えてください。例:『商品マスタと受注入力の画面を作って』。デザインだけを扱うなら `DesignProjects\Project` フォルダで起動するとホストのソースが視界に入らず身軽です」
- デザインプロジェクトのフォルダ名は `Project`。変えたければフォルダをリネームすればよい（`launch.json` の Designer 構成のパスも合わせる）こと
- 参考実装は Claude Code が `ClaudeCodeForDesigner\_samples\` から読めるので、サンプル集を画面で見たくなったら「サンプル集を展開して」と言えばよいこと
- ソースコード（C#）も同じフォルダ（ROOT）にあり、必要なら ROOT で Claude Code を起動して変更できるが、**ふつうの画面追加はデザインだけで済む**こと

---

## うまくいかないとき

| 症状 | 対処 |
|---|---|
| `dotnet build` で `NU1101` / `Unable to find package Codeer.LowCode.Blazor...` | nuget.org に到達できていない。プロキシ環境なら `$env:HTTPS_PROXY` を確認。社内 NuGet フィードしか使えない環境はユーザーに nuget.org のミラーがあるか聞く |
| `dotnet build` で `NETSDK1045`（必要な SDK が無い） | .NET SDK 10 が入っていない。Step 1 をやり直す |
| ビルドは通るが `LowCodeNativeSamples.Designer.exe` が無い | `Source\LowCodeNativeSamples.Designer\bin\Debug\net10.0-windows\` を再確認。無ければ `dotnet build "<ROOT>\Source\LowCodeNativeSamples.Designer\LowCodeNativeSamples.Designer.csproj"` |
| `template-create` の JSON に `template not found` | `Start-Process "<DESIGNER_EXE>" -Wait -ArgumentList @("template-list","--out","<ROOT>\Local\tl.json")` で一覧を出し `folderName` を確認する |
| `template-create` の JSON に `--out-dir must be empty` | `<ROOT>\DesignProjects\Project\design` に既にファイルがある。中身を確認し、ユーザーの物でなければ削除して再実行 |
| 頼まれていないサンプル（PatternShowcase 等）を作ってしまった | デザイナを終了 → `DesignProjects\PatternShowcase` を削除 → `Local\Data\sqlite_patterns_v*.db` を削除 → ユーザーのアプリを `deploy "<ROOT>\DesignProjects\Project\design"` でデプロイし直す（App.zip が上書きされる）。ユーザーに何を消したか報告する |
| `template-create` や `claude-workspace` を実行するとデザイナの **ウィンドウが開いて** JSON ができない | デザイナのパッケージが古い（`template-create` は Codeer.LowCode.Blazor.Designer 1.3.24 以降）。`Source\LowCodeNativeSamples.Designer\LowCodeNativeSamples.Designer.csproj` の `Codeer.LowCode.Blazor.Designer` の版を確認する。このリポジトリを最新から取得していれば起きない |
| サーバー起動で `address already in use` | ポートが使用中。`launchSettings.json` の `applicationUrl` のポート番号を空いている番号に変えて再起動し、完了メッセージの URL も合わせる |
| ブラウザで開くと真っ白／`design not found` | `<ROOT>\Local\Designs\App.zip` が無い。Step 6 のデプロイが失敗している。`Start-Process "<DESIGNER_EXE>" -Wait -ArgumentList @("deploy","<ROOT>\DesignProjects\Project\design","--out","<ROOT>\Local\deploy.json")` で作り直す |
| `developer-workspace` / `claude-workspace` で `--out` の JSON ができない（ウィンドウが開く） | デザイナのパッケージが古い（`developer-workspace` は Codeer.LowCode.Blazor.Designer.Standard 0.8.3 以降）。このリポジトリを最新から取得していれば起きない |
| ログインできない（Cookie） | サンプル DB に `admin` がいるはず。`appsettings.Development.json` の `ConnectionStrings` が `<ROOT>\Local\Data\...` を指しているか、ファイルが存在するかを確認 |
| `winget` が見つからない | Step 1 の dotnet-install.ps1 経路 |
| PowerShell で `&` でデザイナ exe を呼ぶと即戻って何も起きない | 正常（GUI サブシステム）。必ず `Start-Process -Wait` + `--out` |
| Excel / PDF 出力でフォントのエラー | `<ROOT>\Local\Font` に `NotoSansJP.ttf` と `NotoSansJP#b.ttf` があるか確認する（export-app が置く。無ければ Starter の `Source\Font` からコピー） |

