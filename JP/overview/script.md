# スクリプト

Codeer.LowCode.Blazor のスクリプトは、**C# とほぼ同じ構文で記述できる軽量スクリプト**です。
デザイナで Field やイベントを選択し、そこにスクリプトを書くことで、画面の挙動を柔軟にカスタマイズできます。

<img src="images/script_summary.png">

- **どこに書くか**: 各 Field の `OnClick` / `OnDataChanged`、Module の `Scripts` など
- **いつ実行されるか**: イベントをトリガーに発火
- **メソッドの公開**: Module に定義したメソッドは **public** で、他モジュールから呼び出し可能
- **スコープ**: Field は名前でそのまま参照できる

学ぶ順番はこちら:

- [チュートリアル: スクリプトの基本](../tutorials/tutorial_script.md) — 手を動かして覚える
- [チュートリアル: モジュール連携](../tutorials/tutorial_modules.md)
- [スクリプトデバッガ](script_debugger.md) — ステップ実行の方法

---

## 利用可能な構文

C# 準拠の主要構文が使えます:

- if / else / else if / switch
- 三項演算子 / null 合体 / null 条件
- for / foreach / while
- break / continue / return
- using 文
- async / await
- ラムダ式 / LINQ メソッド（限定的）
- 文字列補間（`$"..."`）

> スクリプト言語の詳細な仕様・破壊的変更履歴は [破壊的変更](../breaking_changes/breaking_changes.md) を参照。

---

## 利用可能な型

### プリミティブ

`bool` / `byte` / `char` / `ushort` / `short` / `uint` / `int` / `ulong` / `long` / `float` / `decimal` / `string`

### 構造体・クラス

`MemoryStream` / `Math` / `DateTime` / `DateOnly` / `TimeOnly` / `TimeSpan` / `DateTimeOffset` / `Guid` / `MidpointRounding`

### Module・Field

- [Module](../module/module.md)
- [Field 共通プロパティ](../fields/common_properties.md)
- [各 Field のプロパティ・メソッド](../fields/field.md)

---

## メソッドの定義と呼び出し

Module 内にメソッドを定義して呼び出せます。定義したメソッドは **public** として外部公開されます。

```csharp
void Button_OnClick()
{
    // 同じ Module 内のメソッド呼び出し
    var result = MyMethod(10, 100);

    // 別 Module のメソッド呼び出し
    var author = new Author();
    var info = author.GetInfo();
}

int MyMethod(int a, int b)
{
    return a + b;
}
```

---

## 組み込みサービス

スクリプトから呼べる組み込みサービス一覧。

### Logger — デバッグログ

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Logger.Log(string)` | void | info レベル |
| `Logger.Warn(string)` | void | warning レベル |
| `Logger.Error(string)` | void | error レベル |

### MessageBox — モーダルダイアログ

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `MessageBox.Show(string)` | Task<string> | ダイアログを表示し、押されたボタンを返す |

### Toaster — トースト通知

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Toaster.Success(string)` | void | 成功通知 |
| `Toaster.Error(string)` | void | エラー通知 |
| `Toaster.Warn(string)` | void | 警告通知 |

### NavigationService — 画面遷移

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `NavigationService.GetModuleUrl()` | string | 現在のモジュール URL |
| `NavigationService.GetModuleDataUrl()` | string | 現在のモジュールデータ URL |
| `NavigationService.NavigateTo(url)` | void | 画面遷移 |
| `NavigationService.ReplaceTo(url)` | void | 履歴を置き換えて画面遷移 |
| `NavigationService.GetQueryParameters()` | Dictionary<string, List<string>> | クエリパラメータ取得 |
| `NavigationService.Logout()` | Task | ログアウト |

