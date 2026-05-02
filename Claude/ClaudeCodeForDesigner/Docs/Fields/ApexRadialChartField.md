# ApexRadialChartField - 円形チャートフィールド（ドーナツ/円/極座標）

**TypeFullName:** `Codeer.LowCode.Bindings.ApexCharts.Designs.ApexRadialChartFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Bindings.Blazor-ApexCharts`（ApexCharts ベースのチャートコンポーネント）

データを検索条件（SearchCondition）で取得し、ドーナツチャート・円グラフ・極座標チャートとして表示する読み取り専用フィールド。DB列へのマッピングは持たない。1つの NumberField の値をカテゴリ別に円形で表示する。

棒グラフ・折れ線グラフ・散布図等には [ApexChartField.md](ApexChartField.md) を使用する。

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。
> ApexChartFieldDesignBase 共通プロパティ（SearchCondition, DisplayName, CategoryField, CategoryFormat, SeriesFractionDigits, ShowLegend）は [ApexChartField.md](ApexChartField.md) の「ApexChartFieldDesignBase 共通プロパティ」を参照。

### ApexRadialChartFieldDesign 固有プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `SeriesType` | SeriesType | `"Donut"` | チャートタイプ。`"Donut"` / `"Pie"` / `"PolarArea"`。 |
| `SeriesField` | string? | `null` | 表示するNumberFieldのフィールド名。`SearchCondition.ModuleName` のモジュール内のNumberFieldを指定する。 |

## 列挙型

### SeriesType（ApexRadialChartFieldDesign で使用可能な値）

| 値 | 説明 |
|---|---|
| `Donut` | ドーナツチャート（中央が空いた円グラフ） |
| `Pie` | 円グラフ（標準的なパイチャート） |
| `PolarArea` | 極座標エリアチャート |

### AnnotationAxis

スクリプトの ChartAnnotation で使用する。詳細は [ApexChartField.md](ApexChartField.md) を参照。

| 値 | 説明 |
|---|---|
| `X` | X軸（縦線のアノテーション） |
| `Y` | Y軸（横線のアノテーション） |

**注意事項:**
- `SeriesField` にはNumberFieldのフィールド名のみ指定可能。
- 1つの系列のみ表示する。複数系列が必要な場合は [ApexChartField](ApexChartField.md) を使用する。

## JSON例

### ドーナツチャート（カテゴリ別売上）

```json
{
  "SeriesType": "Donut",
  "SeriesField": "Amount",
  "SearchCondition": {
    "LimitCount": null,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "CategorySales",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "DisplayName": "カテゴリ別売上",
  "CategoryField": "CategoryName",
  "CategoryFormat": null,
  "SeriesFractionDigits": 0,
  "ShowLegend": true,
  "IgnoreModification": false,
  "Name": "CategoryChart",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexRadialChartFieldDesign"
}
```

### 円グラフ（部門別人数）

```json
{
  "SeriesType": "Pie",
  "SeriesField": "EmployeeCount",
  "SearchCondition": {
    "LimitCount": null,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Department",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "DisplayName": "部門別人数",
  "CategoryField": "DepartmentName",
  "CategoryFormat": null,
  "SeriesFractionDigits": 0,
  "ShowLegend": true,
  "IgnoreModification": false,
  "Name": "DeptChart",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexRadialChartFieldDesign"
}
```

### 親レコードに紐づくデータを円グラフ表示

```json
{
  "SeriesType": "Donut",
  "SeriesField": "Amount",
  "SearchCondition": {
    "LimitCount": null,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "OrderItem",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [
        {
          "SearchTargetVariable": "OrderId.Value",
          "Comparison": "Equal",
          "Variable": "Id.Value",
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
        }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "DisplayName": "注文内訳",
  "CategoryField": "ProductName",
  "CategoryFormat": null,
  "SeriesFractionDigits": 0,
  "ShowLegend": true,
  "IgnoreModification": false,
  "Name": "OrderBreakdown",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexRadialChartFieldDesign"
}
```

## ランタイム動作

- `SearchCondition` に基づいて対象モジュールからデータを取得し、ApexCharts ライブラリで円形チャートを描画する。
- **読み取り専用フィールド**。DB列へのマッピングは持たず、`CreateData()` は `null` を返す。`IsModified` は常に `false`。
- `CategoryField` で指定したフィールドの値が各セクション（パイの各スライス）のラベルになる。
- `SeriesField` で指定した NumberField の値が各セクションの大きさになる。
- 他フィールドの値変更（FieldVariableMatchCondition で参照しているフィールド）時に自動的にデータを再読み込みする。
- チャートの高さはコンテナ（レイアウトセル）の高さに合わせて 100% で描画される。**レイアウトの行高さ（GridRow）を十分に確保すること。**
- ツールバーとツールチップはデフォルトで無効。

## スクリプトAPI

> ApexChartField と共通のランタイムクラスを使用する。スクリプトAPIの詳細は [ApexChartField.md](ApexChartField.md) の「スクリプトAPI」セクションを参照。

主要なメソッド:
- `Reload()` - データ再読み込み
- `SetAdditionalCondition(ModuleSearcher)` - 追加検索条件
- `AddAnnotation(name, annotation)` / `RemoveAnnotation(name)` / `ClearAnnotation()` - アノテーション操作

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [Scripts.md](../Scripts.md) を参照。
