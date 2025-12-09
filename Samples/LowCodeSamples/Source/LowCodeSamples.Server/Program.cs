using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.License;
using Codeer.LowCode.Blazor.SystemSettings;
using LowCodeSamples.Client.Shared.Samples.ColorPicker;
using LowCodeSamples.Server.Services;
using LowCodeSamples.Server.Services.DataChangeHistory;
using LowCodeSamples.Server.Services.FileManagement;
using PdfSharp.Fonts;
using System.Globalization;
using System.Text.Json.Serialization;
using Codeer.LowCode.Bindings.MudBlazor.Installer;
using Codeer.LowCode.Bindings.Radzen.Blazor.Installer;
using LowCodeSamples.Server.Services.AI;
using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.Fluent.Blazor.Designs;
typeof(Microsoft.FluentUI.AspNetCore.Components.Appearance).ToString();

//load dll.
typeof(ApexChartFieldDesign).ToString();
typeof(SeriesType).ToString();
typeof(ColorPickerField).ToString();
typeof(FluentTextFieldDesign).ToString();
MudBlazorLoader.LoadAssemblies();
RadzenLoader.LoadAssemblies();

var builder = WebApplication.CreateBuilder(args);

GlobalFontSettings.FontResolver = new CustomFontResolver();

LicenseManager.DomainLicense = builder.Configuration.GetSection("DomainLicense").Get<string>() ?? string.Empty;
LicenseManager.IsAutoUpdate = builder.Configuration.GetSection("IsLicenseAutoUpdate").Get<bool>();
SystemConfig.Instance.CanUpdate = builder.Configuration.GetSection("CanUpdate").Get<bool>();
SystemConfig.Instance.UseHotReload = builder.Configuration.GetSection("UseHotReload").Get<bool>();
SystemConfig.Instance.CanScriptDebug = builder.Configuration.GetSection("CanScriptDebug").Get<bool>();
SystemConfig.Instance.DataSources = builder.Configuration.GetSection("DataSources").Get<DataSource[]>() ?? [];
SystemConfig.Instance.FileStorages = builder.Configuration.GetSection("FileStorages").Get<FileStorage[]>() ?? [];
SystemConfig.Instance.DataChangeHistoryTableInfo = builder.Configuration.GetSection("DataChangeHistoryTableInfo").Get<DataChangeHistoryTableInfo[]>() ?? [];
SystemConfig.Instance.TemporaryFileTableInfo = builder.Configuration.GetSection("TemporaryFileTableInfo").Get<TemporaryFileTableInfo[]>() ?? [];
SystemConfig.Instance.DesignFileDirectory = builder.Configuration["DesignFileDirectory"] ?? string.Empty;
SystemConfig.Instance.FontFileDirectory = builder.Configuration["FontFileDirectory"] ?? string.Empty;
SystemConfig.Instance.AISettings = builder.Configuration.GetSection("AISettings").Get<AISettings>() ?? new();
SystemConfig.Instance.DataSources.ToList().ForEach(e => e.ConnectionString = builder.Configuration.GetConnectionString(e.Name) ?? string.Empty);
SystemConfig.Instance.FileStorages.ToList().ForEach(e => e.ConnectionString = builder.Configuration.GetConnectionString(e.Name) ?? string.Empty);
SystemConfig.Instance.AISettings.OpenAIKey = builder.Configuration.GetConnectionString("OpenAIKey") ?? string.Empty;
SystemConfig.Instance.AISettings.DocumentAnalysisKey = builder.Configuration.GetConnectionString("DocumentAnalysisKey") ?? string.Empty;

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
    builder.Services.AddHostedService<FileWatcherService>();
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
