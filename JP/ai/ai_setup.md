# AIを使うための設定
AIの機能はテンプレートで出力したコードで実現しています。デフォルトでは Azure OpenAI を使うようになっていますが、ここを書き換えることでその他のサービスに変更できます。プロンプトもそれぞれの用途に合わせて調整してください。

## 必要なものと設定
- OpenAIやAzure Open AI等のサブスクリプション
- 開発Windowsの環境変数へEndPointとKeyの登録
- Codeer.LowCode.BlazorのサーバープロジェクトでEndPointとKeyの登録
- PDF/画像分析を使用する場合はDocumentAnalysisEndPoint及びDocumentAnalysisKeyの登録も必要
## 設定方法
以下のステップは、Azure Open AI等のEndPointとKeyが取得済みであることを前提とします。

### 開発Windowsの環境変数の設定
1. Windowsの環境変数エディタを開きます：スタートメニュー　→　「環境変数」で検索
2. ユーザー環境変数リストに以下２つの項目を新規追加します：
   ```
   変数名：AZURE_OPENAI_API_ENDPOINT
   変数値：EndPointのURL
   ```
   ```
   変数名：AZURE_OPENAI_API_KEY
   変数値：APIキー
   ```
   変数名はデフォルトで上記となりますが、変更することが可能です。
   Codeer.LowCode.BlazorのデザイナプロジェクトのApp.xaml.csで変更できます。

   ```C#
           protected override void OnStartup(StartupEventArgs e)
           {
            AISettings.Instance.OpenAIEndPoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_ENDPOINT") ?? string.Empty;
            AISettings.Instance.OpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;
            AISettings.Instance.ChatModel = "gpt-4o";
            //...
           }
   ```
### Codeer.LowCode.Blazorのサーバープロジェクトの設定(開発環境)
1. Codeer.LowCode.Blazorのテンプレートで作成されたソリューションをVisual Studioで開きます
2. サーバープロジェクトのappsettings.Development.jsonをひらきます
3. 以下の項目を実際の値に設定します
```JSON
  "AISettings": {
    "OpenAIEndPoint": "",
    "OpenAIKey": "",
    "ChatModel": "",
    "DocumentAnalysisEndPoint": "",
    "DocumentAnalysisKey": ""
  },
```
### 発行(Publish)環境でのサーバー設定
発行(Publish)の際はappsettings.jsonあるいはappsettings.Development.jsonにセンシティブな情報を置かないことが多いです。サーバーの環境変数へ登録するなどの方法があります。詳しくはサーバーソフトのドキュメントをご参照ください。

