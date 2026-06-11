# SubmitButtonField - データ送信ボタンフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.SubmitButtonFieldDesign`

データ送信ボタン。クリック時に全フィールドのバリデーションを実行し、検証に成功した場合にデータベースへ保存する。カスタムスクリプトは持たず、常にSubmit処理を実行する。

## C# クラス定義 (真実の源)

```csharp
public class SubmitButtonFieldDesign : FieldDesignBase
{
    public string Text { get; set; } = "Submit";
    [Obsolete] public string ImageResourcePath { get; set; } = string.Empty;
    public ButtonImageSet ImageResourceSet { get; set; } = new();
    public string Icon { get; set; } = string.Empty;
    public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;   // enum: Primary/Secondary/...
    public bool IsBlock { get; set; } = true;
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Text` | string | `"Submit"` | ボタンのラベルテキスト。 |
| `Icon` | string | `""` | アイコン識別子。 |
| `Variant` | ButtonVariant | `"Primary"` | ボタンの外観スタイル。Bootstrap準拠。 |
| `IsBlock` | bool | `true` | `true` の場合、ボタンが親要素の全幅を占める（ブロックレベルボタン）。 |
| `ImageResourceSet` | ButtonImageSet | `new()` | ボタンの各状態に対応する画像リソース。`Default`, `Focus`, `Active`, `Hover`, `Disabled` を個別に指定可能。 |

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

### 基本的な送信ボタン（全幅）

```json
{
  "Text": "登録",
  "Icon": "",
  "Variant": "Primary",
  "IsBlock": true,
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "IgnoreModification": false,
  "Name": "SubmitButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SubmitButtonFieldDesign"
}
```

### コンパクトな更新ボタン

```json
{
  "Text": "更新",
  "Icon": "ContentSave",
  "Variant": "Success",
  "IsBlock": false,
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "IgnoreModification": false,
  "Name": "UpdateButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SubmitButtonFieldDesign"
}
```

## ランタイム動作

- クリック時に以下の処理が順番に実行される:
  1. モジュール内の全フィールドに対して `ValidateInput()` を呼び出す。
  2. バリデーションに失敗した場合、エラーメッセージを表示して処理を中断する。
  3. バリデーションに成功した場合、`Module.SubmitAsync()` を呼び出してデータをデータベースに保存する。
  4. 保存結果に応じて成功/失敗の通知を表示する。
- `ButtonFieldDesign` と異なり、`OnClick` スクリプトイベントは持たない。常にSubmit処理が実行される。
- データの永続化は行わない（ボタン自体はデータを持たない）。`CreateData()` は `null` を返す。
- `IsBlock = true` の場合、CSS の `width: 100%` が適用され、ボタンが親コンテナの全幅に広がる。

## 検索

検索には対応しない。DB列マッピングなし。

---

## DOM構造（CSS用）

### 通常

```html
<button class="btn btn-[Variant]" type="submit" style="[インラインスタイル]">
  <span class="[Iconクラス] me-2" aria-hidden="true"></span>
  テキスト
</button>
```

### ブロック表示（IsBlock = true）

```html
<div class="d-grid">
  <button class="btn btn-[Variant]" type="submit" style="[インラインスタイル]">
    <span class="[Iconクラス] me-2" aria-hidden="true"></span>
    テキスト
  </button>
</div>
```

### CSSセレクタ例

```css
/* 送信ボタンのスタイル */
[data-name="SubmitButton"] .btn {
  font-size: 1.1rem;
  padding: 0.75rem 2rem;
}

/* ブロック表示時のスタイル */
[data-name="SubmitButton"] .d-grid .btn {
  border-radius: 0;
}
```
