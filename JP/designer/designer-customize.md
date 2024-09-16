# デザイナ本体のカスタマイズ

デザイナは `OnStartup` メソッドに対していくつかの処理を追加することで動作をカスタマイズすることができます。
ここでは、デザイナの動作をカスタマイズするためのいくつかの例を紹介します。

## デザイナのスタートアップファイル

デザイナのスタートアップメソッドは `App.xaml` に、`DesignerApp` を継承したクラスとして実装されています。

このドキュメントで特に記載がない場合、`App`クラスの`OnStartup`メソッドに対して処理を追加してください。

## スクリプトAPIの追加

スクリプトに公開するAPIを追加できます。Designerではスクリプトは実行されませんがスクリプトエディタのインテリセンスに追加されます。スクリプトの追加方法の詳細は[こちら](../overview/script.md)を参照してください。

```cs
ScriptRuntimeTypeManager.AddType(typeof(ExcelCellIndex));
ScriptRuntimeTypeManager.AddService(new WebApiService(null!, null!));
```
## メニューの追加
Designerにメニューとその処理を追加できます。<br/>
DesignerEnvironment.AAddMainMenu(Action handler, params string[] menuLocation);<br/>
handlerはメニューをクリックしたときの処理で、menuLocationは階層型でメニューを指定できます。

 [Codeer.LowCode.Blazor.Templates](https://marketplace.visualstudio.com/items? から作成したコードには以下のメニュー機能がコードで追加されています。実装時のサンプルとして参照してください。
- [mport Module from Excel](import_module_from_excel.md)
- [Export PageObject](export_pageobject.md)

```cs
DesignerEnvironment.AddMainMenu(ImportExcel, "Tools", "Import Module from Excel");
DesignerEnvironment.AddMainMenu(ExportPageObject, "Tools", "Export PageObject");
```
DesignerEnvironmentクラスからDesignerのデータにアクセスできます。

### DesignerEnvironment
| プロパティ名          | 型            | 説明             |
|-----------------|--------------|----------------|
| CurrentFileDirectory       | string         | 現在開いている app.clprj のフォルダ       |

| メソッド定義                                   | 説明                |
|---------------------------------------------|---------------------|
| DesignData GetDesignData()                                     | Designerの現在編集中のデザインデータの取得  |
| void AddMainMenu(Action handler, params string[] menuLocation) | Designerにメニューを追加<br/>hander - 実行する処理<br/>menuLocation - メニュー表示文字、階層型で指定できる
| void ShowToast(string message, bool isSuccess)                 | Designerでトーストを表示する<br/>message - メッセージ<br/>isSuccess - 成功メッセージ/エラーメッセージ 成功メッセージの場合は緑で表示 |





## テンプレートの追加

新規作成時のテンプレートを追加することができます。初期状態では2種類のテンプレートが設定されていますが、これらを削除したり、新しいテンプレートを追加することができます。


```cs
DesignerTemplateCandidate.Templates.Add(new DesignerTemplate
{
    Create = OnCreate,
    Name = "Your Template Name",
    Description = "Your Template Description",
});
```

ここで、`OnCreate` は新規作成時に呼び出されるメソッドです。このメソッドは、受け取ったパスに対してテンプレートを展開する処理を実装する必要があります。
次の例は、事前にZIPファイルとして用意されたテンプレートを指定されたパスに展開します。

```cs
private void OnCreate(string path)
{
    var templatePath = "path/to/your/template.zip";
    ZipFile.ExtractToDirectory(templatePath, path);
}
```

テンプレートとして必要なものは、デザイナで使用している `app.clprj` を含んだファイル以下の構造を持つ一連のファイルです。
必要に応じてSqliteなどのファイルを配置することも可能です。

## 外部アセンブリに存在するスタイル/スクリプトの読み込み

### 外部アセンブリがScoped CSSを含んでいる場合

Scoped CSSを含んだアセンブリを読み込む場合は、必ず `InstallBundleCss` を呼び出してください。
このメソッドを呼び出さない場合、Scoped CSSが正しく読み込まれません。

```cs
// 外部アセンブリの名前が Your.External.Assembly.Name の場合
BlazorRuntime.InstallBundleCss("Your.External.Assembly.Name");
```

### 外部アセンブリにコンテンツとして含まれているCSSを読み込む場合

外部アセンブリの `wwwroot` ディレクトリに含まれているコンテンツを読み込む場合は、次のように `InstallContentCss` を呼び出してください。

```cs
// Your.Exteranl.Assembly.Name に含まれている wwwroot/path/to/your/css/file.css を読み込む場合
InstallContentCss("Your.External.Assembly.Name", "path/to/your/css/file.css");
```

### 外部アセンブリにコンテンツとして含まれているJavaScriptを読み込む場合

外部アセンブリの `wwwroot` ディレクトリに含まれているコンテンツを読み込む場合は、次のように `InstallContentScript` を呼び出してください。

```cs
// Your.Exteranl.Assembly.Name に含まれている wwwroot/path/to/your/js/file.js を読み込む場合
InstallContentScript("Your.External.Assembly.Name", "path/to/your/js/file.js");
```

## ボタンなどに使用できるアイコンの追加

### 一覧への追加

デザイナで選択できるアイコンは標準でBootstrap Iconsが設定されていますが、これに加えてFont Awesomeなど追加のアイコンを設定することができます。
`IconCandidate.Icons` に対して、追加したいアイコンのクラス名を追加してください。

```cs
IconCandidate.Icons.Add("fa-solid fa-house");
IconCandidate.Icons.Add("fa-regular fa-comment");
```

### スタイルの追加

追加したクラス名に対して正しくアイコンが表示されるようにするためには、CSSを追加する必要があります。
ここではアイコンを `WebApp.Client.Shared` プロジェクトの `wwwroot/css` へ `additional-icon.css` という名前で追加した例を示します。

```cs
BlazorRuntime.InstallContentCss("WebApp.Client.Shared", "css/additional-icon.css");
```

## ウィンドウタイトルの変更

ウィンドウタイトルは `base.OnStartup(e);` より後ろであればいつでも変更することができます。

```cs
MainWindow.Title = "Your Application Name";
```

