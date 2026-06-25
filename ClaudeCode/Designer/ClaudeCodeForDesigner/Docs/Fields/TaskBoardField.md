# TaskBoardField - タスクボード（カンバン）フィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Extras.Designs.TaskBoardFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Blazor.Extras`

別モジュールのレコードを**カンバンボード**（ステータス列ごとのカード）として表示・編集するフィールド。`Statuses` で列（ステータス）を定義し、各カードは `SearchCondition.ModuleName` のモジュールのレコード 1 件。カードの `StatusField`（ステータス値）でどの列に入るかが決まる。カードのドラッグ＆ドロップで列移動（＝`StatusField` の更新）と列内並べ替え（`SortIndexField` の更新）ができる。カードのダブルクリック／追加で `PopupLayoutName` のダイアログを開く。

DB列へのマッピングは持たず、カードレコードは親モジュールの Submit で一括保存される。`IFillHeightFieldDesign`（高さいっぱい）・`ISearchResultsViewFieldDesign`（検索結果ビュー化可）を実装する。

> **使い方の実用パターンは [../AppPatterns/visualization_dashboard.md](../AppPatterns/visualization_dashboard.md) を必ず読む**（親詳細への埋め込み・親FK自動セット・編集ダイアログ作法・高さ・横断ビューの保存制約）。
>
> 要点:
> - 親詳細に埋め込むと **`ListField` と同等**（`SearchCondition` の `FieldVariableMatchCondition` で子を絞り、カード追加時に親FK自動セット）。子モジュールに `DataSourceName` と FK が必須。
> - **`Statuses.Items` は配列**。各要素が `DisplayText`/`Value`/`Color`/`BackgroundColor`/`CanAdd` を**別プロパティ**で持つ（SelectField の「表示,値」区切り方式ではない）。カードの所属列は `StatusField` の値と `Statuses.Items[i].Value` の**文字列一致**で決まる。
> - **`StatusField` は `SelectField` か `TextField` のみ**。SelectField を使うなら `Candidates` の値部と `Statuses.Items[i].Value` を一致させる。
> - **`SortIndexField`（NumberField）が無いと並べ替えが永続化されない**。
> - **`DbTable:""` の表示専用モジュールに埋めると保存できない**（横断ビューは閲覧専用と割り切る）。

## C# クラス定義 (真実の源)

このフィールドは外部ライブラリ `Codeer.LowCode.Blazor.Extras` で定義されている。`FieldDesignBase` を継承し、`IDisplayName` / `ISearchResultsViewFieldDesign` / `IFillHeightFieldDesign` を実装する。

## プロパティ

> 共通プロパティ（Name, IgnoreModification, OnValidateInput）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DisplayName` | string | `""` | 表示名。 |
| `SearchCondition` | SearchCondition | `new()` | カード元データの取得条件。`ModuleName` にカードモジュールを指定する。 |
| `Statuses` | TaskBoardStatuses | `{ "Items": [] }` | ボードの列（ステータス）定義。下記「Statuses の構造」参照。 |
| `StatusField` | string | `""` | カードのステータスに使う、カードモジュール内の **SelectField または TextField**。この値が列の `Value` と一致する列にカードが入る。 |
| `CardLayoutName` | string | `""` | カードの見た目に使う、カードモジュールの DetailLayout 名。 |
| `PopupLayoutName` | string | `""` | 追加／編集ダイアログに使う、カードモジュールの DetailLayout 名。`CardLayoutName` と同じでもよい。 |
| `EnableDoubleClickPopup` | bool | `true` | カードのダブルクリックで編集ダイアログを開くか。 |
| `SortIndexField` | string | `""` | 列内の並び順に使う、カードモジュール内の **NumberField**。指定すると D&D 並べ替えで自動採番される。未指定なら並べ替え不可。 |
| `OnDataChanged` | string | `""` | カードの追加・更新・削除・移動で発火するスクリプトイベント名。 |

各フィールド名・レイアウト名は `SearchCondition.ModuleName` に存在するかデザインチェックで検証される。

### Statuses の構造

`Statuses.Items` は列定義の配列。各列は以下を持つ。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DisplayText` | string | `""` | 列ヘッダーの表示テキスト。 |
| `Value` | string | `""` | この列に対応するステータス値（カードの `StatusField` の値と突き合わせる）。空の場合は `DisplayText` が値として使われる。 |
| `Color` | string | `""` | 列ヘッダーの文字色（HEX）。 |
| `BackgroundColor` | string | `""` | 列ヘッダーの背景色（HEX）。 |
| `CanAdd` | bool | `true` | この列で新規カード追加を許可するか。 |

## JSON例

```json
{
  "DisplayName": "案件ボード",
  "SearchCondition": {
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Task"
  },
  "Statuses": {
    "Items": [
      { "DisplayText": "未着手", "Value": "Todo", "Color": "", "BackgroundColor": "#EEEEEE", "CanAdd": true },
      { "DisplayText": "進行中", "Value": "Doing", "Color": "", "BackgroundColor": "#E3F2FD", "CanAdd": true },
      { "DisplayText": "完了", "Value": "Done", "Color": "", "BackgroundColor": "#E8F5E9", "CanAdd": false }
    ]
  },
  "StatusField": "Status",
  "CardLayoutName": "Card",
  "PopupLayoutName": "",
  "EnableDoubleClickPopup": true,
  "SortIndexField": "SortIndex",
  "OnDataChanged": "",
  "Name": "Board",
  "IgnoreModification": false,
  "OnValidateInput": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Extras.Designs.TaskBoardFieldDesign"
}
```

> 既定状態は [../../Defaults/TaskBoardFieldDesign.json](../../Defaults/TaskBoardFieldDesign.json) を参照。

## ランタイム動作

- `Statuses.Items` の各列に、`StatusField` の値が一致するカードを並べる（`SortIndexField` 昇順）。
- カードを別列へドラッグすると `StatusField` を移動先の値に更新。同列内ドラッグで `SortIndexField` を採番し直す。
- 列の「追加」（`CanAdd: true` の列のみ）またはカードのダブルクリック（`EnableDoubleClickPopup: true`）で `PopupLayoutName` のダイアログを開く。追加時はステータスに列の値が初期セットされる。
- 追加・更新・削除・移動は親モジュールの Submit で一括保存される。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `AllowLoad` | bool | 読み書き | データ読み込みを許可するか。 |

### メソッド

| メソッド | 説明 |
|---|---|
| `Reload()` | カードを再読み込み。 |
| `SetAdditionalCondition(ModuleSearcher searcher)` | 追加の絞り込み条件を設定。 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) / [_ScriptApi.md](_ScriptApi.md) を参照。
