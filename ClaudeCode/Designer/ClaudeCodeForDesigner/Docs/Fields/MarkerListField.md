# MarkerListField - 画像マーカーフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Extras.Designs.MarkerListFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Blazor.Extras`

背景画像（フロアマップ・図面・写真など）の上に、X/Y 座標でマーカー（ピン）を配置するフィールド。各マーカーは `SearchCondition.ModuleName` のモジュールのレコード 1 件に対応し、X/Y 座標・ラベル・色をそのモジュールのフィールドにマッピングする。画像のダブルクリックでマーカー追加、マーカークリックで編集（`DetailLayoutName` のダイアログ）。背景画像は固定リソース（`ResourcePath`）か、同じモジュール内の FileField（`ImageFileField`）から取得する。

DB列へのマッピングは持たず、マーカーレコードは親モジュールの Submit で一括保存される。`IFillHeightFieldDesign`（高さいっぱい）・`ISearchResultsViewFieldDesign`（検索結果ビュー化可）を実装する。

## C# クラス定義 (真実の源)

このフィールドは外部ライブラリ `Codeer.LowCode.Blazor.Extras` で定義されている。`FieldDesignBase` を継承し、`ISearchResultsViewFieldDesign` / `IDataDependentField` / `IFillHeightFieldDesign` を実装する。

## プロパティ

> 共通プロパティ（Name, IgnoreModification, OnValidateInput）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `ResourcePath` | string | `""` | 背景画像に使う Resources 内の画像パス。`ImageFileField` 未指定時に使う。 |
| `ImageFileField` | string | `""` | 背景画像に使う、**同じモジュール内の FileField** のフィールド名。指定するとレコードごとにアップロードされた画像が背景になる（`ResourcePath` より優先）。 |
| `SearchCondition` | SearchCondition | `new()` | マーカー元データの取得条件。`ModuleName` にマーカーモジュールを指定する。 |
| `DetailLayoutName` | string | `""` | マーカーの追加／編集ダイアログに使う、マーカーモジュールの DetailLayout 名。 |
| `XField` | string | `""` | マーカーの X 座標に使う、マーカーモジュール内の **NumberField**。 |
| `YField` | string | `""` | マーカーの Y 座標に使う、マーカーモジュール内の **NumberField**。 |
| `LabelField` | string | `""` | マーカーのラベルに使う、マーカーモジュール内の **TextField**。 |
| `MarkerColor` | string | `""` | マーカーの既定色（HEX 色文字列）。 |
| `MarkerColorField` | string | `""` | マーカーごとの色に使う、マーカーモジュール内の **TextField または ColorPickerField**。 |
| `OnDataChanged` | string | `""` | マーカーの追加・更新・削除で発火するスクリプトイベント名。 |
| `OnClickMarker` | string | `""` | マーカークリック時のスクリプトイベント名（引数: `id`）。**設定すると標準の編集ダイアログの代わりにこのスクリプトが実行される。** |
| `OnDoubleClickPoint` | string | `""` | 画像のダブルクリック時のスクリプトイベント名（引数: `x`, `y`）。**設定すると標準の追加ダイアログの代わりにこのスクリプトが実行される。** |

各フィールド名・レイアウト名は存在確認される（`ImageFileField` は自モジュール、X/Y/Label/MarkerColorField/DetailLayoutName は `SearchCondition.ModuleName`）。

## JSON例

```json
{
  "ResourcePath": "floor_map.png",
  "ImageFileField": "",
  "SearchCondition": {
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Pin"
  },
  "DetailLayoutName": "",
  "XField": "PosX",
  "YField": "PosY",
  "LabelField": "Label",
  "MarkerColor": "#FF4560",
  "MarkerColorField": "",
  "OnDataChanged": "",
  "OnClickMarker": "",
  "OnDoubleClickPoint": "",
  "Name": "FloorMap",
  "IgnoreModification": false,
  "OnValidateInput": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Extras.Designs.MarkerListFieldDesign"
}
```

> 既定状態は [../../Defaults/MarkerListFieldDesign.json](../../Defaults/MarkerListFieldDesign.json) を参照。

## ランタイム動作

- 背景画像の上に、各マーカーレコードの X/Y 座標でピンを表示する。
- 画像をダブルクリックすると、`OnDoubleClickPoint` 未設定なら新規マーカーレコードを `DetailLayoutName` のダイアログで作成（X/Y にクリック座標が初期セット）。設定済みならそのスクリプトに `x, y` を渡して実行。
- マーカーをクリックすると、`OnClickMarker` 未設定なら編集ダイアログ（更新／削除）。設定済みならそのスクリプトに `id` を渡して実行。
- 追加・更新・削除は親モジュールの Submit で一括保存される。

## スクリプトAPI

### メソッド

| メソッド | 説明 |
|---|---|
| `Reload()` | マーカーを再読み込み。 |
| `SetAdditionalCondition(ModuleSearcher searcher)` | 追加の絞り込み条件を設定。 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) / [_ScriptApi.md](_ScriptApi.md) を参照。

### イベントハンドラ例

```csharp
// マーカークリックで独自処理 (標準の編集ダイアログを出さない)
void FloorMap_OnClickMarker(string id)
{
    MessageBox.Show("マーカー " + id + " が選択されました");
}

// 画像ダブルクリックで独自処理
void FloorMap_OnDoubleClickPoint(int x, int y)
{
    Logger.Log("座標: " + x + ", " + y);
}
```
