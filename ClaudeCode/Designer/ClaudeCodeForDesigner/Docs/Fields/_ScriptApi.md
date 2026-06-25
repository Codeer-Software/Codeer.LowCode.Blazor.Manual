# フィールド共通スクリプトAPI

スクリプト (`.mod.cs`) 内でフィールドに対して使用できるプロパティ・メソッドのリファレンス。

フィールドはアプリケーション側で独自に追加可能。追加されたフィールドも `FieldBase` を継承するため、ここに記載する共通APIが利用できる。各フィールド型固有のAPIは `Docs/Fields/` の各フィールドドキュメントを参照。

---

## 全フィールド共通（FieldBase）

すべてのフィールド型が持つプロパティ・メソッド。

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Color` | string | 読み書き | テキスト色（CSS色値） |
| `BackgroundColor` | string | 読み書き | 背景色（CSS色値） |
| `IsEnabled` | bool | 読み書き | 有効/無効。`false` でグレーアウト |
| `IsVisible` | bool | 読み書き | 表示/非表示 |
| `IsViewOnly` | bool | 読み書き | 読み取り専用モード |
| `IsValid` | bool | 読み取り | バリデーション結果 |
| `ErrorText` | string | 読み取り | バリデーションエラーメッセージ |
| `ClassName` | string | 読み書き | CSSクラス名 |
| `IgnoreModification` | bool | 読み書き | 変更追跡から除外するか |

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `SetError(string err)` | void | バリデーションエラーを設定 |
| `ClearError()` | void | バリデーションエラーをクリア |
| `Focus()` | void | フィールドにフォーカスを設定 |
| `HasFocus()` | bool | このフィールドが現在フォーカスを持っているか |
| `GetClientRect()` | Rect | フィールドのDOM位置・サイズを取得 |

### 使用例

```csharp
// 表示制御
Name.IsEnabled = false;
Name.IsVisible = true;
Name.IsViewOnly = true;

// スタイル制御
Name.Color = "#ff0000";
Name.BackgroundColor = "#ffffcc";
Name.ClassName = "highlight";

// エラー制御
if (Name.Value == "")
{
    Name.SetError("名前は必須です");
}
else
{
    Name.ClearError();
}

// フォーカス
Name.Focus();

// バリデーション結果の確認
if (!Name.IsValid)
{
    Logger.Warn("エラー: " + Name.ErrorText);
}
```

---

## モジュール（this）のスクリプトAPI

`this` キーワードで現在のモジュール（Module）のプロパティ・メソッドにアクセスできる。

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `IsNewData` | bool | 読み取り | 新規レコード（未保存）かどうか |
| `IsModified` | bool | 読み取り | 未保存の変更があるか |
| `IsDeleted` | bool | 読み取り | 削除されたか |
| `IsEnabled` | bool | 読み書き | モジュール全体の有効/無効 |
| `IsViewOnly` | bool | 読み書き | モジュール全体の読み取り専用 |
| `PageTitle` | string | 読み書き | ページタイトル |
| `DialogTitle` | string | 読み書き | ダイアログタイトル |
| `ClassName` | string | 読み書き | CSSクラス名 |
| `Color` | string? | 読み書き | 文字色 |
| `BackgroundColor` | string? | 読み書き | 背景色 |

### データ操作メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Submit()` | bool? | 現在のデータをDB保存する。戻り値は `true`=保存成功 / `false`=バリデーション等で中断 / `null`=確認ダイアログでキャンセル。**必ず戻り値で成否分岐すること**（保存できていないのに後続処理を続けない） |
| `Submit(List<Module> simultaneousWriteModules)` | bool? | `this`（自モジュール）と引数リストの全モジュールを**1トランザクションで同時保存**する。戻り値は `Submit()` と同じ `bool?`。`this` も一緒に保存されるので、`this` を引数リストに入れる必要はない。失敗時は全ロールバック。他モジュールのレコードを生成・更新して一括保存するときに使う（後述の「他モジュールの生成・更新と同時保存」参照） |
| `Delete()` | void | レコードを削除する |
| `Reload()` | void | DBからデータを再読み込みする |
| `NewModule()` | void | 新規レコードモードにする |
| `CopyModule()` | void | 現在のレコードをコピーして新規作成モードにする |
| `ValidateInput()` | bool | 全フィールドのバリデーションを実行。`true`=有効 |
| `GetField(string name)` | FieldBase | フィールドを名前で取得 |
| `GetParentModule()` | Module? | 親モジュールを取得（子レコードの場合） |
| `GetParentField()` | FieldBase? | 親フィールドを取得 |

