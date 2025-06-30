using Codeer.LowCode.Blazor.SystemSettings;
using WebApp.Services;
using PdfSharp.Fonts;
using WebApp.Services.FileManagement;
using WebApp.Services.DataChangeHistory;
using Excel.Report.PDF;
using WebApp.Services.AI;

var builder = WebApplication.CreateBuilder(args);

GlobalFontSettings.FontResolver = new CustomFontResolver();
WebApp.Client.Shared.ScriptObjects.Excel.ConvertPdf = e => ExcelConverter.ConvertToPdf(e, 1);
WebApp.Client.Shared.ScriptObjects.MailService.SendEmailAsyncCore = MailService.SendEmailAsync;

SystemConfig.Instance.UseHotReload = builder.Configuration.GetSection("UseHotReload").Get<bool>();
SystemConfig.Instance.DataSources = builder.Configuration.GetSection("DataSources").Get<DataSource[]>() ?? [];
SystemConfig.Instance.FileStorages = builder.Configuration.GetSection("FileStorages").Get<FileStorage[]>() ?? [];
SystemConfig.Instance.DataChangeHistoryTableInfo = builder.Configuration.GetSection("DataChangeHistoryTableInfo").Get<DataChangeHistoryTableInfo[]>() ?? [];
SystemConfig.Instance.TemporaryFileTableInfo = builder.Configuration.GetSection("TemporaryFileTableInfo").Get<TemporaryFileTableInfo[]>() ?? [];
SystemConfig.Instance.DesignFileDirectory = builder.Configuration["DesignFileDirectory"] ?? string.Empty;
SystemConfig.Instance.FontFileDirectory = builder.Configuration["FontFileDirectory"] ?? string.Empty;
SystemConfig.Instance.MailSettings = builder.Configuration.GetSection("MailSettings").Get<MailSettings>() ?? new();
SystemConfig.Instance.AISettings = builder.Configuration.GetSection("AISettings").Get<AISettings>() ?? new();
SystemConfig.Instance.DataSources.ToList().ForEach(e => e.ConnectionString = builder.Configuration.GetConnectionString(e.Name) ?? string.Empty);
SystemConfig.Instance.FileStorages.ToList().ForEach(e => e.ConnectionString = builder.Configuration.GetConnectionString(e.Name) ?? string.Empty);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSharedServices();

if (SystemConfig.Instance.UseHotReload)
{
    builder.Services.AddSignalR();
    builder.Services.AddHostedService<FileWatcherService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

if (SystemConfig.Instance.UseHotReload)
{
    app.MapHub<HotReloadHub>("/hot_reload_hub");
}

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
