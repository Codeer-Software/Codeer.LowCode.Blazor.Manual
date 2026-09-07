using Codeer.LowCode.Bindings.ApexCharts;
using Codeer.LowCode.Bindings.Fluent.Blazor.Designs;
using Codeer.LowCode.Bindings.MudBlazor.Installer;
using Codeer.LowCode.Bindings.Radzen.Blazor.Installer;
using Codeer.LowCode.Blazor.DbAccess;
using Codeer.LowCode.Blazor.Extras;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.License;
using Codeer.LowCode.Blazor.SystemSettings;
using Microsoft.AspNetCore.ResponseCompression;
using PdfSharp.Fonts;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using LowCodeSamples.Client.Shared.Samples;
using LowCodeSamples.Server.Services;
using LowCodeSamples.Server.Services.DataChangeHistory;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Extras.Server.Mail;
using Codeer.LowCode.Blazor.Extras.Server.Excel;
using Codeer.LowCode.Blazor.Extras.Server.FileManagement;
using Codeer.LowCode.Blazor.Extras.Server.Web;
using Microsoft.AspNetCore.SignalR;

//load dll.
typeof(CodeBehindSample).ToString();
ApexChartsServerInitializer.Initialize();
ExtrasServerInitializer.Initialize();
//サンプル固有: UI ライブラリのバインディング
typeof(FluentTextFieldDesign).ToString();
typeof(Microsoft.FluentUI.AspNetCore.Components.Appearance).ToString();
MudBlazorLoader.LoadAssemblies();
RadzenLoader.LoadAssemblies();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

//日本語デモサイトのため、サーバーの既定カルチャを ja-JP に固定する。
//デザインの読込時に解決されるコード定義 enum (承認状態など) の表示名はサーバーのカルチャで決まり、
//Azure 上では既定が英語になるため (ブラウザの言語に関係なく "In Progress" と表示されてしまう)
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ja-JP");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ja-JP");

LicenseManager.DomainLicense = builder.Configuration.GetSection("DomainLicense").Get<string>() ?? string.Empty;
LicenseManager.IsAutoUpdate = builder.Configuration.GetSection("IsLicenseAutoUpdate").Get<bool>();
//デモサイトはデータ更新を受け付けない (CanUpdate=false)。ローカルで動かすときは appsettings.Development.json で true にする
SystemConfig.Instance.CanUpdate = builder.Configuration.GetSection("CanUpdate").Get<bool>();
SystemConfig.Instance.UseHotReload = builder.Configuration.GetSection("UseHotReload").Get<bool>();
SystemConfig.Instance.CanScriptDebug = builder.Configuration.GetSection("CanScriptDebug").Get<bool>();
SystemConfig.Instance.DataSources = builder.Configuration.GetSection("DataSources").Get<DataSource[]>() ?? [];
//ファイル保存先の設定 (種類ごとのセクション。実体は Services/FileStorageTable が組み立てる)
SystemConfig.Instance.FileSystemStorages = builder.Configuration.GetSection("FileSystemStorages").Get<FileSystemStorageSettings[]>() ?? [];
SystemConfig.Instance.AzureBlobStorages = builder.Configuration.GetSection("AzureBlobStorages").Get<AzureBlobStorageSettings[]>() ?? [];
SystemConfig.Instance.S3Storages = builder.Configuration.GetSection("S3Storages").Get<S3StorageSettings[]>() ?? [];
SystemConfig.Instance.FileStorages = builder.Configuration.GetSection("FileStorages").Get<FileStorage[]>() ?? [];
//Azure Blob の接続文字列は ConnectionStrings:<Name> にも置ける
foreach (var storage in SystemConfig.Instance.AzureBlobStorages) if (string.IsNullOrEmpty(storage.ConnectionString) && string.IsNullOrEmpty(storage.BlobServiceUri)) storage.ConnectionString = builder.Configuration.GetConnectionString(storage.Name) ?? string.Empty;
foreach (var storage in SystemConfig.Instance.FileStorages) if (string.IsNullOrEmpty(storage.ConnectionString)) storage.ConnectionString = builder.Configuration.GetConnectionString(storage.Name) ?? string.Empty;
SystemConfig.Instance.DataChangeHistoryTableInfo = builder.Configuration.GetSection("DataChangeHistoryTableInfo").Get<DataChangeHistoryTableInfo[]>() ?? [];
SystemConfig.Instance.TemporaryFileTableInfo = builder.Configuration.GetSection("TemporaryFileTableInfo").Get<TemporaryFileTableInfo[]>() ?? [];
SystemConfig.Instance.DesignFileDirectory = builder.Configuration["DesignFileDirectory"] ?? string.Empty;
SystemConfig.Instance.FontFileDirectory = builder.Configuration["FontFileDirectory"] ?? string.Empty;
SystemConfig.Instance.Mail = builder.Configuration.GetSection("Mail").Get<MailConfig>() ?? new();
//メールのプロバイダ設定はそれぞれ独立したセクション (使うものだけ書けばよい)
SystemConfig.Instance.Smtp = builder.Configuration.GetSection("Smtp").Get<SmtpSettings>() ?? new();
SystemConfig.Instance.GraphApi = builder.Configuration.GetSection("GraphApi").Get<GraphApiSettings>() ?? new();
SystemConfig.Instance.SendGrid = builder.Configuration.GetSection("SendGrid").Get<SendGridSettings>() ?? new();
SystemConfig.Instance.Gmail = builder.Configuration.GetSection("Gmail").Get<GmailSettings>() ?? new();
SystemConfig.Instance.AISettings = builder.Configuration.GetSection("AISettings").Get<AISettings>() ?? new();
SystemConfig.Instance.DataSources.ToList().ForEach(e => e.ConnectionString = builder.Configuration.GetConnectionString(e.Name) ?? string.Empty);
//AI のキーは接続文字列として設定する (Azure App Service の接続文字列設定に置くため)
SystemConfig.Instance.AISettings.OpenAIKey = builder.Configuration.GetConnectionString("OpenAIKey") ?? SystemConfig.Instance.AISettings.OpenAIKey;
SystemConfig.Instance.AISettings.DocumentAnalysisKey = builder.Configuration.GetConnectionString("DocumentAnalysisKey") ?? SystemConfig.Instance.AISettings.DocumentAnalysisKey;

