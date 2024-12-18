# Databaseからモジュールを作成する

## 使用シーン
すでに既存のデータベースをお持ちで、速やかにCodeer.LowCode.Blazorアプリと連携して使用したい場合は、「Databaseからモジュールを作成」機能をご利用ください。

## 操作方法
### 本チュートリアルは以下のデータベースが存在し、[DesignerSettings](../designer/designer_settings.md)ファイルでDataSources情報が設定完了していることを前提とします。
<img width=800 src="../../Image/Db2Mod_DataSources.png">

### Step 1. デザイナの「Tools」メニューを開き、「Import Modules from Database」をクリックします
<img width=800 src="../../Image/Db2Mod_Menu.png">

### Step 2. 出てくる「DB TABLE SELECT」ウィンドウでインポートしたいテーブルを選択して、OKを押します
<img width=800 src="../../Image/Db2Mod_Dialog.png">

### Step 3. 選択されたテーブルをベースにモジュールが自動的に作成されます
<img width=800 src="../../Image/Db2Mod_Modules.png">

### Step 4. 自動作成されたモジュールにカスタマイズを加えることでニーズに合うモジュールに仕上げます

## 関連ページ
- [DesignerSettings](../designer/designer_settings.md)
- [Module](module/module.md)