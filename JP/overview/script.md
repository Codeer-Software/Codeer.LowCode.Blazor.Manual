# スクリプト

<img src="images/script_summary.png">

C#の基本的な構文で記述できます。イベントをトリガとして実行され、モジュールのメソッドとして追加されます。全てpublicとなり外部から呼び出すことも可能です。Moduleやその中で使っているFieldを操作できます。プロコードから利用可能なクラスを設定できるのでスクリプトからプロコードで実装した機能を呼び出すことが可能です。

## 利用可能文法

- if, else, elseif
- 三項演算子
- switch
- for, foreach
- using
- break, continue
- return

## 利用可能な型

### プリミティブな型

- bool
- byte
- char
- ushort
- short
- uint
- int
- ulong
- long
- float
- decimal
- string

### その他使える.NETの型
- MemoryStream
- Math
- DateTime
- DateOnly
- TimeOnly
- TimeSpan
- DateTimeOffset
- Guid
- MidpointRounding

### Module, 各Field

- [module](../module/module.md)
- [field](../fields/field.md)

## メソッドの定義と呼び出し

メソッドは定義して呼び出すことができます。また定義したメソッドはpublicになっていて別のモジュールから呼び出すことも可能です。

```csharp
void Button_OnClick()
{
    // メソッドの定義と呼び出し
    var result = MyMethod(10, 100);
    
    // 別のModuleのメソッドを呼び出す
    var author = new Author();
    var info = author.GetInfo();
}
```

## ProCode連携
<a id="procode-line"></a>
.Netのクラスを追加することができます。IAppInfoServiceを実装するクラスでScriptRuntimeTypeManager GetScriptRuntimeTypeManager()で返すオブジェクトに追加できます。

### AddType

利用できるタイプを追加できます。追加されたタイプは利用可能なコンストラクタがあればnewで生成することができ、staticな操作があれば利用できるようになります。またenumも使えるようになります。コンストラクタ、メソッドは利用可能なタイプのもののみ使えます。例えば以下の例でMathクラスを登録していますがこれだけでは public static decimal Round(decimal d, MidpointRounding mode) は利用することができません。一緒にMidpointRoundingを登録しているのでRoundメソッドが利用可能になります。

※以下の例の型はライブラリ内でデフォルトで登録されています。

```csharp
scriptRuntimeTypeManager.AddType<MemoryStream>();
scriptRuntimeTypeManager.AddType(typeof(Math));
scriptRuntimeTypeManager.AddType<MidpointRounding>();
```

スクリプト例

```csharp
var result = Math.Round(1.23, MidpointRounding.AwayFromZero);
```

### AddService

サービスとしてインスタンスを登録できます。スクリプト中ではstaticメソッドのように利用できます。

※以下の例はテンプレートコード内で登録されています。

```csharp
scriptRuntimeTypeManager.AddService(new WebApiService(http, logger));
```

```csharp
var data = WebApiService.Get("/testapi").JsonObject;
WeatherForecastList.DeleteAllRows();
foreach(var e in data)
{
    var row = new WeatherForecast();
    row.Date.Value = e.Date;
    row.TemperatureC.Value = e.TemperatureC;
    row.TemperatureF.Value = e.TemperatureF;
    row.Summary.Value = e.Summary;
    WeatherForecastList.AddRow(row);
}    
```

### その他スクリプトから使えるAPI
- JsonObject

  | メソッド名             | 戻り値        | 説明              |
  |-------------------|------------|-----------------|
  | SerializeObject() | void       | objectをシリアライズする |
  | ToJsonObject()    | JsonObject | jsonObjectを返却する |
  | ToJsonString()    | string     | jsonを返却する       |
- Logger

  | メソッド名   | 戻り値  | 説明               |
  |---------|------|------------------|
  | Error() | void | errorレベルでログを出力する |
  | Warn()  | void | warnレベルでログを出力する  |
  | Log()   | void | infoレベルでログを出力する  |
