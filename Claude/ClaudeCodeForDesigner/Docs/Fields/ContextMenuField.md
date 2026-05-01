# ContextMenuField - 右クリックコンテキストメニュー

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.ContextMenuFieldDesign`

右クリック時に表示されるコンテキストメニューを定義するフィールド。他のフィールドのレイアウトで `ContextMenu` プロパティにこのフィールド名を指定することで、対象フィールド上での右クリックメニューとして機能する。`FieldDesignBase` を直接継承する。

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Items` | List\<string\> | `[]` | メニュー項目のラベルリスト。各文字列が1つのメニューエントリとして表示される。 |
| `OnClick` | string | `""` | メニュー項目クリック時に呼ばれるスクリプトイベント名。`.mod.cs` にメソッドを定義する。 |

## JSON例

### 基本的なコンテキストメニュー

```json
{
  "Items": ["編集", "コピー", "削除"],
  "OnClick": "RecordContextMenu_OnClick",
  "IgnoreModification": false,
  "Name": "RecordContextMenu",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ContextMenuFieldDesign"
}
```

### レイアウトでの関連付け（FieldLayoutDesign の ContextMenu に指定）

```json
{
  "FieldName": "Name",
  "ContextMenu": "RecordContextMenu",
  "ClassName": "",
  "FontFamily": "",
  "Color": "",
  "BackgroundColor": "",
  "Name": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
}
```

### スクリプト例（.mod.cs）

```csharp
void RecordContextMenu_OnClick(string item)
{
    if (item == "編集")
    {
        // 編集処理
    }
    else if (item == "コピー")
    {
        // コピー処理
    }
    else if (item == "削除")
    {
        // 削除処理
    }
}
```

## スクリプトAPI

### イベントハンドラ: OnClick

`OnClick` プロパティに指定したメソッド名が、メニュー項目クリック時に呼ばれる。**引数 `string item`** にクリックされた項目のテキストが渡される。

```csharp
// mod.json: { "OnClick": "RecordContextMenu_OnClick" }
void RecordContextMenu_OnClick(string item)
{
    if (item == "編集")
    {
        this.IsViewOnly = false;
    }
    else if (item == "コピー")
    {
        this.CopyModule();
    }
    else if (item == "削除")
    {
        var result = MessageBox.Show("削除しますか？", "はい", "いいえ");
        if (result == "はい")
        {
            this.Delete();
        }
    }
}
```

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [Scripts.md](../Scripts.md) を参照。

## ランタイム動作

- レイアウト内の `FieldLayoutDesign.ContextMenu` にこのフィールド名を指定すると、対象フィールド上での右クリックでメニューが表示される。
- `Items` で定義した文字列がメニューエントリとして表示される。
- メニュー項目をクリックすると `OnClick` イベントが発火する。
- **OnClick メソッドシグネチャ:** `void OnClick(string item)` - 引数 `item` にクリックされたメニュー項目のテキストが渡される。
- DB列マッピングなし。検索対象外。

---

## DOM構造（CSS用）

### コンテキストメニュー

```html
<div class="dropdown-menu show">
  <button class="dropdown-item" type="button">メニュー項目1</button>
  <button class="dropdown-item" type="button">メニュー項目2</button>
  <!-- ... -->
</div>
```

**注意:** コンテキストメニューは右クリック時にポップアップ表示される。`FieldLayoutDesign.ContextMenu` プロパティでフィールドに関連付ける。

### CSSセレクタ例

```css
/* メニュー項目のスタイル */
.dropdown-menu .dropdown-item {
  padding: 0.5rem 1rem;
}

.dropdown-menu .dropdown-item:hover {
  background-color: #f0f7ff;
}
```
