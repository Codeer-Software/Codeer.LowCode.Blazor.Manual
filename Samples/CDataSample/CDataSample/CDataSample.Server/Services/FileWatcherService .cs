using Microsoft.AspNetCore.SignalR;

namespace CDataSample.Server.Services
{
    public class FileWatcherService : IHostedService
    {
        readonly IHubContext<HotReloadHub> _hubContext;
        FileSystemWatcher? _fileWatcher;

        public FileWatcherService(IHubContext<HotReloadHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(SystemConfig.Instance.DesignFileDirectory);
            _fileWatcher = new FileSystemWatcher
            {
                Path = SystemConfig.Instance.DesignFileDirectory,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                Filter = "*.zip"
            };
            _fileWatcher.Changed += OnChanged;
            _fileWatcher.EnableRaisingEvents = true;

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _fileWatcher?.Dispose();
            await Task.CompletedTask;
        }

        async void OnChanged(object sender, FileSystemEventArgs e)
            => await _hubContext.Clients.All.SendAsync("ExecuteHotReload");
    }
}
