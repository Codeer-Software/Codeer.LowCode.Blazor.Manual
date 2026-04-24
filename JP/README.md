# Codeer.LowCode.Blazor

<img src="../Image/lc_logo.png">

Codeer.LowCode.Blazor は、**Blazor アプリにローコード機能を組み込むためのライブラリ**です。
デザイナで画面やデータモデルを設定し、ホットリロードで Web アプリに即反映できます。

[サンプルギャラリー](https://lowcodedemo.azurewebsites.net/) ・ [YouTube チュートリアル](https://youtu.be/MchuOxWYR1o?si=7I9FfQB55dP9ctY-) ・ [製品ページ](https://www.codeer.co.jp/LowCode)

---

## 目的別の入り口

| あなたの状況 | 最短ルート |
|---|---|
| **まず何ができるか知りたい** | → [Codeer.LowCode.Blazor とは](introduction/what_is_lowcode.md) |
| **とりあえず動かしてみたい** | → [クイックスタート（10 分）](quickstart/quickstart.md) |
| **自分で画面を作ってみたい** | → [はじめてのモジュール作成（30 分）](quickstart/first_module.md) |
| **段階的に力をつけたい** | → [チュートリアル](#3-チュートリアル段階的に学ぶ) |
| **特定の機能の作り方を知りたい** | → [ガイド](#4-ガイド目的別の作り方) |
| **全仕様を引きたい** | → [リファレンス](#5-リファレンス全仕様) |

---

## 1. はじめに

Codeer.LowCode.Blazor を触り始める前に、全体像を掴むためのセクションです。

- [Codeer.LowCode.Blazor とは](introduction/what_is_lowcode.md) — できること・3 つの開発スタイル・対応 DB
- [コア概念](introduction/concepts.md) — PageFrame / Module / Field / Layout / Script の関係
- [入手とライセンス](introduction/installation.md) — Visual Studio 拡張のインストール・ライセンス種別

---

## 2. クイックスタート

手を動かして最短で感触をつかむセクションです。

- [クイックスタート（10 分）](quickstart/quickstart.md) — サンプル入りプロジェクトを作成して Web で動かす
- [はじめてのモジュール作成（30 分）](quickstart/first_module.md) — DB と連動した CRUD 画面を作る

---

## 3. チュートリアル（段階的に学ぶ）

クイックスタートの次の一歩。手を動かしながらよくあるパターンを身につけるセクションです。

- [スクリプトの基本](tutorials/tutorial_script.md) — ボタンイベント・Field 操作・メッセージ表示・バリデーション
- [モジュール連携](tutorials/tutorial_modules.md) — LinkField・ModuleSearcher・2 つのリストの連動
- [検索を作り込む](tutorials/tutorial_search.md) — And/Or の組合せ・初期値・複数レイアウト・URL 連動
- [Excel 帳票と PDF 出力](tutorials/tutorial_excel_pdf.md) — テンプレートで帳票を作り、PDF に変換
- [WebAPI 連携](tutorials/tutorial_webapi.md) — 外部 API・カスタム Controller・JsonObject
- [認証を有効にする](tutorials/tutorial_auth.md) — CurrentUserModule・アプリ/画面/モジュール/データ単位の認可

---

## 4. ガイド（目的別の作り方）

「こういうことをやりたい」から逆引きで読めるセクションです。

### モジュールの作り方

- [基本的な作成方法と DB 接続](designer/create_module_with_db_simple.md)
- [モジュール作成時の注意点](Help/PointToNote_CreateModule.md)
- [Excel から画面と DDL を作成](designer/import_module_from_excel.md)
- [既存 DB からモジュールを一括作成](designer/import_modules_from_db.md)
- [AI でモジュールを作成](ai/ai_modules.md)

### データベース活用

- [Query フィールド](db/query_field.md) — カスタム SQL で一覧を作る
- [ExecuteSql フィールド](db/execute_sql_field.md) — 任意の SQL を実行する

### スクリプトでよくやること（Tips）

- [Text フィールドを読み取り専用にする](Examples/Tips_IsViewOnly.md)
- [AnchorTag のサイズ調整](Examples/Tips_AnchorTagSizeSetting.md)
- [Label の位置調整](Examples/Tips_LabelPositionSetting.md)
- [検索条件に初期値を設定](Examples/Tips_SearchCriteriaInitialValueSetting.md)
- [Submit 時に処理を追加](Examples/Tips_AddProcessingSubmit.md)
- [ModuleSearcher で他モジュールにアクセス](Examples/Tips_ModuleSearcher.md)
- [リスト同士の連携](Examples/DoubleList.md)
- [メールを送信する](Examples/SendingMail.md)

### 認証・認可

- [認証 / 認可の概要](authorization/authorization.md)

### AI 連携

- [AI 概要](ai/ai_overview.md)
- [AI を使うための設定](ai/ai_setup.md)
- [AITextAnalyzerField](ai/AITextAnalyzerField.md)
- [AI でクエリを作成](ai/ai_query.md)

### プロコード拡張

- [プロコード概要](overview/procode.md)
- [ユーザーコード](user_code/user_code.md)

### 見た目・スタイル

- [CSS](look_and_feel/css.md)
- [カスタムスタイル](look_and_feel/custom_styles.md)
- [Fluent Design](look_and_feel/fluent_design.md)
- [Material Design](look_and_feel/material_design.md)

### デプロイ

- [Visual Studio ソリューション構成とデプロイ](overview/vs_projects.md)
- [デプロイフォルダ](overview/deploy_folder.md)
- [Web サーバーへのデプロイ](overview/server_deploy.md)
- [オプション: VS Code を使う](overview/vscode.md)

### テスト

- [PageObject のエクスポート（自動テスト）](designer/export_pageobject.md)

### サードパーティ UI ライブラリとの連携

- [MudBlazor サンプル](https://lowcodedemo.azurewebsites.net/MudBlazor/MudBlazorHome)
- [Radzen.Blazor サンプル](https://lowcodedemo.azurewebsites.net/RadzenBlazor/RadzenBlazorHome)
- [IgniteUI サンプル](https://lowcodedemo.azurewebsites.net/Bootstrap/ChartSample)

---

## 5. リファレンス（全仕様）

項目ごとに詳しく引くためのセクションです。

### デザイナ

- [デザイナ概要](designer/designer.md)
- [app.clprj](designer/app_clprj.md) — アプリ全体のプロジェクト設定
- [designer.settings](designer/designer_settings.md) — Data Source 等の設定
- [PageFrame](designer/page_frame.md) — アプリの外枠
- [デザイナのカスタマイズ](designer/designer-customize.md)
- [検索コンポーネントのカスタマイズ](designer/designer-match-customize.md)

### モジュール

- [Module 概要](module/module.md)
- [全体設定](module/module_general.md)
- [詳細設定](module/module_detail.md)
- [一覧設定](module/module_list.md)
- [検索設定](module/module_search.md)
- [Document Outline と Property パネル](module/DocumentOutline.md)
- [データモデルと Module の関係](data_model/data-model.md)

### レイアウト

- [レイアウト（Grid / Canvas / Flow）](module/layout.md)

### Field（入力・表示部品）

- [Field 概要・System Field](fields/field.md)
- [Field 共通プロパティ](fields/common_properties.md)
- 入力系: [Text](fields/Text.md) / [Number](fields/Number.md) / [Boolean](fields/Boolean.md) / [Date](fields/Date.md) / [DateTime](fields/DateTime.md) / [Time](fields/Time.md) / [Password](fields/Password.md) / [File](fields/File.md)
- 選択系: [Select](fields/Select.md) / [RadioButton](fields/RadioButton.md) / [RadioGroup](fields/RadioGroup.md) / [Link](fields/Link.md)
- 表示系: [Label](fields/Label.md) / [AnchorTag](fields/AnchorTag.md) / [Id](fields/Id.md) / [ImageViewer](fields/ImageViewer.md) / [MarkupString](fields/MarkupString.md)
- 構造系: [List](fields/List.md) / [DetailList](fields/DetailList.md) / [TileList](fields/TileList.md) / [ListNumber](fields/ListNumber.md) / [Module](fields/Module.md) / [Search](fields/Search.md)
- 操作系: [Button](fields/Button.md) / [SubmitButton](fields/SubmitButton.md)
- 特殊: [ProCode](fields/ProCode.md) / [OptimisticLocking](fields/OptimisticLocking.md)

### スクリプト

- [スクリプト概要](overview/script.md)
- [スクリプトデバッガ](overview/script_debugger.md)

### プロジェクト構成

- [概略](overview/overview.md)
- [Visual Studio ソリューション構成](overview/vs_projects.md)
- [ユーザーコード](user_code/user_code.md)

### ライセンス登録詳細

- [ライセンスについて（種別と登録方法の全体）](overview/about_license.md)
- [オンライン登録](overview/license_online_registration.md) / [オフライン登録](overview/license_web_registration.md)
- [Windows CLI 登録](overview/licence_windows_cli_registration.md) / [Linux 登録](overview/license_linux_registration.md)
- [ドメインライセンス登録](overview/domain_license_registration.md) / [解除](overview/domain_license_cancellation.md)
- [LicenseRegister アプリ](overview/license_license_register_application.md)

---

## 6. リリースノート

- [破壊的変更一覧](breaking_changes/breaking_changes.md)

---

## 動画

- [動画ガイド一覧](movies.md)

---

## ライセンス情報

Codeer.LowCode.Blazor のライセンスは以下の通りです。

- **試用版**: 評価目的のみ 30 日間無料
- **商用利用**: 開発・本番運用ともにライセンス購入が必要
- **コミュニティ利用**: 申請により無償ライセンス発行（[Codeer](https://www.codeer.co.jp/LowCode) へお問い合わせ）

詳細:

- [ソフトウェアライセンス契約書（原本・英語）](https://www.nuget.org/packages/Codeer.LowCode.Blazor/1.0.9/License)
- [日本語版](LicenseJP.md)（※英語版と齟齬がある場合は英語版が優先）
- [ライセンス種別と登録方法の全体](overview/about_license.md)

ご購入・お問い合わせ: [codeer.co.jp/LowCode](https://www.codeer.co.jp/LowCode)
