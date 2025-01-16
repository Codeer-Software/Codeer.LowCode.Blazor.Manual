# クエリをAIで作成
Codeer.LowCode.BlazorはLowCodeアプリの開発者が定義したSQL文をベースに、モジュールフィールドを作成することができ、より自由度の高いデータ集計をサポートしています。

AIを利用すれば、素早く複雑なSQL文を生成することができます。

## AIによるクエリ生成一例
### 環境
この例では、以下のデータベーステーブル"sales"があることを前提とします。
<img width=500 src="../../Image/query_sample_table.png">

### ステップ
1. [AIを使うための設定](ai_setup.md)が完了していない場合はまず行ってください
2. Designerでモジュール「販売集計」を作成し、Data Sourceを設定します
<img width=800 src="../../Image/query_module_new.png">
3. モジュールのFields一覧にQueryフィールドを追加し、「Open Settings」を開きます
<img width=800 src="../../Image/query_add_query_field.png">
4. Query Settings画面でAIとチャットする形でSQL文とパラメーター一覧を作成します。
　OKを押すことで設定データが保存され、ツールボックスにSQLフィールドが生成されます
<img width=800 src="../../Image/query_settings.png">
5. Query Settingsにより生成されたSQLフィールドをモジュールに配置します
<img width=800 src="../../Image/query_list_fields.png">
<img width=800 src="../../Image/query_search_fields.png">
6. デザインプロジェクトをデプロイしサーバーを起動します。下図は完成イメージです：
<img width=800 src="../../Image/query_result.png">

## 関連情報
- [AI](ai_overview.md)
- [AIを使うための設定](ai_setup.md)
- [Module](../module/module.md)



