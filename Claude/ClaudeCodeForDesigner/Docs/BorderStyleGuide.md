# BorderStyle ガイド

## カラム枠線の設定方法

カラムに枠線をつけるには `BorderStyle` プロパティを使用する。
`Border` プロパティ（"None"/"All"等）は**非推奨**。使用しないこと。

### BorderStyleDesign のプロパティ

```csharp
public double? Left { get; set; }      // 左線の太さ（null=なし）
public double? Top { get; set; }       // 上線の太さ
public double? Right { get; set; }     // 右線の太さ
public double? Bottom { get; set; }    // 下線の太さ
public string LeftColor { get; set; }  // 左線の色（例: "#d0d0d0"）
public string TopColor { get; set; }   // 上線の色
public string RightColor { get; set; } // 右線の色
public string BottomColor { get; set; }// 下線の色
```

**太さを指定しないと線は表示されない。** 色だけ設定しても不可。太さ（通常 `1`）と色の両方を設定すること。

### JSON例

```json
"BorderStyle": {
  "Left": 1,
  "Top": 1,
  "Right": 1,
  "Bottom": 1,
  "LeftColor": "#d0d0d0",
  "TopColor": "#d0d0d0",
  "RightColor": "#d0d0d0",
  "BottomColor": "#d0d0d0"
}
```

## 罫線の重複回避ルール（エクセル方式）

グリッド内のカラムに枠線を付ける場合、**隣接するセルと罫線が重複しないように**片側のみ設定する。

### パターン

```
┌───┬───┬───┐   Row0 Col0: Left+Top+Right+Bottom (4辺全て)
│   │   │   │   Row0 Col1: Top+Right+Bottom (Leftなし=左隣と共有)
├───┼───┼───┤   Row0 Col2: Top+Right+Bottom (同上)
│   │   │   │   Row1 Col0: Left+Right+Bottom (Topなし=上行と共有)
├───┼───┼───┤   Row1 Col1: Right+Bottom (左と上は隣セルと共有)
│   │   │   │   Row1 Col2: Right+Bottom (同上)
└───┴───┴───┘
```

### ルールまとめ

| 位置 | Left | Top | Right | Bottom |
|---|---|---|---|---|
| Row0 Col0（左上） | ○ | ○ | ○ | ○ |
| Row0 Col1以降（上辺） | - | ○ | ○ | ○ |
| Row1以降 Col0（左辺） | ○ | - | ○ | ○ |
| Row1以降 Col1以降（内側） | - | - | ○ | ○ |

### JSON例（2行×2列のグリッド）

```json
// Row0 Col0: 左上
"BorderStyle": { "Left": 1, "Top": 1, "Right": 1, "Bottom": 1, "LeftColor": "#d0d0d0", "TopColor": "#d0d0d0", "RightColor": "#d0d0d0", "BottomColor": "#d0d0d0" }

// Row0 Col1: 上辺（Leftなし）
"BorderStyle": { "Top": 1, "Right": 1, "Bottom": 1, "TopColor": "#d0d0d0", "RightColor": "#d0d0d0", "BottomColor": "#d0d0d0" }

// Row1 Col0: 左辺（Topなし）
"BorderStyle": { "Left": 1, "Right": 1, "Bottom": 1, "LeftColor": "#d0d0d0", "RightColor": "#d0d0d0", "BottomColor": "#d0d0d0" }

// Row1 Col1: 内側（Left/Topなし）
"BorderStyle": { "Right": 1, "Bottom": 1, "RightColor": "#d0d0d0", "BottomColor": "#d0d0d0" }
```

## 検索グリッドをカード表示にする

SearchLayoutのGridに枠（カード）をつけるには `IsBordered: true` を設定する。

```json
"SearchLayouts": {
  "": {
    "Layout": {
      "IsBordered": true,
      "UseBorderedShrinkWrap": true,
      "IsExpandable": true,
      "ExpanderLabel": "検索条件",
      "IsExpanderDefaultOpened": false,
      ...
    }
  }
}
```

- `IsBordered: true` → グリッド全体にカード風の枠が付く
- `IsExpandable: true` → 折りたたみ可能になる
- `ExpanderLabel` → 折りたたみ時のラベル

## 典型的な使い方：エクセル風情報テーブル

ラベル列（灰色背景）＋値列（白背景）を罫線で区切るパターン。

```
┌──────────┬──────────┬──────────┬──────────┐
│ 取引先    │ [値]     │ ファイル名│ [値]      │
├──────────┼──────────┼──────────┼──────────┤
│ 取込日時  │ [値]     │ 取込ユーザ│ [値]      │
└──────────┴──────────┴──────────┴──────────┘
```

- ラベル列: `BackgroundColor: "#f0f0f0"`
- 値列: `BackgroundColor: "#ffffff"`, `IsViewOnly: true`
- 罫線: 上記の重複回避ルールで `BorderStyle` を設定
- 行: `IsRowMarginRemoved: true` で行間の隙間を除去
- 全体を `GridLayoutDesign` でネストして配置
