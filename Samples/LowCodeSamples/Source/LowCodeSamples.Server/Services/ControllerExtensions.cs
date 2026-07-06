using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;

namespace LowCodeSamples.Server.Services
{
    static class ControllerExtensions
    {
        //内容ハッシュのETag付きでファイルを返す。ブラウザは再訪時にIf-None-Matchで再検証し、
        //内容が変わっていなければ304でダウンロードを省略できる(FileがIf-None-Matchを自動処理する)。
        internal static IActionResult FileWithETag(this ControllerBase controller, byte[] content, string contentType)
        {
            controller.Response.Headers.CacheControl = "no-cache";
            var etag = new EntityTagHeaderValue($"\"{Convert.ToHexString(SHA256.HashData(content))}\"");
            return controller.File(content, contentType, lastModified: null, entityTag: etag);
        }
    }
}
