# GanttField - ガントチャートフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Extras.Designs.GanttFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Blazor.Extras`

別モジュールのレコードを**ガントチャートのタスクバー**として表示・編集するフィールド。`SearchCondition.ModuleName` のモジュールをタスク元とし、開始/終了日時・進捗・タスク名等のフィールドをバーにマッピングする。タスクの追加（セルクリック）・編集（バークリックで `DetailLayoutName` のダイアログ）・バーのドラッグによる期間変更に対応する。タスク間の依存（先行→後続）を別モジュールで管理することもできる。

DB列へのマッピングは持たず、表示中のタスク／依存レコードは親モジュールの Submit で一括保存される。`IFillHeightFieldDesign`（高さいっぱい）・`ISearchResultsViewFieldDesign`（検索結果ビュー化可）を実装する。

> ガント Field はヘッドレス（Playwright headless 等）では描画されないことがある（実機/headed で確認する）。

> **使い方の実用パターンは [../AppPatterns/visualization_dashboard.md](../AppPatterns/visualization_dashboard.md) を必ず読む**（親レコード詳細への埋め込み・親FK自動セット・編集ダイアログ作法・高さの出し方・横断ビューの保存制約）。本ページはプロパティ仕様の正典。
>
> 要点:
> - **親詳細に埋め込むと親子は `ListField` と同等に動く**。`SearchCondition.Condition` の `FieldVariableMatchCondition`（`SearchTargetVariable`＝子FK、`Variable`＝親 `Id.Value`）で子を絞り、**バー追加時に親FKが自動セットされる**。対象（子）モジュールには `DataSourceName` と FK フィールドが必須。
> - **`DbTable:""` の表示専用モジュールに埋めると編集を保存できない**（保存は埋め込み先の親モジュールの Submit に依存）。横断ビュー専用画面は閲覧専用と割り切る。
> - **旧 `FrappeGantt`（`...Bindings.FrappeGantt...`、プロパティ名 `NameField`/`StartDateField`/`EndDateField`）とは別フィールド型**。現行 Extras Gantt は `TextField`/`StartField`/`EndField`。旧 FrappeGantt の例（`NameField`/`StartDateField` を使うもの）は**参照しない**。実装サンプルは [`Samples/ProjectManagementTemplate/App/Modules/プロジェクト.mod.json`](../../Samples/ProjectManagementTemplate/App/Modules/) を開く。

## C# クラス定義 (真実の源)

このフィールドは外部ライブラリ `Codeer.LowCode.Blazor.Extras` で定義されている。`FieldDesignBase` を継承し、`IDisplayName` / `ISearchResultsViewFieldDesign` / `IFillHeightFieldDesign` を実装する。

## プロパティ

> 共通プロパティ（Name, IgnoreModification, OnValidateInput）は [_FieldCommon.md](_FieldCommon.md) を参照。

### タスクデータのマッピング

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DisplayName` | string | `""` | 表示名。 |
| `SearchCondition` | SearchCondition | `new()` | タスク元データの取得条件。`ModuleName` にタスクモジュールを指定する。表示中ビュー期間に開始/終了が重なるタスクが自動で絞り込まれる。 |
| `TextField` | string | `""` | タスク名に使う **TextField** のフィールド名。 |
| `StartField` | string | `""` | タスク開始に使う **DateTimeField / DateField** のフィールド名。`DateField`（日付のみ）だと終日扱いになり Day ビューは無効化される。 |
| `EndField` | string | `""` | タスク終了に使う **DateTimeField / DateField** のフィールド名。 |
| `ProgressField` | string | `""` | 進捗率に使う **NumberField** のフィールド名（0〜100 にクランプ）。 |
| `IdField` | string | `""` | タスクの **IdField** のフィールド名（依存の対応付けに使う）。 |
| `ProcessingCounterField` | string | `""` | 依存追加時にインクリメントされる **NumberField**（処理カウンタ）のフィールド名。 |
| `BarColorField` | string | `""` | タスクごとのバー色に使う **TextField または ColorPickerField** のフィールド名。 |
| `DetailLayoutName` | string | `""` | 追加／編集ダイアログに使う、タスクモジュールの DetailLayout 名。 |

### タスク依存（先行→後続）

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DependenciesModule` | SearchCondition | `new()` | 依存関係を保持する中間モジュールの取得条件。`ModuleName` 未指定なら依存機能は無効。 |
| `DependencySourceIdField` | string | `""` | 依存モジュール内で先行タスク ID を保持する **IdField / LinkField**。 |
| `DependencyDestinationIdField` | string | `""` | 依存モジュール内で後続タスク ID を保持する **IdField / LinkField**。 |

