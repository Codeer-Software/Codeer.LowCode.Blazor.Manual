# バッチ処理（ジョブ）

ジョブ（バッチ処理）は 2 種類の書き方があります。

| 種類 | 書く場所 | 説明 |
|---|---|---|
| Job スクリプト | デザイナ（デザインの `Jobs/<ジョブ名>.job.cs`） | 画面のスクリプトと同じ言語で書きます。ホストのビルドは要りません。→ [Job スクリプト](../script/script_job.md) |
| C# のジョブ（`IJob`） | Web サーバーのプロジェクト（`Server/Jobs`） | 通常の .NET (C#) で書きます。このページで説明します |

起動のしかた（コマンドライン・画面のボタン）は共通です。同じ名前のジョブが両方にあれば `IJob` が動きます。
まずは Job スクリプトで書けるかを考え、コマンドラインの引数を受け取りたいとき・.NET のライブラリを直接使いたいときなどは C# のジョブで書きます。

C# のジョブは通常の .NET での実装になりますが、ローコードの機能を利用して作成できます。

- 処理の流れは C# のクラスで書きます（ジョブ）
- その中で、デザイナで作ったモジュールをモジュール名・フィールド名でそのまま読み書きできます
- 権限・保存時の処理（`CustomizedModuleDataIO` の上書き）・編集履歴などは、画面から保存したときと同じように働きます
- Web サーバーと同じプログラム・同じ設定（`appsettings`・`App.zip`）で動くので、バッチ用のプロジェクトや接続設定を別に用意する必要はありません

起動の仕方は 2 つです。

| 起動 | 使いどころ |
|---|---|
| コマンドライン（タスクスケジューラ・Azure WebJobs など） | 夜間の締め処理・定期的な取り込みなど、決まった時刻に動かす |
| 画面のボタン（[ジョブトリガー](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/JobTriggerField.md)。拡張ライブラリ Extras のフィールド） | 画面の内容を渡して、その場で実行する |

ジョブの仕組みはアプリのテンプレート（Web サーバー）の `Server/Jobs` フォルダにソースコードとして入っています。詳しい約束（引数・終了コード・制限）は同じフォルダの `README.md` にあります。

> アプリのテンプレート 1.3.53.0 以降で作ったアプリに入っています（Codeer.LowCode.Blazor 1.3.53・Codeer.LowCode.Blazor.Extras / Extras.Server 0.23.0 以降）。それより前に作ったアプリは [Codeer.LowCode.Blazor.Starter](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Starter) の `Source/Hosts/Cookie/LowCodeApp.Server` から `Jobs/`・`Controllers/ExtrasController.cs`・`Extras/ExtrasApiTable.cs`・`Program.cs` の `job` の分岐を取り込み、Extras / Extras.Server を最新に上げてください。
>
> サービスアカウント（下の「権限」）と Job スクリプトは Codeer.LowCode.Blazor 1.3.69 以降です（Job スクリプトの `CreateUpdateData()` などの書き方は 1.3.70・Codeer.LowCode.Blazor.Extras / Extras.Server 0.34.0 以降）。

---

## ジョブを書く

`Server/Jobs` フォルダに `IJob` を実装したクラスを足します。登録は要りません。

```csharp
public class CloseOrdersJob : IJob
{
    public string Name => "CloseOrders";

    public async Task<string?> RunAsync(DataService data, string[] args)
    {
        //画面で作った「受注」モジュールをそのまま検索する
        var page = await data.ModuleDataIO.GetListAsync(new SearchCondition("Order"), 0);

        //... 締めの処理。保存も data.ModuleDataIO で行う

        return $"{page.TotalCount} 件を締めました";
    }
}
```

