# DateTimeField - 日時入力フィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.DateTimeFieldDesign`

日時データ（DateTime 型）を入力するフィールド。`<input type="datetime-local">` として動作する。UTC保存モードを備え、タイムゾーンを考慮した保存が可能。表示専用モードではフォーマット文字列で書式化して表示できる。

## C# クラス定義 (真実の源)

```csharp
public class DateTimeFieldDesign : DbValueFieldDesignBase
{
    public bool SaveAsUtc { get; set; }
    public string Format { get; set; } = string.Empty;
    public override string DbColumn { get; set; } = string.Empty;
    // 親階層から継承 (詳細は _FieldCommon.md)
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification, DisplayName, IsRequired, OnDataChanged, DbColumn, IsUpdateProtected, IsSimpleSearchParameter, OnSearchDataChanged）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | マッピング先のDB列名。snake_case推奨。 |
| `Format` | string | `""` | 表示専用モードでの日時表示フォーマット。.NET の日時書式文字列（例: `"yyyy/MM/dd HH:mm"`, `"yyyy年M月d日 H時m分"`）。空の場合はデフォルト形式。 |
| `SaveAsUtc` | bool | `false` | `true` でDBにUTC（協定世界時）として保存する。`false` でローカル時刻として保存する。 |

## JSON例

### 基本的な日時フィールド（更新日時）

```json
{
  "DbColumn": "updated_at",
  "Format": "yyyy/MM/dd HH:mm",
  "SaveAsUtc": false,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "更新日時",
  "IsRequired": false,
  "IgnoreModification": true,
  "OnDataChanged": "",
  "Name": "UpdatedAt",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateTimeFieldDesign"
}
```

### UTC保存の日時フィールド（作成日時）

```json
{
  "DbColumn": "created_at",
  "Format": "yyyy/MM/dd HH:mm:ss",
  "SaveAsUtc": true,
  "IsUpdateProtected": true,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "作成日時",
  "IsRequired": false,
  "IgnoreModification": true,
  "OnDataChanged": "",
  "Name": "CreatedAt",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateTimeFieldDesign"
}
```

### イベント開始日時（検索可能）

```json
{
  "DbColumn": "event_start_at",
  "Format": "yyyy/MM/dd HH:mm",
  "SaveAsUtc": false,
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "開始日時",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "EventStartAt",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.DateTimeFieldDesign"
}
```

## ランタイム動作

- **入力UI:** `<input type="datetime-local">` としてレンダリングされる。ブラウザネイティブの日時ピッカーが表示される。
- **SaveAsUtc:**
  - `true`: 入力された日時をUTCに変換してDBに保存する。DB読み込み時にはローカル時刻に変換して表示する。グローバルなアプリケーションやタイムゾーンをまたぐ運用に適している。
  - `false`: 入力された日時をそのままローカル時刻としてDBに保存する。単一タイムゾーンで運用する場合に適している。
- **Format:** 表示専用モード（閲覧時）で日時をフォーマットして表示する。
  - 例: `"yyyy/MM/dd HH:mm"` -> `2025/01/15 14:30`
  - 例: `"yyyy年M月d日 H時m分"` -> `2025年1月15日 14時30分`
  - 編集モードではブラウザの日時ピッカーが使用されるため、Format は適用されない。

## 検索

- 範囲検索（Range Search）をサポートする。DateField と同じパターンで、最小日時と最大日時の2つの入力欄が表示される。
- 検索条件として `GreaterThanOrEqual`（以上）と `LessThanOrEqual`（以下）の条件が生成される。
  - SearchMin のみ指定: `>= SearchMin` の条件。
  - SearchMax のみ指定: `<= SearchMax` の条件。
  - 両方指定: `>= SearchMin AND <= SearchMax` の範囲条件。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | DateTime? | 読み書き | 日時の値 |
| `SearchMin` | DateTime? | 読み書き | 検索時の最小日時 |
| `SearchMax` | DateTime? | 読み書き | 検索時の最大日時 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 編集モード

```html
<input type="datetime-local" class="form-control [is-invalid]" style="[インラインスタイル]" />
<div class="invalid-feedback">エラーメッセージ</div>
```

### 表示モード

```html
<span class="d-block py-2 text" style="[インラインスタイル]">フォーマット済み日時</span>
```

### CSSセレクタ例

```css
/* 日時入力の幅 */
[data-name="UpdatedAt"] .form-control {
  max-width: 250px;
}
```
