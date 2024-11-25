# Visual Studio ソリューションおよびデプロイ
## 概要
[Codeer.LowCode.Blazor.Templates](https://marketplace.visualstudio.com/items?itemName=Codeer.LowCodeBlazor)を使ってVisual Studioソリューションを作成すれば、複数のプロジェクトに含まれる「**ユーザーコード**」が出力されます。
テンプレートのタイプはBlazor/WPF/WinFormsになります。
<img width=800 src="../../Image/Project_Templetes.png">

## プロジェクトの種類と役割
### Blazorタイプのソリューション

<img width=800 src="../../Image/step2.png">

| Project | 説明 |
| --------------- | --------------- | 
|[ProjectName].Server  | Blazorアプリのサーバー部分、 WebApi等でカスタマイズ可能 | 
|[ProjectName].Server.Shared  | DesignerとServerが共有する部分| 
|[ProjectName].Client  |BlazorアプリのClient(WebAssembly)部分、HTML/JS/CSS等を含むことが可能です| 
|[ProjectName].Client.Shared  | DesignerとClientが共有する部分| 
|[ProjectName].Designer  | Designer(WPF)アプリの部分、プロコードによるメニュー追加カスタマイズ可能 | 

これらのプロジェクトに[プロコード](overview/procode.md)を格納することが可能です。

### WPF/WinFormsソリューション

WPF/WinFormsソリューションでは、ServerおよびClientプロジェクトはWPFあるいはWinFormsプロジェクトに集約されています。

<img width=800 src="../../Image/Wpf_WinForms_Solutions.png">

## デプロイ方法
### Blazor/WPF/WinFormsアプリの部分
成果物としてアプリをデプロイするには、Server/WPF/WinFormsプロジェクトを選択したうえで、Visual Studioの「**ビルド**」メニューから「**発行**」あるいは「**公開**」してください。
### Designerの部分
同上。

```注意：Debug構成でデザイナをビルドしますと、正常に動作しない場合がありますので、必ずRelease設定でビルドしてください。```
### Designerプロジェクトの部分
デザイナメニュー「ファイル」→「デプロイ」を選んでください。デザイナプロジェクトの関連ファイルは[デプロイフォルダ](deploy_folder.md)に出力されます。

## 関連ページ
- [プロコード](overview/procode.md)
- [デプロイフォルダ](deploy_folder.md)