- `Name` がジョブ名です（大文字・小文字は区別しません）
- `data.ModuleDataIO` で読み書きします。読み方は [C# でのデータ取得ヘルパ](module_data_io_extensions.md)、保存は [プロコードでのデータ保存](data_submit.md) と同じです
- 戻り値は成功したときの文言です（画面のボタンから起動したときは成功の通知に出ます）
- 失敗は例外を投げます（終了コードが 1 になり、画面のボタンから起動したときは例外のメッセージがエラーの通知に出ます）
- 書き込みは `data.ModuleDataIO` で行ってください。SQL を直接実行すると編集履歴などが付きません

完成形の例はテンプレートの `Server/Jobs/README.md` にあります。

## 権限（誰として動くか）

| 起動 | 権限 |
|---|---|
| 画面のボタン | ボタンを押したユーザー |
| コマンドライン | ユーザーなし（匿名）。ログインが必要なアプリでは読み書きできません |
| サービスアカウントを指したジョブ | 起動のしかたによらず、サービスアカウントのユーザー |

権限の条件はこのユーザーで判定されます。コマンドラインから動かすジョブは、サービスアカウントを指してください。

サービスアカウントは `appsettings.json` の `ServiceAccounts` に、名前とユーザーのログイン ID（ログイン画面で入力するもの）を書きます。

```json
"ServiceAccounts": [
  { "Name": "System", "User": "<ログイン ID>" }
]
```

ジョブは `ServiceAccount` プロパティでその名前を指します（Job スクリプトでは、デザイナの Job の設定 `ServiceAccount` に同じ名前を書きます）。

```csharp
public class CloseOrdersJob : IJob
{
    public string Name => "CloseOrders";
    public string ServiceAccount => "System";
```

- 指した名前が `ServiceAccounts` に無いとき、そのログイン ID のユーザーが見つからない・停止中のときは、ジョブは失敗します
- すべての権限を持つ特別なユーザーはありません。ジョブに要る権限は、サービスアカウントのユーザーの権限で調整します
- 画面のボタンから起動できるかは、ジョブトリガー（`JobTriggerField`）に書き込めるかだけで決まります（サービスアカウントのユーザーは関係しません）
- サービスアカウントを指したジョブでは、画面から渡った内容をそのまま保存しないでください。ボタンを押せるだけのユーザーが、細工した内容をサービスアカウントの権限で保存できてしまいます。保存する項目と値はジョブが決め、渡った内容からは要る値だけを取り出して検証します

---

## コマンドラインから起動する

```
dotnet LowCodeApp.Server.dll job <ジョブ名> [引数...]
```

- Web ホストは立てずに、ジョブを 1 つ実行して終了します
- 終了コードは 成功 0 / 失敗 1 / ジョブが無い 2 です
- `job` だけで実行すると、使い方とジョブの一覧を表示します
- Visual Studio からは起動プロファイル `job` で実行できます
- コマンドラインから起動したジョブは匿名で動きます。タスクスケジューラ・WebJobs から動かすジョブは、サービスアカウントを指してください（上の「権限」）
- Job スクリプトには、ジョブ名の後ろの引数は渡りません

### タスクスケジューラ

「操作」の「プログラム/スクリプト」に `dotnet`、「引数の追加」に `<配置先>\LowCodeApp.Server.dll job <ジョブ名>` を指定します。結果はタスクの「前回の実行結果」（終了コード）で確認できます。

### Azure App Service の WebJobs

Web を公開した App Service にトリガー WebJob を足し、次の 1 行を書いた `run.cmd` を zip にしてアップロードします。

```
dotnet %HOME%\site\wwwroot\LowCodeApp.Server.dll job <ジョブ名>
```

定期実行は CRON 式で指定します。接続文字列などは Web と同じ App Service の設定が使われます。

---

## 画面のボタンから起動する

モジュールに [ジョブトリガー](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/JobTriggerField.md)（拡張ライブラリ Extras の `JobTriggerField`）を置き、ジョブ名を設定します。ボタンを押すと、画面の内容（JSON）を渡してジョブを実行します。

- 終わるまで待って結果を通知する（既定）か、起動だけしてすぐ戻る（非同期）かを選べます
- ジョブを動かす場所は `JobTriggerField` の `ProcessType` で選べます。既定の `NewProcess` はサーバーの別プロセス、`InProcess` は Web サーバーのプロセスの中で動きます。プロセスの中で動かすと起動が速い代わりに、メモリと CPU を画面の処理と分け合い、サーバーの再起動・デプロイで止まります。短く軽いジョブはプロセスの中、長く重いジョブは別プロセスが向いています

## 注意

- 同じジョブが同時に動くことがあります（二人がボタンを押す・前回の定期実行が終わっていない等）。排他が必要ならジョブの中で取ってください
- ジョブがデータを保存しても、開いている画面には反映されません
- 画面のボタンから待つ形で起動したときは、画面が処理の終わりを待ちます。時間のかかる処理は非同期にしてください
