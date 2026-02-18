namespace CodeerLowCodeBlazorTemplate.Server.Services.FileManagement
{
    public class FileStorage
    {
        public FileStorageType FileStorageType { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Directory { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
    }
}
