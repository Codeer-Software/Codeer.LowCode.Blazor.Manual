using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.Fluent.Blazor.Designs;
using Codeer.LowCode.Bindings.MudBlazor.Installer;
using Codeer.LowCode.Bindings.Radzen.Blazor.Installer;
using Codeer.LowCode.Blazor.Extras;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.License;
using Codeer.LowCode.Blazor.SystemSettings;
using LowCodeSamples.Client.Shared.Samples.ColorPicker;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Extras.Server.Excel;
using Codeer.LowCode.Blazor.Extras.Server.FileManagement;
using Codeer.LowCode.Blazor.Extras.Server.Mail;
using Codeer.LowCode.Blazor.Extras.Server.Web;
using LowCodeSamples.Server.Services;
using LowCodeSamples.Server.Services.DataChangeHistory;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using PdfSharp.Fonts;
using System.Globalization;
using System.Text.Json.Serialization;
typeof(Microsoft.FluentUI.AspNetCore.Components.Appearance).ToString();

//load dll.
typeof(ApexChartFieldDesign).ToString();
typeof(SeriesType).ToString();
typeof(ColorPickerField).ToString();
typeof(FluentTextFieldDesign).ToString();
MudBlazorLoader.LoadAssemblies();
RadzenLoader.LoadAssemblies();
ExtrasServerInitializer.Initialize();

var builder = WebApplication.CreateBuilder(args);

//日本語デモサイトのため、サーバーの既定カルチャを ja-JP に固定する。
//デザインの読込時に解決されるコード定義 enum (承認状態など) の表示名はサーバーのカルチャで決まり、
//Azure 上では既定が英語になるため (ブラウザの言語に関係なく "In Progress" と表示されてしまう)
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ja-JP");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ja-JP");

LicenseManager.DomainLicense = builder.Configuration.GetSection("DomainLicense").Get<string>() ?? string.Empty;
LicenseManager.IsAutoUpdate = builder.Configuration.GetSection("IsLicenseAutoUpdate").Get<bool>();
SystemConfig.Instance.CanUpdate = builder.Configuration.GetSection("CanUpdate").Get<bool>();
SystemConfig.Instance.UseHotReload = builder.Configuration.GetSection("UseHotReload").Get<bool>();
SystemConfig.Instance.CanScriptDebug = builder.Configuration.GetSection("CanScriptDebug").Get<bool>();
SystemConfig.Instance.DataSources = builder.Configuration.GetSection("DataSources").Get<DataSource[]>() ?? [];
//ファイル保存先は種類ごとのセクションを FileStorageTable が読む (FileSystemStorages / AzureBlobStorages / S3Storages / 簡易形式 FileStorages)
SystemConfig.Instance.FileStorages = FileStorageTable.Create(builder.Configuration);
SystemConfig.Instance.DataChangeHistoryTableInfo = builder.Configuration.GetSection("DataChangeHistoryTableInfo").Get<DataChangeHistoryTableInfo[]>() ?? [];
SystemConfig.Instance.TemporaryFileTableInfo = builder.Configuration.GetSection("TemporaryFileTableInfo").Get<TemporaryFileTableInfo[]>() ?? [];
SystemConfig.Instance.DesignFileDirectory = builder.Configuration["DesignFileDirectory"] ?? string.Empty;
SystemConfig.Instance.FontFileDirectory = builder.Configuration["FontFileDirectory"] ?? string.Empty;
SystemConfig.Instance.AISettings = builder.Configuration.GetSection("AISettings").Get<AISettings>() ?? new();
SystemConfig.Instance.Mail = builder.Configuration.GetSection("Mail").Get<MailConfig>() ?? new();
//メールのプロバイダ設定はそれぞれ独立したセクション (使うものだけ書けばよい)
SystemConfig.Instance.Smtp = builder.Configuration.GetSection("Smtp").Get<SmtpSettings>() ?? new();
SystemConfig.Instance.GraphApi = builder.Configuration.GetSection("GraphApi").Get<GraphApiSettings>() ?? new();
SystemConfig.Instance.Gmail = builder.Configuration.GetSection("Gmail").Get<GmailSettings>() ?? new();
SystemConfig.Instance.DataSources.ToList().ForEach(e => e.ConnectionString = builder.Configuration.GetConnectionString(e.Name) ?? string.Empty);
SystemConfig.Instance.AISettings.OpenAIKey = builder.Configuration.GetConnectionString("OpenAIKey") ?? string.Empty;
SystemConfig.Instance.AISettings.DocumentAnalysisKey = builder.Configuration.GetConnectionString("DocumentAnalysisKey") ?? string.Empty;

GlobalFontSettings.FontResolver = new CustomFontResolver(SystemConfig.Instance.FontFileDirectory);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddControllers()
      .AddJsonOptions(options =>
      {
          options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
          options.JsonSerializerOptions.Converters.AddJsonConverters();
      });

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

if (SystemConfig.Instance.UseHotReload)
{
    builder.Services.AddSignalR();
    builder.Services.AddHostedService(sp => new FileWatcherService(
        sp.GetRequiredService<IHubContext<HotReloadHub>>(), SystemConfig.Instance.DesignFileDirectory));
}

//Localize
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
app.UseStaticFiles();

app.UseRouting();

if (SystemConfig.Instance.UseHotReload)
{
    app.MapHub<HotReloadHub>("/hot_reload_hub");
}

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

// Exception handling.
app.UseExceptionHandlerSendToFront();
app.Run();
