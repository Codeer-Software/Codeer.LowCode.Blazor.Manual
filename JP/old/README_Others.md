## 特徴
Codeer.LowCode.Blazorは以下の特徴を持っています:
- 強力なローコードエンジン
- ノーコード/ローコード/プロコードのシームレスな連携
- 一般的な.NET/Blazorコードとの高い親和性

## サンプル
https://lowcodedemo.azurewebsites.net/

## 利用方法
各セクションを参考にしてください:
- [Getting Started](#getting-started)
- [概略](overview/overview.md)
- [デザイナ](designer/designer.md)
- [Module](module/module.md)
- [Field](fields/field.md)
- [データモデルとModule](data_model/data-model.md)
- [PageFrame](designer/page_frame.md)
- [app.clprj](designer/app_clprj.md)
- [designer.settings](designer/designer_settings.md)
- [認証/認可](authorization/authorization.md)
- [スクリプト](overview/script.md)
- [プロコード](overview/procode.md)
- [ユーザーコード(プロコード)](user_code/user_code.md)
- [Excelから画面とDDLを作成する](designer/import_module_from_excel.md)
- [自動テストのサポート](designer/export_pageobject.md)
- [css](look_and_feel/css.md)
- [破壊的変更](breaking_changes/breaking_changes.md)



## こんなプロジェクトにおすすめ
- コストと時間を節約したい
- RDBを効果的に活用したい
- 既存のデータ、システムを活用したい
- **WinForms, WebFormsをモダンなフレームワークにリプレイスしたい**
- こだわりの機能がある
- リリース後にカスタマイズしたい

## ポトペタで画面作成
CanvasLayout、GridLayout、FlowLayoutの組み合わせで自由に画面を作成できます。通常の画面だけでなくダイアログも作成可能です。各UI部品の連動もノーコードや僅かなスクリプトで作成可能です。サイドバー、ヘッダ、フッターなど一般に必要なものは取り揃えております。こだわりのあるお客様はプロコードで各種カスタマイズしたものに交換できます。

## RDBと自在に連携
FormとDBのTableを関連付けてデータの入出力ができます。複数のFormを連携させることでJoinや1:Nの関係を表現できます。FormはTableだけでなくViewにも関連付けることができるのでBI機能も簡単に実現できます。論理削除/楽観ロック/作成更新情報など一般的にDBの操作で必要になるものは取り揃えています。変更履歴ももちろん残せます。

## スクリプトでより自由に
C#とほぼ同じ構文で記述できます。僅かな実装で機能を実現できるようにAPI設計をしています。もちろんコード補完も効くので簡単に実装できます。カスタマイズができてプロコードで実装した機能を呼び出すこともできます。基本的にはクライアントサイドで実行されますが、サーバーサイドでの実行もサポートしています。
- 一般的な演算処理
- 画面制御
- WebAPIの実行
- Excel編集/PDF作成

## Excelとの連携のサポート
一般的なデータ入出力はもちろん、テンプレートをExcelで作成してそれを書き換えることで帳票にも対応できます。pdfへの変換も可能です。

## 認証・認可
認証は一般的なCookie認証やAzureActiveDirectoryを使った認証をテンプレートコードで提供します。その他の認証もカスタマイズで対応可能です。認可に関してはアプリ、画面、データそれぞれでアクセス制御が可能です。

## こだわりの機能はプロコードで実装
場合によっては特殊な画面/機能が必要になることもあります。Codeer.LowCodeはBlazorのライブラリなので、そのような場合は.NETのコードを追加して作りこみが可能です。さらにField単位で作っておけばそれを様々な箇所で利用することができます。

