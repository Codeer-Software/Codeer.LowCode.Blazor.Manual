# CalendarField - カレンダー表示フィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Extras.Designs.CalendarFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Blazor.Extras`

別モジュールのレコードを**カレンダー（月／週／日ビュー）上の予定**として表示・編集するフィールド。`SearchCondition.ModuleName` で対象モジュールを指定し、そのモジュールの開始日時／終了日時／タイトル等のフィールドを予定にマッピングする。カレンダー上のセルクリックで新規追加、予定クリックで編集（いずれも対象モジュールの `DetailLayoutName` のレイアウトをダイアログ表示）。DB列へのマッピングは持たず、表示している子レコードは親モジュールの保存（Submit）と一緒に保存される。

`IFillHeightFieldDesign` を実装するため、`IsFillAvailable` のグリッドに置くと高さいっぱいに広がる。検索結果ビュー（`SearchField.ResultsViewFieldName`）としても使える（`ISearchResultsViewFieldDesign`）。

## C# クラス定義 (真実の源)

このフィールドは外部ライブラリ `Codeer.LowCode.Blazor.Extras` で定義されている。`FieldDesignBase` を継承し、`IDisplayName` / `ISearchResultsViewFieldDesign` / `IFillHeightFieldDesign` を実装する。

## プロパティ

> 共通プロパティ（Name, IgnoreModification, OnValidateInput）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DisplayName` | string | `""` | 表示名。 |
| `SearchCondition` | SearchCondition | `new()` | カレンダーに表示するデータの取得条件。`ModuleName` に予定元モジュールを指定する。表示中のビュー期間で開始/終了日時が範囲に重なるレコードが自動で絞り込まれる。 |
| `TextField` | string | `""` | 予定のタイトルに使う、対象モジュール内の **TextField** のフィールド名。 |
| `StartField` | string | `""` | 予定の開始日時に使う、対象モジュール内の **DateTimeField または DateField** のフィールド名。 |
| `EndField` | string | `""` | 予定の終了日時に使う、対象モジュール内の **DateTimeField または DateField** のフィールド名。 |
| `AllDayField` | string | `""` | 終日フラグに使う、対象モジュール内の **BooleanField** のフィールド名。 |
| `ColorField` | string | `""` | 予定の色に使う、対象モジュール内の **TextField** のフィールド名（色文字列を保持する列）。 |
| `DetailLayoutName` | string | `""` | 追加／編集ダイアログに使う、対象モジュールの DetailLayout 名。空でデフォルトレイアウト。 |
| `EnableMonthView` | bool | `true` | 月ビューを有効にする。 |
| `EnableWeekView` | bool | `true` | 週ビューを有効にする。 |
| `EnableDayView` | bool | `true` | 日ビューを有効にする。 |
| `OnDataChanged` | string | `""` | 予定の追加／更新／削除で発火するスクリプトイベント名。 |

`TextField` / `StartField` / `EndField` / `AllDayField` / `ColorField` / `DetailLayoutName` は、`SearchCondition.ModuleName` のモジュール内に存在するかデザインチェックで検証される。

## JSON例

```json
{
  "DisplayName": "予定",
  "SearchCondition": {
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Event"
  },
  "TextField": "Title",
  "StartField": "StartAt",
  "EndField": "EndAt",
  "AllDayField": "IsAllDay",
  "ColorField": "Color",
  "DetailLayoutName": "",
  "EnableMonthView": true,
  "EnableWeekView": true,
  "EnableDayView": true,
  "OnDataChanged": "",
  "Name": "Calendar",
  "IgnoreModification": false,
  "OnValidateInput": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Extras.Designs.CalendarFieldDesign"
}
```

> 既定状態は [../../Defaults/CalendarFieldDesign.json](../../Defaults/CalendarFieldDesign.json) を参照。

## ランタイム動作

- 表示中のビュー（月／週／日）の期間に開始日時または終了日時が重なるレコードを `SearchCondition` から取得して予定として並べる。
- セルをクリックすると、対象モジュールの新規レコードを `DetailLayoutName` のダイアログで作成（開始/終了日時にクリック日が初期セットされる）。予定クリックで編集（更新／削除）。
- 追加・更新・削除は親モジュールの Submit で一括保存される（このフィールド単体では DB に書かない）。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `SelectedDate` | DateTime | 読み書き | 表示中の基準日。設定すると該当期間に再読み込みされる。 |
| `ViewMode` | CalendarViewMode | 読み書き | 表示ビュー（`Month` / `Week` / `Day`）。無効化されたビューは設定しても切り替わらない。 |
| `RangeStart` | DateTime | 読み取り | 表示中ビューの期間開始。 |
| `RangeEnd` | DateTime | 読み取り | 表示中ビューの期間終了。 |
| `AllowLoad` | bool | 読み書き | データ読み込みを許可するか。 |

### メソッド

| メソッド | 説明 |
|---|---|
| `Reload()` | 現在の期間で予定を再読み込み。 |
| `SetAdditionalCondition(ModuleSearcher searcher)` | 追加の絞り込み条件を設定する。 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) / [_ScriptApi.md](_ScriptApi.md) を参照。

## 列挙型

### CalendarViewMode

| 値 | 説明 |
|---|---|
| `Month` | 月ビュー |
| `Week` | 週ビュー |
| `Day` | 日ビュー |
