# Codeer.LowCode.Blazor

Codeer.LowCode.Blazorは、Blazorアプリにローコード機能を追加するためのライブラリです。素早く簡単にアプリケーションを構築できるよう設計されています。

<img src="../Image/lc_logo.png">

## 目次

- [Codeer.LowCode.Blazorへようこそ](#codeerlowcodeblazorへようこそ)
- [入手する](#codeerlowcodeblazorを入手する)
- [Getting Started](#getting-started)
- [使いこなす](#codeerlowcodeblazorを使いこなす)
- [ライセンス情報](#ライセンス情報)
    - [ライセンスについて](overview/about_license.md)

## Codeer.LowCode.Blazorへようこそ
<img width=800 src="../Image/BlazorDesignerVs.png">

### What:　Codeer.LowCode.Blazorとは
Codeer.LowCode.Blazorは、Blazorアプリにローコード機能を追加するためのライブラリです。素早く簡単にアプリケーションを構築できるよう設計されています。

ローコードアプリを使ってビジネスアプリを作ることにとどまらず、ローコードアプリ自体を開発する強力なエンジンを提供します。


### Why：　使う理由
- 強力なローコードエンジン
- ノーコード/ローコード/プロコードのシームレスな連携
- 一般的な.NET/Blazorコードとの高い親和性
- 素早く簡単に多彩なWeb(Blazor)/WinForms/WPFアプリを作成・変更
- RDBと自在に連携
- ローコード(Script)およびプロコードによる高いカスタマイズ能力
- 各種CSSライブラリ、MudBlazor、Radzen.Blazor、IgniteUIと高い互換性
- Excelとの連携でデータ入力からPDF変換まで対応
- Cookie認証やAzureActiveDirectory認証をテンプレートコードで提供、他の認証もカスタマイズで対応可能
- OpenAI APIによるテキスト解析で拡張可能性は無限大
### How:　使い方
主に３つの開発スタイルがあります：

- ノーコード：　パーツポトペタ＋プロパティ設定
- ローコード：　C#ライクなスクリプトで演算処理、画面制御、WebAPIの実行、Excel編集/PDF作成等を実装
- プロコード：　C#によるコードビハインドやRazorページの追加等が可能

### Who:　こんなプロジェクトにおすすめ
- コストと時間を節約したい
- RDBを効果的に活用したい
- 既存のデータ、システムを活用したい
- **WinForms, WebFormsをモダンなフレームワークにリプレイスしたい**
- こだわりの機能がある
- リリース後にカスタマイズしたい

### Highlights
#### ポトペタで画面作成
CanvasLayout、GridLayout、FlowLayoutの組み合わせで自由に画面を作成できます。通常の画面だけでなくダイアログも作成可能です。各UI部品の連動もノーコードや僅かなスクリプトで作成可能です。サイドバー、ヘッダ、フッターなど一般に必要なものは取り揃えております。こだわりのあるお客様はプロコードで各種カスタマイズしたものに交換できます。

#### RDBと自在に連携
FormとDBのTableを関連付けてデータの入出力ができます。複数のFormを連携させることでJoinや1:Nの関係を表現できます。FormはTableだけでなくViewにも関連付けることができるのでBI機能も簡単に実現できます。論理削除/楽観ロック/作成更新情報など一般的にDBの操作で必要になるものは取り揃えています。変更履歴ももちろん残せます。

#### スクリプトでより自由に
C#とほぼ同じ構文で記述できます。僅かな実装で機能を実現できるようにAPI設計をしています。もちろんコード補完も効くので簡単に実装できます。カスタマイズができてプロコードで実装した機能を呼び出すこともできます。基本的にはクライアントサイドで実行されますが、サーバーサイドでの実行もサポートしています。
- 一般的な演算処理
- 画面制御
- WebAPIの実行
- Excel編集/PDF作成

#### Excelとの連携のサポート
一般的なデータ入出力はもちろん、テンプレートをExcelで作成してそれを書き換えることで帳票にも対応できます。pdfへの変換も可能です。

#### 認証・認可
認証は一般的なCookie認証やAzureActiveDirectoryを使った認証をテンプレートコードで提供します。その他の認証もカスタマイズで対応可能です。認可に関してはアプリ、画面、データそれぞれでアクセス制御が可能です。

#### こだわりの機能はプロコードで実装
場合によっては特殊な画面/機能が必要になることもあります。Codeer.LowCodeはBlazorのライブラリなので、そのような場合は.NETのコードを追加して作りこみが可能です。さらにField単位で作っておけばそれを様々な箇所で利用することができます。

## Codeer.LowCode.Blazorを入手する
### トライアルライセンスで30日間無料でご利用いただけます
Visual Studio拡張機能を使用してプロジェクトを作成できます。 

[Codeer.LowCode.Blazor.Templates](https://marketplace.visualstudio.com/items?itemName=Codeer.LowCodeBlazor)

### ご購入、その他お問合せは[こちら](https://www.codeer.co.jp/LowCode)からお願いします

## Getting Started
[![GettingStarted](../Image/GettingStarted_YtCover.png)](https://youtu.be/4Neyew4_tp0)
### Step1
プロジェクト新規作成

<img width=800 src="../Image/step1.png">

### Step2
BlazorアプリとWPFのデザイナアプリをビルドして起動

```Designerアプリは必ず"発行"でビルドし、Windows Explorerから起動してください。通常ビルドの場合は正常に動作しない場合があります```

<img width=800 src="../Image/step2.png">

### Step3
デザイナで新しいプロジェクトを作成します。サンプルを含むプロジェクトが作成されます。

<img width=800 src="../Image/step3.png">

### Step4
それをWebアプリにデプロイします。画面がホットリロードされ、デザイナの設定に従って画面が表示されます。

<img width=800 src="../Image/step4.png">

### Step5
デザイナの設定を確認し、小さな変更を加えてWebアプリに送信して、感覚をつかんでください。

<img width=1200 src="../Image/PartsOverView.png">


## Codeer.LowCode.Blazorを使いこなす
- [各部分紹介]()
    - [概略](overview/overview.md)
    - [Visual Studioソリューションおよびデプロイ](overview/vs_projects.md)
    - [デザイナ](designer/designer.md)
        - [app.clprj](designer/app_clprj.md)
        - [designer.settings](designer/designer_settings.md)
        - [PageFrame](designer/page_frame.md)
        - [Module](module/module.md)
            - [Field](fields/field.md)
            - [Document OutlineとPropertyパネル](module/DocumentOutline.md)
        - [データモデルとModule](data_model/data-model.md)
    - [デプロイフォルダ](overview/deploy_folder.md)
- [機能]()
    - [認証/認可](authorization/authorization.md)
    - [スクリプト](overview/script.md)
    - [プロコード](overview/procode.md)
    - [ユーザーコード(プロコード)](user_code/user_code.md)
    - [AI](ai/ai_overview.md)
        - [AIを使うための設定](ai/ai_setup.md)
        - [AITextAnalyzerField](ai/AITextAnalyzerField.md)
        - [モジュールをAIで作成](ai/ai_modules.md)
        - [クエリをAIで作成](ai/ai_query.md)
    - [Excelから画面とDDLを作成する](designer/import_module_from_excel.md)
    - [Databaseからモジュール作成する](designer/import_modules_from_db.md)
    - [自動テストのサポート](designer/export_pageobject.md)
    - [css](look_and_feel/css.md)
    - 第三者ライブラリとの連携
        - MudBlazor
        - Radzen.Blazor
        - IgniteUI
    - [Webサーバーへのデプロイ](overview/server_deploy.md)
- [サンプル・Tips]()
    - [サンプルギャラリー](https://lowcodedemo.azurewebsites.net/)
    - [TextFieldを読み取り専用にする](Examples/Tips_IsViewOnly.md)
    - [AnchorTagのサイズを調整する](Examples/Tips_AnchorTagSizeSetting.md)
    - [Labelの位置を調整する](Examples/Tips_LabelPositionSetting.md)
    - [検索条件に初期値を設定する](Examples/Tips_SearchCriteriaInitialValueSetting.md)
    - [Submit時に処理を追加する](Examples/Tips_AddProcessingSubmit.md)
    - [リストの連携](Examples/DoubleList.md)
- [ヘルプ]()
    - [モジュール作成時の注意点](Help/PointToNote_CreateModule.md)
    - [ListのConditionsを設定する場合の注意点](Help/PointToNote_ListCondition.md)
- [リリースノート]()
    - [破壊的変更](breaking_changes/breaking_changes.md)
- [Q&A]()


## ライセンス情報
Codeer.LowCode.Blazorの使用ライセンスの詳細については、[こちら](https://www.nuget.org/packages/Codeer.LowCode.Blazor/1.0.9/License)をご覧ください。
日本語版は[こちら](LicenseJP.md)です。本契約の英語版とその翻訳版の間に何らかの齟齬または相違が生じた場合、英語版が優先され本契約の意味を規律するものとします。

- このソフトウェアの試用版は本製品の評価目的でのみ無料で30日間使用できます。
- ライセンスの購入は、商用目的でのソフトウェアの使用、または商用配布を目的とした製品の開発プロセスの一部としてのソフトウェアの使用を含むがこれに限定されない、あらゆる形式の商用使用に必要です。
- コミュニティ利用を希望される方には、申請により無償利用ライセンスを発行いたしますのでCodeerまでお問合せ下さい。

開発、運用時に必要なライセンスについては[こちら](overview/about_license.md)をご覧ください。