### Resources — リソースアクセス

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Resources.GetMemoryStream(path)` | Task<MemoryStream?> | リソースファイルを取得 |

### WebApiService — HTTP 呼び出し

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `WebApiService.Get(url)` | WebApiResult | GET リクエスト |
| `WebApiService.Post(url, body)` | WebApiResult | POST リクエスト |

`WebApiResult` は `.StatusCode`（int）と `.JsonObject`（JSON 応答）を持ちます。

### ModuleSearcher — 他モジュールの検索

```csharp
var searcher = new ModuleSearcher<Customer>();
searcher.AddEquals(c => c.Email.Value, "foo@example.com");
searcher.AddLike(c => c.Name.Value, "山田");
searcher.OrderBy(c => c.Id.Value);
var results = searcher.Execute();
```

| メソッド | 用途 |
|---|---|
| `AddEquals` / `AddLessThan` / `AddLessThanOrEqual` / `AddGreaterThan` / `AddGreaterThanOrEqual` | 比較条件 |
| `AddLike` | あいまい検索 |
| `AddConditions(other)` | 別 Searcher の条件をまとめて追加 |
| `OrderBy` / `OrderByDescending` | ソート |
| `Execute()` | 検索実行 |
| `ExecuteWithLock()` | 排他ロック付きで検索実行 |

プロパティ:

| 名前 | 型 | 説明 |
|---|---|---|
| `IsOrMatch` | bool | 条件を OR で結ぶか |

### Excel — 帳票出力

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `CopyCells(from, to)` | void | セル範囲をコピー |
| `FindCellByText(keyword)` | ExcelCellIndex? | キーワードでセルを検索 |
| `SetCellValue(cell, value)` | void | セルに値を設定 |
| `Overwrite()` | Task | 変更を保存 |
| `Download(fileName)` | Task<bool> | Excel でダウンロード |
| `DownloadPdf(fileName)` | Task<bool> | PDF に変換してダウンロード |
| `Dispose()` | void | リソース解放 |

### JsonObject — JSON 操作

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `SerializeObject()` | void | 対象にシリアライズ |
| `ToJsonObject()` | JsonObject | JsonObject 化 |
| `ToJsonString()` | string | JSON 文字列化 |

---

## 列挙体

### ModuleLayoutType

| 値 | 説明 |
|---|---|
| `None` | なし |
| `Detail` | 詳細 |
| `List` | 一覧 |
| `Search` | 検索 |

### MatchComparison

| 値 | 説明 |
|---|---|
| `Equal` | 一致 |
| `NotEqual` | 不一致 |
| `LessThan` | 未満 |
| `LessThanOrEqual` | 以下 |
| `GreaterThan` | より大きい |
| `GreaterThanOrEqual` | 以上 |
| `Like` | あいまい |
| `Exists` | 存在する |
| `NotExists` | 存在しない |

### TransactionMode

| 値 | 説明 |
|---|---|
| `Insert` | 登録 |
| `Update` | 更新 |
| `Delete` | 削除 |

---

## プロコード連携

<a id="procode-line"></a>

`.NET` のクラス・独自サービスをスクリプトから呼べるようにするには、ユーザーコードの `IAppInfoService` 実装で登録します。

### AddType — 型を追加

```csharp
scriptRuntimeTypeManager.AddType<MemoryStream>();
scriptRuntimeTypeManager.AddType(typeof(Math));
scriptRuntimeTypeManager.AddType<MidpointRounding>();
```

使用例:
```csharp
var result = Math.Round(1.23, MidpointRounding.AwayFromZero);
```

### AddService — サービスを追加

```csharp
scriptRuntimeTypeManager.AddService(new WebApiService(http, logger));
```

使用例:
```csharp
var data = WebApiService.Get("/testapi").JsonObject;
foreach(var e in data)
{
    var row = new WeatherForecast();
    row.Date.Value = e.Date;
    WeatherForecastList.AddRow(row);
}
```

---

## 関連項目

- [プロコード](procode.md) — C# / Blazor での拡張
- [スクリプトデバッガ](script_debugger.md)
- [チュートリアル: スクリプトの基本](../tutorials/tutorial_script.md)
- [チュートリアル: モジュール連携](../tutorials/tutorial_modules.md)
- [破壊的変更一覧](../breaking_changes/breaking_changes.md) — スクリプト仕様の変更履歴
