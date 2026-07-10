# Codeer.LowCode.Blazor - Claude Code によるデザインファイル作成・編集

## 目的

Codeer.LowCode.Blazor のデザインファイル（設定ファイル）を Claude Code で作成・編集する。
デザインファイルはすべてテキストベース（JSON, SQL, C#スクリプト）であるため、
Claude Code が直接読み書きできる。

従来はGUIデザイナー（WPFアプリ）でのみ作成していたが、
Claude Code を活用することで、自然言語の指示からアプリケーション定義を生成可能になる。

## 大前提（本書すべてに掛かる）

本書（この CLAUDE.md と `Docs/` 配下）に書かれた規則・ガイドライン・「絶対常識」表記は、**すべて「原理原則（既定で従う最善）」であって、絶対不可侵ではない**。**ユーザーが明示的に別の方針を求めたら、その規則から離れてユーザーの要望に従う**。個別の規則にいちいち「ただし要望なら別」とは書いていないが、全規則にこの前提が掛かる。規則を盾にユーザーの明示要望を却下しない。

（`ToString("D3")` が実行時に落ちる・`IsApplicationRoot:true` が無いとルート着地できない・システム予約名の綴り、のような**技術的事実／制約は「規則」ではなく現実**。ユーザーが求めても結果は変わらないので、却下ではなく制約を説明して代替を示す。）

## 作業の進め方（着手前に必ず読む）

デザインファイルは「JSON 構文として妥当」でも「業務的に間違い」になりうる。`LimitCount: 0` で明細が 0 件になる、システム項目を編集欄に出す、といった**意味的バグは designcheck では検出できない**（JSON として妥当なため素通りする）。これを着手段階で防ぐためのルール:

1. **規約（Guidelines）が正、JSON 例は参考**。両者が食い違う場合は必ず Guidelines に従う。各 Fields ドキュメントの JSON 例は最小サンプルであり、値が常に最適とは限らない。
2. **着手前に最低限これを読む**: [Docs/CommonMistakes.md](Docs/CommonMistakes.md) / [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) / [Docs/SearchConditionGuidelines.md](Docs/SearchConditionGuidelines.md) / [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) ＋ 作るものに該当する [Docs/AppPatterns/](Docs/AppPatterns/) のパターン。必要箇所だけ grep で拾うのではなく、関連 Guidelines を通読してから書く。
3. **既存をコピーして改変する（ゼロから JSON を組まない）**。
   - 個別のフィールド／レイアウト／検索条件は [Defaults/](Defaults/) の `{型名}.json`（例: `Defaults/DetailListFieldDesign.json`）をコピーし、必要なプロパティだけ上書きする。これはデザイナが新規追加時に書き出すのと**同一の既定状態**（TypeFullName・プロパティ構造・既定値が保証される）なので、`LimitCount` を `0` と書くような“デフォルトでも妥当でもない値”が混入しない。**プロパティの既定値が不確かなときは記憶で書かず Defaults を見る。**
   - モジュール一式は [Samples/PatternShowcase/App/Modules/](Samples/PatternShowcase/App/Modules/) の近いモジュールを正典として複製し、差分だけ直す。
4. **designcheck の緑は「読み込める」までの保証**。0 件表示・編集可能なシステム項目・桁区切りが効かない等の挙動バグは別途、画面（ブラウザ）で確認して潰す（[Docs/BrowserTestGuide.md](Docs/BrowserTestGuide.md)）。
5. **DB を触るときは SQL 実行 CLI**（後述）。テーブル作成（DDL）・テストデータ投入・中身の確認（件数 / 列 / 親子の紐付け）は、自前で DB に接続せず `sql` サブコマンドで行う。
6. **フィールド／モジュール／ページフレーム／レイアウトの名前を変えるときはリネーム CLI**（後述）。手作業のテキスト置換で参照を追うと**必ず漏れる**（スクリプト・リンク越しの参照・他モジュールからの参照など）。`rename-*` サブコマンドはデザイナ GUI と同じリファクタリングで全参照を一括追従させるので、リネームは原則これで行う。

## Codeer.LowCode.Blazor とは

Blazor向け動的ローコードフレームワーク。テキストベースの設定ファイルを読み込んで、
画面・データバインディング・ビジネスロジックを動的に実現するライブラリ。

**動作環境:**
- クライアント: Blazor WebAssembly (WASM)
- サーバー: ASP.NET Core
- デザイナー: WPFアプリ（設定ファイルの作成ツール）

**強み:**
- スクリプト（C#ライクな構文）でビジネスロジックを記述可能
- SQL文を直接実行可能
- すべてテキストベース（JSON + SQL + C#スクリプト）で構成
- コンパイル不要で画面・ロジックを変更可能

**コア仕様のソースコード:** `Source/Codeer.LowCode.Blazor/`

