# ImageViewer

imageを表示するField

<img src="images/ImageViewer表示.png" alt="ImageViewer表示" title="ImageViewer表示" style="border: 1px solid;">

<img src="images/ImageViewer設定.png" alt="ImageViewer設定" title="ImageViewer設定" style="border: 1px solid;" >

1. FieldType
    - ImageViewerを設定する
2. Name
    - フィールド名の設定. 全体設定時に表示される.
3. ResourcePath
    - `${APPLICATION_HOME}/Resource` からの相対パスを指定する

<img src="images/ImageViewer詳細.png" alt="ImageViewer詳細" title="ImageViewer詳細" style="border: 1px solid;">


## スクリプト
| プロパティ名          | 型       | 説明             |
|-----------------|---------|----------------|
| BackgroundColor | string? | Fieldの背景色      | 
| Base64Data      | string  | Fieldの背景色      | 
| Color           | string? | Fieldの色        |
| ImageExtension  | string  | Fieldの色        |
| IsEnabled       | bool    | Fieldの有効/無効    |
| IsVisible       | bool    | Fieldの表示/非表示   |
| IsViewOnly      | bool    | Fieldの編集可/編集不可 |
| ResourcePath    | string  | Fieldの編集可/編集不可 |

| メソッド名             | 戻り値 | 説明             |
|-------------------|-----|----------------|
| SetBase64Data()   | なし  | ダウンロードする       |
| SetMemoryStream() | なし  | メモリーストリームを取得する |
