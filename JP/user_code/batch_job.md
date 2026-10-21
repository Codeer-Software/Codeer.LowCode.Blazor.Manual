# バッチ処理（ジョブ）

バッチ処理は通常の .NET (C#) での実装になりますが、ローコードの機能を利用して作成できます。

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

> アプリのテンプレート 1.3.53.0 以降で作ったアプリに入っています（Codeer.LowCode.Blazor 1.3.53・Codeer.LowCode.Blazor.Extras / Extras.Server 0.23.0 以降）。それより前に作ったアプリは [Codeer.LowCode.Blazor.Starter](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Starter) の `Source/Hosts/Cookie/LowCodeApp.Server` から `Jobs/`・`Controllers/JobController.cs`・`Program.cs` の `job` の分岐を取り込み、Extras.Server を 0.23.0 以降に上げてください。

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

テンプレートには例として `SampleJob.cs` が入っています。

## 権限（誰として動くか）

| 起動 | 権限 |
|---|---|
| コマンドライン | `appsettings.json` の `Jobs:UserId` のユーザー（ユーザーモジュールの Id）。空なら匿名 |
| 画面のボタン | ボタンを押したユーザー |

権限の条件はこのユーザーで判定されます。ログインが必要なアプリでは、コマンドラインから動かすジョブ用のユーザーを `Jobs:UserId` に設定してください。

---

## コマンドラインから起動する

```
dotnet LowCodeApp.Server.dll job <ジョブ名> [引数...]
```

- Web ホストは立てずに、ジョブを 1 つ実行して終了します
- 終了コードは 成功 0 / 失敗 1 / ジョブが無い 2 です
- `job` だけで実行すると、使い方とジョブの一覧を表示します
- Visual Studio からは起動プロファイル `job` で実行できます

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
- ジョブはサーバーの別プロセスとして動きます

## 注意

- 同じジョブが同時に動くことがあります（二人がボタンを押す・前回の定期実行が終わっていない等）。排他が必要ならジョブの中で取ってください
- ジョブがデータを保存しても、開いている画面には反映されません
- 画面のボタンから待つ形で起動したときは、画面が処理の終わりを待ちます。時間のかかる処理は非同期にしてください
