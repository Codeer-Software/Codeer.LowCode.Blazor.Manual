# ApexHBarChartField - 横棒グラフフィールド

**TypeFullName:** `Codeer.LowCode.Bindings.ApexCharts.Designs.ApexHBarChartFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Bindings.Blazor-ApexCharts`（ApexCharts ベースのチャートコンポーネント）

データを検索条件（SearchCondition）で取得し、**横棒グラフ（水平方向の棒グラフ）** として表示する読み取り専用フィールド。DB列へのマッピングは持たない（データは対象モジュールから動的に取得）。

縦棒・折れ線・面・散布・ヒートマップ等の混在チャートには [ApexChartField.md](ApexChartField.md)、円グラフ・ドーナツチャート等の円形系には [ApexRadialChartField.md](ApexRadialChartField.md) を使う。このフィールドは**横棒に特化**しており、`ApexChartField` のような系列ごとのチャートタイプ切り替えやグリッド線の設定オプションは持たない。

## C# クラス定義 (真実の源)

このフィールドは外部ライブラリ `Codeer.LowCode.Bindings.Blazor-ApexCharts` で定義されているため、`Codeer.LowCode.Blazor` リポジトリ内に C# 定義は存在しない。`ApexChartFieldDesignBase` を継承し、`SeriesType` と `Series` のみを固有プロパティとして持つ。

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

### ApexChartFieldDesignBase 共通プロパティ

ApexChartField / ApexHBarChartField / ApexRadialChartField が継承する共通プロパティ。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `SearchCondition` | SearchCondition | `new()` | データ取得条件。`ModuleName` にデータ元のモジュール名を指定する。 |
| `DisplayName` | string | `""` | チャートのタイトル表示名。 |
| `CategoryField` | string? | `null` | カテゴリ軸（横棒グラフでは縦軸＝項目軸）に使用するフィールド名。`SearchCondition.ModuleName` のモジュール内のフィールドを指定する。TextField, NumberField, DateField, DateTimeField が使用可能。 |
| `CategoryFormat` | string? | `null` | カテゴリ値のフォーマット文字列。日付の場合 `"yyyy-MM-dd"` 等。 |
| `SeriesFractionDigits` | int | `2` | 値の小数桁数。**整数のみ（`2.0` は不可、`2` と書くこと）** |
| `ShowLegend` | bool | `true` | 凡例の表示/非表示。 |

### ApexHBarChartFieldDesign 固有プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `SeriesType` | SeriesType | `"Bar"` | チャートタイプ。横棒グラフでは `"Bar"` を使う。 |
| `Series` | ChartSeries | `{ "Series": [] }` | 表示する系列の定義。各系列で対象フィールド名と色を指定する。 |

### Series（ChartSeries）の構造

`Series` プロパティは系列のリストを保持する。各系列は以下の構造を持つ。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Name` | string | `""` | 表示する NumberField のフィールド名。`SearchCondition.ModuleName` のモジュール内の NumberField を指定する。 |
| `Color` | string | `""` | 系列の色（HEX形式、例: `"#FF4560"`）。空の場合はデフォルトテーマ色が自動割り当てされる。 |
| `Type` | SeriesType | `"Line"` | 系列のチャートタイプ。横棒グラフでは `"Bar"` を指定する。 |

**注意事項:**
- 系列の `Name` には `SearchCondition.ModuleName` で指定したモジュール内の **NumberField** のフィールド名のみ使用可能。デザインチェックで存在確認される。

## JSON例

```json
{
  "SeriesType": "Bar",
  "Series": {
    "Series": [
      { "Name": "Amount", "Color": "", "Type": "Bar" }
    ]
  },
  "SearchCondition": {
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "SalesByCategory"
  },
  "CategoryField": "CategoryName",
  "DisplayName": "カテゴリ別売上",
  "SeriesFractionDigits": 2,
  "ShowLegend": true,
  "Name": "SalesChart",
  "IgnoreModification": false,
  "OnValidateInput": "",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexHBarChartFieldDesign"
}
```

> `null` のプロパティ（`CategoryField` / `CategoryFormat` 未設定時）は出力されない。既定状態は [../../Defaults/ApexHBarChartFieldDesign.json](../../Defaults/ApexHBarChartFieldDesign.json) を参照。

## 列挙型

### SeriesType

| 値 | 説明 |
|---|---|
| `Bar` | 棒（横棒グラフではこれを使う） |
| `Line` | 折れ線 |
| `Area` | 面 |
| `Scatter` | 散布 |
| `Heatmap` | ヒートマップ |

## スクリプトAPI

表示専用フィールド。共通スクリプトプロパティ（`Color` / `BackgroundColor` / `IsEnabled` / `IsVisible` / `IsViewOnly` 等）は [_FieldCommon.md](_FieldCommon.md)・[_ScriptApi.md](_ScriptApi.md) を参照。

## DOM構造（CSS用）

ApexCharts のチャートコンポーネントが描画される。フィールドのルートは `[data-name="<フィールド名>"]` で参照できる。チャート本体の SVG/要素は ApexCharts ライブラリが生成する。
