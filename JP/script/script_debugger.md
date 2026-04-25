# スクリプトデバッガ

<img src="../overview/images/script_debugger.png">

スクリプトをステップ実行できます。Web アプリ側で `CanScriptDebug` を `true` にする必要があります。

## 設定

`IAppInfoService` の `CanScriptDebug` で `true` を返すと利用可能になります。
`Codeer.LowCode.Blazor.Templates` Ver1.1.22.1 以降のテンプレートでは
`appsettings.Development.json` で指定できます。

```cs
public class AppInfoService : IAppInfoService
{
    readonly NavigationManager _navigationManager;
    readonly HttpService _http;
    readonly ScriptRuntimeTypeManager _scriptRuntimeTypeManager = new();
    readonly ToasterEx _toaster;
    HubConnection? _hubConnection;
    DesignData? _design;
    DateTime _lastHotReload = DateTime.Now;
    SystemConfigForFront? _config;

    public ModuleData? CurrentUserData { get; private set; }

    public string CurrentUserId { get; set; } = string.Empty;

    public Guid Guid { get; set; } = Guid.NewGuid();

    public event EventHandler OnHotReload = delegate { };

    public bool IsDesignMode => false;

    public DesignData GetDesignData() => _design ?? new();

    // スクリプトデバッグの設定
    public bool CanScriptDebug => _config?.CanScriptDebug == true;
```

```json
{
  "ConnectionStrings": {
    "Main": "",
    "SampleSQLite": "Data Source=C:\\Codeer.LowCode.Blazor.Local\\Data\\sqlite_sample.db;Version=3;"
  },
  "FileStorages": [
    {
      "Name": "Local",
      "FileStorageType": "FileSystem",
      "Directory": "C:\\Codeer.LowCode.Blazor.Local\\Storages"
    }
  ],
  "DesignFileDirectory": "C:\\Codeer.LowCode.Blazor.Local\\Designs",
  "FontFileDirectory": "C:\\Codeer.LowCode.Blazor.Local\\Font",
  "UseHotReload": true,
  "CanScriptDebug": true, // スクリプトデバッグの設定
  "IsLicenseAutoUpdate": true,
  "IsLicenseAuthenticationByDomain": false,
  "AISettings": {
    "OpenAIEndPoint": "",
    "OpenAIKey": "",
    "ChatModel": "",
    "DocumentAnalysisEndPoint": "",
    "DocumentAnalysisKey": ""
  },
  "MailSettings": {
    "Host": "",
    "Port": "",
    "SenderMailAddress": "",
    "Password": "",
    "SSL": ""
  }
}
```

## 操作

①でデバッグ用のブラウザを起動できます。⑥にデバッグ対象の URL を入力してください。
あとの操作は VisualStudio 等と同じです。

| 操作 | ショートカットキー | ボタン |
|----------|----------|----------|
| デバッガ起動 | F5 | ① |
| 実行 | F5 | ② |
| ステップイン | F11 | ③ |
| ステップオーバー | F10 | ④ |
| ステップアウト | Shift+F11 | ⑤ |

---

## 関連項目

- [スクリプト概要](script.md)
- [スクリプト構文リファレンス](script_syntax.md)
