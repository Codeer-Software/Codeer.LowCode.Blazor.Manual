# トラブルシューティング

デザイナ／実行時／配布時によくある症状と対処をまとめています。
症状から逆引きできるように並べてあります。該当するものが見つからない場合は[お問い合わせ](https://www.codeer.co.jp/LowCode)までご連絡ください。

---

## デザイナ・配布アプリの起動

### デザイナを起動するとプレビューが真っ白／コンポーネントが描画されない

**原因**: Microsoft Edge WebView2 ランタイムが PC にインストールされていない。

デザイナや WPF / WinForms 版アプリは内部で WebView2 を使って Blazor コンポーネントを描画します。WebView2 ランタイムが無いと画面が真っ白なまま何も表示されません。

**対処**: [Microsoft Edge WebView2 ダウンロードページ](https://developer.microsoft.com/microsoft-edge/webview2/) から「Evergreen Standalone Installer」をインストールします。

- Windows 11 は標準搭載なので通常は問題ありません。
- Windows 10 でも Microsoft Edge と一緒にインストールされていることが多いです。
- **Windows Server は標準では未搭載**なので、Server にデザイナや WPF / WinForms 版を配置する場合は特に注意してください。

詳細: [デプロイ先 PC の前提](../overview/vs_projects.md#デプロイ先-pc-の前提)

---

### WPF / WinForms 版を配布した PC で画面が真っ白になる

**原因**: 配布先 PC に WebView2 ランタイムが入っていない。

**対処**: 上記と同じ。配布先 PC で WebView2 ランタイムをインストールしてください。Windows Server や、キッティング前のクリーンな PC でよく起こります。

---

### デザイナの動作がおかしい／Razor がコンパイルされない

**原因**: Debug 構成で起動している。

**対処**: デザイナは **必ず Release 構成で発行（Publish）**して、Windows Explorer から起動してください。Visual Studio から F5（Debug）で起動すると正常に動作しません。

詳細: [Visual Studio ソリューションおよびデプロイ](../overview/vs_projects.md#デプロイ方法)

---

## ホットリロード

### ホットリロードが効かない

**原因**: 設定や接続経路の問題。

**確認するポイント**:

1. サーバー側の `appsettings.json` で `"UseHotReload": true` になっているか
2. ブラウザの開発者ツールのコンソールで SignalR 接続エラーが出ていないか
3. ファイアウォール／プロキシで `/hot_reload_hub` がブロックされていないか

ホットリロードはサーバー側からブラウザへ SignalR で通知し、ブラウザを完全リロード（`Refresh(true)`）させる仕組みです。

---

## フィールド配置

### Id フィールドが見つからない

**原因**: Id（主キー）はツールボックスの **「システムフィールド」** カテゴリにあります。「一般フィールド」の中を探しても見つかりません。

CreatedAt / UpdatedAt / Creator / Updater / LogicalDelete / OptimisticLocking なども同じく「システムフィールド」カテゴリです。これらは DB 連動のお決まり項目として分けられています。

**対処**: ツールボックスの「システムフィールド」を開いて Id をドラッグしてください。

---

### 別テーブルを参照する列に NumberField を使ったがうまく動かない

**原因**: 外部キー（他モジュールへの参照）には専用のフィールドを使う必要があります。

**対処**:
- 検索ダイアログで他モジュールを選択する場合 → **LinkField**
- 単純に親モジュールの Id を保持する場合 → **IdField**

`NumberField` は通常の数値入力なので、フレームワーク内部の親子関係解決に乗らず、新規作成時の親 Id の引き渡しなどがうまく動作しません。DB の列の型自体は `INTEGER` でも `VARCHAR` でも構いません。

詳細: [LinkField](../fields/Link.md) / [IdField](../fields/Id.md)

---

### LinkField の参照先のフィールドを画面に出したい

**原因**: 参照先モジュールのフィールドはデフォルトでは画面に出せません。

**対処**: 参照元モジュールの **`LinkFieldNames`** に参照したいパス（例: `Category.Name`）を追加すると、レイアウトでそのフィールドを参照できるようになります。

---

### リストにチェックボックスを置きたい

**原因**: ListField の `CanSelect` は行選択（ハイライト）のためのプロパティで、**チェックボックスではありません**。

**対処**: チェックボックスは **BooleanField**（UIType: CheckBox）を行モジュールに追加し、レイアウトで `IsViewOnly: false` で配置してください。`CanSelect` は `false` のままにします。

---

## モジュール全体の設定

### 表示専用モジュールで入力したのに値が保持されない

**原因**: DB と結びつかない表示専用モジュール（`DbTable` が空）でも、リスト内に入力可能なフィールド（チェックボックス、数値入力など）がある場合、`CanUpdate: false` だと画面全体が ViewOnly になります。

**対処**: `CanUpdate: true` に設定してください。

---

### 子リスト（DetailListField/ListField）で「追加」しても保存されない

**原因**: 親レコードがまだ保存されていない状態で子レコードを追加した場合、親の Id が確定していないため正しく保存できないことがあります。

**対処**:
- まず親レコードに必要項目を入力して保存してから子を追加してください。
- もしくは、親と子を一括で保存できるように親モジュール側で **SubmitButtonField** を配置し、`Submit` で親と子を同一トランザクションで書き込むようにしてください。

---

## モジュール（デザインファイル）

### モジュール一覧から特定のモジュールが消える（エラーも出ない）

**原因**: JSON ファイルの型ミスマッチ。エラーが出ずに静かに読み込み失敗します。

**よくあるパターン**:
- `int?` や `double?` 型のプロパティに `"200"` のように **文字列** で値を書いている（正: `200`）
- 数値型に `null` を書きたいときに `"null"` と文字列で書いている

**対処**: JSON 構文自体は正しくても、各プロパティの C# 型に合致した値の書き方をしているか確認してください。デザイナで編集してから保存すれば正しい型で書き戻されます。

---

### ViewEditToggleButton を置いたらサブミットボタンが消えた

**原因**: 仕様です（バグではありません）。

ViewEditToggleButton（閲覧編集切り替えボタン）をモジュールに置くと、初期化時に同一モジュール内の全 SubmitButton が非表示になります。**編集モードに切り替えたときだけ表示**される動作を意図しています。

**対処**: 編集ボタンを押して編集モードに切り替えれば SubmitButton が表示されます。

詳細: [ViewEditToggleButtonField](../fields/ViewEditToggleButton.md)

---

### 一覧ページのソート初期値が反映されない

**原因**: 設定場所が違う。

**対処**: モジュールの `ListPageFieldDesign` ではなく、**PageFrame の `ListPageDesign.ListFieldDesign.SearchCondition.SortConditions`** で設定してください。一覧ページのソート設定は PageFrame 側が優先されます。

詳細: [PageFrame](../designer/page_frame.md)

---

## データベース

### PostgreSQL で楽観ロックがうまく動かない

**原因**: PostgreSQL では行のバージョン管理に内部列 `xmin` を使いますが、これを通常の数値列としてマップすると正しく動作しません。

**対処**: 楽観ロック用の `xmin` 列は **OptimisticLockingField** にマップしてください。デザイナでデータベースからモジュールを一括作成すると自動でこの形にマップされます。

詳細: [OptimisticLockingField](../fields/OptimisticLocking.md)

---

### ExecuteSqlField を Create/Update タイミングで置いたら標準の登録／更新処理が走らなくなった

**原因**: `ExecuteSqlField` の `WithStandardIO` プロパティの設定です。

| 値 | 動作 |
|---|---|
| `None` | 標準処理は実行されず、カスタム SQL のみ |
| `Before` | 標準処理 → カスタム SQL |
| `After` | カスタム SQL → 標準処理 |

**対処**: 標準の Insert/Update/Delete も実行したいなら `Before` か `After` を指定してください。標準処理を完全に置き換えるなら `None` のままで OK。

詳細: [ExecuteSqlField](../db/execute_sql_field.md)

---

## 画像・リソース

### 画像が表示されない

**確認するポイント**:

1. 画像ファイルがデザイナの **「Resources」フォルダ**（プロジェクトルートの `Resources/`）に配置されているか
2. `ImageViewerField` や `ButtonField.ImageResourcePath` などで指定するパスは Resources フォルダからの相対パス（例: `logo.png` や `sub/icon.png`）になっているか
3. ファイル名に全角文字やスペースが混ざっていないか

**対処**: Resources フォルダにファイルを置き、デザイナで該当フィールドを選択してリソース選択ダイアログから指定するのが確実です。

---

## レイアウト

### 検索画面が表示されない／表示はされるが空っぽ

**原因**: 検索レイアウト（`SearchLayouts`）にデフォルトレイアウト（キー名 `""`）が定義されていない、もしくは中身が空。

**対処**: モジュールの「検索」タブを開いて、デフォルトレイアウト上に検索したいフィールドを配置してください。
個別のフィールドを単純な検索条件としてだけ使うなら、フィールド側の `IsSimpleSearchParameter: true` を設定すれば、検索レイアウトに自動で並びます。

---

## スクリプト

### `await this.Submit()` でエラーになる

**原因**: スクリプトエンジンが非同期メソッドを自動で同期処理するため、`await` を書く必要はありません。

**対処**: `await` を外して `this.Submit()` と書いてください。
同様に `await MessageBox.Show()` ではなく `MessageBox.Show()` です。

詳細: [スクリプト構文リファレンス](../script/script_syntax.md)

---

### スクリプトでフィールドの値が変わったときに処理を実行したい

**対処**: 値変更時のイベントハンドラを `{フィールド名}_OnDataChanged` という名前で書きます。

```csharp
void Status_OnDataChanged()
{
    LabelResult.Text = "ステータスが変更されました";
}
```

詳細: [スクリプト概要](../script/script.md)

---

## ライセンス

### ライセンス登録ができない／登録したのに「未登録」と表示される

**確認するポイント**:
- マシン固有のキーが取れない PaaS 環境（Azure App Service など）の場合、**ドメインライセンス**を使う必要があります。
- 別 PC で登録したライセンスはその PC でしか有効になりません。

詳細:
- [ライセンスについて](../overview/about_license.md)
- [ドメインライセンスの登録](../overview/domain_license_registration.md)
- [オンライン登録](../overview/license_online_registration.md) / [オフライン登録](../overview/license_web_registration.md)

---

## それでも解決しない場合

- ハマったケース・症状を [Codeer](https://www.codeer.co.jp/LowCode) までご連絡ください。
- 解決が判明したものはこのページに追記していきます。
