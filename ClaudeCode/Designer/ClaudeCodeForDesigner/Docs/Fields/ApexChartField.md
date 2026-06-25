# ApexChartField - チャート表示フィールド（棒/折れ線/面/散布/ヒートマップ）

**TypeFullName:** `Codeer.LowCode.Bindings.ApexCharts.Designs.ApexChartFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Bindings.Blazor-ApexCharts`（ApexCharts ベースのチャートコンポーネント）

データを検索条件（SearchCondition）で取得し、棒グラフ・折れ線グラフ・面グラフ・散布図・ヒートマップとして表示する読み取り専用フィールド。DB列へのマッピングは持たない（データは対象モジュールから動的に取得）。複数系列の表示や、系列ごとのチャートタイプ・色の設定に対応する。

円グラフ・ドーナツチャート等の円形系チャートには [ApexRadialChartField.md](ApexRadialChartField.md)、横棒に特化したものは [ApexHBarChartField.md](ApexHBarChartField.md) を使用する。

> **ダッシュボードの組み方は [../AppPatterns/visualization_dashboard.md](../AppPatterns/visualization_dashboard.md)（集計クエリ＋チャートの二層）を読む。**
> - **`CategoryField` / `Series[].Name` が参照するのは、`SearchCondition.ModuleName` のモジュールの「フィールドの `Name`」**（DB列名ではない）。
> - 月別推移・件数・率などの集計は、生テーブルでなく **GROUP BY した QueryField モジュール**をデータ元にする。
> - 高さはチャートを置く `GridRow` に十分な高さを与える。円/ドーナツは系列配列でなく `SeriesField`（単数）を使う [ApexRadialChartField.md](ApexRadialChartField.md)。

## C# クラス定義 (真実の源)

このフィールドは外部ライブラリ `Codeer.LowCode.Bindings.Blazor-ApexCharts` で定義されているため、`Codeer.LowCode.Blazor` リポジトリ内に C# 定義は存在しない。プロパティ仕様は本ドキュメントの表とライブラリの NuGet パッケージのソースを参照。`FieldDesignBase` を継承するため、共通プロパティ (Name / IgnoreModification / OnValidateInput) を持つ。

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

### ApexChartFieldDesignBase 共通プロパティ

ApexChartField と ApexRadialChartField の両方が継承する共通プロパティ。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `SearchCondition` | SearchCondition | `new()` | データ取得条件。`ModuleName` にデータ元のモジュール名を指定する。 |
| `DisplayName` | string | `""` | チャートのタイトル表示名。 |
| `CategoryField` | string? | `null` | X軸（カテゴリ軸）に使用するフィールド名。`SearchCondition.ModuleName` のモジュール内のフィールドを指定する。TextField, NumberField, DateField, DateTimeField が使用可能。 |
| `CategoryFormat` | string? | `null` | カテゴリ値のフォーマット文字列。日付の場合 `"yyyy-MM-dd"` 等。 |
| `SeriesFractionDigits` | int | `2` | Y軸の値の小数桁数。**整数のみ（`2.0` は不可、`2` と書くこと）** |
| `ShowLegend` | bool | `true` | 凡例の表示/非表示。`true` でチャート下部に凡例を表示する。 |

### ApexChartFieldDesign 固有プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `SeriesType` | SeriesType | `"Bar"` | デフォルトのチャートタイプ。`"Bar"` / `"Line"` / `"Area"` / `"Scatter"` / `"Heatmap"`。Series 内の個別設定で上書き可能。 |
| `Series` | ChartSeries | `{ "Series": [] }` | 表示する系列の定義。各系列で対象フィールド名、チャートタイプ、色を指定する。 |
| `FullWidthBar` | bool | `false` | `true` で棒グラフのカラム幅を100%にする。棒グラフ間の隙間がなくなる。 |
| `ShowXAxisGrid` | bool | `false` | X軸のグリッド線を表示するか。 |
| `ShowYAxisGrid` | bool | `true` | Y軸のグリッド線を表示するか。 |

### Series（ChartSeries）の構造

