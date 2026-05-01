# JsonField - JSON構造化データフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.JsonFieldDesign`

構造化されたJSONデータをDB列に保存するフィールド。`JsonFieldSettings` でスキーマを定義し、各ノードが入力フィールドとしてレンダリングされる。`DbValueFieldDesignBase` を継承する。

## プロパティ

> 共通プロパティ（Name, IgnoreModification, DisplayName, IsRequired, OnDataChanged, DbColumn, IsUpdateProtected, IsSimpleSearchParameter, OnSearchDataChanged）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | JSON文字列を保存するDB列名。TEXT型またはJSONB型推奨。snake_case推奨。 |
| `JsonFieldSettings` | JsonFieldSettings | `new()` | JSONスキーマ定義。ノードのリストを保持する。 |

### JsonFieldSettings の構造

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `JsonNodes` | List\<JsonFieldNodeSettings\> | `[]` | JSONプロパティノードの定義リスト。 |

### JsonFieldNodeSettings の構造

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Key` | string | `""` | JSONプロパティ名。 |
| `NodeType` | JsonNodeType | `"Text"` | ノードの型。 |

## 列挙型

### JsonNodeType

| 値 | 説明 |
|---|---|
| `Text` | テキスト入力 |
| `Number` | 数値入力 |
| `Boolean` | チェックボックス |

## JSON例

### 基本的なJSONフィールド（住所データ）

```json
{
  "DbColumn": "address_data",
  "JsonFieldSettings": {
    "JsonNodes": [
      { "Key": "postalCode", "NodeType": "Text" },
      { "Key": "prefecture", "NodeType": "Text" },
      { "Key": "city", "NodeType": "Text" },
      { "Key": "street", "NodeType": "Text" }
    ]
  },
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "住所データ",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "AddressData",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.JsonFieldDesign"
}
```

### 複合型のJSONフィールド（設定データ）

```json
{
  "DbColumn": "settings",
  "JsonFieldSettings": {
    "JsonNodes": [
      { "Key": "maxRetries", "NodeType": "Number" },
      { "Key": "enableNotification", "NodeType": "Boolean" },
      { "Key": "apiEndpoint", "NodeType": "Text" },
      { "Key": "timeout", "NodeType": "Number" }
    ]
  },
  "IsUpdateProtected": false,
  "IsSimpleSearchParameter": false,
  "OnSearchDataChanged": "",
  "DisplayName": "設定",
  "IsRequired": false,
  "IgnoreModification": false,
  "OnDataChanged": "",
  "Name": "Settings",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.JsonFieldDesign"
}
```

### DBに保存されるJSON文字列の例

```json
{
  "postalCode": "100-0001",
  "prefecture": "東京都",
  "city": "千代田区",
  "street": "千代田1-1"
}
```

## ランタイム動作

- `JsonFieldSettings` で定義されたスキーマに基づいて、各 `JsonNode` が個別の入力フィールドとしてレンダリングされる。
  - `Text` ノード: テキスト入力。
  - `Number` ノード: 数値入力。
  - `Boolean` ノード: チェックボックス。
- 入力値はJSON文字列としてシリアライズされ、DB列に保存される。
- DB列は TEXT 型または JSONB 型を使用する。
- 検索対象外（`GetSearchWebComponentTypeFullName` が空）。

---

## DOM構造（CSS用）

### 編集モード

```html
<!-- 単一行 -->
<input type="text" class="form-control [is-invalid]" style="[インラインスタイル]" />

<!-- 複数行 -->
<textarea class="form-control [is-invalid]" rows="..." style="[インラインスタイル]"></textarea>

<div class="invalid-feedback">エラーメッセージ</div>
```

### 表示モード

```html
<span class="d-block py-2 text" style="[インラインスタイル]">JSON文字列</span>
```

### CSSセレクタ例

```css
/* JSON入力に等幅フォント */
[data-name="Metadata"] .form-control {
  font-family: monospace;
  font-size: 0.875rem;
}
```
