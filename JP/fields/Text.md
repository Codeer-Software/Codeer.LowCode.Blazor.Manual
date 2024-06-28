# Text

テキストを表示する

<img src="images/Text表示.png" alt="Text表示" title="Text表示" style="border: 1px solid;">

<img src="images/Text設定.png" alt="Text設定" title="Text設定" style="border: 1px solid;" >

1. FieldType
    - Textを設定する
2. Name
    - フィールド名の設定. 全体設定時に表示される.
3. DisplayName
    - TBD
4. DbColumn
    - テーブルのカラムの設定
5. Placeholder
    - placeholderの設定
6. MaxLength
   - 最大文字数の設定
7. Rows
   - textareaにする場合，行数を指定する. 

<img src="images/Text詳細.png" alt="Text詳細" title="Text詳細" style="border: 1px solid;">

## スクリプト
| プロパティ名          | 型               | 説明                                            |
|-----------------|-----------------|-----------------------------------------------|
| Value           | string?         | Fieldの値                                       |
| SearchValue     | string?         | 検索条件のinputフィールドのvalue                         |
| Comparison      | MatchComparison | 検索条件のinputフィールドの条件区分<br>`Equal`, `Like`が使用できる |
| BackgroundColor | string?         | Fieldの背景色                                     | 
| Color           | string?         | Fieldの色                                       |
| IsEnabled       | bool            | Fieldの有効/無効                                   |
| IsVisible       | bool            | Fieldの表示/非表示                                  |
| IsViewOnly      | bool            | Fieldの編集可/編集不可                                |
| IsModified      | bool            | Fieldが変更されたどうか                                |
