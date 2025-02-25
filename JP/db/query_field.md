## Query Field概要
Codeer.LowCode.Blazorは、標準として提供しているデータベースとの連携機能以外に、カスタムクエリを実行し、画面にデータを表示する`Query`フィールドを搭載しています。

SQL文とカスタムフィールド・パラメーターを設定したうえで、実行結果をDesignerでプレビューしたり、Web画面に表示したりすることができます。

また、SQLはAIの補助でより効率的に作ることができます。
詳しくは[クエリをAIで作成](../ai/ai_query.md)をご参照ください。

<img width=800 src="../../Image/query_add_query_field.png">

<img width=800 src="../../Image/query_overview.png">


## SQLクエリ
カスタムSQL文を使うことができます。計算列やGROUP BY等を用いて自由なクエリを作成することができます。

### Custom Paging使用
複雑なクエリの場合は、結果のCOUNT文を指定することで、Webページの表示時に正確にページングすることができます。
`Custom Paging`使用をONにし、カスタムCOUNT文を指定します。

<img width=800 src="../../Image/Query_CustomPaging.png">


また、1ページの行数上限は下図のようにモジュールの`ListField Setting`→`LimitCount`で設定することができます。

<img width=800 src="../../Image/List_LimitCount.png">

## Queryにより生成されたSQLフィールド
Query設定画面で定義されたColumnおよびParameterはSQLフィールドノードの下に追加されます。

<img width=500 src="../../Image/Query_Sql_Fields.png">

これらのフィールドを`詳細`、`一覧`、`検索`画面にドラッグ&ドロップで設置したら、データを表示したり、検索条件として値を読み込んだりすることができます。

<img width=800 src="../../Image/query_list_fields.png">

<img width=800 src="../../Image/query_search_fields.png">

<img width=800 src="../../Image/query_result.png">

## 関連情報
- [クエリをAIで作成](../ai/ai_query.md)
- [ExecuteSqlフィールド](execute_sql_field.md)



