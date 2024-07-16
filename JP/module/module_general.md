# モジュール全体設定

全体設定ではDBとの入出力やアクセス権の設定など主にデータとしての振る舞いを設定します。 画面上に表示しないFieldもここで設定することができます。スクリプト中でModuleSearcherを用いてデータとして取得/利用することができます。

<img src="images/module_general.png" width="400" alt="モジュール全体" title="モジュール全体" style="border: 1px solid;">

## ツールボックス
モジュールで使用するFieldを選択します．

| 項目           | 説明                                                                        |
|--------------|---------------------------------------------------------------------------|
| SystemFields | SystemFieldの一覧が表示されます。|
| CommonFields | CommonFieldの一覧が表示されます。|
| DB Fields    | 全体設定でDataSourceを指定した場合に，テーブルのカラムからField候補が表示されます。|
| Rest Fields  | DB Fieldsで詳細設定で（まだ）使用していないFieldが表示されます。|
| Link Fields  | Linkフィールドを作成した場合に，Link先のフィールドが表示されます。Gridに配置することでLink先を選択時にフィールドを表示できます。|
| Layout       | 詳細画面でGridレイアウト、Canvasレイアウトを設定時に使用します。|

## 設定項目
| 項目            | 説明                                                                                                       |
|---------------|----------------------------------------------------------------------------------------------------------|
| DisplayName   | 表示名です。現在はModuleSelectFieldで表示するときだけに使われています。|
| PageTitle     | htmlのタイトルに設定されます。|
| DataSource    | DBとテーブルを指定することによりEntityとして設定します。<br/>DataSourceは[designer.settings](../designer/designer_settings.md)で設定。|
| ListPageField | 一覧ページでのリスト設定です。|
| Option        | `作成`, `更新`, `削除`, `一括ダウンロード`, `一括更新` の有無を設定します。|
| Access        | - ユーザーとデータ内容によるアクセス制御を設定します。[認証/認可](authorization/authorization.md)を参照してください。|
| Fields        | Module内で使用するFieldをToolBoxから定義します。|
| Scripts       | Scriptを定義します。|


## プロパティ
選択しているFieldのプロパティが表示されます。
