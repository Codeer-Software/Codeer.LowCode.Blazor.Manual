# Password

Passwordを表すField

<img src="images/Password表示.png" alt="Password表示" title="Password表示" style="border: 1px solid;">

<img src="images/Password設定.png" alt="Password設定" title="Password設定" style="border: 1px solid;" >

1. FieldType
    - Passwordを設定する
2. Name
    - フィールド名の設定. 全体設定時に表示される.
3. DisplayName
    - TBD
4. IsRequired
    - 登録時，必須にする
5. DbColumn
    - テーブルのカラムの設定
6. OnDataChanged
    - 変更時のスクリプト

<img src="images/Password詳細.png" alt="Password詳細" title="Password詳細" style="border: 1px solid;">

## スクリプト
| プロパティ名          | 型        | 説明             |
|-----------------|----------|----------------|
| BackgroundColor | string?  | Fieldの背景色      | 
| Color           | string?  | Fieldの色        |
| IsEnabled       | bool     | Fieldの有効/無効    |
| IsModified      | bool     | Fieldが変更されたどうか |
| IsVisible       | bool     | Fieldの表示/非表示   |
| IsViewOnly      | bool     | Fieldの編集可/編集不可 |
| Value           | string   | Fieldの値        |

| メソッド名           | 戻り値  | 説明                                        |
|-----------------|------|-------------------------------------------|
| CheckPassword() | bool | パスワードが入力されているか，パスワードと確認用パスワードが一致するかチェックする |
| Clear()         | なし   | パスワード，確認用パスワードをクリアする                      |