### ダイアログ表示メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `ShowDialog(params string[] buttons)` | string | モーダルダイアログを表示。選択されたボタンのテキストを返す |
| `ShowPopup(int x, int y, params string[] buttons)` | string | 指定座標にポップアップ表示 |
| `ShowPanel(params string[] buttons)` | string | パネルを表示 |
| `CloseDialog(string button)` | void | ダイアログを閉じる |
| `ClosePopup(string button)` | void | ポップアップを閉じる |
| `ClosePanel(string button)` | void | パネルを閉じる |

### JSON変換メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `SetJsonObject(JsonObject json)` | void | JSONオブジェクトからデータを設定 |
| `ToJsonObject()` | JsonObject | 現在のデータをJSONに変換 |

### 使用例

```csharp
// 保存前バリデーション
void SubmitButton_OnClick()
{
    if (!this.ValidateInput())
    {
        MessageBox.Show("入力エラーがあります");
        return;
    }
    this.Submit();
    MessageBox.Show("保存しました");
}

// 新規データの場合のデフォルト値設定
void Detail_OnAfterInit()
{
    if (this.IsNewData)
    {
        Status.Value = "Draft";
        CreatedDate.Value = DateOnly.FromDateTime(DateTime.Today);
    }
}

// 削除確認
void DeleteButton_OnClick()
{
    var result = MessageBox.Show("本当に削除しますか？", "はい", "いいえ");
    if (result == "はい")
    {
        this.Delete();
    }
}

// ダイアログ表示（別モジュールをダイアログで開く）
void OpenDialog_OnClick()
{
    var searcher = new ModuleSearcher<Product>();
    var products = searcher.Execute();
    if (products.Count > 0)
    {
        var product = products[0];
        var result = product.ShowDialog("選択", "キャンセル");
        if (result == "選択")
        {
            ProductName.Value = product.Name.Value;
        }
    }
}
```

---

## 値フィールド共通（ValueFieldBase）

`Value` プロパティを持つフィールドの共通API。FieldBase の全APIも継承。

### プロパティ

| プロパティ | 型 | 読み書き | 説明 |
|---|---|---|---|
| `Value` | T | 読み書き | フィールドの値。型はフィールドごとに異なる |
| `IsModified` | bool | 読み取り | 値が変更されたか（初期値から） |

### 使用例

```csharp
// 値の読み書き
Name.Value = "テスト";
var text = Name.Value;

// 変更チェック
if (Price.IsModified)
{
    Logger.Log("価格が変更されました");
}
```

---

各フィールド型固有のスクリプトAPIは、`Docs/Fields/` 内の各フィールドドキュメントを参照。

---

## フィールドの拡張

フィールド型はアプリケーション側で独自に追加できる（`FieldDesignBase` を継承した新しいフィールド型を作成し、リフレクションで自動検出される）。追加されたフィールドのスクリプトAPIは、`public` なプロパティ・メソッドが自動的にスクリプトからアクセス可能になる。

- `[ScriptHide]` 属性でスクリプトから非表示にできる
- `[ScriptName("Name")]` 属性でスクリプト内の名前を変更できる
- `[ScriptMethodToProperty("Name")]` 属性で非同期メソッドをプロパティとして公開できる

詳細は [ScriptExtensions.md](../ScriptExtensions.md) を参照。
