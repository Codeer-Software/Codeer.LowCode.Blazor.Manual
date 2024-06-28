using Azure.Storage.Blobs;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;

namespace CustomLayoutSample.Server.Services.FileManagement
{
    public class StorageAccess
    {
        public static async Task<MemoryStream> ReadFileAsync(FileLocation file)
        {
            var storage = SystemConfig.Instance.FileStorages.FirstOrDefault(e => e.Name == file.StorageName);
            if (storage == null) throw LowCodeException.Create($"{file.StorageName} Invalid storage name");

            if (storage.FileStorageType == FileStorageType.AzureBlobStorage)
            {
                BlobContainerClient container = new(storage.ConnectionString, storage.ContainerName);
                var blobClient = container.GetBlobClient($"{file.Guid}");
                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                var bin = memoryStream.ToArray();
                memoryStream.Position = 0;
                return memoryStream;
            }
            else if (storage.FileStorageType == FileStorageType.FileSystem)
            {
                if (string.IsNullOrEmpty(storage.Directory)) throw LowCodeException.Create("invalid directory");
                var path = Path.Combine(storage.Directory, file.Guid.ToString());
                return new MemoryStream(await File.ReadAllBytesAsync(path));
            }

            throw LowCodeException.Create("invalid storage type");
        }

        public static async Task DeleteFiles(string storageName, Guid[] files)
        {
            var storage = SystemConfig.Instance.FileStorages.FirstOrDefault(e => e.Name == storageName);
            if (storage == null) throw LowCodeException.Create($"{storageName} Invalid storage name");

            foreach (var file in files)
            {
                try
                {
                    if (storage.FileStorageType == FileStorageType.AzureBlobStorage)
                    {
                        BlobContainerClient container = new(storage.ConnectionString, storage.ContainerName);
                        var blobClient = container.GetBlobClient($"{file}");
                        await blobClient.DeleteIfExistsAsync();
                    }
                    else if (storage.FileStorageType == FileStorageType.FileSystem)
                    {
                        if (string.IsNullOrEmpty(storage.Directory)) throw LowCodeException.Create("invalid directory");
                        var path = Path.Combine(storage.Directory, file.ToString());
                        File.Delete(path);
                    }
                }
                catch { }
            }
        }

        public static async Task WriteFile(string? storageName, Guid guid, MemoryStream memoryStream)
        {
            var storage = SystemConfig.Instance.FileStorages.FirstOrDefault(e => e.Name == storageName);
            if (storage == null) throw LowCodeException.Create($"{storageName} Invalid storage name");

            if (storage.FileStorageType == FileStorageType.AzureBlobStorage)
            {
                BlobContainerClient container = new(storage.ConnectionString, storage.ContainerName);
                var blobClient = container.GetBlobClient($"{guid}");
                await blobClient.UploadAsync(memoryStream, true);
            }
            else if (storage.FileStorageType == FileStorageType.FileSystem)
            {
                if (string.IsNullOrEmpty(storage.Directory)) throw LowCodeException.Create("invalid directory");
                Directory.CreateDirectory(storage.Directory);
                var path = Path.Combine(storage.Directory, guid.ToString());
                await File.WriteAllBytesAsync(path, memoryStream.ToArray());
            }
        }
    }
}
