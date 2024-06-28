# Module

ModuleをネストするField

<img src="images/Module表示.png" alt="Module表示" title="Module表示" style="border: 1px solid;">

<img src="images/Module設定.png" alt="Module設定" title="Module設定" style="border: 1px solid;" >

1. FieldType
    - Moduleを設定する
2. ModuleName
    - 入れ子にするModuleを指定する.
3. LayoutName
    - 表示するLayout(入れ子にするModuleの詳細設定で作成する)を指定する.
4. OnDataChanged
    - 変更時のスクリプト
5. DbColumn
    - テーブルのカラムの設定

<img src="images/Module詳細.png" alt="Module詳細" title="Module詳細" style="border: 1px solid;">




## スクリプト
| プロパティ名          | 型       | 説明             |
|-----------------|---------|----------------|
| BackgroundColor | string? | Fieldの背景色      | 
| ChildModule     | Module  | 子モジュール         |
| Color           | string? | Fieldの色        |
| IsEnabled       | bool    | Fieldの有効/無効    |
| IsModified      | bool    | Fieldが変更されたどうか |
| IsVisible       | bool    | Fieldの表示/非表示   |
| IsViewOnly      | bool    | Fieldの編集可/編集不可 |

