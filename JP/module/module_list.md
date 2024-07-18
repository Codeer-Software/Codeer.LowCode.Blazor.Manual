# モジュール一覧設定

Entityの一覧画面の設定をします.

全体設定で作成したフィールドから一覧に表示するフィールドを選択します.

<img src="images/module_list.png" width="400" alt="モジュール一覧" title="モジュール一覧" style="border: 1px solid;">

## ツールボックス
モジュールで使用するFieldを選択します．

| 項目           | 説明                                               |
|--------------|--------------------------------------------------|
| SystemFields | SystemFieldの一覧が表示されます                            |
| CommonFields | CommonFieldの一覧が表示されます                            |
| DB Fields    | 全体設定でDataSourceを指定した場合に，テーブルのカラムからField候補が表示されます |
| Rest Fields  | DB Fieldsで詳細設定で（まだ）使用していないFieldが表示されます．          |
| Link Fields  | Linkフィールドを作成した場合に，Link先のフィールドが表示されます.            |
| Layout       | 詳細画面でGridレイアウト，Canvasレイアウトを設定時に使用します．            |

## 一覧設定
列，段の数を指定して一覧表示の設定をします．
追加ボタンをクリックして複数のレイアウトを作成できます.

一覧に表示するフィールドが多い場合，多段リストを使えます．

<img src="images/多段List設定.png" alt="多段List設定" title="多段List設定" width="400" style="border: 1px solid;">
<img src="images/多段リスト表示.png" alt="多段リスト表示" title="多段リスト表示" width="400" style="border: 1px solid;">


## プロパティ
選択しているFieldのプロパティが表示されます

## レイアウト設定

<img src="images/list.png" alt="一覧" title="一覧" width="400" style="border: 1px solid;">

モジュールの一覧設定画面で一覧のレイアウトを設定します．

## デフォルトレイアウト

モジュールの一覧画面に使うレイアウト設定はdefaultという名前で作成されます．（変更できません）

モジュールの一覧ページにはdefaultのレイアウトが適用されます．

## 複数レイアウト

<img src="images/list_multiple.png" alt="一覧複数" title="一覧複数" width="400" style="border: 1px solid;">

追加ボタンをクリックしてデフォルトとは異なるレイアウトを作成できます．

`LinkField` にdefaultを含む作成済みのすべてのレイアウトから, 適用するレイアウトを指定できます.

<img src="images/list_settings.png" alt="一覧設定" title="一覧設定" width="400" style="border: 1px solid;">