### ビューモード

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `EnableDayView` | bool | `true` | 日ビューを有効にする（日付のみフィールドのときは無効）。 |
| `EnableWeekView` | bool | `true` | 週ビューを有効にする。 |
| `EnableMonthView` | bool | `true` | 月ビューを有効にする。 |
| `CustomRange` | bool | `false` | `true` で初期表示をカスタム期間ビューにする。 |
| `CustomRangeEditable` | bool | `true` | カスタム期間をユーザーが変更できるか。 |

### 表示

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `FitToWidth` | bool | `false` | `true` で期間を幅に合わせる。 |
| `ShowDetailHeader` | bool | `true` | 詳細ヘッダーを表示する。 |
| `ShowToolbar` | bool | `true` | ツールバー（ビュー切替等）を表示する。 |
| `BarColor` | string | `""` | バーの既定色（`BarColorField` 未設定／空のタスクに適用）。HEX 色文字列。 |

### イベント

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `OnDataChanged` | string | `""` | タスク／依存の追加・更新・削除で発火するスクリプトイベント名。 |

各フィールド名・レイアウト名は `SearchCondition.ModuleName`（依存系は `DependenciesModule.ModuleName`）に存在するかデザインチェックで検証される。

## JSON例

```json
{
  "DisplayName": "工程",
  "SearchCondition": {
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Task"
  },
  "TextField": "Name",
  "StartField": "StartDate",
  "EndField": "EndDate",
  "ProgressField": "Progress",
  "IdField": "Id",
  "ProcessingCounterField": "",
  "BarColorField": "",
  "DetailLayoutName": "",
  "DependenciesModule": {
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "TaskDependency"
  },
  "DependencySourceIdField": "FromTaskId",
  "DependencyDestinationIdField": "ToTaskId",
  "EnableDayView": true,
  "EnableWeekView": true,
  "EnableMonthView": true,
  "CustomRange": false,
  "CustomRangeEditable": true,
  "FitToWidth": false,
  "ShowDetailHeader": true,
  "ShowToolbar": true,
  "BarColor": "",
  "OnDataChanged": "",
  "Name": "Gantt",
  "IgnoreModification": false,
  "OnValidateInput": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Extras.Designs.GanttFieldDesign"
}
```

> 依存機能が不要なら `DependenciesModule.ModuleName` を空のままにする（依存系プロパティも未使用）。既定状態は [../../Defaults/GanttFieldDesign.json](../../Defaults/GanttFieldDesign.json) を参照。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `ViewStart` | DateTime | 読み書き | 表示開始日。設定すると該当期間に再読み込み。 |
| `ViewMode` | GanttViewMode | 読み書き | 表示ビュー（`Day` / `Week` / `Month` / `CustomRange`）。無効化ビューは設定しても切り替わらない。 |
| `RangeStart` | DateTime | 読み取り | 表示期間の開始。 |
| `RangeEnd` | DateTime | 読み取り | 表示期間の終了。 |
| `AllowLoad` | bool | 読み書き | データ読み込みを許可するか。 |

### メソッド

| メソッド | 説明 |
|---|---|
| `Reload()` | 現在の期間でタスク・依存を再読み込み。 |
| `SetAdditionalCondition(ModuleSearcher searcher)` | 追加の絞り込み条件を設定。 |
| `SetCustomRange(DateTime start, DateTime end)` | カスタム期間ビューに切り替え、期間を設定。 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) / [_ScriptApi.md](_ScriptApi.md) を参照。

## 列挙型

### GanttViewMode

| 値 | 説明 |
|---|---|
| `Day` | 日ビュー |
| `Week` | 週ビュー |
| `Month` | 月ビュー |
| `CustomRange` | カスタム期間ビュー |
