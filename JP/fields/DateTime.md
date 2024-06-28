# DateTime

<img src="images/DateTime表示.png" alt="DateTime表示" title="DateTime表示" style="border: 1px solid;">

<img src="images/DateTime設定.png" alt="DateTime設定" title="DateTime設定" style="border: 1px solid;" >

1. FieldType
    - DateTimeを設定する
2. Name
    - フィールド名の設定. 全体設定時に表示される.
3. DisplayDane
    - TBD
4. IsRequired
    - 登録時，必須にする
5. OnDataChanged
    - 日時変更時の挙動を定義する.
6. IsUtc
    - UTCでDBに保存する.
7. HasTimeZone
    - TimeZone付きで保存する.
8. DbColumn
    - カラムの設定
    

<img src="images/DateTime詳細.png" alt="DateTime詳細" title="DateTime詳細" style="border: 1px solid;">

## スクリプト
| プロパティ名          | 型         | 説明             |
|-----------------|-----------|----------------|
| Value           | DateTime? | Fieldの値        |
| BackgroundColor | string?   | Fieldの背景色      | 
| Color           | string?   | Fieldの色        |
| IsEnabled       | bool      | Fieldの有効/無効    |
| IsVisible       | bool      | Fieldの表示/非表示   |
| IsViewOnly      | bool      | Fieldの編集可/編集不可 |
| IsModified      | bool      | Fieldが変更されたどうか |
| SearchMax       | DateTime? | 検索条件の日時の最大値    |
| SearchMin       | DateTime? | 検索条件の日時の最大値    |