# デザイナ本体のカスタマイズ

デザイナは `OnStartup` メソッドに対していくつかの処理を追加することで動作をカスタマイズすることができます。
ここでは、デザイナの動作をカスタマイズするためのいくつかの例を紹介します。

## デザイナのスタートアップファイル

デザイナのスタートアップメソッドは `App.xaml` に、`DesignerApp` を継承したクラスとして実装されています。

このドキュメントで特に記載がない場合、`App`クラスの`OnStartup`メソッドに対して処理を追加してください。


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
InstallBundleCss("Your.External.Assembly.Name");
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
IconCandidate.Add("fa-solid fa-house");
IconCandidate.Add("fa-regular fa-comment");
```

### スタイルの追加

追加したクラス名に対して正しくアイコンが表示されるようにするためには、CSSを追加する必要があります。
ここではアイコンを `WebApp.Client.Shared` プロジェクトの `wwwroot/css` へ `additional-icon.css` という名前で追加した例を示します。

```cs
InstallContentCss("WebApp.Client.Shared", "css/additional-icon.css");
```

## ウィンドウタイトルの変更

ウィンドウタイトルは `base.OnStartup(e);` より後ろであればいつでも変更することができます。

```cs
MainWindow.Title = "Your Application Name";
```

