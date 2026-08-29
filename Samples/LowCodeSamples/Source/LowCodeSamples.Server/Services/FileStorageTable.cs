using Codeer.LowCode.Blazor.Extras.Server.FileManagement;

namespace LowCodeSamples.Server.Services
{
    /// <summary>
    /// appsettings の保存先設定 → IFileStorage の対応表。保存先の種類ごとに独立したセクションを読む
    /// (メールの MailSenderTable と同じ考え方)。独自の保存先を足すときは IFileStorage を実装してここに追加する。
    /// </summary>
    public static class FileStorageTable
    {
        public static List<IFileStorage> Create(IConfiguration config)
        {
            var list = new List<IFileStorage>();
            foreach (var e in config.GetSection("FileSystemStorages").Get<FileSystemStorageSettings[]>() ?? [])
            {
                list.Add(new FileSystemFileStorage(e));
            }
            //Azure Blob: 接続文字列 (ConnectionStrings:<Name> にも置ける) か、無ければ BlobServiceUri + DefaultAzureCredential (Managed Identity 等)
            foreach (var e in config.GetSection("AzureBlobStorages").Get<AzureBlobStorageSettings[]>() ?? [])
            {
                if (string.IsNullOrEmpty(e.ConnectionString) && string.IsNullOrEmpty(e.BlobServiceUri)) e.ConnectionString = config.GetConnectionString(e.Name) ?? string.Empty;
                list.Add(new AzureBlobFileStorage(e));
            }
            foreach (var e in config.GetSection("S3Storages").Get<S3StorageSettings[]>() ?? [])
            {
                list.Add(new S3FileStorage(e));
            }
            //簡易形式 (FileStorages: 種別と設定を1クラスに持つ。FileSystem / Azure Blob 接続文字列)
            foreach (var e in config.GetSection("FileStorages").Get<FileStorage[]>() ?? [])
            {
                //Azure の接続文字列は ConnectionStrings:<Name> にも置ける
                if (string.IsNullOrEmpty(e.ConnectionString)) e.ConnectionString = config.GetConnectionString(e.Name) ?? string.Empty;
                list.Add(e.ToFileStorage());
            }
            return list;
        }
    }
}