`Series` プロパティは系列のリストを保持する。各系列は以下の構造を持つ。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Name` | string | `""` | 表示するNumberFieldのフィールド名。`SearchCondition.ModuleName` のモジュール内のNumberFieldを指定する。 |
| `Color` | string | `""` | 系列の色（HEX形式、例: `"#FF4560"`）。空の場合はデフォルトテーマ色が自動割り当てされる。デフォルトテーマ: `#008FFB`, `#00E396`, `#FEB019`, `#FF4560`, `#775DD0` |
| `Type` | SeriesType | `"Line"` | この系列のチャートタイプ。`"Bar"` / `"Line"` / `"Area"` / `"Scatter"` / `"Heatmap"`。 |

**注意事項:**
- Heatmap 系列と非 Heatmap 系列を混在させることはできない。
- 系列の `Name` には `SearchCondition.ModuleName` で指定したモジュール内の **NumberField** のフィールド名のみ使用可能。

## 列挙型

### SeriesType（ApexChartFieldDesign で使用可能な値）

| 値 | 説明 |
|---|---|
| `Bar` | 棒グラフ |
| `Line` | 折れ線グラフ |
| `Area` | 面グラフ |
| `Scatter` | 散布図 |
| `Heatmap` | ヒートマップ |

### AnnotationAxis

スクリプトの ChartAnnotation で使用する。

| 値 | 説明 |
|---|---|
| `X` | X軸（縦線のアノテーション） |
| `Y` | Y軸（横線のアノテーション） |

## JSON例

### 基本的な棒グラフ（月別売上）

```json
{
  "SeriesType": "Bar",
  "Series": {
    "Series": [
      {
        "Name": "Amount",
        "Color": "",
        "Type": "Bar"
      }
    ]
  },
  "FullWidthBar": false,
  "ShowXAxisGrid": false,
  "ShowYAxisGrid": true,
  "SearchCondition": {
    "LimitCount": null,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "MonthlySales",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "DisplayName": "月別売上",
  "CategoryField": "Month",
  "CategoryFormat": null,
  "SeriesFractionDigits": 0,
  "ShowLegend": false,
  "IgnoreModification": false,
  "Name": "SalesChart",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexChartFieldDesign"
}
```

### 複数系列の折れ線グラフ（売上と利益の推移）

```json
{
  "SeriesType": "Line",
  "Series": {
    "Series": [
      {
        "Name": "Revenue",
        "Color": "#008FFB",
        "Type": "Line"
      },
      {
        "Name": "Profit",
        "Color": "#00E396",
        "Type": "Line"
      }
    ]
  },
  "FullWidthBar": false,
  "ShowXAxisGrid": false,
  "ShowYAxisGrid": true,
  "SearchCondition": {
    "LimitCount": null,
    "SelectFields": [],
    "SortConditions": [
      {
        "Variable": "YearMonth.Value",
        "IsDescending": false
      }
    ],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "MonthlyReport",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "DisplayName": "売上・利益推移",
  "CategoryField": "YearMonth",
  "CategoryFormat": "yyyy/MM",
  "SeriesFractionDigits": 0,
  "ShowLegend": true,
  "IgnoreModification": false,
  "Name": "RevenueChart",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexChartFieldDesign"
}
```

### 棒グラフと折れ線の複合チャート（売上数量と単価）

```json
{
  "SeriesType": "Bar",
  "Series": {
    "Series": [
      {
        "Name": "Quantity",
        "Color": "#008FFB",
        "Type": "Bar"
      },
      {
        "Name": "UnitPrice",
        "Color": "#FF4560",
        "Type": "Line"
      }
    ]
  },
  "FullWidthBar": false,
  "ShowXAxisGrid": true,
  "ShowYAxisGrid": true,
  "SearchCondition": {
    "LimitCount": null,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "ProductSales",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "DisplayName": "商品別売上分析",
  "CategoryField": "ProductName",
  "CategoryFormat": null,
  "SeriesFractionDigits": 2,
  "ShowLegend": true,
  "IgnoreModification": false,
  "Name": "ProductChart",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexChartFieldDesign"
}
```

### 親レコードに紐づくデータをチャート表示

