# Label

ラベルを表示するField

<img src="images/Label表示.png" alt="Label表示" title="Label表示" style="border: 1px solid;">

<img src="images/Label設定.png" alt="Label設定" title="Label設定" style="border: 1px solid;" >

1. FieldType
    - Labelを設定する
2. Name
    - フィールド名の設定. 全体設定時に表示される.
3. Text
    - Labelに表示するテキストを設定する.
4. Style
    - H1～H6から選択する
5. RelativeField
    - 関連するFieldをしていする
    - 一覧でLabelのD&DでLabel,Fieldを設定できる
    - <img src="images/Label関連.png" alt="Label関連" title="Label関連" style="border: 1px solid;">


<img src="images/Label詳細.png" alt="Label詳細" title="Label詳細" style="border: 1px solid;">

## スクリプト
| プロパティ名          | 型               | 説明                                            |
|-----------------|-----------------|-----------------------------------------------|
| BackgroundColor | string?         | Fieldの背景色                                     | 
| Color           | string?         | Fieldの色                                       |
| IsEnabled       | bool            | Fieldの有効/無効                                   |
| IsVisible       | bool            | Fieldの表示/非表示                                  |
| IsViewOnly      | bool            | Fieldの編集可/編集不可                                |
| Text            | string          | ラベルのテキスト                                      |