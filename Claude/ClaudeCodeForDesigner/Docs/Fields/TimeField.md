# TimeField - 時刻入力フィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.TimeFieldDesign`

時刻データ（TimeOnly 型）を入力するフィールド。`<input type="time">` として動作する。UTC保存モードを備え、タイムゾーンを考慮した保存が可能。

## プロパティ

> 共通プロパティ（Name, IgnoreModification, DisplayName, IsRequired, OnDataChanged, DbColumn, IsUpdateProtected, IsSimpleSearchParameter, OnSearchDataChanged）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | マッピング先のDB列名。snake_case推奨。 |
| `SaveAsUtc` | bool | `false` | `true` でDBにUTC（協定世界時）として保存する。`false` でローカル時刻として保存する。 |

## JSON例

### 基本的な時刻フィールド（開始時刻）

```json
{
  "DbColumn": "start_time",
  "SaveAsUtc": false,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "開始時刻",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "StartTime",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TimeFieldDesign"
}
```

### 終了時刻フィールド

```json
{
  "DbColumn": "end_time",
  "SaveAsUtc": false,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "終了時刻",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "EndTime",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TimeFieldDesign"
}
```

### UTC保存の時刻フィールド（グローバル対応）

```json
{
  "DbColumn": "notification_time",
  "SaveAsUtc": true,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "通知時刻",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "NotificationTime_OnDataChanged",
  "Name": "NotificationTime",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TimeFieldDesign"
}
```

## ランタイム動作

- **入力UI:** `<input type="time">` としてレンダリングされる。ブラウザネイティブの時刻ピッカーが表示される。
- **SaveAsUtc:**
  - `true`: 入力された時刻をUTCに変換してDBに保存する。DB読み込み時にはローカル時刻に変換して表示する。異なるタイムゾーンのユーザーが利用する場合に適している。
  - `false`: 入力された時刻をそのままローカル時刻としてDBに保存する。営業時間や定刻など、タイムゾーンに依存しない時刻を扱う場合に適している。
- **表示専用モード:** 時刻の値がそのまま表示される。

## 検索

- 範囲検索（Range Search）をサポートする。最小時刻と最大時刻の2つの入力欄が表示される。
- 検索条件として `GreaterThanOrEqual`（以上）と `LessThanOrEqual`（以下）の条件が生成される。
  - SearchMin のみ指定: `>= SearchMin` の条件。
  - SearchMax のみ指定: `<= SearchMax` の条件。
  - 両方指定: `>= SearchMin AND <= SearchMax` の範囲条件。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | TimeOnly? | 読み書き | 時刻の値 |
| `SearchMin` | TimeOnly? | 読み書き | 検索時の最小時刻 |
| `SearchMax` | TimeOnly? | 読み書き | 検索時の最大時刻 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 編集モード

```html
<input type="time" class="form-control [is-invalid]" style="[インラインスタイル]" />
<div class="invalid-feedback">エラーメッセージ</div>
```

### 表示モード

```html
<span class="d-block py-2 text" style="[インラインスタイル]">フォーマット済み時刻</span>
```

### CSSセレクタ例

```css
/* 時刻入力の幅 */
[data-name="StartTime"] .form-control {
  max-width: 150px;
}
```
