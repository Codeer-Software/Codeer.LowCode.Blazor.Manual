# Id

Idを表すField

<img src="images/Id表示.png" alt="Id表示" title="Id表示" style="border: 1px solid;">

<img src="images/Id設定.png" alt="Id設定" title="Id設定" style="border: 1px solid;" >

1. FieldType
    - Idを設定する
2. Name
    - フィールド名の設定. 全体設定時に表示される.
3. DisplayDane
    - TBD
4. IsRequired
    - 登録時，必須にする
5. OnDataChanged
    - データ変更時のスクリプト
6. DbColumn
    - テーブルのカラムの設定

<img src="images/Id詳細.png" alt="Id詳細" title="Id詳細" style="border: 1px solid;">

## スクリプト
| プロパティ名          | 型       | 説明                    |
|-----------------|---------|-----------------------|
| Value           | string? | Fieldの値               |
| BackgroundColor | string? | Fieldの背景色             | 
| Color           | string? | Fieldの色               |
| IsEnabled       | bool    | Fieldの有効/無効           |
| IsVisible       | bool    | Fieldの表示/非表示          |
| IsViewOnly      | bool    | Fieldの編集可/編集不可        |
| IsModified      | bool    | Fieldが変更されたどうか        |

