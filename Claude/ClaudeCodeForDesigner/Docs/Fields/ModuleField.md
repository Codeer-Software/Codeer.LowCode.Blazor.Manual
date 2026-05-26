# ModuleField - モジュール埋め込みフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.ModuleFieldDesign`

別モジュールをインラインで埋め込むフィールド。1:1 リレーションシップやネストされたフォームの実現に使用する。外部キー列（DbColumn）で埋め込みモジュールのIDと関連付け、DetailLayout で子モジュールを表示する。

## C# クラス定義 (真実の源)

```csharp
public class ModuleFieldDesign : FieldDesignBase, IUpdateProtected
{
    public string DbColumn { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string LayoutName { get; set; } = "";
    public bool IsUpdateProtected { get; set; }
    public string OnDataChanged { get; set; } = string.Empty;
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DbColumn` | string | `""` | 埋め込みモジュールのIDにリンクする外部キー列名。このカラムに埋め込みモジュールのレコードIDが格納される。 |
| `ModuleName` | string | `""` | 埋め込むモジュールの名前。 |
| `LayoutName` | string | `""` | 使用するDetailLayout名。空の場合はデフォルトレイアウト。`ModuleName` で指定したモジュールのDetailLayoutを参照する。 |
| `IsUpdateProtected` | bool | `false` | `true` にすると、レコード作成後は読み取り専用になる（埋め込みモジュールの変更不可）。 |
| `OnDataChanged` | string | `""` | 埋め込みモジュールのデータが変更された時に呼ばれるスクリプトイベント名。`.mod.cs` にメソッドを定義する。 |

## JSON例

### 基本的なモジュール埋め込み（1:1 リレーション）

```json
{
  "DbColumn": "address_id",
  "ModuleName": "Address",
  "LayoutName": "",
  "IsUpdateProtected": false,
  "OnDataChanged": "",
  "IgnoreModification": false,
  "Name": "Address",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ModuleFieldDesign"
}
```

### 読み取り保護付きの埋め込みモジュール

```json
{
  "DbColumn": "user_profile_id",
  "ModuleName": "UserProfile",
  "LayoutName": "CompactView",
  "IsUpdateProtected": true,
  "OnDataChanged": "Profile_OnDataChanged",
  "IgnoreModification": false,
  "Name": "Profile",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ModuleFieldDesign"
}
```

### DB列なしの埋め込み（UI用途のみ）

```json
{
  "DbColumn": "",
  "ModuleName": "SettingsForm",
  "LayoutName": "",
  "IsUpdateProtected": false,
  "OnDataChanged": "",
  "IgnoreModification": false,
  "Name": "Settings",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ModuleFieldDesign"
}
```

## ランタイム動作

- 子モジュールのインスタンスを `ModuleLayoutType.Detail` として作成し、指定された `DetailLayout` でレンダリングする。
- `DbColumn` が指定されている場合、その列の値をIDとして子モジュールのデータをロードする。
- 子モジュールのデータが変更されると、`OnChildDataChangedAsync()` を通じて親モジュールに変更が伝播される。`OnDataChanged` スクリプトイベントが発火する。
- `ValidateInput()` は子モジュールに委譲される。子モジュールの全フィールドのバリデーションが実行される。
- `IsUpdateProtected = true` の場合、レコード作成後は埋め込みモジュールが読み取り専用になる。
- `CreateData()` は `ModuleFieldData` を返す。`ModuleFieldData.Id` に子モジュールのレコードIDが格納される。

## 検索

検索には対応しない。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `ChildModule` | Module | 読み取り | 子モジュールインスタンス |

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `SetModule(string moduleName, string layoutName)` | void | モジュールとレイアウトを動的に切り替える |

#### `SetModule` の制約

- **`Design.DbColumn` が空のフィールドでのみ使用可能** (= DB列を持たない、フロント側のみで使う ModuleField)。`DbColumn` が設定されているフィールドは DB に永続化されるため、SetModule で動的に差し替えるとデータ整合性が壊れるので例外: `SetModule on ModuleField '{0}' is not allowed: Design.DbColumn is set (DB-persisted).`
- **`Design.ModuleName` / `Design.LayoutName` が設定済みでも、`DbColumn` が空なら SetModule で上書き可能**。これらは「フロント側の初期値」扱いで、DB に書かれない以上は動的差し替えしても永続化整合性に影響しない (SetModule を呼んだ側の責任で UI 表示が期待と異なる可能性はある)。
- 現在の effective state (`ModuleName == 引数 moduleName && ModuleLayoutName == 引数 layoutName`) と完全一致するときは no-op で return (idempotent。`ChildModule` も再生成されず、編集中データが保持される)。

```csharp
// OK: DbColumn 空 → 任意のモジュールに切替可能
EmbeddedSlot.SetModule("SettingsForm", "");

// OK: DbColumn 空 + Design.ModuleName 設定済 → 上書き OK
EmbeddedSlot.SetModule("OtherForm", "CompactView");

// NG: DbColumn 設定済 → 例外
PersistedSlot.SetModule("AnyForm", ""); // throws LowCodeException
```

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [_FieldCommon.md](_FieldCommon.md) の「FieldDesignBase」、[_ScriptApi.md](_ScriptApi.md) を参照。

---

## DOM構造（CSS用）

### 子モジュール表示

```html
<div class="[DetailLayoutDesign.ClassName]">
  <!-- GridLayoutRenderer で子モジュールのレイアウトを描画 -->
  <div data-layout="grid">
    <!-- ... 子モジュールの GridLayoutDesign の構造 ... -->
  </div>
</div>
```

### CSSセレクタ例

```css
/* 埋め込みモジュール全体 */
[data-name="EmbeddedForm"] {
  border: 1px solid #dee2e6;
  border-radius: 0.25rem;
  padding: 1rem;
}
```