- MessageBox

  | メソッド名  | 戻り値    | 説明             |
  |--------|--------|----------------|
  | Show() | string | メッセージボックスを表示する |
- NavigationService

  | メソッド名                | 戻り値                              | 説明                 |
  |----------------------|----------------------------------|--------------------|
  | GetModuleUrl()       | string                           | ModuleUrlを取得する     |
  | GetModuleDataUrl()   | string                           | ModuleDataUrlを取得する |
  | NavigateTo()         | void                             | 画面遷移する             |
  | ReplaceTo()          | void                             | 画面を入れ替える           |
  | GetQueryParameters() | Dictionary<string, List<string>> | クエリーパラメーターを取得する    |
  | Logout()             | Task                             | ログアウトする            |

- Resources

  | メソッド名             | 戻り値                 | 説明                |
  |-------------------|---------------------|-------------------|
  | GetMemoryStream() | Task<MemoryStream?> | MemoryStreamを取得する |

- ModuleSearcher

  | プロパティ名    | 型    | 説明       |
  |-----------|------|----------|
  | IsOrMatch | bool | OR検索かどうか |

  | メソッド名                   | 戻り値  | 説明        |
  |-------------------------|------|-----------|
  | AddEquals()             | void | 検索条件を追加する |
  | AddLessThan()           | void | 検索条件を追加する |
  | AddLessThanOrEqual()    | void | 検索条件を追加する |
  | AddGreaterThan()        | void | 検索条件を追加する |
  | AddGreaterThanOrEqual() | void | 検索条件を追加する |
  | AddLike()               | void | 検索条件を追加する |
  | AddConditions()         | void | 検索条件を追加する |
  | OrderBy()               | void | 検索条件を追加する |
  | OrderByDescending()     | void | 検索条件を追加する |
  | Execute()               | void | 検索を実行する   |
  | ExecuteWithLock()       | void | 検索を実行する   |
 
---
- ModuleLayoutType
- 
  | 区分     | 説明 |
  |--------|----|
  | None   | なし |
  | Detail | 詳細 |
  | List   | 一覧 |
  | Search | 検索 |
- MatchComparison

  | 区分                 | 説明     |
  |--------------------|--------|
  | Equal              | 同じ     |
  | NotEqual           | 同じでない  |
  | LessThan           | 未満     |
  | LessThanOrEqual    | 以下     |
  | GreaterThan        | より大きい  |
  | GreaterThanOrEqual | 以上     |
  | Like               | あいまい検索 |
  | Exists             | 存在する   |
  | NotExists          | 存在しない  |
 
- TransactionMode

  | 区分     | 説明 |
  |--------|----|
  | Insert | 登録 |
  | Update | 更新 |
  | Delete | 削除 |
 
- ResumeNotifyStateChangedInvoker
 
  | メソッド名            | 戻り値             | 説明           |
  |------------------|-----------------|--------------|
  | Dispose()        | void            | 破棄する         |
----
- Excel
 
  | メソッド名            | 戻り値             | 説明           |
  |------------------|-----------------|--------------|
  | CopyCells()      | void            | セルをコピーする     |
  | Dispose()        | void            | 破棄する         |
  | Download()       | Task<bool>      | ダウンロードする     |
  | DownloadPdf()    | Task<bool>      | pdfをダウンロードする |
  | FindCellByText() | ExcelCellIndex? | セルを検索する      |
  | Overwrite()      | Task            | 上書きする        |
  | SetCellValue()   | void            | セルの値を設定する    |
 
- WebApiResult

  | メソッド名        | 戻り値        | 説明              |
  |--------------|------------|-----------------|
  | JsonObject() | JsonObject | JsonObjectを取得する |
  | StatusCode() | int        | Statusコードを取得する  |

- Toaster
 
  | メソッド名     | 戻り値  | 説明           |
  |-----------|------|--------------|
  | Success() | void | 成功メッセージを表示する |
  | Error()   | void | 失敗メッセージを表示する |
  | Warn()    | void | 警告メッセージを表示する |
