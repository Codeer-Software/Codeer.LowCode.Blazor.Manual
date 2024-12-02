# ScriptからWebAPIを使う

[デザイナ](../designer/designer.md)の[Script](../overview/script.md)からWebAPIを使うには、[プロコード](../overview/procode.md)でWeb APIを定義しラッピングしてから、Scriptからコールする流れになります。

この例では、デザイナ側でサーバー上の画像を表示するための手順を説明します。

## 1. プロコードでWeb APIを定義する
```Project名.Server```プロジェクト　→　Controllerフォルダの下で```ImageController```クラスを作成します。
```CSharp
    [ApiController]
    [Route("api/image")]
    public class ImageController : ControllerBase
    {
        //サーバー上画像の保存場所
        const string IMG_DISK_PATH_ON_SERVER = @"C:\Codeer.LowCode.Blazor.Local\Storages\img";

        [HttpGet("{imageName}")]
        public async Task<IActionResult> GetImage(string imageName)
        {
            var fullPath = Path.Combine(IMG_DISK_PATH_ON_SERVER, $"{imageName}.png");
            if (!System.IO.File.Exists(fullPath))
            {
                fullPath = Path.Combine(IMG_DISK_PATH_ON_SERVER, $"not_found.png");
            }

            var img = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(img, "image/png");
        }
    }
```

## 2. プロコードでWeb APIをラッピングする
```Project名.Client.Shared```プロジェクト　→　ScriptObjectsフォルダの下で```WebApiService```クラスを開きます。

WebApiServiceの中で定義されているpublicメソッドはデザイナのScriptからコールすることができます。

以下のメソッドを追加します。
```CSharp
        public async Task<string> GetImage(string imageName)
        {//Scriptからコール
            var response = await _http.GetAsync($"/api/image/{imageName}");
            if (response == null) return string.Empty;

            var image = await response.Content.ReadAsByteArrayAsync();
            return Convert.ToBase64String(image);
        }

```


## 3. デザイナのScriptからGetImage()をコールする
```CSharp
 var imageData = WebApiService.GetImage("my_image");
 //Getされた画像をImageViewerフィールドで表示する
 this.ImageViewer.SetBase64Data("my_image.png", imageData);
```
## 関連ページ
- [デザイナ](../designer/designer.md)
- [Script](../overview/script.md)
- [プロコード](../overview/procode.md)
- [ImageViewer Field](../fields/ImageViewer.md)
