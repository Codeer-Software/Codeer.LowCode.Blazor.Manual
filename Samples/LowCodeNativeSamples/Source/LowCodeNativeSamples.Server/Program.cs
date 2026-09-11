using Codeer.LowCode.Bindings.ApexCharts;
using Codeer.LowCode.Blazor.DbAccess;
using Codeer.LowCode.Blazor.Extras;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.License;
using Codeer.LowCode.Blazor.SystemSettings;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using PdfSharp.Fonts;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using LowCodeNativeSamples.Client.Shared.Samples;
using LowCodeNativeSamples.Server;
using LowCodeNativeSamples.Server.Services;
using LowCodeNativeSamples.Server.AI;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Extras.Server.Mail;
using Codeer.LowCode.Blazor.Extras.Server.Excel;
using Codeer.LowCode.Blazor.Extras.Server.FileManagement;
using Codeer.LowCode.Blazor.Extras.Server.Web;
using Codeer.LowCode.Blazor.Extras.Server.Auth;
using Microsoft.AspNetCore.SignalR;

//load dll.
typeof(CodeBehindSample).ToString();
ApexChartsServerInitializer.Initialize();
ExtrasServerInitializer.Initialize();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

LicenseManager.DomainLicense = builder.Configuration.GetSection("DomainLicense").Get<string>() ?? string.Empty;
LicenseManager.IsAutoUpdate = builder.Configuration.GetSection("IsLicenseAutoUpdate").Get<bool>();
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
SystemConfig.Instance.AIChat = builder.Configuration.GetSection("AIChat").Get<AIChatSettings>() ?? new();
SystemConfig.Instance.AllowPasswordLogin = builder.Configuration.GetValue<bool?>("AllowPasswordLogin") ?? true;
//外部 IdP の設定 (種類ごとのセクション。実体は Services/ExternalLoginTable が組み立てる)
SystemConfig.Instance.EntraLogin = builder.Configuration.GetSection("EntraLogin").Get<EntraLoginSettings>() ?? new();
SystemConfig.Instance.GoogleLogin = builder.Configuration.GetSection("GoogleLogin").Get<GoogleLoginSettings>() ?? new();
SystemConfig.Instance.CognitoLogin = builder.Configuration.GetSection("CognitoLogin").Get<CognitoLoginSettings>() ?? new();
SystemConfig.Instance.OidcLogins = builder.Configuration.GetSection("OidcLogins").Get<OidcLoginSettings[]>() ?? [];
SystemConfig.Instance.MobileLoginCallbackUrl = builder.Configuration["MobileLoginCallbackUrl"] ?? string.Empty;
SystemConfig.Instance.TotpLogin = builder.Configuration.GetSection("TotpLogin").Get<TotpLoginSettings>() ?? new();
SystemConfig.Instance.EmailOtpLogin = builder.Configuration.GetSection("EmailOtpLogin").Get<EmailOtpLoginSettings>() ?? new();
SystemConfig.Instance.DataSources.ToList().ForEach(e => e.ConnectionString = builder.Configuration.GetConnectionString(e.Name) ?? string.Empty);
//AI のキーは接続文字列 (ConnectionStrings:OpenAIKey / DocumentAnalysisKey) にも置ける。App Service の「接続文字列」欄で管理するため (AISettings:xxxKey より優先)
SystemConfig.Instance.AISettings.OpenAIKey = builder.Configuration.GetConnectionString("OpenAIKey") ?? SystemConfig.Instance.AISettings.OpenAIKey;
SystemConfig.Instance.AISettings.DocumentAnalysisKey = builder.Configuration.GetConnectionString("DocumentAnalysisKey") ?? SystemConfig.Instance.AISettings.DocumentAnalysisKey;

GlobalFontSettings.FontResolver = new CustomFontResolver(SystemConfig.Instance.FontFileDirectory);

// Cookie authentication.
builder.UseCookieAuthentication();

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

//Localize
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("ja-JP")
    };
    //set neutral as default
    options.DefaultRequestCulture = new RequestCulture(string.Empty);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders.Add(new CustomRequestCultureProvider(async context =>
    {
        //localization by the request header
        var userLanguages = context.Request.Headers["Accept-Language"].ToString();
        var firstLanguage = userLanguages.Split(',').FirstOrDefault();
        if (firstLanguage == "ja") firstLanguage = "ja-JP";

        return await Task.FromResult(new ProviderCultureResult(firstLanguage));
    }));
});

builder.Services.AddHttpContextAccessor();
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

// Cookie authentication.
app.UseCookieAuthentication();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html", htmlNoCache);

// Exception handling.
app.UseExceptionHandlerSendToFront();

app.Run();
