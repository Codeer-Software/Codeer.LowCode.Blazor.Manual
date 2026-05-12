# PasswordField - パスワード入力

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.PasswordFieldDesign`

パスワード入力フィールド。マスク表示でパスワードを入力し、確認用パスワードとの一致検証を行う。
`ValueFieldDesignBase` を継承する（`DbValueFieldDesignBase` ではないため `DbColumn` を持たない）。

## C# クラス定義 (真実の源)

```csharp
public class PasswordFieldDesign : ValueFieldDesignBase
{
    // 独自プロパティ無し
    // 親階層から継承: Name, IgnoreModification, OnValidateInput,
    //                DisplayName, IsRequired, OnDataChanged
}
```

## プロパティ

> 共通プロパティは [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DisplayName` | string | `""` | 表示ラベル名。 |
| `IsRequired` | bool | `false` | 入力必須かどうか。 |
| `OnDataChanged` | string | `""` | 値変更時のスクリプトイベント名。 |

PasswordFieldDesign 固有のプロパティはなく、`ValueFieldDesignBase` の共通プロパティのみを継承する。

## JSON例

```json
{
  "DisplayName": "パスワード",
  "IsRequired": true,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Password",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.PasswordFieldDesign"
}
```

## ランタイム動作

- `<input type="password">` としてレンダリングされ、入力値がマスク表示される。
- ランタイムに `ConfirmPassword` プロパティを持ち、パスワード確認入力に使用される。
- `ValidateInput()` でパスワードと確認パスワードの一致チェックが行われる。不一致の場合はバリデーションエラーが表示される。
- `ClearAsync()` でパスワードと確認パスワードの両方がクリアされる。
- DBカラムに直接マッピングされない。パスワードの保存はスクリプトや専用APIを通じて行う。

## 検索

検索には対応しない（`GetSearchWebComponentTypeFullName()` が空を返す）。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | string? | 読み書き | パスワードの値 |

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Clear()` | void | パスワードと確認入力をクリアする |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 編集モード

```html
<label class="form-label">パスワード</label>
<input type="password" class="form-control [is-invalid]" style="[インラインスタイル]" />
<label class="form-label">パスワード確認</label>
<input type="password" class="form-control [is-invalid]" style="[インラインスタイル]" />
<div class="invalid-feedback">エラーメッセージ</div>
```

### CSSセレクタ例

```css
/* パスワード入力のスタイル */
[data-name="Password"] .form-control {
  letter-spacing: 0.1em;
}

[data-name="Password"] .form-label {
  font-weight: 600;
}
```
