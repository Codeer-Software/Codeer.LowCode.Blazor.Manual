# Time

時刻を表すフィールド

<img src="images/Time表示.png" alt="Time表示" title="Time表示" style="border: 1px solid;">

<img src="images/Time設定.png" alt="Time設定" title="Time設定" style="border: 1px solid;" >

1. FieldType
    - Timeを設定する
2. Name
    - フィールド名の設定. 全体設定時に表示される.
3. DisplayName
    - TBD
4. IsRequired
    - 登録時，必須にする
5. IsUtc
   - UTCかどうかを設定する.
6. HasTimeZone
    - タイムゾーン付きかどうかを設定する.
7. DbColumn
    - テーブルのカラムの設定

<img src="images/Time詳細.png" alt="Time詳細" title="Time詳細" style="border: 1px solid;">

## スクリプト
| プロパティ名          | 型         | 説明             |
|-----------------|-----------|----------------|
| BackgroundColor | string?   | Fieldの背景色      | 
| Color           | string?   | Fieldの色        |
| DisplayText     | string?   | Fieldの色        |
| IsEnabled       | bool      | Fieldの有効/無効    |
| IsModified      | bool      | Fieldが変更されたどうか |
| IsVisible       | bool      | Fieldの表示/非表示   |
| IsViewOnly      | bool      | Fieldの編集可/編集不可 |
| SearchMax       | TimeOnly? | 検索条件の時刻の最大値    |
| SearchMin       | TimeOnly? | 検索条件の時刻の最大値    |
| Value           | string    | Fieldの値        |