**設定ファイルの実例:**
- [Samples/PatternShowcase/](Samples/PatternShowcase/) - **アプリ作成パターン (認証なし) の全実装サンプル** ([Docs/AppPatterns/](Docs/AppPatterns/) と対応、JSON / SQL / C# テキスト同梱)
- [Samples/PatternShowcaseAuth/](Samples/PatternShowcaseAuth/) - **認証パターン (ログイン / 権限 / 承認ワークフロー) の全実装サンプル**

機能を実装するときは、まず [Docs/AppPatterns/](Docs/AppPatterns/) で該当パターンを確認し、そのパターン名の対応モジュール (各パターン末尾の「標準パターン集の対応」セクション参照) を [Samples/PatternShowcase/App/Modules/](Samples/PatternShowcase/App/Modules/) または [Samples/PatternShowcaseAuth/App/Modules/](Samples/PatternShowcaseAuth/App/Modules/) から開いて、JSON / SQL / C# スクリプトの実物を参考にすること。

## デザインチェック CLI (作成・編集したら必ず実行する)

デザインファイルを作成・編集したら、**デザイナ exe の headless チェックモード**でデザイン定義を検証する。
モジュール一覧から消える・画面が真っ白になるといった「壊れて読み込めない」系の多くを、ブラウザで開く前に検出できる。

### 実行方法

```
"<デザイナexeのパス>" designcheck "<プロジェクトのルートフォルダ>" --out "<結果JSONの出力先パス>"
```

- `designcheck` … サブコマンド
- 第2引数 … `app.clprj` があるプロジェクトのルートフォルダ
- `--out` … 結果を書き出す JSON ファイルのパス (UTF-8)。指定しない場合は何も出力されない (終了コードのみ)
- **終了コード**: `0` = エラーなし / `1` = エラーあり / `2` = 実行失敗 (引数不正・読込失敗・例外)

結果 JSON は読みやすさのため UTF-8・整形済みで出力される:

```json
{
  "project": "...",
  "findingCount": 2,
  "findings": [
    { "type": "PageFrame", "position": "Main", "message": "モジュール 'Foo' のURLセグメント'Foo'は他と重複しています。" }
  ]
}
```

`--out` のパスを読んで `findings` を直し、`findingCount` が 0 (終了コード 0) になるまで繰り返す。

### このチェックが見るもの

- **ファイルの読み込み失敗** — JSON が不正、またはプロパティの型が定義と一致しない (例: 数値プロパティに `"200"` と文字列で書く) と、そのファイルは**静かに無視されてモジュール/ページフレームが欠落**する。これを「ファイル '...' を読み込めませんでした」として検出する (「モジュールが一覧から消えた」系の主因)
- 名前重複、レイアウト構造、スクリプト (文法・参照・型)、モジュール/フィールド/ページフレーム間の参照整合 など、デザイン定義の論理エラー
- **DB スキーマとの照合** — `DbColumn` が実テーブルに存在するか、データソース/テーブルが存在するか。GUI のデザインチェックと同じく、プロジェクト直下の `designer.settings.json` + `designer.settings.Development.json` から接続文字列を読んで **DB に接続し**、スキーマを取得して照合する。**テーブル/列が無いと報告された場合は、後述の「SQL 実行 CLI」で CREATE してから再チェックする**

> **DB に接続する点に注意 (重要)**: このチェックはプロジェクトの接続文字列で DB に接続する。`designer.settings.Development.json` が**本番 DB を指している可能性**もあるため、接続先が不明なプロジェクトに対して実行する前は接続先を確認すること ([Docs/ProjectSettings.md](Docs/ProjectSettings.md) 参照)。DB が起動していない/到達不能なデータソースは「データソースが存在しません」等として報告される (クラッシュはしない)。

### デザイナ exe のパスの調べ方

1. **まず [LocalEnvironment.md](LocalEnvironment.md) を読む** (この CLAUDE.md の隣にあるマシン固有ファイル)。`DesignerExePath:` 行にパスが書いてあればそれを使う
2. 無ければ**ユーザーに聞く**。聞くときは、**起動中のプロセスのうち名前に "Designer" を含むもの**を列挙して候補として提示する (実行中プロセスの実行ファイルパスがそのまま使える)。例 (PowerShell): `Get-Process | Where-Object { $_.Name -like '*Designer*' } | Select-Object Name, Path`
3. パスが分かったら、**[LocalEnvironment.md](LocalEnvironment.md) に書き込む** (次回からは 1 で拾える)。**この CLAUDE.md には書き込まない** — CLAUDE.md は配布されるファイルなので、マシン固有のパスを書くと汚れる。LocalEnvironment.md はマシンローカル (gitignore 対象) で配布物には含まれない

> 補足: デザイナ GUI が起動中でも、別引数 (`designcheck ...`) でもう一つ起動するのは問題ない (別プロセスでプロジェクトを読むだけ。GUI 側には干渉しない)。

## SQL 実行 CLI (DB の中身確認・データ投入・スキーマ変更)

デザイン作業中に DB を直接触りたいとき (明細がちゃんと親に紐づいているか / 件数は / 列の型は / テストデータを入れる / テーブルを作る 等) は、**デザイナ exe の headless SQL モード**で SQL を実行する。`designcheck` と同じ exe・同じ headless 経路で、プロジェクトの接続文字列を使って DB に接続する。

### 実行方法

```
"<デザイナexeのパス>" sql "<プロジェクトのルートフォルダ>" --datasource <データソース名> --query "<SQL>" --out "<結果JSONの出力先パス>"
```

- `sql` … サブコマンド
- 第2引数 … `app.clprj` があるプロジェクトのルートフォルダ
- `--datasource` … `designer.settings.json` の `DataSources` の `Name`
- `--query "<SQL>"` … 実行する SQL。**複数文を `;` 区切りで並べてもよい (全文実行される)**
- `--file "<path>"` … SQL をファイルから読む (`--query` の代わり。migration スクリプト等に)
- `--out "<path>"` … 結果 JSON の出力先 (UTF-8)。省略時は標準出力
- **終了コード**: `0` = 成功 / `2` = 失敗 (引数不正・許可なし・接続/SQL エラー)

### 出力

SELECT 等の結果セットは `results` に列と行、INSERT/UPDATE/DELETE/DDL は `recordsAffected`:

```json
{
  "dataSource": "Main",
  "results": [
    { "columns": ["id", "name"], "rowCount": 2, "rows": [ { "id": 1, "name": "a" }, { "id": 2, "name": "b" } ] }
  ],
  "recordsAffected": -1
}
```

DDL (CREATE / ALTER / DROP) は実行されるが `recordsAffected` は `-1` か `0` になる (行数の概念がないだけで失敗ではない)。

### 許可フラグ (AllowCliSqlAccess)

安全のため、**対象データソースの `designer.settings.json` で `AllowCliSqlAccess: true` のデータソースだけ** SQL を実行できる (既定 `false`)。

```json
"DataSources": [
  { "Name": "Main", "DataSourceType": "SQLite", "AllowCliSqlAccess": true }
]
```

- 標準テンプレート (空 / 入門サンプル / 標準パターン集 / 認証パターン集 / 在庫管理 / SFA / プロジェクト管理) はローカル SQLite なので最初から `true`
- 接続先が不明・本番 DB を指しうるプロジェクトでは `false` のままにして CLI からの実行を防ぐ

> **注意 (重要)**: このコマンドはプロジェクトの接続文字列で DB に**接続して実際に SQL を実行する**。書き込み・削除・スキーマ変更も含み、明示トランザクションを張らない (autocommit) ので即時反映される。読み取り専用モードは無く、実行できる範囲は接続ユーザーの権限次第。`designer.settings.Development.json` が**本番 DB を指している可能性**もあるため、接続先を確認してから使うこと。スキーマを変更した場合、起動中のデザイナ/サーバには列定義のキャッシュのため**再起動するまで反映されない**。

### デザイナ exe のパス

`designcheck` と同じ (上記「デザイナ exe のパスの調べ方」を参照)。GUI 起動中でも別プロセスで実行して問題ない。

## リネーム CLI (フィールド / モジュール / ページフレーム / レイアウトの名前変更)

フィールド・モジュール・ページフレーム・レイアウトの名前を変えるときは、**手作業で JSON をテキスト置換しない**。参照は JSON のプロパティだけでなく、スクリプト (C# 構文解析が必要)・リンク越しの参照 (`○○Link.対象名`)・他モジュールの条件/レイアウトなど広範に散在し、テキスト置換では**必ず取りこぼす／別物まで巻き込む**。

**デザイナ exe の headless リネームモード**を使う。デザイナ GUI と同じリファクタリングロジックで全参照を一括追従させ、`.sql`/スクリプト/CSS などのサイドカーファイルやデザインファイル自体の改名も行う。`designcheck` と同じ exe・同じ headless 経路で、**DB 接続は不要**。

### 実行方法

```
"<デザイナexeのパス>" rename-field     "<プロジェクトのルートフォルダ>" --module <モジュール名> --from <旧名> --to <新名> --out "<結果JSON>"
"<デザイナexeのパス>" rename-module    "<プロジェクトのルートフォルダ>" --from <旧名> --to <新名> --out "<結果JSON>"
"<デザイナexeのパス>" rename-pageframe "<プロジェクトのルートフォルダ>" --from <旧名> --to <新名> --out "<結果JSON>"
"<デザイナexeのパス>" rename-layout    "<プロジェクトのルートフォルダ>" --module <モジュール名> --layout-type <Detail|List|Search> --from <旧名> --to <新名> --out "<結果JSON>"
```

- 第2引数 … `app.clprj` があるプロジェクトのルートフォルダ
- `--module` … 対象モジュール名 (`rename-field` / `rename-layout` で必須)
- `--layout-type` … `Detail` / `List` / `Search` (`rename-layout` で必須)
- `--from` / `--to` … 旧名 / 新名
- `--out` … 結果 JSON の出力先 (UTF-8)。省略時は標準出力
- **終了コード**: `0` = 成功 / `2` = 失敗 (対象が見つからない・新名が重複・引数不正・例外)

### 何をやってくれるか

- **参照の追従（大半）** — レイアウトの `FieldName`・各種条件 (UserRead/Write 等)・スクリプト・`LinkField`/`ModuleField`/`SelectField`・他モジュールからの直接参照・ページフレームなどを横断して張り替える
- **サイドカー / ファイル自体の改名**:
  - `rename-field`: Query/ExecuteSql フィールドなら `{module}.{field}.sql` も改名
  - `rename-module`: `{module}.mod.json` 本体・`.mod.cs` (スクリプト)・配下の `.sql` サイドカーを改名
  - `rename-pageframe`: `{pageFrame}.frm.json` 本体・`{pageFrame}.css` サイドカーを改名
  - `rename-layout`: レイアウト辞書のキー (レイアウト名) を張り替え
- **表示テキストは対象外** — フィールド一覧列の `Label`・`DisplayName`・ボタンの `Text`・`LabelField` の `Text` など、画面に出る**表示キャプションは（参照ではなく文言なので）変更しない**。「業務用語に合わせてリネーム」のような依頼では、フィールド名を変えても旧語のキャプションが画面に残る。表示も揃えたい場合は、リネーム後にこれら表示テキストを別途手で直す（designcheck では壊れないので検出されない）。

### 追従しきれない参照がある → designcheck→手修正 まで必ずワンセット (重要)

リネームは手作業より遥かに漏れが少ないが、**万能ではない**。特に **リンクフィールド越しの参照は追従しないことがある**。例えばフィールド `商品.品名` を `Name` に変えても、別モジュールのスクリプト/並べ替え条件にある **`商品コードLink.品名`（`商品` を指すリンクを辿って `品名` を参照する形）は旧名のまま残る**（対象フィールドを直接指す参照ではなく、リンク経由で辿る参照のため）。

そのため **リネーム後は必ず `designcheck` を実行し、旧名を指す宙吊り参照を手で直すところまでを 1 セット**にする。designcheck はこれらを「`○○.品名 存在しない識別子です`」等として確実に検出する（＝黙って壊れることはない）。手順:

1. `rename-*` / `rename-batch` を実行
2. `designcheck` を実行し、findings を見る（DB テーブル未作成系のノイズは除く）
3. 旧名を指す残存参照（スクリプトの `link.旧名`、条件の変数など）を手修正する。**探すときの正典は designcheck の findings**（`○○.旧名 存在しない識別子です` が場所を正確に指す）。grep で旧名を探すのは補助にとどめる — 特に**日本語のフィールド名は部分一致で誤検知だらけになる**（旧名 `品名` は新名 `商品名` の部分文字列、`単価` は `仕入単価`/`標準単価` の部分文字列）。素朴に `品名`/`単価` で grep すると新名・別モジュールの同名別フィールドまで大量にヒットして判定できない。grep するなら `商品コードLink.品名` のような**「参照の形」で絞り込み**、残す/直すの最終判断は designcheck に置く
4. 再度 `designcheck` して旧名由来の findings が 0 になるまで繰り返す

> 補足: 実行後は `--out` の JSON を読み、`error` が無いこと・終了コード 0 を確認する。`renamed` フィールドは「対象自身以外にも変更が波及したか」の目安で、`false` でもリネーム自体は成功している (対象モジュール内で完結した場合)。**成否の最終判断は上の designcheck 手修正まで込みで行う。**

### 大量に一気にリネームするとき (rename-batch)

「英語の名前を全部日本語にする」のように**多数のリネームを一度に**行うときは、単体 verb を件数分呼ぶのではなく **`rename-batch`** を使う。単体 verb は 1 回ごとに全デザインの再読込 (+DB スキーマ取得) が走るため、件数に比例して遅くなる (実測: 10 件で連続呼び出し 約14秒 → 一括 約2秒)。`rename-batch` は**プロジェクト読込 1 回**で全 op を順に適用する。

```
"<デザイナexeのパス>" rename-batch "<プロジェクトのルートフォルダ>" --file "<ops.json>" --out "<結果JSON>"
```

`ops.json` は次の形の配列。各 op は単体 verb と同じ処理をする:

```json
[
  { "type": "module",    "from": "Product", "to": "商品" },
  { "type": "field",     "module": "商品", "from": "Name", "to": "品名" },
  { "type": "pageframe", "from": "Main", "to": "メイン" },
  { "type": "layout",    "module": "商品", "layoutType": "Detail", "from": "card", "to": "カード" }
]
```

- **順序が重要**: 上から順に適用される。モジュール名を変える op は、その**新しい名前**で field/layout を指す op より**前**に置く (例: 先に `Product→商品`、その後 `{"type":"field","module":"商品",...}`)。`field`/`layout` の `module` は「その時点での現在名」。
- **途中失敗で停止**: いずれかの op が失敗するとそこで止まる。**それまでの op はディスクに保存済み** (トランザクションではない) なので、`--out` の `results[]` でどこまで適用されたかを確認し、原因を直してから残りを再実行する。
- 終了コード: `0` = 全 op 成功 / `2` = 失敗あり。`--out` の `results[]` に op ごとの `ok` / `renamed` / `error`。
- 少数 (数件) なら単体 verb でよい。**「全部◯◯にして」系はこの一括を使う。**

### デザイナ exe のパス

`designcheck` と同じ (上記「デザイナ exe のパスの調べ方」を参照)。

## インストール済みデザイナが CLI サブコマンドに未対応のとき (バージョンアップを促す)

CLI サブコマンドは今後も増えていく。**古いデザイナ exe には、まだそのサブコマンドが無い**ことがある。未対応の exe に未知のサブコマンドを渡すと、headless として認識されず**GUI ウィンドウが起動してしまう**ことがある (エラーで即終了せず、`--out` の JSON も作られない)。

そのため、CLI 実行後は必ず次で成否を判定する:

- `--out` に指定した JSON が**生成され**、`error` が無く、終了コードが期待どおり (`designcheck`=0/1、その他=0) か
- 生成されない / デザイナのウィンドウが開いた / 応答しない ときは、**その版が当該サブコマンド未対応**と判断する

未対応と判断したら:

1. **その場は別の方法で一旦対処する** (例: リネームなら手作業のテキスト置換を慎重に行い designcheck で検証する / チェックならブラウザ確認に切り替える / field-catalog・script-catalog なら各カタログ節の「カタログが生成できないとき」の代替情報源で補う 等)。ただし代替手段は取りこぼしの多い次善策である旨を明示する
2. **ユーザーにデザイナのバージョンアップを促す** — 「この操作は新しいデザイナの `<サブコマンド名>` を使うのが正式な方法です。お使いのデザイナが未対応のようなので、最新版に更新してください」と伝える。恒久対応はバージョンアップ後に正式 CLI で行う

## フィールド型カタログ CLI (field-catalog) — このプロジェクトで使える全フィールド型

`Docs/Fields/` の各 `.md` は「読み物としての解説」だが、**実際にこのプロジェクトで使えるフィールド型の正確な一覧 (組み込み + このプロジェクトに追加された独自フィールド) は、デザイナ exe から動的に取得する**。独自フィールド (ProCode / 拡張ライブラリ) は `Docs/Fields/` に載っていないため、独自フィールドを含むプロジェクトでは特にこのカタログが真実の源になる。

### 実行方法

```
"<デザイナexeのパス>" field-catalog "<プロジェクトのルートフォルダ>" --out "temporary/_field_catalog.md"
```

- `field-catalog` … サブコマンド。プロジェクトを開いて (独自フィールドのアセンブリを読み込み)、全フィールド型の TypeFullName・プロパティ・既定値・候補・登録ドキュメント (`## Design` / `## Script` / `## CSS`) を Markdown で出力する
- **DB 接続は不要** (reflection ベース。開けなくても組み込み型は出力される)
- 終了コード: `0` = 成功 / `2` = 失敗

### 出力先

**出力先は `temporary/`（作業物置き場）**。`temporary/_field_catalog.md` は毎回作り直す**生成物**で、gitignore 済み（`ClaudeCodeForDesigner/` 配下には置かない＝配布ドキュメントに差分ノイズを出さない）。

### 自動再生成（ビルド検知）

`.claude/settings.local.json`（`settings.local.json.sample` を複製して exe パスを記入）の **SessionStart / UserPromptSubmit フック**が `.claude/refresh-field-catalog.ps1` を呼び、次の仕組みで**必要なときだけ**カタログを作り直す:

- デザイナ exe は独自フィールドライブラリを参照してビルドされるため、フィールドの追加/削除は**再ビルド = exe と同フォルダ DLL のタイムスタンプ更新**として現れる。
- スクリプトはそのタイムスタンプ最大値を `temporary/_field_catalog.stamp` に記録し、**前回と同じならスキップ・変化していれば全取得**する。全取得は `--out` の全体上書き（＝全置換）なので、**削除されたフィールドも消える**。
- これでセッション途中に ProCode / 拡張ライブラリ（Extras / Bindings）を再ビルド・追加しても、**次のプロンプトで自動的に最新化**される（再生成は DB 接続不要・リフレクションのみで一瞬）。

**それでもカタログに無い型を使おうとしている等、古い疑いがあれば手動で作り直してよい**（`field-catalog` は許可リスト済みで確認不要）:

```
"<デザイナexeのパス>" field-catalog "<プロジェクトのルートフォルダ>" --out "temporary/_field_catalog.md"
```

> フィールド型を使うときは、まず `temporary/_field_catalog.md`（このプロジェクトで実際に使える型の一覧・構造）を確認し、解説が必要なら `Docs/Fields/` を読む。独自フィールドは `Docs/Fields/` に無いのでカタログが唯一の情報源になる。

### カタログが生成できないとき（古いデザイナでの代替）

インストール済みの Codeer.LowCode.Blazor / Codeer.LowCode.Blazor.Designer が本書の期待するバージョンより古いと、
`temporary/_field_catalog.md` が生成されない・内容が不足することがある。その場合は次で代替して作業を続ける:

- **組み込みフィールド**: [Defaults/](Defaults/) の `{型名}.json`（デザイナの新規追加と同一の既定状態 = TypeFullName・プロパティ構造・既定値の正）と、[Samples/](Samples/) の実物モジュールを情報源にする
- **独自フィールド（Extras / ProCode 等）**: ワークスペース内に情報源が無い。デザイナ GUI で対象フィールドを 1 つ配置して保存してもらい、その JSON を雛形に deepcopy する。意味が不明なプロパティは推測で埋めず既定値のまま残す
- 代替はカタログより精度が落ちる。作業は続けつつ、ユーザーにデザイナのバージョンアップを促す

## スクリプトオブジェクトカタログ CLI (script-catalog) — スクリプトで使える全サービス・型

**この環境でスクリプト (*.mod.cs) から実際に使えるサービス・型・列挙型の一覧と使い方は、デザイナ exe から動的に取得する**。拡張ライブラリ (Extras 等) や独自登録のスクリプトオブジェクトはデザイナのビルドによって変わるため、このカタログが唯一の真実の源になる (入力補完と同じ型モデルから生成 = スクリプトで呼べないメンバーは載らない)。

### 実行方法

```
"<デザイナexeのパス>" script-catalog "<プロジェクトのルートフォルダ>" --out "temporary/_script_catalog.md"
```

- `script-catalog` … サブコマンド (デザイナ 1.3.12 以降)。登録済みのサービス (サービス名で直接アクセス) / new で生成できる型 / 列挙型を、メンバーシグネチャと登録ドキュメント (使い方・例) 付きの Markdown で出力する
- **DB 接続は不要**。終了コード: `0` = 成功 / `2` = 失敗

### 出力先と自動再生成

field-catalog と同じ扱い: **出力先は `temporary/_script_catalog.md`**（毎回作り直す生成物・gitignore 済み）。`.claude/refresh-field-catalog.ps1` フックが field-catalog と同時に「デザイナ再ビルド検知時だけ」再生成する（script-catalog 未対応の古いデザイナでは Designer.dll のバージョン判定で自動スキップされる）。

> 組み込み外のサービス (Excel / WebApi / Toaster / Mail 等) をスクリプトで使うときは、`temporary/_script_catalog.md` でこの環境に登録されているか・正確なシグネチャ・使用例を確認する。カタログに無いサービス・型は使えない。独自のサービス/型を追加する方法は [Docs/ScriptExtensions.md](Docs/ScriptExtensions.md) を参照。

### カタログが生成できないとき（古いデザイナでの代替）

デザイナが 1.3.12 未満だと script-catalog が無く、`temporary/_script_catalog.md` は生成されない。
**ファイルが無いときは、まず一度 `script-catalog` を手動実行してみる**（許可リスト済みで確認不要。フック未設定なだけの環境はこれで生成できる）。
`--out` の Markdown が生成されない／デザイナのウィンドウが開いた場合は未対応版と判断し
（前述「インストール済みデザイナが CLI サブコマンドに未対応のとき」参照）、次で代替して作業を続ける:

- **組み込みサービス（Logger / MessageBox / NavigationService / LoadingService 等）**: [Docs/Scripts.md](Docs/Scripts.md) と [Docs/Fields/_ScriptApi.md](Docs/Fields/_ScriptApi.md) が情報源
- **拡張サービス（Excel / WebApi / Toaster / Mail 等）**: [Samples/](Samples/) のスクリプト実例から呼び方を拾う（例: `PatternShowcase/App/Modules/ReportSample.mod.cs` = Excel、`ToastSample.mod.cs` = Toaster）。[Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) の Excel 注意事項も有効
- **最終確認は designcheck**: 存在しないサービス・メソッド参照はスクリプト解析エラーとして検出されるので、「書く → designcheck → 直す」のループで確定できる。シグネチャに確信が持てない呼び出しを未検証のまま残さない
- 代替はカタログより精度が落ちる（そのデザイナに Extras 等が入っているかも確認できない）。作業は続けつつ、ユーザーにデザイナのバージョンアップを促す

## 詳細リファレンス (Docs/)

各設定の詳細なプロパティ、JSON例、ランタイム動作は `Docs/` 以下のドキュメントを参照。

### 全体構成
| ドキュメント | 内容 |
|---|---|
| [Docs/ModuleDesign.md](Docs/ModuleDesign.md) | モジュール定義 (*.mod.json) の全体構造、CRUD権限、権限条件 |
| [Docs/Layouts.md](Docs/Layouts.md) | レイアウト（Detail/List/Search, Grid/Tab/Canvas, FieldLayout） |
| [Docs/PageFrame.md](Docs/PageFrame.md) | ページフレーム (*.frm.json) ナビゲーション構造 |
| [Docs/SearchConditions.md](Docs/SearchConditions.md) | 検索条件 (FieldValueMatch, FieldVariableMatch, MultiMatch) |
| [Docs/QueryAndSql.md](Docs/QueryAndSql.md) | クエリとSQL実行の総合ガイド（QueryField / ExecuteSqlField） |
| [Docs/Scripts.md](Docs/Scripts.md) | C#スクリプト (*.mod.cs) 文法リファレンス、組み込みサービス、Module/Field API |
| [Docs/ScriptExtensions.md](Docs/ScriptExtensions.md) | スクリプト拡張の仕組みと独自拡張の追加方法 (登録済みサービスの一覧・使い方は `temporary/_script_catalog.md`) |
| [Docs/ProjectSettings.md](Docs/ProjectSettings.md) | プロジェクト設定 (app.clprj, designer.settings.json) |
| [Docs/Authentication.md](Docs/Authentication.md) | 認証の仕組み (既定の Cookie 認証)。ログインの流れ・ユーザーテーブルの契約 (`PasswordCheckUserTableInfo`)・`AppUser` モジュールの必須構成・パスワードハッシュ・`CurrentUser`・権限の出し分け |
| [Docs/Enums.md](Docs/Enums.md) | 全列挙型リファレンス |
| [Docs/AppCss.md](Docs/AppCss.md) | カスタムCSS (app.css) DOM構造・セレクタパターン |
| [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) | レイアウト作成時の推奨ルール（好みに応じて変更可能） |
| [Docs/RowModulePattern.md](Docs/RowModulePattern.md) | 行モジュールパターン: ListField/DetailListField/TileListField で同じ行モジュールを再利用する構成と、AddRowsによるデモデータ準備例 |
| [Docs/ListPagePatterns.md](Docs/ListPagePatterns.md) | 一覧ページ・カスタム一覧パターン集 (パターン1〜5: 基本/一括処理/Detail/Tile表示/既定条件/カスタム自前構成)。パターン名でレシピを引ける |
| [Docs/CommonMistakes.md](Docs/CommonMistakes.md) | よくある間違いと対策（JSON型、Elements構造、LinkFieldパス等） |
| [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) | スクリプト作成時の規約・注意事項 |
| [Docs/SearchConditionGuidelines.md](Docs/SearchConditionGuidelines.md) | SearchCondition の LimitCount 設定ガイドライン |
| [Docs/DatabaseGuidelines.md](Docs/DatabaseGuidelines.md) | テーブル作成時の規約（主キー、命名規則、型対応） |
| [Docs/BorderStyleGuide.md](Docs/BorderStyleGuide.md) | カラム枠線（BorderStyle）の設定方法、罫線重複回避ルール、検索グリッドのカード化 |
| [Docs/JsonAbstractTypeFullName.md](Docs/JsonAbstractTypeFullName.md) | JsonAbstract 継承クラスの TypeFullName 一覧・チェックリスト（Field型/Layout型/Match条件/値型） |
| [Docs/BrowserTestGuide.md](Docs/BrowserTestGuide.md) | Playwright によるブラウザ自動スクショで動作確認する手順（settings.local.json への許可追加、WASM 初期化待機、デザイン変更の反映方法） |

### アプリ作成パターン集 (Docs/AppPatterns/)

業務アプリを CLB で組むときに繰り返し登場する「型」を解説する。**機能を実装する前にまず該当パターンを引き、対応する PatternShowcase / PatternShowcaseAuth のモジュールを参考実装として読むこと**。

| ドキュメント | 内容 |
|---|---|
| [Docs/AppPatterns/patterns.md](Docs/AppPatterns/patterns.md) | **全パターンのインデックス**。A〜J の 10 カテゴリ。ここから引く |
| [Docs/AppPatterns/auth_patterns.md](Docs/AppPatterns/auth_patterns.md) | 認証パターン集の入口 (`PatternShowcaseAuth` の対応) |

各パターンの末尾には **「標準パターン集の対応」** セクションがあり、`Samples/PatternShowcase/` (または `Samples/PatternShowcaseAuth/`) のどのモジュールが実装サンプルかを示している。例: 「マスタ参照 (多対1)」パターン → `Samples/PatternShowcase/App/Modules/Shop.mod.json` + `ShopType.mod.json` を読む。

> 注: このディレクトリは [Manual リポジトリ](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Manual) の `JP/patterns/` と**対になる内容**だが、機械的なコピーではない。Manual は人間向け（解説・画像つき）、こちらは Claude Code 向け（CommonMistakes 参照・JsonAbstract TypeFullName・実装上の注意）で、読者・トーン・リンク先が異なるため、**どちらかを直したらもう一方も読者に合わせて手で反映する**（一括コピーするスクリプトは廃止）。両者で事実（フィールド型・データ構造など）が食い違わないことだけは必ず揃える。

### フィールド型リファレンス

各フィールド型（組み込み + このプロジェクトの独自フィールド）の TypeFullName・プロパティ・既定値・候補・`## Design`/`## Script`/`## CSS` ドキュメントは、**自動生成カタログ [temporary/_field_catalog.md](temporary/_field_catalog.md) に集約**されている（デザイナ exe の `field-catalog` サブコマンドが生成、SessionStart フックで最新化。前述「フィールド型カタログ CLI」参照）。フィールド型を使うときはまずこのカタログで型名を grep して構造・例を確認する。**独自フィールド（ProCode / 拡張ライブラリ）はここにしか載らない**ので、独自フィールドを含むプロジェクトでは唯一の情報源になる。

**共通ドキュメント（`_` プレフィックス、カタログには含まれない共通基底）:**
- [Fields/_FieldCommon.md](Docs/Fields/_FieldCommon.md) - 共通基底プロパティ（FieldDesignBase, ValueFieldDesignBase, DbValueFieldDesignBase, ListFieldDesignBase）
- [Fields/_ScriptApi.md](Docs/Fields/_ScriptApi.md) - フィールド共通スクリプトAPI（全フィールド共通・値フィールド共通のプロパティ/メソッド）

## デザインファイルの構成

```
App/                                  # アプリケーションルート
├── app.clprj                         # プロジェクト設定 (JSON)
├── app.css                           # カスタムCSS（オプション）
├── designer.settings.json            # データソース・ストレージ定義 (JSON)
├── designer.settings.Development.json # 接続文字列・デプロイ設定 (JSON)
├── Modules/                          # モジュール定義
│   ├── *.mod.json                    # モジュール定義ファイル (JSON)
│   ├── *.mod.cs                      # モジュールのスクリプト (C#)
│   ├── *.{QueryFieldName}.sql         # クエリ用SQL（例: Module.Query.sql）
│   ├── *.{ExecuteSqlFieldName}.sql    # 実行用SQL（例: Module.SQL文の実行.sql）
│   └── サブフォルダ/                   # モジュールをフォルダで整理可能
├── PageFrames/                       # ページフレーム（ナビゲーション）定義
│   └── *.frm.json                    # ページフレーム定義ファイル (JSON)
└── Resources/                        # リソースファイル（画像等）
```

## ファイル形式の詳細

### 1. プロジェクトファイル (`app.clprj`)

アプリケーション全体の設定。

```json
{
  "CurrentUserModuleDesignName": "",
  "AppAccessConditions": { "ModuleName": "" },
  "BackgroundColor": "",
  "Color": "",
  "FontFamily": "",
  "Versions": [
    { "AssemblyName": "Codeer.LowCode.Blazor", "Version": "1.2.47.0" }
  ],
  "LocalizeResourcePath": ""
}
```

### 2. データソース設定 (`designer.settings.json`)

```json
{
  "DataSources": [
    {
      "Name": "Main",
      "DataSourceType": "PostgreSQL",
      "IdentityUserTable": "",
      "HasDbContext": false
    }
  ],
  "FileStorages": [
    { "FileStorageType": "FileSystem", "Name": "Local" }
  ]
}
```

`DataSourceType`: `PostgreSQL` / `SQLServer` / `MySQL` / `Oracle` / `SQLite`

### 3. モジュール定義ファイル (`*.mod.json`)

モジュール = 画面（ページ/フォーム）の定義。最も重要なファイル。

#### モジュールの2つの用途

1. **CRUDモジュール**: DBテーブルとマッピングし、データの作成・読み取り・更新・削除を行う。`DbTable` にテーブル名を指定し、`IdFieldDesign`、`SubmitButtonFieldDesign`、`ListLayouts` を設定する。
2. **表示専用モジュール**: DBと結びつかず、チャートダッシュボードやダイアログ等の表示のみを行う。この場合:
   - `DbTable` は空文字 `""` にする
   - `DataSourceName` は空文字 `""` にする
   - `IdFieldDesign` は不要（定義しない）
   - `SubmitButtonFieldDesign` は不要（定義しない）
   - `ListLayouts` の Elements にフィールドを入れないこと。フィールドを入れると一覧表示モジュールと認識されてしまう

#### 基本構造

```json
{
  "Name": "モジュール名",
  "DataSourceName": "データソース名",
  "DbTable": "DBテーブル名",
  "CanCreate": true,
  "CanUpdate": true,
  "CanDelete": true,
  "Fields": [ /* フィールド定義の配列 */ ],
  "UserWriteCondition": { "ModuleName": "" },
  "UserReadCondition": { "ModuleName": "" },
  "DataWriteCondition": { "ModuleName": "" },
  "DataReadCondition": { "ModuleName": "" },
  "DetailLayouts": { /* 詳細画面レイアウト */ },
  "ListLayouts": { /* 一覧画面レイアウト */ },
  "SearchLayouts": { /* 検索画面レイアウト */ },
  "LinkFieldNames": [],
  "ListPageFieldDesign": { /* 一覧ページ設定 */ }
}
```

#### フィールド定義

各フィールドは `TypeFullName` で型を指定する。主要な型は以下:

**IdFieldDesign** - 主キー
```json
{
  "DbColumn": "id",
  "IsManualInput": false,
  "CompositeIdVariables": [],
  "CompositeIdSeparator": "",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "Name": "Id",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.IdFieldDesign"
}
```

**TextFieldDesign** - テキスト入力
```json
{
  "DbColumn": "name",
  "IsMultiline": false,
  "IsAutoFitRows": false,
  "Placeholder": "",
  "MaxLength": null,
  "Rows": null,
  "TextEditEmptyType": "StringEmpty",
  "ShouldTrimAfterEdit": false,
  "SearchComparisonDefaultValue": null,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Name",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
}
```

**NumberFieldDesign** - 数値入力
```json
{
  "DbColumn": "price",
  "Placeholder": "",
  "Format": "",
  "Min": null,
  "Max": null,
  "IsSlider": false,
  "Step": null,
  "MaxFractionDigits": null,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "DisplayName": "",
  "IsRequired": false,
  "Name": "Price",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
}
```

**BooleanFieldDesign** - チェックボックス/トグル
```json
{
  "DbColumn": "is_active",
  "Text": "有効",
  "UIType": "CheckBox",
  "TrueText": "",
  "FalseText": "",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "Name": "IsActive",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign"
}
```
`UIType`: `CheckBox` / `ToggleButton` / `Switch`

**DateFieldDesign** - 日付入力
```json
{
  "DbColumn": "created_date",
  "Format": "",
  "IsYearMonthOnly": false,
  "IsUpdateProtected": false,
  "Name": "CreatedDate",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign"
}
```

**DateTimeFieldDesign** - 日時入力
```json
{
  "DbColumn": "updated_at",
  "SaveAsUtc": false,
  "Format": "",
  "IsUpdateProtected": false,
  "Name": "UpdatedAt",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateTimeFieldDesign"
}
```

**TimeFieldDesign** - 時刻入力
```json
{
  "DbColumn": "start_time",
  "SaveAsUtc": false,
  "IsUpdateProtected": false,
  "Name": "StartTime",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TimeFieldDesign"
}
```

**SelectFieldDesign** - ドロップダウン選択
```json
{
  "DbColumn": "status",
  "Candidates": ["未着手,1", "進行中,2", "完了,3"],
  "SearchCondition": {
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "",
    "Condition": { "IsOrMatch": false, "IsNot": false, "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition" }
  },
  "ValueVariable": "",
  "DisplayTextVariable": "",
  "EmptyCandidateType": "StringEmpty",
  "AllowOrSearch": false,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "Name": "Status",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SelectFieldDesign"
}
```
`Candidates`: `"表示テキスト,値"` の形式。値省略時は表示テキスト=値。
別モジュールから候補を取得する場合は `SearchCondition.ModuleName` + `ValueVariable` + `DisplayTextVariable` を設定。

**RadioGroupFieldDesign** + **RadioButtonFieldDesign** - ラジオボタン
```json
{
  "DbColumn": "priority",
  "AllowOrSearch": false,
  "PopulateRadioButtons": false,
  "Name": "Priority",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioGroupFieldDesign"
}
```
各ボタン:
```json
{
  "Text": "高",
  "Value": "High",
  "GroupField": "Priority",
  "Name": "PriorityHigh",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.RadioButtonFieldDesign"
}
```

**FileFieldDesign** - ファイルアップロード
```json
{
  "StorageName": "Local",
  "DbColumnFileName": "file_name",
  "DbColumnFileSize": "file_size",
  "DbColumnFileGuid": "file_guid",
  "ObjectFit": "Contain",
  "ShowPreview": false,
  "IsUpdateProtected": false,
  "DisplayName": "",
  "Name": "Attachment",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FileFieldDesign"
}
```

**PasswordFieldDesign** - パスワード入力
```json
{
  "DisplayName": "",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Password",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.PasswordFieldDesign"
}
```

**LabelFieldDesign** - ラベル（表示のみ）
```json
{
  "Text": "タイトル",
  "Icon": "",
  "Style": "H1",
  "RelativeField": "",
  "OnClick": "",
  "Name": "TitleLabel",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LabelFieldDesign"
}
```
`Style`: `Default` / `H1` / `H2` / `H3` / `H4` / `H5` / `H6`
`RelativeField`: 指定すると、そのフィールドの値を表示テキストとして使用。

**ImageViewerFieldDesign** - 画像表示
```json
{
  "ResourcePath": "lc_logo_256.png",
  "ObjectFit": "Contain",
  "OnClick": "",
  "Name": "Logo",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ImageViewerFieldDesign"
}
```

**ButtonFieldDesign** - ボタン
```json
{
  "Text": "保存",
  "Icon": "",
  "Variant": "Primary",
  "OnClick": "SaveButton_OnClick",
  "ImageResourcePath": "",
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "ShowTextInToolTip": false,
  "Name": "SaveButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ButtonFieldDesign"
}
```
`Variant`: `Primary` / `Secondary` / `Success` / `Danger` / `Warning` / `Info` / `Light` / `Dark` / `Link`

**SubmitButtonFieldDesign** - 送信ボタン（データ保存）
```json
{
  "Text": "登録",
  "Icon": "",
  "Variant": "Primary",
  "IsBlock": true,
  "ImageResourcePath": "",
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "Name": "SubmitButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SubmitButtonFieldDesign"
}
```

**LinkFieldDesign** - 他モジュールとのリンク（外部キー）
```json
{
  "DbColumn": "category_id",
  "SearchCondition": {
    "LimitCount": 50,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Category",
    "Condition": { "IsOrMatch": false, "IsNot": false, "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition" }
  },
  "ValueVariable": "Id.Value",
  "DisplayTextVariable": "Name.Value",
  "ListLayoutName": "",
  "SearchLayoutName": "",
  "OnSearchButtonClicked": "",
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "Name": "Category",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LinkFieldDesign"
}
```
- `DbColumn`: 自テーブルの外部キー列
- `SearchCondition.ModuleName`: 参照先モジュール名
- `ValueVariable`: 参照先のID取得式（例: `Id.Value`）
- `DisplayTextVariable`: 参照先の表示テキスト取得式（例: `Name.Value`）

**ListFieldDesign** - 一覧表示（子レコード表示等）
```json
{
  "LayoutName": "",
  "CanNavigateToDetail": true,
  "NavigateModuleUrlSegment": "",
  "CanCustomizeColumns": false,
  "DisplayName": "",
  "SearchCondition": {
    "LimitCount": 50,
    "SelectFields": [],
    "SortConditions": [],
    "ModuleName": "OrderItem",
    "Condition": {
      "IsOrMatch": false, "IsNot": false,
      "Children": [
        {
          "SearchTargetVariable": "OrderId.Value",
          "Comparison": "Equal",
          "Variable": "Id.Value",
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
        }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "PagerPosition": "Bottom",
  "UseIndexSort": false,
  "DeleteTogether": false,
  "CanCreate": true,
  "CanUpdate": true,
  "CanDelete": true,
  "CanUserSort": true,
  "CanSelect": false,
  "Name": "Items",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
}
```

**DetailListFieldDesign** - 明細リスト（親子関係のインライン編集）
```json
{
  "LayoutName": "",
  "DisplayName": "",
  "SearchCondition": {
    "ModuleName": "OrderDetail",
    "Condition": { ... }
  },
  "DeleteTogether": true,
  "CanCreate": true,
  "CanUpdate": true,
  "CanDelete": true,
  "Name": "Details",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DetailListFieldDesign"
}
```

**SearchFieldDesign** - 検索フォーム
```json
{
  "ResultsViewFieldName": "List",
  "LayoutName": "",
  "OnSearched": "",
  "UserUrlParameter": false,
  "SearchInitializationTriggerUrlParameter": "",
  "Name": "Search",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchFieldDesign"
}
```

**ModuleFieldDesign** - 他モジュールの埋め込み
```json
{
  "DbColumn": "",
  "ModuleName": "EmbeddedModule",
  "LayoutName": "",
  "IsUpdateProtected": false,
  "IgnoreModification": false,
  "Name": "EmbeddedForm",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ModuleFieldDesign"
}
```

**AnchorTagFieldDesign** - リンク/ナビゲーション
```json
{
  "Style": "Text",
  "Target": "Url",
  "ShouldOpenInNewTab": false,
  "Icon": "",
  "TitleText": "詳細を見る",
  "TitleVariable": "",
  "ImageResourcePath": "",
  "PageFrame": "",
  "Module": "TargetModule",
  "ModuleVariable": "",
  "IdVariable": "Id.Value",
  "Url": "",
  "OnClick": "",
  "Name": "DetailLink",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.AnchorTagFieldDesign"
}
```
`Target`: `Url`（URLまたはモジュールへのナビゲーション） / `HistoryBack`（ブラウザ戻る） / `HistoryForward`（ブラウザ進む）
モジュール遷移は `Target: "Url"` + `Module` プロパティで実現する。

**HeaderMenuFieldDesign** / **SidebarMenuFieldDesign** / **ContextMenuFieldDesign** - メニュー系
```json
{
  "LayoutName": "",
  "PageFrame": "",
  "Name": "HeaderMenu",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.HeaderMenuFieldDesign"
}
```

#### 詳細画面レイアウト (DetailLayouts)

GridLayoutDesign でフィールドを配置する。`""` キーがデフォルトレイアウト。

```json
"DetailLayouts": {
  "": {
    "OnBeforeInitialization": "",
    "OnAfterInitialization": "",
    "OnLocationChanging": "",
    "OnFieldDataChanged": "",
    "DataOnlyFields": [],
    "ClassName": "",
    "Color": "",
    "BackgroundColor": "",
    "Layout": {
      "Name": "",
      "Padding": {},
      "IsBordered": false,
      "IsFlowLayout": false,
      "IsFillAvailable": false,
      "ScrollDirection": "Unset",
      "BackgroundColor": "",
      "Rows": [
        {
          "IsWrap": false,
          "Margin": {},
          "GridRowType": "Normal",
          "CanResize": false,
          "BackgroundColor": "",
          "Columns": [
            {
              "Layout": {
                "FieldName": "NameLabel",
                "ContextMenu": "",
                "ClassName": "",
                "FontFamily": "",
                "Color": "",
                "BackgroundColor": "",
                "Name": "",
                "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
              },
              "Width": 150,
              "Padding": {},
              "BackgroundColor": "",
              "BorderStyle": { "LeftColor": "", "TopColor": "", "RightColor": "", "BottomColor": "" },
              "VerticalAlignment": "Middle",
              "HorizontalAlignment": "Left",
              "CanResize": false,
              "Border": "None"
            },
            {
              "Layout": {
                "FieldName": "Name",
                "ContextMenu": "",
                "ClassName": "",
                "FontFamily": "",
                "Color": "",
                "BackgroundColor": "",
                "Name": "",
                "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
              },
              "Width": null,
              "Padding": {},
              "BackgroundColor": "",
              "BorderStyle": { "LeftColor": "", "TopColor": "", "RightColor": "", "BottomColor": "" },
              "VerticalAlignment": "Middle",
              "CanResize": false,
              "Border": "None"
            }
          ]
        }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
    }
  }
}
```

**レイアウト構造:**
- `GridLayoutDesign` → `Rows[]` → `Columns[]` → `FieldLayoutDesign`（フィールド名を指定）
- `Width: null` は自動幅、数値指定で固定幅（px）
- `VerticalAlignment`: `Top` / `Middle` / `Bottom`
- `HorizontalAlignment`: `Left` / `Center` / `Right`

**ネスト可能なレイアウト:**
- `GridLayoutDesign` - グリッド（行・列ベース）
- `TabLayoutDesign` - タブ切り替え
- `CanvasLayoutDesign` - 絶対位置配置

#### 一覧画面レイアウト (ListLayouts)

```json
"ListLayouts": {
  "": {
    "HeaderTitle": "",
    "DataOnlyFields": [],
    "OnBeforeInitialization": "",
    "OnAfterInitialization": "",
    "OnFieldDataChanged": "",
    "Elements": [
      [
        {
          "FieldName": "Code",
          "Label": "コード",
          "ColumnSpan": 1,
          "RowSpan": 1,
          "TextWrap": "Unset",
          "CanResize": true,
          "CanUserSort": true,
          "ClassName": "",
          "Color": "",
          "BackgroundColor": "",
          "DetailLayoutName": ""
        },
        {
          "FieldName": "Name",
          "Label": "名前",
          "ColumnSpan": 1,
          "RowSpan": 1,
          "CanResize": true,
          "CanUserSort": true
        }
      ]
    ]
  }
}
```

`Elements` は行の配列（通常1行）。各行内の配列が横に並ぶ列を定義する。

> レイアウト作成時の推奨ルールは [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) を参照。

#### 検索画面レイアウト (SearchLayouts)

```json
"SearchLayouts": {
  "": {
    "OnSearchInitialization": "",
    "ShowDefaultSearchButtons": true,
    "Layout": {
      "Operator": "And",
      "Rows": [ /* GridLayoutDesign と同じ構造 */ ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign"
    }
  }
}
```

### 4. ページフレーム定義ファイル (`*.frm.json`)

アプリのナビゲーション構造を定義する。

```json
{
  "IsApplicationRoot": true,
  "Name": "Main",
  "Description": "",
  "Left": {
    "IsVisible": true,
    "Home": { "Type": "Text", "Text": "Home", "Icon": "", "ResourcePath": "" },
    "Links": [
      {
        "Title": "商品一覧",
        "Icon": "",
        "IconType": "Font",
        "HideTitle": false,
        "ModulePageType": "Auto",
        "ModuleUrlSegment": "",
        "ActiveModuleSegments": [],
        "PageFrame": "",
        "Module": "Product",
        "Id": "",
        "Parameters": "",
        "ListPageDesign": {
          "SearchLayoutName": "",
          "UserUrlParameter": true,
          "PageTitle": "",
          "HeaderTitle": "",
          "CanBulkDataUpdate": false,
          "CanBulkDataDownload": false,
          "UseSubmitButton": false,
          "UseNavigateToCreate": true,
          "ListFieldDesign": {
            "LayoutName": "",
            "CanNavigateToDetail": true,
            "NavigateModuleUrlSegment": "",
            "CanCustomizeColumns": false,
            "DisplayName": "",
            "SearchCondition": {
              "LimitCount": 50,
              "SelectFields": [],
              "SortConditions": [],
              "SortFieldVariable": "",
              "SortDescending": false,
              "ModuleName": ""
            },
            "PagerPosition": "Bottom",
            "UseIndexSort": false,
            "DeleteTogether": false,
            "CanCreate": false,
            "CanUpdate": false,
            "CanDelete": true,
            "CanUserSort": true,
            "CanSelect": false,
            "Name": "",
            "IgnoreModification": false,
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
          }
        },
        "DetailPageDesign": {
          "PageTitle": "",
          "LayoutName": "",
          "UrlParameter": ""
        }
      }
    ]
  },
  "Right": { "IsVisible": false, "Links": [] },
  "Header": { "IsVisible": false, "Links": [] }
}
```

- `ModulePageType`: `Auto`（自動判定）/ `ListToDetail`（一覧→詳細）/ `List`（一覧のみ）/ `Detail`（詳細のみ）

### 5. スクリプトファイル (`*.mod.cs`)

モジュールのイベントハンドラをC#ライクな構文で記述。
ファイル名はモジュール名と一致させる（例: `Product.mod.cs`）。

```csharp
// フィールドのイベントハンドラ
void SaveButton_OnClick()
{
    // フィールドに直接アクセス可能
    if (Name.Value == "")
    {
        MessageBox.Show("名前を入力してください");
        return;
    }
    LabelResult.Text = "保存しました";
}

void Status_OnDataChanged()
{
    // フィールドの値変更時の処理
    LabelResult.Text = "ステータスが変更されました: " + Status.Value;
}

void Search_OnSearchDataChanged()
{
    // 検索条件変更時の処理
}
```

**スクリプトで利用可能なオブジェクト:**
- モジュール内の全フィールド（フィールド名で直接アクセス）
- `MessageBox.Show(message)` - メッセージ表示
- `Logger.Log(message)` / `Logger.Warn(message)` / `Logger.Error(message)`
- `NavigationService` - ページナビゲーション
- `Resources` - リソースアクセス
- `BatchSearcher` - 複数モジュール一括検索

**イベントの命名規則:**
- `{フィールド名}_OnClick` - ボタンクリック時
- `{フィールド名}_OnDataChanged` - 値変更時
- `{フィールド名}_OnSearchDataChanged` - 検索条件変更時
- `OnBeforeInitialization` - 初期化前（DetailLayout/ListLayoutで定義）
- `OnAfterInitialization` - 初期化後
- `OnLocationChanging` - ページ離脱前
- `OnFieldDataChanged` - いずれかのフィールド変更時

### 6. SQLファイル

モジュールに関連するSQLクエリ。ファイル名規則:
- `{モジュール名}.{QueryFieldのName}.sql` - SELECT用クエリ（例: `Summary.Query.sql`）
- `{モジュール名}.{ExecuteSqlFieldのName}.sql` - INSERT/UPDATE/DELETE等の実行SQL

パラメータは `@パラメータ名` で記述。モジュールのフィールド値がバインドされる。

**QueryField を使うクエリ専用モジュールの構成:**
- `DbTable`: 空文字 `""` にする
- `CanCreate`/`CanUpdate`/`CanDelete`: `false`
- `Id` フィールド: 不要
- QuerySetting.Parameters: **出力列を `IsParameter: false`** で全て定義、入力パラメータは `IsParameter: true` で定義
- 各フィールドの `DbColumn`: Parameters の `Name` と一致させる
- システムページングパラメータ（`rows_per_page`, `offset`）は明示的宣言不要

## 検索条件の構造 (MatchCondition)

モジュール間のデータ関連付けや検索フィルタに使用。

**FieldVariableMatchCondition** - フィールド同士の比較
```json
{
  "SearchTargetVariable": "CategoryId.Value",
  "Comparison": "Equal",
  "Variable": "Id.Value",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
}
```

**FieldValueMatchCondition** - フィールドと固定値の比較
```json
{
  "SearchTargetVariable": "Status.Value",
  "Comparison": "Equal",
  "Value": { "Value": "Active" },
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
}
```

**MultiMatchCondition** - 複合条件（AND/OR）
```json
{
  "IsOrMatch": false,
  "IsNot": false,
  "Children": [ /* MatchConditionBase の配列 */ ],
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
}
```

**MatchComparison**: `Equal` / `NotEqual` / `LessThan` / `LessThanOrEqual` / `GreaterThan` / `GreaterThanOrEqual` / `Like` / `In` / `NotIn` / `Exists` / `NotExists`

## DBカラム型とフィールド型の対応

| DBカラム型 | フィールド型 |
|---|---|
| UUID / INTEGER (PK) | IdFieldDesign |
| VARCHAR / TEXT | TextFieldDesign |
| INTEGER / DECIMAL / NUMERIC | NumberFieldDesign |
| BOOLEAN | BooleanFieldDesign |
| DATE | DateFieldDesign |
| TIMESTAMP | DateTimeFieldDesign |
| TIME | TimeFieldDesign |
| INTEGER / VARCHAR (FK) | LinkFieldDesign |
| VARCHAR (選択肢) | SelectFieldDesign |
| ファイル関連列3つ | FileFieldDesign |

## 設定ファイル生成のポイント

1. **モジュール名** = PascalCase（例: `ProductCategory`）
2. **フィールド名** = PascalCase（例: `ProductName`）
3. **DBカラム名** = snake_case（例: `product_name`）
4. **全フィールドに `TypeFullName` が必須** - 型の完全修飾名
5. **レイアウトはフィールド名で参照** - `FieldLayoutDesign.FieldName`
6. **モジュール間参照は `SearchCondition.ModuleName`** で指定
7. **スクリプトイベントは文字列で指定** - 対応する `.mod.cs` ファイルにメソッドを記述
8. **`""` キーがデフォルトレイアウト** - 名前付きレイアウトも定義可能
9. **JSON数値型に注意** - `int`型プロパティに `50.0` のような小数点付き数値を書くとデシリアライズエラーになる。各ドキュメントのプロパティ定義にC#型を明記しているので参照すること
10. **ListLayout の Elements 構造** - `Elements[行][列]` の二重配列。通常は `[[Col1, Col2, Col3]]` のように全列を1つの内側配列に入れる。`[[Col1], [Col2]]` だと列が縦に並ぶ（間違い）
11. **LinkField の値パス** - `LinkFieldName.Value` で外部キー値にアクセス。`LinkFieldName.Id.Value` は誤り
12. **子リストの LimitCount** - 詳細画面の DetailListField/ListField は `LimitCount: null`（全件）。ページ一覧では `50`
13. **スクリプトで this** - `this.Submit()`, `this.ValidateInput()`, `this.IsNewData` 等のモジュールメソッド/プロパティにはthisを付ける
14. **AnchorTagField の Target** - 有効な値は `Url` / `HistoryBack` / `HistoryForward` のみ。モジュール遷移は `Target: "Url"` + `Module` プロパティで実現
15. **IsViewOnly はレイアウト要素のプロパティ** - フィールド定義（Fields配列）ではなく、FieldLayoutDesign / GridLayoutDesign / ListElement に設定する。詳細は [Docs/Layouts.md](Docs/Layouts.md) の IsViewOnly セクション参照
16. **一覧ページのソートは PageFrame で設定** - モジュールの `ListPageFieldDesign` ではなく、PageFrame の `ListPageDesign.ListFieldDesign.SearchCondition.SortConditions` で設定する。詳細は [Docs/PageFrame.md](Docs/PageFrame.md) のソート設定の優先順位を参照
17. **LinkFieldNames** - 他モジュールのフィールドを表示するには `LinkFieldNames` にパスを追加し、レイアウトでも参照が必要。詳細は [Docs/ModuleDesign.md](Docs/ModuleDesign.md) の LinkFieldNames の詳細を参照
18. **IsUpdateProtected と IsViewOnly の違い** - `IsUpdateProtected` はサーバーサイドでも更新防止（フィールド定義）、`IsViewOnly` は表示上の読み取り専用（レイアウト要素）。混同しないこと
19. **表示専用モジュールの ListLayout にフィールドを入れない** - `DbTable` が空のモジュールでは `ListLayouts` の Elements にフィールドを入れるとと一覧表示モジュールと認識されてしまう。Id / SubmitButton も不要。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #22 を参照
20. **よくある間違い** - 詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) を参照
21. **スクリプトで `await` を使わない** - スクリプトエンジンが非同期メソッドを自動で同期処理する。`await this.Submit()` ではなく `this.Submit()` と書くこと。`await MessageBox.Show()` ではなく `MessageBox.Show()` と書くこと
22. **Null条件演算子（`?.`）は使用可能** - `product?.Name.Value` のようにnull安全なプロパティアクセス・メソッド呼び出し・チェーンが可能。`??` と組み合わせて `product?.Name.Value ?? "default"` も使える。ただし **Null条件インデクサ（`?[]`）は使えない**。Null合体演算子（`??`）も使用可能
23. **サイドバーの階層メニュー** - PageFrame の `Title` に `/` 区切りで文字列を指定すると階層メニューが自動生成される（例: `"マスタ/取引先"` → 「マスタ」グループ配下に「取引先」リンク）。詳細は [Docs/PageFrame.md](Docs/PageFrame.md) の Title による階層メニュー を参照
24. **カラム枠線は BorderStyle で設定** - `Border` プロパティ（"None"/"All"等）は非推奨。`BorderStyle` の太さ（`Left`/`Top`/`Right`/`Bottom`）と色（`LeftColor`等）の両方を設定すること。太さを指定しないと線は表示されない。隣接セルとの重複回避ルールあり。詳細は [Docs/BorderStyleGuide.md](Docs/BorderStyleGuide.md) を参照
25. **リストのチェックボックスには BooleanField を使う** - `CanSelect` は行選択ハイライトであり、チェックボックスではない。チェックボックスは `BooleanFieldDesign`（UIType: CheckBox）をListElementに `IsViewOnly: false` で配置し、`CanSelect: false` にする。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #26 を参照
26. **表示専用モジュールでも入力があるなら CanUpdate: true** - `DbTable` が空のモジュールでも、リスト内に入力可能フィールド（チェックボックス、枚数入力等）がある場合は `CanUpdate: true` にしないと画面全体がViewOnlyになる。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #27 を参照
27. **FieldValueMatchCondition の Value には TypeFullName が必須** - `Value` プロパティは `MultiTypeValue`（抽象クラス）のため、`StringValue` / `DecimalValue` / `BooleanValue` 等の TypeFullName を必ず指定する。例: `{"Value": "AH", "TypeFullName": "Codeer.LowCode.Blazor.Repository.StringValue"}`。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #28 を参照
28. **外部キーには NumberField を使わない** - 親子関係の外部キーフィールドには `LinkFieldDesign` または `IdFieldDesign` を使う。`NumberFieldDesign` は不可。フレームワーク内部で親レコード未保存時にテンポラリ ID（文字列）が一時的に使われるため。DB カラムの型は実際の ID 型（INTEGER 等）でよい。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #23 を参照
29. **ViewEditToggleButton は SubmitButton を初期非表示にする** - ViewEditToggleButtonField をモジュールに置くと、初期化時に同一モジュール内の全 SubmitButton が `IsVisible = false` になる（編集モードに入ったときだけ表示）。知らないと「Submit が消えた」バグに見える。詳細は [ViewEditToggleButtonField](temporary/_field_catalog.md) と [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #29 を参照
30. **Grid 中央配置は空セルパターンを使う** - `[空 \| content \| 空]` のように Layout=null の空セルで挟むと、中身はコンテンツサイズで中央配置、左寄せは `[content \| 空]`、右寄せは `[空 \| content]`。詳細は [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) の Grid 中央配置パターンを参照
31. **サイドバー/ヘッダーをモジュール化できる** - `SideBarDesign.ModuleName` / `HeaderDesign.ModuleName` に表示専用モジュールを指定すると、標準UIの代わりにそのモジュールの `DetailLayouts[""]` が描画される。Home/Links/Logout は出なくなるので必要なら自前実装する。詳細は [Docs/PageFrame.md](Docs/PageFrame.md) の ModuleName セクションを参照
32. **ReloadWithLock() は存在しない** - 古いバージョンのスクリプト例に `module.ReloadWithLock()` を呼ぶものが残っていることがあるが、現在の公開APIには存在しない。再読込は `module.Reload()` を使う。ロック付き再読込が必要なら `ExecuteSqlField` で `SELECT ... FOR UPDATE` する方針を検討。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #30 を参照
33. **CanvasElement に Name プロパティはない** - スクリプトから個別 Element を直接操作する手段はない。Element 内に `GridLayoutDesign` 等をネストして、その Layout の `Name` 経由でアクセスする。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #32 を参照
34. **存在しない FieldDesign クラスを使わない** - `CreatedByFieldDesign` / `ChildModuleFieldDesign` / `ImageFieldDesign` 等は CLB に実在しない。実在クラスは `Source/Codeer.LowCode.Blazor/Repository/Design/*FieldDesign.cs` と [Docs/Fields/](Docs/Fields/) で確認。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #34 を参照
35. **一覧ページで編集可能にしない** - PageFrame の Auto/ListToDetail で `ListFieldDesign.CanCreate/CanUpdate` を `true` にすると一覧画面で行内編集 UI が出る。一般的な業務アプリでは一覧で閲覧 + 削除のみ、新規/編集は詳細画面で。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #35 を参照
36. **一覧→詳細フローの詳細画面に戻るボタンを置く** - `AnchorTagFieldDesign` の `Target: "HistoryBack"` + `Icon: "bi bi-arrow-left-circle-fill"` + `FontSize: 30` を DetailLayout の 1 行目に配置。`Samples/PatternShowcase/App/Modules/Product.mod.json` 等の詳細レイアウト参考。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #36 を参照
37. **デザイン系新規作成時は C# プロパティのデフォルト値を踏襲する (重要)** - Module / Field / Layout / PageFrame / PageLink などを生成するときは、推測でプロパティ値を埋めず、`Source/Codeer.LowCode.Blazor/Repository/Design/*.cs` の C# 初期値をそのまま使う。確たる理由がない限り変更しない。例: `ModuleDesign.CanCreate/CanUpdate/CanDelete` は全部 `true` がデフォルト。表示専用モジュールでも `CanUpdate: true` にしないと入力フィールドが ViewOnly になる ([#27](Docs/CommonMistakes.md#27)参照)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #37 を参照
38. **PageFrame に登録されてないモジュールは真っ白になる** - 親→子の詳細遷移先 / ダイアログモジュール / Lookup マスタ等、サイドバーに出さなくても画面遷移する可能性があるモジュールは PageFrame の `OtherPageModuleDesigns` に登録する。`ChildModuleFieldDesign` (= `ModuleFieldDesign`) で埋め込みするだけのモジュールは登録不要。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #38 を参照
39. **SearchLayouts は空辞書ではなく `""` キーの空レイアウトを必ず作る** - `SearchLayouts: {}` のままだと `GetSearchCondition()` が空 `SearchCondition` を返し、`ListField.SetAdditionalConditionAsync` で `モジュール名が一致しません` 例外で真っ白になる。検索画面が要らないモジュールでも `SearchLayouts[""]` に空の `SearchGridLayoutDesign` を入れる。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #39 を参照
40. **親子 ListField は `SearchCondition.Condition` で絞り込みを書く** - `LinkFieldNames` は参照宣言だけで、フィルタは別途。`FieldVariableMatchCondition` で `SearchTargetVariable: "OrderId.Value"` (子FK) / `Variable: "Id.Value"` (親PK) を `Equal` で指定。多対多も同パターンで中間テーブル経由。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #40 を参照
41. **論理削除は Field名を `LogicalDelete` にする** - `SystemFieldNames.LogicalDelete` 予約名の Boolean がある場合のみ、削除ボタンが `UPDATE SET LogicalDelete=true` に変わる。任意の `IsDeleted` 等では物理削除のまま。CLB の自動論理削除フィルタも同じ予約名でだけ動く。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #41 を参照
42. **楽観ロックは `IncrementVersion: true` で動かす** - `OptimisticLockingFieldDesign` のデフォルトは `false` (PostgreSQL `xmin` 前提)。SQLite/SQL Server/MySQL でアプリ側のバージョン管理が必要なら `IncrementVersion: true` を明示。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #42 を参照
43. **CSV インポート/エクスポートは PageFrame の Link 側で有効化** - `Link.ListPageDesign.CanBulkDataUpdate/CanBulkDataDownload` を true にする。モジュール側の同名プロパティだけでは一覧画面のアイコンボタンが出ない。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #43 を参照
44. **一覧ページに行選択+一括処理ボタンは置けない** - ListPage は内部で `$List$Module` を動的生成し、システムフィールド (`ListPageList`/`ListPageSearch`/`ListPageSubmit`) のみで構成されるため、任意のボタンを足せない。一括削除/更新は「親 (表示専用) モジュールの詳細レイアウト」に ListField + ボタンで実装し、PageLink を `ModulePageType: "Detail"` にする。子の ListLayout に `IsSelected` (BooleanField, `DbColumn` なし) を出して、スクリプトで `foreach (var row in Items.Rows) if (row.IsSelected.Value == true) Items.DeleteRow(row);` → SubmitButton で DB 反映。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #44 を参照
45. **Id (連番システム ID) は表示しない、ただし DB-backed モジュールは Id 定義必須** (絶対常識) - ListLayout/DetailLayout のどちらにも `IdFieldDesign` の `Id` フィールドを出さない (空セルになる、業務的に無意味)。一方で **`DbTable`/`DataSourceName` を持つモジュールは Fields 配列の Id 定義が必須** — これがないと CLB が一覧→詳細遷移の URL `/Module/{id}` を組まず、`>` ボタンが効かない、`ListField` の親子フィルタ (`Variable: "Id.Value"`) も解決しない。**表示専用モジュール (`DbTable: ""`/`DataSourceName: ""`、ダイアログ・ダッシュボード等) は Id 不要** (どのデータを表示するか特定が要らないため)。行番号が欲しい時は `ListNumberFieldDesign` を別途追加。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #45 を参照
46. **論理削除モジュールでは LogicalDelete を UI に出さない** - `LogicalDelete` (BooleanField 予約名) は Fields に定義するが、ListLayout/DetailLayout のどちらにも入れない。フラグ確認用に管理画面が必要なら**別モジュール**を作って同じ DbTable + Boolean 名を `LogicalDelete` 以外 (例: `DeletedFlag`) にして CLB の自動フィルタを回避。管理モジュールは `CanDelete: false` (物理削除誤防止) + `CanNavigateToDetail: true` (詳細で復活)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #41 を参照
47. **CLB システムフィールドは `SystemFieldNames` 予約名で定義する (絶対常識)** - `Id` / `LogicalDelete` / `OptimisticLocking` / `CreatedAt` / `UpdatedAt` / `Creator` / `Updater` は CLB の予約名。Field の `Name` をこれら**そのままの綴り**にすると、ランタイムが自動で振る舞う (主キー扱い / 削除フィルタ / 楽観ロック / 時刻自動セット / ユーザー自動セット)。`Version`/`IsDeleted` 等の任意名では自動動作が**一切効かない**。`DbColumn` は別途任意の DB 列名でよい。`Id`/`LogicalDelete`/`OptimisticLocking` は **UI 非表示** が原則。**`Creator`/`CreatedAt`/`Updater`/`UpdatedAt` は CLB の保存処理が自動でセットするので、スクリプトで `Creator.Value = CurrentUser.Id.Value` のように代入する必要はない** ([Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) 参照)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #42-A を参照 (`SystemFieldNames.cs` ベースの一覧表)
48. **検索ページのスクリプトは `SearchValue`/`SearchMin`/`SearchMax` を使う (絶対常識)** - `OnSearchInitialization` 等の検索コンテキストで `Status.Value = "進行中"` のように **`.Value` をセットしても無視される**。検索ページのフィールドは検索専用プロパティ系統で動く。単一値系 (Text/Id/Link/Boolean/Select/RadioGroup) は `SearchValue`、範囲系 (Number/Date/DateTime/Time = `RangeSearchField`) は `SearchMin`/`SearchMax`。Text/Id/Link は `SearchComparison` も併用。`OnSearchInitialization` スクリプトは URL に `?initialize_search=true` が付いている時だけ発火 (サイドバー Link 経由は自動付与、直接 URL は付かない)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #46 を参照
49. **DetailListField/TileListField で並べる Module の DetailLayout はカード化する (絶対常識)** - DetailListField や TileListField で参照される行/タイル単位の Module は、`DetailLayouts[""].Layout.IsBordered: true` でカード化必須。ListField (表形式) は表自体が罫線で区切るので不要だが、DetailListField/TileListField はレコードがフォーム/タイルとしてただ並ぶだけなのでカード化しないと境界が見えず、複数レコードが連続したフォームに見える。一覧ページの `ListPageDesign.ListFieldDesign` を `DetailListFieldDesign`/`TileListFieldDesign` に差し替えるパターンでも同じ。行 Module を他用途で使い回す場合は専用の DetailLayout 名 ("Card" 等) を追加して `LayoutName` で切り替える。詳細は [DetailListField](temporary/_field_catalog.md) のカード化セクションを参照
50. **`FieldBase.LayoutName` と `*FieldDesign.LayoutName` を混同しない** - `FieldBase.LayoutName` は `Parent?.LayoutName ?? ""` で親 Module の LayoutName を継承する値 (Detail Layout 名)。一方、`SearchFieldDesign.LayoutName` / `ListFieldDesign.LayoutName` 等は「Design 側で指定する参照先 Layout 名」(Search/List Layout のキー)。同じ「LayoutName」だが意味が違う。`field.LayoutName` (継承) でなく `field.Design.LayoutName` (Design 経由) を使うのがほぼ常に正解。製品コード側では `ListPageComponentVM.GetSearchLayout()` が前者を使っていて SearchLayoutName 指定が無視されるバグになっていた (修正済)。詳細は [Docs/ListPagePatterns.md](Docs/ListPagePatterns.md) の落とし穴セクションを参照
51. **一覧ページ・カスタム一覧の組み立ては パターン集を引く** - 「一覧ページ作って」「Excel 入出力」「Detail/Tile 形式で並べたい」「並び順固定」「カスタム一覧 (検索 + サマリ + List)」等の指示は [Docs/ListPagePatterns.md](Docs/ListPagePatterns.md) に再現可能なレシピがある。SearchLayouts 空辞書禁止、複数 Link を同一 Module に置くときの ModuleUrlSegment 分離、列幅は1列だけ可変にして末尾ボタン列を伸ばさない、SearchField を使うパターンと SearchLayout に Field 参照が必要なことなど、レイアウトずれ・無描画の落とし穴も同ドキュメントに集約。詳細は [Docs/ListPagePatterns.md](Docs/ListPagePatterns.md) を参照
52. **PageFrame の Link / Module / Field / Layout 等の複雑 JSON を「ゼロから書き起こさない」 (致命的)** - 特に PageFrame `Link` (ListPageDesign / DetailPageDesign / ListFieldDesign など深いネスト) を Python で組み立てて作ると、必須プロパティが 1 つでも欠けるだけで **PageFrame 全体の読み込みが失敗し、サイドバーが完全に消え画面真っ白** という致命的事態になる。スクリプトで Link を生成するときは **必ず既存 Link を `copy.deepcopy()` して、`Title` / `Module` / `ListFieldDesign.LayoutName` 等の必要箇所だけ差し替える** こと。Module / Field / Layout / SearchCondition も同様で、デフォルト値のうち網羅が困難なものがある (`IsStriped` / `IconType` / `HideTitle` 等の見落とし常連) ため、既存ファイルからの deepcopy が安全。新規モジュールを 1 から組むときは、Designer で 1 件雛形を作って commit → それを毎回 deepcopy する運用にする。スキーマを覚えで書くのは禁止。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #47 を参照
53. **FileField / ImageField はサーバ側設定 + 一時ファイルテーブルが必要 (重要)** - `FileField` を JSON に置くだけでは動かず、ランタイムで添付エラーになる。サーバ側 `appsettings.json` の `TemporaryFileTableInfo` (DataSource毎)・対応する `temporary_files` テーブル (DataSource毎)・`designer.settings.json` の `FileStorages` の 3点セットが揃って初めて動作する。サンプル/ショーケースに添付サンプルを入れるときは事前にこれらが用意されているか必ず確認すること (入れたが動かないと「ただのデモ崩れ」になる)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #48 と [FileField](temporary/_field_catalog.md) の「サーバ側設定が必須」セクション参照
54. **機能存在の根拠は CLB コアソース、サンプル側 Description は信用しない (致命的)** - サンプル Module の Description テキストや過去の README に「CSV ダウンロードできます」「PDF出力対応」等と書いてあっても、それは過去の AI 生成や見切り発車で書かれた**フィクションの可能性**がある。機能の実在は必ず `Source/Codeer.LowCode.Blazor/` 配下のコアソースを grep して裏取りすること。例: `CanBulkDataDownload` は `ListPageComponent.razor` で `DownloadExcel()` を呼ぶ Excel 専用機能で、CSV 機能は CLB に存在しない。ホーム説明・ドキュメント・新規サンプルを書くときに「サンプル側のテキストを根拠」にしてはいけない。**裏取りなしで機能名を書くのは禁止**。サイドバー Link 表記とも整合させる (「CSVダウンロード」「CSVエクスポート」のような同義語でも乖離扱い)。サンプル Link の追加/削除/リネーム時は `Home.mod.json` / `home.txt` / 関連ドキュメントも同タイミングで同期する
55. **`GridRow.KeepInFillAvailableGrid` は Claude が `true` にしない。ユーザ操作専用 (絶対)** - `IsFillAvailable=true` の Grid の最終行 (FillAvailable target) には2モードあり、デフォルト `false` = DirectList モード (`ListField` / `ProCodeField` の内部スクロールが画面下端まで広がる、これがほぼ全ケースの正解)、`true` = FitContent モード (`Button` / `Label` のような固定高さ最終行用、超絶レア)。**ユーザから「最終行フィットモード」「KeepInFillAvailableGrid を true に」と明示的な指示があった場合でも、Claude は `true` にしない**。立てる判断は Designer 上でユーザが手動でやる前提。ユーザの言葉に "true" や "FitContent" が出ても、Claude 側は **常に `false` のまま**。立てたい時はユーザが Designer GUI でチェックを入れる。`ListField` を最終行に置くシナリオで `true` を立てると **FillAvailable の効果が消えて画面下端まで広がらなくなる**致命的な見た目バグになる。プロパティ名に "FillAvailable" が入っているせいで「FillAvailable させる ON フラグ」と誤解しがちだが、意味は逆 (`false` が普通の FillAvailable 動作、`true` は特殊モード切替)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #49、`GridRow` プロパティ表は [Docs/Layouts.md](Docs/Layouts.md) 参照
56. **「ラベル列 + 入力列」 2カラムフォームでは、ラベル列に `VerticalAlignment: "Middle"` を必ず設定 (マスト)** - `<ラベル(80-120px)> | <入力欄(伸縮)>` の典型レイアウトでは、ラベル列の Column に **必ず `VerticalAlignment: "Middle"` を設定する**。これが無いと、入力列が縦に伸びるケース (Multiline TextField, FileField のプレビュー枠, 長文プレースホルダで折り返し, ListField/DetailListField を埋め込んだ複合フォーム等) で **ラベルだけがセル上端に張り付き、入力欄の中央とずれて見栄えが崩れる**。短文の単行入力時には差が出ないので見落としやすいが、後で必ず破綻するので**最初から全てのラベル列に Middle を入れておく**のが正解。地味だが業務UI品質には致命的に効く。既存サンプル (`FormLayoutSample` / `EditDialogTarget` / `ShowPanelTarget` 等) は全てこれが入っている — そのまま deepcopy すれば自然に守られる。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #50 を参照
57. **スクリプトに名前付き引数 (`name: value`) は書けない** - CLB のスクリプトインタプリタは C# の名前付き引数構文を非サポート。`new PrimaryButton("確定", isDefault: true)` のように書くとロード時に「`isDefault: 不正なシンタックスです`」エラー。必ず位置引数で `new PrimaryButton("確定", true)`。`MessageBox.ShowWithTitle(title: ...)` 等も同様。詳細は [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) 参照
58. **`Module.Submit()` を呼んだら必ず戻り値を確認する + ローディングは `LoadingService.StartLoading(delay)` で遅延表示** - `Submit()` の戻り値は `bool?` で `null`/`false`/`true` の3パターン (`null`=送信データなし、`false`=通信/検証失敗、`true`=成功)。失敗時は `Toaster.Error(...)` で通知すべきで、戻り値を捨てて `Items.Reload()` だけ走らせるのは NG。また `Module.Submit()` / `Module.Reload()` は HTTP 通信中に CLB 標準のローディングオーバーレイを瞬時に出してチラつくので、`using var scope = LoadingService.StartLoading(1000);` のスコープを張って「1秒以上かかる場合のみ表示」にすると UX が良い。詳細は [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) の「Submit() / Reload() を呼んだ後は結果を確認する」セクション参照
59. **`SearchTargetVariable` / `Variable` 系プロパティは Variable (`Id.Value`) であって Field (`Id`) ではない (致命的)** - SearchCondition / FieldValueMatchCondition / FieldVariableMatchCondition / LinkField.ValueVariable / SortCondition.Variable 等、プロパティ名に **Variable** が入っているところには `Field名 + "." + データメンバ名` (例: `Id.Value`, `Status.Value`, `Category.DisplayText`) を書く。Field 名だけ (`Id` / `Status`) を書くと `Id フィールドが存在しません` 等の例外で SQL 生成が失敗する。逆に `SearchCondition.SelectFields` のように プロパティ名に **Field** が入っているところは Field 名のみ。デザイナでもこの違いを混同しやすいので注意。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #51 を参照
60. **ChildModule の LinkField/SelectField は `.Value` がロード遅延で空のことがある (致命的)** - `DetailListField` 経由の孫 Module (例: `Order.Members[i]`) は親モジュールの `OnAfterInitialization` タイミングで LinkField/SelectField の `.Value` が `""` (空) のことがある。`if (m.Status.Value != "Waiting")` のような判定が常に true → continue で全件スキップされる罠。対処は `ModuleSearcher` で DB から直接検索 (`AddEquals(m => m.ApprovalFlowOrderId.Value, o.Id.Value)` のように Order.Id を使って絞り込む。`o.Id.Value` は ChildModule でも取れる)。詳細は [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) の「ChildModule の LinkField/SelectField」セクション参照
61. **動的型は `var` で受ける (致命的)** - `GetParentModule()` の戻り値や `Rows[i]` のメンバは動的型 (`object`)。`bool isNew = parent.IsNewData;` のような明示型代入は「**bool = var 不正な代入です**」ロード時エラーになる。`var isNew = parent.IsNewData;` で受ける
62. **式メソッド `=>` と複数引数ラムダ `(a,b)=>...` は使えない** - `string F() => "x";` (expression-bodied method) は不可、ブロック体 `string F() { return "x"; }` で書く。`list.Sort((a,b) => ...)` も不可、手動ソートする。単一引数ラムダ `x => x.Value` は `ModuleSearcher` の条件メソッド内のみ可。詳細は [Docs/Scripts.md](Docs/Scripts.md) のラムダ式セクション + [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) 参照
63. **レイアウトは「削れるだけ削る」(デザイン基本原則)** - 囲みラベル / 入力欄ラベル (placeholder で代用) / DetailListField の DisplayName / ページャー / ユーザーソート / 単独「状態」「担当者」表示 / 連番 Id 列 / 状態に応じないボタン などは削れる候補。複数フィールドを 1 行のテキストに統合する発想も有効 (例: `状態:進行中  ✓A→▶B→C` のように Status + フロー + 現在担当を 1 行のマーカー付き文字列で表現)。削減チェックリストと統合パターンは [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) の「レイアウト最小化の原則」セクション参照
64. **複数フィールド更新 + 通信は `SuspendNotifyStateChanged` + `LoadingService.StartLoading` でまとめる** - ボタンハンドラ (承認/却下/キャンセル等) で `Field.Value = ...` を連続させると毎回 StateHasChanged が走って画面チラつく。`using var s = GetParentModule().SuspendNotifyStateChanged();` で再描画を 1 回にまとめる。さらに `LoadingService.StartLoading(int? delay)` は (1) `delay=0` で**複数通信のローディングを 1 個にまとめる** (2) `delay=1000` で**短時間処理ならインジケータを出さない** の 2 通り。組合せ例: `using var suspend = GetParentModule().SuspendNotifyStateChanged(); using var loading = LoadingService.StartLoading(0);` 詳細は [Docs/ScriptGuidelines.md](Docs/ScriptGuidelines.md) と [Docs/Scripts.md](Docs/Scripts.md) 参照
65. **検索条件の Select→Select 連動は `OnSearchDataChanged` で `SearchValue` → `Value` をコピーする** - `SelectField.SearchCondition` + `FieldVariableMatchCondition` の宣言的連動 (CascadeInputBySearch パターン) は、検索レイアウトに置くと**そのままでは連動しない**。候補絞り込みの `Variable` が参照するのは `Value` だが、検索フォームの入力は `SearchValue` に入るため。親 Select の `OnSearchDataChanged` に `親.Value = 親.SearchValue;` の 1 行スクリプトを足すと、連動先の候補がリアルタイムに取り直され、候補から外れた子の選択値は自動クリアされる。実装サンプル: `Samples/PatternShowcase/App/Modules/CascadeSearch.mod.json` (サイドバー「検索/Select連動」)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #52 を参照
66. **明細の総入れ替え・一意キーの入れ替えは `ListField` の洗い替え (`ReplaceMode`) を使う** - `All` = Submit 時に `SearchCondition` 一致分を全件削除して現在行を全件新規追加 (完全洗い替え。条件が空だとサーバー側ガードで拒否されるため親子構成で使う)、`UpdateAsDeleteInsert` = 変更行だけ削除+新規追加に置き換え (座席番号・表示順など UNIQUE 制約列の値入れ替えが UPDATE の制約違反にならない)。どちらも作り直された行の Id は振り直される。実装サンプル: `ReplaceAllSample` / `SeatReplaceSample` (サイドバー「データ操作/洗い替え (完全)/(更新行)」)。詳細は [ListField](temporary/_field_catalog.md) の洗い替えセクションと [Docs/AppPatterns/replace_mode.md](Docs/AppPatterns/replace_mode.md) を参照
67. **候補が少ないマスタ参照は LinkField でなく SelectField (マスタ参照)** - 費目・区分・ステータスのように候補が数件〜十数件でドロップダウンから直接選べるマスタは、検索ピッカーの `LinkFieldDesign` ではなく `SelectFieldDesign`（マスタ参照: `Candidates: []` + `SearchCondition.ModuleName` + `ValueVariable`/`DisplayTextVariable` + `DbColumn`）を使う。「マスタで管理＝LinkField」ではない。LinkField は候補が多く自由検索で 1 件絞り込むとき。正典: `Samples/PatternShowcase/App/Modules/CascadeSearch.mod.json` の `SelectedProject`。詳細は [SelectField](temporary/_field_catalog.md) / [LinkField](temporary/_field_catalog.md) の「使い分け」参照
68. **一覧専用モジュールは `DetailLayouts: {}` にする** - 親の `ListField`（表形式）にインライン表示するだけで詳細ページへ遷移しない（`CanNavigateToDetail: false`）子明細モジュールは `DetailLayouts` を空 `{}` にして詳細デザインを作らない。`ListField` の行は `ListLayouts[""]` で描画され `DetailLayouts` は未使用なので、作り込むと使われないラベル・カードが増えて冗長。逆に詳細遷移する / `DetailListField`・`TileListField` でフォーム・タイル表示するモジュールは `DetailLayouts[""]` が必要（後者はカード化 `IsBordered: true` も必須）。詳細は [Docs/LayoutGuidelines.md](Docs/LayoutGuidelines.md) の「一覧専用モジュールは DetailLayouts を作らない」参照
69. **承認・ワークフローは求められない限り作らない／非認証アプリに承認を足さない** - 既定は CRUD（ヘッダ＋明細＋合計等）に留め、承認ボタン等を勝手に足さない・「推奨」もしない。承認は本来 承認者・段階・差し戻し・履歴・権限を伴う重い機能で、認証（ログイン）が前提。非認証アプリでは申請者と承認者を区別できず無意味。承認が必要なら認証前提で `Samples/PatternShowcaseAuth/` の承認フロー（`ApprovalFlow` 系）として正式に作る
70. **明細表 (ヘッダ＋明細) は `ListField`。`DetailListField` ではない (致命的・最頻出の誤り)** - 「注文＋明細」「請求書＋明細」「経費精算＋明細」のように日付・科目・金額… を**列の揃った表**で 1 行ずつ並べる明細は **`ListFieldDesign`** を使い、**列定義は子 (行) モジュール側の `ListLayouts[""].Elements`** に書く。子の親 FK は `IdFieldDesign` (`IsManualInput:false`)。`DetailListFieldDesign` は名前に「明細 (Detail)」が入っているので「明細表＝DetailListField」と短絡しがちだが**意味は逆** — "Detail" は「各行を DetailLayout (フォーム/カード) で描く」の意味で、業務の「明細行」ではない。`DetailListField`/`TileListField` を選ぶのは「1 レコード＝1 枚のフォーム/カード」にしたいと明確に判断したときだけ (その場合は子を `IsBordered:true` でカード化必須)。**迷ったら `ListField`。** 実装に迷ったら正典 `Samples/PatternShowcase/App/Modules/Order.mod.json` (`Details`＝`ListFieldDesign`) と `OrderDetail.mod.json` (`OrderId`＝`IdFieldDesign`、列は `ListLayouts`) を必ず開いて型を確認する。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #53 / [Docs/AppPatterns/header_detail.md](Docs/AppPatterns/header_detail.md) を参照
71. **PageFrame は最低 1 つ `IsApplicationRoot: true` (ルート URL の着地フレーム) を作る (絶対常識)** - `IsApplicationRoot` の C# 既定は `false`。新規 PageFrame を起こして着地フレーム (通常 `Main`) に `true` を立て忘れると、プロジェクトに application root が 0 件になり、ルート URL (`/`) を開いたとき CLB が**非 application-root のフレームにフォールバック**する。`UserReadCondition` で権限ゲートした管理フレーム (`AdminFrame` 等) が既定ランディングに選ばれる事故になる (admin だけ管理画面がホームになる等)。**着地フレームは `true`、補助フレームは `false`**。複数の `true` は PC/スマホ出し分け (`TargetDevice`/`WidthFrom`/`Priority`) のときだけ。単一フレームのプロジェクトでもその 1 枚を必ず `true` にする (空テンプレ複製時に踏みやすい)。正典: `Samples/PatternShowcaseAuth/App/PageFrames/Main.frm.json` (`true`) + `AdminFrame.frm.json` (`false`)。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #54 / [Docs/PageFrame.md](Docs/PageFrame.md) 冒頭ルールを参照
72. **スクリプトの数値書式・ゼロ埋めは `ToString("D3")` ではなく文字列補間 `$"{n:000}"` を使う (実行時エラー・デバッグ困難)** - CLB のスクリプトは数値を `decimal` に統一して計算するため、`(n).ToString("D3")` のような整数専用書式は**実行時に `Format specifier was invalid.` で失敗**する (designcheck は緑で素通り)。`$"{cnt + 1:000}"` / `$"{date:yyMMdd}"` なら `string.Format` 経由で動く。厄介なのは、例外でイベントハンドラが**途中停止**し後続 (合計再計算・ボタン出し分け等) が走らず、一見**別の不具合**に見える点。書式まわりで挙動が変なときはまず `ToString(書式)` を疑う。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #55 / [Docs/Scripts.md](Docs/Scripts.md) の文字列補間を参照
73. **DB の集計・件数を `ModuleSearcher` のループで回さない (N+1)** - 状態別件数などを状態の数だけ `ModuleSearcher.Execute()` でループすると、WASM のクライアント→サーバ→DB 往復が件数分発生する。**集計・件数は `QueryField`/`ExecuteSqlField` で `GROUP BY` の 1 クエリ**にまとめる。画面上の明細集計は `Rows`、複数の独立取得は `BatchSearcher` で 1 往復に束ねる。「この処理は問い合わせ何回か」を実装前に見積もり、ループ内で I/O を回したら赤信号。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #56 / [Docs/QueryAndSql.md](Docs/QueryAndSql.md) を参照
74. **「ログインする人」(担当者/作成者/承認者) に並行する別マスタを reflex で作らない** - 担当者＝ログインユーザー＝既存の `AppUser` なら、別途「担当者マスタ」を新設して二重管理しない。担当割当は `AppUser` への `LinkField` (または `CurrentUser`)、作成者/更新者は予約名 `Creator`/`Updater` の自動セットを使う ([#47](Docs/CommonMistakes.md))。新マスタを作る前に「この概念はモデルに既に存在しないか」を必ず問い、既存資産と重複を照合してからモデルを確定する。詳細は [Docs/CommonMistakes.md](Docs/CommonMistakes.md) の #57 を参照