```json
{
  "SeriesType": "Area",
  "Series": {
    "Series": [
      {
        "Name": "Score",
        "Color": "#775DD0",
        "Type": "Area"
      }
    ]
  },
  "FullWidthBar": false,
  "ShowXAxisGrid": false,
  "ShowYAxisGrid": true,
  "SearchCondition": {
    "LimitCount": null,
    "SelectFields": [],
    "SortConditions": [
      {
        "Variable": "TestDate.Value",
        "IsDescending": false
      }
    ],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "TestResult",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [
        {
          "SearchTargetVariable": "StudentId.Value",
          "Comparison": "Equal",
          "Variable": "Id.Value",
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
        }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "DisplayName": "テスト成績推移",
  "CategoryField": "TestDate",
  "CategoryFormat": "yyyy/MM/dd",
  "SeriesFractionDigits": 1,
  "ShowLegend": false,
  "IgnoreModification": false,
  "Name": "ScoreChart",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexChartFieldDesign"
}
```

## ランタイム動作

- `SearchCondition` に基づいて対象モジュールからデータを取得し、ApexCharts ライブラリでチャートを描画する。
- **読み取り専用フィールド**。DB列へのマッピングは持たず、`CreateData()` は `null` を返す。`IsModified` は常に `false`。
- `CategoryField` で指定したフィールドの値が X 軸のカテゴリラベルになる。
- `Series` の各 `Name` で指定した NumberField の値が Y 軸の値になる。
- 同一カテゴリ値のデータは自動集計（Sum）される（Scatter タイプを除く）。
- Scatter タイプは個々のデータポイントをそのまま表示する。
- `Series` の各系列で異なる `Type` を指定することで、棒グラフと折れ線の複合チャート等を作成できる。
- 他フィールドの値変更（FieldVariableMatchCondition で参照しているフィールド）時に自動的にデータを再読み込みする。
- チャートの高さはコンテナ（レイアウトセル）の高さに合わせて 100% で描画される。**レイアウトの行高さ（GridRow）を十分に確保すること。**
- ツールバーとツールチップはデフォルトで無効。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `AllowLoad` | bool | 読み書き | データ読み込みを許可するか。`false` で読み込みを抑止。 |

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Reload()` | void | データを再読み込みする。 |
| `SetAdditionalCondition(ModuleSearcher searcher)` | void | 追加の検索条件をマージする。動的フィルタリングに使用。 |
| `AddAnnotation(string name, ChartAnnotation annotation)` | void | チャートにアノテーション（基準線）を追加する。`name` は一意の識別子。 |
| `RemoveAnnotation(string name)` | void | 指定名のアノテーションを削除する。 |
| `ClearAnnotation()` | void | 全アノテーションを削除する。 |

### ChartAnnotation オブジェクト

スクリプトからアノテーションを追加する際に使用する。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Axis` | AnnotationAxis | - | `AnnotationAxis.X`（縦線）または `AnnotationAxis.Y`（横線） |
| `Value` | object | `0` | 線を描画する位置の値 |
| `Color` | string | `"#00E396"` | 線の色（HEX形式） |
| `Label` | string? | `null` | 線に付けるラベルテキスト。`null` でラベルなし。 |
| `IsDashed` | bool | `false` | `true` で破線、`false` で実線。 |

### イベントハンドラ例

```csharp
// 動的フィルタリング: カテゴリ選択に応じてチャートデータを絞り込む
void Category_OnDataChanged()
{
    if (Category.Value != "")
    {
        var searcher = new ModuleSearcher<MonthlySales>();
        searcher.AddEquals(e => e.Category.Value, Category.Value);
        SalesChart.SetAdditionalCondition(searcher);
    }
    SalesChart.Reload();
}

// アノテーション: 目標値の基準線を追加
void OnAfterInitialization()
{
    var annotation = new ChartAnnotation();
    annotation.Axis = AnnotationAxis.Y;
    annotation.Value = 1000000;
    annotation.Color = "#FF4560";
    annotation.Label = "目標: 100万";
    annotation.IsDashed = true;
    SalesChart.AddAnnotation("target", annotation);
}
```

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [Scripts.md](../Scripts.md) を参照。
