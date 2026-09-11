using Codeer.LowCode.Blazor.Extras.Server.FileManagement;

namespace LowCodeNativeSamples.Server.Services
{
    /// <summary>
    /// SystemConfig の保存先設定 (種類ごと: FileSystemStorages / AzureBlobStorages / S3Storages と簡易形式 FileStorages) → IFileStorage の対応表
    /// (メールの MailSenderTable と同じ考え方)。独自の保存先を足すときは IFileStorage を実装してここに追加する。
    /// Azure / S3 のクライアントは毎回作らず、初回に組み立てた並びを使い回す。
    /// </summary>
    public static class FileStorageTable
    {
        static readonly Lazy<List<IFileStorage>> _storages = new(Create);

        /// <summary>組み立て済みの保存先 (TemporaryFileManager / StorageAccess に渡す)。</summary>
        public static List<IFileStorage> Storages => _storages.Value;

        static List<IFileStorage> Create()
        {
            var config = SystemConfig.Instance;
            var list = new List<IFileStorage>();
            foreach (var e in config.FileSystemStorages) list.Add(new FileSystemFileStorage(e));
            //Azure Blob: 接続文字列か、無ければ BlobServiceUri + DefaultAzureCredential (Managed Identity 等)
            foreach (var e in config.AzureBlobStorages) list.Add(new AzureBlobFileStorage(e));
            foreach (var e in config.S3Storages) list.Add(new S3FileStorage(e));
            //簡易形式 (FileSystem / Azure Blob 接続文字列)
            foreach (var e in config.FileStorages) list.Add(e.ToFileStorage());
            return list;
        }
    }
}