GlobalFontSettings.FontResolver = new CustomFontResolver(SystemConfig.Instance.FontFileDirectory);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddControllers()
      .AddJsonOptions(options =>
      {
          options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
          options.JsonSerializerOptions.Converters.AddJsonConverters();
      });

if (SystemConfig.Instance.UseHotReload)
{
    builder.Services.AddSignalR();
    builder.Services.AddHostedService(sp => new FileWatcherService(sp.GetRequiredService<IHubContext<HotReloadHub>>(), SystemConfig.Instance.DesignFileDirectory));
}

// Compress dynamic responses (e.g. the design data fetched on every reload).
// Brotli/Gzip are decoded by the browser's native network stack, so this adds
// no decompression cost to the WASM runtime.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Concat(["application/octet-stream"]);
});

//Localize (日本語デモサイトなので ja-JP 固定)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("ja-JP")
    };
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddScoped<DataService>();

var app = builder.Build();

//SQL debug log: dump executed SQL and parameters (enable via appsettings.Development.json or the SqlLog app setting).
//ILogger output reaches the local console, Azure App Service Log Stream and Application Insights
//(raw Console.WriteLine is discarded on Windows App Service).
if (builder.Configuration.GetSection("SqlLog").Get<bool>())
{
    var sqlLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("SqlLog");
    DbAccessor.SqlLog = message => sqlLogger.LogInformation("{SqlLog}", message);
}

app.UseResponseCompression();
app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();

// Always revalidate .html entry pages so a deployed update takes effect
// immediately instead of being served stale from the browser cache.
var htmlNoCache = new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.CacheControl = "no-cache";
        }
    }
};
app.UseStaticFiles(htmlNoCache);

app.UseRouting();

if (SystemConfig.Instance.UseHotReload)
{
    app.MapHub<HotReloadHub>("/hot_reload_hub");
}

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html", htmlNoCache);

// Exception handling.
app.UseExceptionHandlerSendToFront();

app.Run();
