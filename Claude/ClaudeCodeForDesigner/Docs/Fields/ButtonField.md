# ButtonField - アクションボタンフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.ButtonFieldDesign`

アクションボタン。クリック時にスクリプトイベントを実行する。データの永続化は行わず、UI操作専用のフィールド。

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Text` | string | `"Button"` | ボタンのラベルテキスト。複数行テキストに対応。 |
| `Icon` | string | `""` | アイコン識別子。 |
| `Variant` | ButtonVariant | `"Primary"` | ボタンの外観スタイル。Bootstrap準拠。 |
| `OnClick` | string | `""` | クリック時に呼ばれるスクリプトイベント名。`.mod.cs` にメソッドを定義する。 |
| `ImageResourceSet` | ButtonImageSet | `new()` | ボタンの各状態に対応する画像リソース。`Default`, `Focus`, `Active`, `Hover`, `Disabled` を個別に指定可能。 |
| `ShowTextInToolTip` | bool | `false` | `true` にすると、`Text` をボタンラベルではなくツールチップとして表示する。 |

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

### 基本的なアクションボタン

```json
{
  "Text": "保存",
  "Icon": "",
  "Variant": "Primary",
  "OnClick": "SaveButton_OnClick",
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "ShowTextInToolTip": false,
  "IgnoreModification": false,
  "Name": "SaveButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ButtonFieldDesign"
}
```

### アイコン付き削除ボタン

```json
{
  "Text": "削除",
  "Icon": "Delete",
  "Variant": "Danger",
  "OnClick": "DeleteButton_OnClick",
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "ShowTextInToolTip": false,
  "IgnoreModification": false,
  "Name": "DeleteButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ButtonFieldDesign"
}
```

### ツールチップ付きアイコンボタン

```json
{
  "Text": "データをエクスポート",
  "Icon": "Export",
  "Variant": "OutlineSecondary",
  "OnClick": "ExportButton_OnClick",
  "ImageResourceSet": { "Default": "", "Focus": "", "Active": "", "Hover": "", "Disabled": "" },
  "ShowTextInToolTip": true,
  "IgnoreModification": false,
  "Name": "ExportButton",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ButtonFieldDesign"
}
```

## ランタイム動作

- `<button>` 要素としてレンダリングされ、Bootstrap のスタイルクラスが `Variant` に応じて適用される。
- クリック時に `OnClick` で指定されたスクリプトイベントが実行される。スクリプトメソッドは `.mod.cs` ファイルに定義する。
- データの永続化は行わない。`CreateData()` は `null` を返す。
- `ShowTextInToolTip = true` の場合、`Text` はボタン上に表示されず、マウスホバー時のツールチップとして表示される。アイコンのみのボタンに有用。
- `ImageResourceSet` を指定すると、ボタンの各状態（通常・フォーカス・アクティブ・ホバー・無効）に応じた画像が表示される。

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `Text` | string | ボタンテキスト（読み書き可能） |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [Scripts.md](../Scripts.md) を参照。

### イベントハンドラ: OnClick

`OnClick` プロパティに指定したメソッド名が、ボタンクリック時に呼ばれる。引数なし。

```csharp
// mod.json: { "OnClick": "SaveButton_OnClick", "Name": "SaveButton" }
void SaveButton_OnClick()
{
    if (Name.Value == "")
    {
        MessageBox.Show("名前を入力してください");
        return;
    }
    this.Submit();
    Toaster.Success("保存しました");
}
```

```csharp
// ダイアログ表示
void ShowDialog_OnClick()
{
    var dlg = new PersonalInfoDialog();
    var result = dlg.ShowDialog("OK", "Cancel");
    if (result == "OK")
    {
        Name.Value = dlg.Name.Value;
        Email.Value = dlg.Email.Value;
    }
}
```

```csharp
// Excel出力
void ExportExcel_OnClick()
{
    var searchFile = new ModuleSearcher<TestFiles>();
    searchFile.AddEquals(e => e.Name.Value, "Template");
    var file = searchFile.Execute()[0];

    using (var memory = file.File.GetMemoryStream())
    using (var excel = new Excel(memory, file.File.FileName))
    {
        excel.OverWrite(this);
        excel.Download();
    }
}
```

## 検索

検索には対応しない。DB列マッピングなし。

---

## DOM構造（CSS用）

### テキストボタン

```html
<button class="btn btn-[Variant]" style="[インラインスタイル]">
  <span class="[Iconクラス] me-2" aria-hidden="true"></span>
  テキスト
</button>
```

### 画像ボタン（ImageResourcePath 設定時）

```html
<button class="btn p-0" style="[インラインスタイル]">
  <img src="[リソースパス]" />
</button>
```

### 画像セットボタン（ImageResourceSet 設定時）

```html
<button class="btn p-0 image-set-button" style="[インラインスタイル]">
  <img class="default" src="..." />
  <img class="focus" src="..." />
  <img class="hover" src="..." />
  <img class="active" src="..." />
  <img class="disabled" src="..." />
</button>
```

### CSSセレクタ例

```css
/* ボタンのカスタムスタイル */
[data-name="SaveButton"] .btn {
  min-width: 120px;
  border-radius: 2rem;
}

/* 画像ボタン */
[data-name="IconButton"] .btn img {
  width: 32px;
  height: 32px;
}
```
