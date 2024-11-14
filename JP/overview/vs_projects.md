# Visual Studio プロジェクト
## 概要
[Codeer.LowCode.Blazor.Templates](https://marketplace.visualstudio.com/items?itemName=Codeer.LowCodeBlazor)を使ってVisual Studioソリューションを作成すれば、複数のプロジェクトに含まれる「**ユーザーコード**」が出力されます。

これらのプロジェクトでBlazorアプリが成り立ち、ユーザーによるカスタマイズや機能拡張も可能です。

## プロジェクトの種類と役割
<img width=800 src="../../Image/step2.png">

| Project | 説明 |
| --------------- | --------------- | 
|[ProjectName].Server  | Blazorアプリのサーバー部分、 WebApi等でカスタマイズ可能 | 
|[ProjectName].Server.Shared  | DesignerとServerが共有する部分| 
|[ProjectName].Client  |BlazorアプリのClient(WebAssembly)部分、HTML/JS/CSS等を含むことが可能です| 
|[ProjectName].Client.Shared  | DesignerとClientが共有する部分| 
|[ProjectName].Designer  | Designer(WPF)アプリの部分、プロコードによるメニュー追加カスタマイズ可能 | 

これらのプロジェクトに用途に応じて[プロコード](overview/procode.md)を格納することがかのうです。

