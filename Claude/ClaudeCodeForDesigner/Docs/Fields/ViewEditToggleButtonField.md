# ViewEditToggleButtonField - 表示/編集切替ボタン

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.ViewEditToggleButtonFieldDesign`

フォームの表示専用モードと編集モードを切り替えるトグルボタンフィールド。親モジュールの `IsViewOnly` 状態を制御し、全フィールドの編集可否を一括で切り替える。`FieldDesignBase` を直接継承する。

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `ToEditText` | string | `"Edit"` | 編集モードに切り替える時のボタンラベル（表示モード中に表示される）。 |
| `ToViewText` | string | `"View"` | 表示モードに切り替える時のボタンラベル（編集モード中に表示される）。 |
| `ToEditIcon` | string | `""` | 編集モードに切り替える時のアイコン識別子。 |
| `ToViewIcon` | string | `""` | 表示モードに切り替える時のアイコン識別子。 |
| `Variant` | ButtonVariant | `"Primary"` | ボタンのスタイル。 |

## 列挙型

### ButtonVariant

| 値 | 説明 |
|---|---|
| `Primary` | 主要アクション（青） |
| `Secondary` | 副次アクション（灰） |
| `Success` | 成功（緑） |
| `Danger` | 危険/削除（赤） |
| `Warning` | 警告（黄） |
| `Info` | 情報（水色） |
| `Light` | 明るい背景 |
| `Dark` | 暗い背景 |
| `Link` | リンク風表示 |
| `Text` | テキストのみ |
| `OutlinePrimary` | 枠線のみ（青） |
| `OutlineSecondary` | 枠線のみ（灰） |
| `OutlineSuccess` | 枠線のみ（緑） |
| `OutlineDanger` | 枠線のみ（赤） |
| `OutlineWarning` | 枠線のみ（黄） |
| `OutlineInfo` | 枠線のみ（水色） |
| `OutlineLight` | 枠線のみ（明） |
| `OutlineDark` | 枠線のみ（暗） |

## JSON例

### 基本的な表示/編集切替ボタン

```json
{
  "ToEditText": "編集",
  "ToViewText": "表示",
  "ToEditIcon": "",
  "ToViewIcon": "",
  "Variant": "Primary",
  "IgnoreModification": false,
  "Name": "ViewEditToggle",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ViewEditToggleButtonFieldDesign"
}
```

### アイコン付き切替ボタン

```json
{
  "ToEditText": "編集モード",
  "ToViewText": "閲覧モード",
  "ToEditIcon": "Pencil",
  "ToViewIcon": "Eye",
  "Variant": "OutlinePrimary",
  "IgnoreModification": false,
  "Name": "ModeToggle",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ViewEditToggleButtonFieldDesign"
}
```

### シンプルなテキストリンク風

```json
{
  "ToEditText": "Edit",
  "ToViewText": "View",
  "ToEditIcon": "",
  "ToViewIcon": "",
  "Variant": "Link",
  "IgnoreModification": false,
  "Name": "EditToggle",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ViewEditToggleButtonFieldDesign"
}
```

## ランタイム動作

- **表示モード（IsViewOnly = true）:** 全フィールドが読み取り専用で表示される。ボタンには `ToEditText` と `ToEditIcon` が表示される。クリックすると編集モードに切り替わる。
- **編集モード（IsViewOnly = false）:** 編集可能なフィールドがインタラクティブに操作できる。ボタンには `ToViewText` と `ToViewIcon` が表示される。クリックすると表示モードに切り替わる。
- 親モジュールの `IsViewOnly` プロパティをトグルすることで、モジュール内の全フィールドの表示/編集状態を一括制御する。
- `Variant` により Bootstrap ベースのボタンスタイルが適用される。
- DB列マッピングなし。検索対象外。

## 重要な副作用: SubmitButton / CopyModuleButton の挙動

ViewEditToggleButton をモジュールに配置すると、初期化時 (`InitializeDataAsync`) に以下が自動的に実行される:

1. **`Module.IsViewOnly = true`** — モジュール全体が表示モードで開く
2. **同一モジュール内の全 `SubmitButton` が `IsVisible = false`** に設定される（表示モードでは不要なため）
3. **同一モジュール内の全 `CopyModuleButton` が `IsViewOnly = false`** に設定される（表示モードでもコピー操作は許可）

ボタンクリックで View ⇄ Edit を切り替えるたびに SubmitButton の `IsVisible` も同期される:

- **View → Edit:** SubmitButton が `IsVisible = true` に戻る
- **Edit → View:** SubmitButton が再度 `IsVisible = false` になる + `Module.ReloadAsync()` でデータが再読込される

### 設計上の注意点

- **ViewEditToggleButton は SubmitButton との併用が前提** — 片方だけ配置する設計では使わない
- **初期表示では Submit ボタンが見えない** — デザイナーやスクショ確認時に「Submit が消えた」と勘違いしやすい。ViewEditButton クリック → 編集モード → Submit 表示、の動線で確認する
- **表示専用モジュール (DbTable なし) では原則不要** — そもそも編集モードに意味がない

---

## DOM構造（CSS用）

### ボタン表示

```html
<button class="btn btn-[Variant]" style="[インラインスタイル]">
  <span class="[Iconクラス] me-2" aria-hidden="true"></span>
  テキスト
</button>
```

### CSSセレクタ例

```css
/* トグルボタンのスタイル */
[data-name="EditToggle"] .btn {
  min-width: 80px;
}
```
