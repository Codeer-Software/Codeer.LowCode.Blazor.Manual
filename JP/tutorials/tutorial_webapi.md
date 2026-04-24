# チュートリアル: WebAPI 連携

**所要時間: 約 30 分**

外部サービスや独自サーバー API と連携するための方法を学びます。

- 標準の `WebApiService` で外部 API を呼ぶ
- サーバー側にカスタム WebAPI を追加する
- 戻り値（JSON）を画面に反映する

---

## 前提

- [はじめてのモジュール作成](../quickstart/first_module.md) を完了
- [スクリプトの基本](tutorial_script.md) を一読

---

## Part 1. 外部 API を呼び出す

スクリプトから直接 HTTP リクエストを飛ばせます。

### シナリオ: 郵便番号検索 API で住所を自動入力

```csharp
void PostalCode_OnDataChanged()
{
    if (string.IsNullOrEmpty(PostalCode.Value)) return;

    // 外部 API を呼ぶ
    var url = $"https://zipcloud.ibsnet.co.jp/api/search?zipcode={PostalCode.Value}";
    var result = WebApiService.Get(url);

    if (result.StatusCode == 200)
    {
        var data = result.JsonObject;
        var results = data["results"];
        if (results != null && results.Count > 0)
        {
            Address.Value = $"{results[0].address1}{results[0].address2}{results[0].address3}";
        }
    }
}
```

### WebApiService の API

| API | 用途 |
|---|---|
| `WebApiService.Get(url)` | GET リクエスト |
| `WebApiService.Post(url, body)` | POST リクエスト |
| 戻り値 `.StatusCode` | HTTP ステータス |
| 戻り値 `.JsonObject` | 応答 JSON |

---

## Part 2. サーバー側にカスタム WebAPI を追加する

Codeer.LowCode.Blazor のテンプレートは ASP.NET Core ベースなので、**普通に Controller を書けます**。

### Step 1. `.Server` プロジェクトに Controller を追加

`Controllers/CustomController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/custom")]
public class CustomController : ControllerBase
{
    [HttpGet("hello")]
    public IActionResult Hello([FromQuery] string name)
    {
        return Ok(new { message = $"Hello, {name}!" });
    }
}
```

### Step 2. Server を起動し直す

`.Server` プロジェクトを再ビルド・再起動します。デザイナの変更のみなら不要ですが、C# コードを変えたら**サーバー再起動が必要**です。

### Step 3. スクリプトから呼び出す

```csharp
void SayHelloButton_OnClick()
{
    var result = WebApiService.Get($"/api/custom/hello?name={Name.Value}");
    if (result.StatusCode == 200)
    {
        var message = result.JsonObject.message;
        await MessageBox.Show(message);
    }
    else
    {
        Toaster.Error("API 呼び出しに失敗しました");
    }
}
```

### 認証について

テンプレートに含まれる `ModuleDataController` と同様に、`[Authorize]` を付けると認証が必須になります。Cookie 認証などが自動的に効くので、フロントから呼ぶ分には特別な準備は不要です。

---

## Part 3. POST で JSON を送る

### サーバー側

```csharp
public record CreateItemRequest(string Name, int Quantity);

[HttpPost("items")]
public IActionResult CreateItem([FromBody] CreateItemRequest req)
{
    // DB 保存や業務ロジック
    return Ok(new { id = Guid.NewGuid(), name = req.Name });
}
```

### スクリプト側

```csharp
void CreateItemButton_OnClick()
{
    var body = new
    {
        Name = ItemName.Value,
        Quantity = Quantity.Value
    };

    var result = WebApiService.Post("/api/custom/items", body);

    if (result.StatusCode == 200)
    {
        Toaster.Success($"作成しました (Id: {result.JsonObject.id})");
    }
    else
    {
        Toaster.Error("作成に失敗しました");
    }
}
```

---

## Part 4. JsonObject の扱い方

### 動的アクセス

```csharp
var data = result.JsonObject;

// オブジェクトへのアクセス（動的型）
var name = data.user.name;
var age = data.user.age;

// 配列のイテレーション
foreach (var item in data.items)
{
    Logger.Log(item.title);
}
```

### JSON 文字列へのシリアライズ

```csharp
var json = someObject.ToJsonString();
Logger.Log(json);
```

### 既存オブジェクトに JSON を流し込む

```csharp
target.SerializeObject();
```

---

## Tips

### Q. CORS エラーが出る

外部 API を呼ぶ際、先方が CORS を許可していないと失敗します。
その場合は**サーバー側に API を追加して、サーバー経由で外部を呼ぶ**のが確実です。

### Q. 重い処理を非同期で動かしたい

サーバー側の Controller は `async Task<IActionResult>` にし、長時間処理は非同期にしましょう。
クライアントからの `WebApiService.Get/Post` もブロッキングしないので、UI は固まりません。

### Q. 認証トークンを付与したい

テンプレートでは Cookie 認証を使っているので、自動でトークン（AntiForgery Token）が付与されます。
カスタム認証ヘッダが必要な場合は、プロコードで `HttpService` を拡張してください。

---

## 次に読む

- [スクリプト概要](../overview/script.md)
- [プロコード概要](../overview/procode.md)
- [ユーザーコード](../user_code/user_code.md)
- [認証・認可](../authorization/authorization.md)
