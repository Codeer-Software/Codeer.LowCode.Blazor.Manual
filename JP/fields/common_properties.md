# Field 共通プロパティ

すべて、または多くの Field に共通するプロパティをここにまとめます。
各 Field の個別ページでは、**固有プロパティだけ**を記載しています。共通部分を確認したいときはこのページを参照してください。

---

## デザイナでの共通プロパティ

### すべての Field に共通

| プロパティ | 型 | 既定値 | 説明 |
|---|---|---|---|
| **Name** | string | `""` | フィールド識別子。スクリプトや DB 列の参照で使う |
| **IgnoreModification** | bool | `false` | 変更検知（IsModified）から除外する |

### 値を持つ Field（ValueField 系）に共通

| プロパティ | 型 | 既定値 | 説明 |
|---|---|---|---|
| **DisplayName** | string | `""` | 画面表示用の名前（ラベル・ヘッダー）。空なら Name が使われる |
| **IsRequired** | bool | `false` | 入力必須にする |
| **OnDataChanged** | string | `""` | 値が変更された時のスクリプトイベント |

### DB 連動 Field（DbValueField 系）に共通

| プロパティ | 型 | 既定値 | 説明 |
|---|---|---|---|
| **DbColumn** | string | `""` | 対応する DB 列名 |
| **IsUpdateProtected** | bool | `false` | 更新時に値を変更できないようにする |

### 検索画面での共通プロパティ（Search Settings カテゴリ）

| プロパティ | 型 | 既定値 | 説明 |
|---|---|---|---|
| **IsSimpleSearchParameter** | bool | `false` | 簡易検索の対象にする |
| **AllowEmptySearch** | bool | `false` | 空での検索を許可する |
| **OnSearchDataChanged** | string | `""` | 検索条件が変更された時のスクリプトイベント |

---

## スクリプトでの共通プロパティ・メソッド

### すべての Field で使える

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `ClassName` | string? | CSS クラス名を取得・設定 |
| `IsValid` | bool | バリデーション結果 |
| `ErrorText` | string | エラーメッセージ |
| `SetError(string)` | void | エラーを設定 |
| `ClearError()` | void | エラーをクリア |
| `Color` | string? | 文字色 |
| `BackgroundColor` | string? | 背景色 |
| `FontFamily` | string? | フォント名 |
| `FontSize` | string? | フォントサイズ |
| `IsEnabled` | bool | 有効／無効 |
| `IsVisible` | bool | 表示／非表示 |
| `IsViewOnly` | bool | 編集可／読み取り専用 |
| `IsModified` | bool | 変更されたか |
| `IgnoreModification` | bool | 変更検知から除外 |
| `Focus()` | Task | フォーカスを当てる |
| `GetClientRect()` | Task<Rect> | 画面上の矩形を取得 |
| `NotifyStateChanged()` | void | 再描画を促す |

### 値を持つ Field で使える

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `Value` | T? | 値を取得 |
| `SetValueAsync(T?)` | Task | 値を設定（`Value = ...` と書いても同じ） |

### 範囲検索 Field (Number / Date / DateTime / Time) で使える

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `SearchMin` | T? | 検索の最小値 |
| `SearchMax` | T? | 検索の最大値 |
| `SearchIsEmpty` | bool? | 「空」を検索条件にするか |
| `SetSearchMinAsync(T?)` | Task | 最小値を設定 |
| `SetSearchMaxAsync(T?)` | Task | 最大値を設定 |
| `SetSearchIsEmptyAsync(bool?)` | Task | 空検索フラグを設定 |

### 単一値の検索 Field (Text / Boolean / Select / Id など) で使える

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `SearchValue` | T? | 検索値 |
| `SearchComparison` | MatchComparison | 比較方法（Equal, Like など） |
| `SearchIsEmpty` | bool? | 「空」を検索条件にするか |
| `SetSearchValueAsync(T?)` | Task | 検索値を設定 |
| `SetSearchComparisonAsync(MatchComparison)` | Task | 比較方法を設定 |
| `SetSearchIsEmptyAsync(bool?)` | Task | 空検索フラグを設定 |

---

## MatchComparison 列挙体

検索条件の比較方法。`SearchComparison` プロパティで使います。

| 値 | 意味 |
|---|---|
| `Equal` | 一致 |
| `NotEqual` | 不一致 |
| `LessThan` | 未満 |
| `LessThanOrEqual` | 以下 |
| `GreaterThan` | より大きい |
| `GreaterThanOrEqual` | 以上 |
| `Like` | あいまい検索 |
| `Exists` | 存在する |
| `NotExists` | 存在しない |

---

## 関連項目

- [Field 一覧](field.md)
- [スクリプト概要](../overview/script.md)
- [Module](../module/module.md)
