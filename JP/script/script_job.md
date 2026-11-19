# Job スクリプト（サーバーで動くスクリプト）

Job スクリプトは、**サーバーで動く処理をデザイナの中でスクリプトとして書く**機能です。
締め処理・一括更新・帳票の作成・メールの送信などのサーバー側の処理（ジョブ）を、C# のプロジェクトを触らずに作れます。

- **画面のスクリプトと同じ言語**で書けます。モジュール名・項目名がそのまま使え、入力補完と設計チェックが効きます
- **ホストのビルド・再デプロイは要りません**。デザインを送れば反映されます
- 権限・楽観ロック・編集履歴などは、**画面から保存したときと同じ**に働きます
- **画面のボタンからも、コマンドライン（定期実行）からも**起動できます
- ブラウザには配られず、**サーバーでだけ動きます**

> Codeer.LowCode.Blazor 1.3.70・Codeer.LowCode.Blazor.Extras / Extras.Server 0.34.0 以降で使えます。

C# で書くジョブ（`IJob`）もあります。違いと起動のしかたは [バッチ処理（ジョブ）](../user_code/batch_job.md) を参照してください。

---

## 1. 作る

デザイナのソリューションエクスプローラで `Jobs` を右クリックし、「ジョブを作成」を選びます。

`Jobs` の下に 2 つのファイルができます。

| ファイル | 内容 |
|---|---|
| `名前.job.json` | ジョブの設定（サービスアカウント。[8. 権限とサービスアカウント](#8-権限とサービスアカウント)） |
| `名前.job.cs` | スクリプト本体（`名前.job.json` の子に表示されます） |

- それぞれ別のタブで開き、保存もタブごとです。ジョブの改名・削除は 2 つのファイルが一緒に変わります
- ジョブ名はファイル名の最初のドットより前です（`Jobs/CloseOrders.job.cs` なら `CloseOrders`）
- ジョブにできるのは `Jobs` の直下のファイルだけです（サブフォルダのファイルはジョブになりません）
- ジョブ名はモジュール名・列挙型名と重ねないでください（設計チェックでエラーになります）

---

## 2. 最初の例

状態が「Open」の受注を「Closed」にする例です。

```csharp
// Jobs/CloseOrders.job.cs
string Run(JobArguments args)
{
    var search = new ModuleSearcher<Order>();
    search.AddEquals(e => e.Status.Value, Order.StatusOpen);
    var rows = search.ExecuteRaw();
    foreach (var row in rows)
    {
        var change = row.CreateUpdateData();
        change.Status = new TextFieldData(Order.StatusClosed);
        DBService.Update(change);
    }
    return $"{rows.Count} 件を締めました";
}
```

入口は `Run` という名前の関数です。次のどちらかの形で書きます。

| 形 | 説明 |
|---|---|
| `string Run(JobArguments args)` | 戻り値が成功したときの文言です。画面のボタンから起動したときは成功の通知に出ます |
| `void Run(JobArguments args)` | 文言を返しません |

`Run` 以外の関数・変数は、画面のスクリプトと同じように書けます。

---

## 3. 画面のスクリプトとの違い

Job には画面がありません。そのため、データの扱い方が画面のスクリプトと違います。

| | 画面のスクリプト | Job スクリプト |
|---|---|---|
| 画面の部品 | `Module`・Field・`MessageBox`・`NavigationService` などを使う | 使えません（書くと設計チェックで指摘されます） |
| データ | Module の Field で読み書きする | `モジュール名.Data`（型付きのデータ）で読み書きする |
| 検索 | `Execute()` などで Module を取る | `ExecuteRaw()` などの Raw 系でデータを取る |
| 書き込み | `Submit()` | `DBService.Add` / `Update` / `Delete` |

- 使えないものは入力補完に出ません。書いたときは、代わりに使うもの（`Execute` なら `ExecuteRaw`）が指摘に出ます
- 使えるもの: `Logger`・`Math`・`DateTime` などの基本の型・`List` / `Dictionary` / `JsonObject`・デザインの列挙型・`ModuleSearcher`（Raw 系）・`ModuleAggregator`・`DBService`・FieldData 型（`TextFieldData` など）・`CurrentUser` / `Invoker`・`Resources`・`FileService`。アプリのテンプレートで作ったアプリでは `Excel` と `MailService` も使えます（[9. ファイル・Excel・メール](#9-ファイルexcelメール)）
- `BatchSearcher` / `BatchAggregator` は使えません
- 文法は画面のスクリプトと同じです（[スクリプト構文リファレンス](script_syntax.md)）

---

## 4. データを読む

検索は `ModuleSearcher` で、条件の書き方は画面のスクリプトと同じです（[ModuleSearcher / BatchSearcher](script_module_searcher.md)）。実行には、データを返す Raw 系の 3 つを使います。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `ExecuteRaw()` | `List<ModuleData>` | 条件に一致する全件 |
| `ExecuteRawPage(pageIndex)` | `ModuleDataPageResult` | 指定ページの `Items` と総件数 `TotalCount`（ページのサイズは `Limit`） |
| `ExecuteRawFirstOrDefault()` | `ModuleData?` | 先頭 1 件。該当が無ければ `null` |

```csharp
var search = new ModuleSearcher<Order>();
search.AddEquals(e => e.Status.Value, "Open");
foreach (var row in search.ExecuteRaw())
{
    Logger.Log(row.Name.Value);   // 項目が入力補完に出る
}
```

- 結果の行は、検索したモジュールの型付きのデータ（`new ModuleSearcher<Order>()` なら `Order.Data`）です。`var` で受けても、項目が入力補完に出て、無い項目は設計チェックで指摘されます
- 型を書いて受けるなら `Order.Data` / `List<Order.Data>` と書きます
- 読めない行は結果に出ません。読めない項目と、値が無い（null の）項目は、行に載りません

集計は `ModuleAggregator<モジュール名>` で、書き方は画面のスクリプトと同じです（[ModuleAggregator / BatchAggregator](script_module_aggregator.md)）。

---

## 5. データを書く

書き込みは `DBService` で行います。呼んだその場で 1 件ずつ保存します（画面の保存と同じく、権限・楽観ロック・編集履歴が働きます）。

| メソッド | 説明 |
|---|---|
| `DBService.Add(data)` | 追加します。保存の後、`data.Id` は採番された Id になります |
| `DBService.Update(data)` | 更新します。**載せた項目だけを書きます** |
| `DBService.Delete(data)` | 削除します |
| `DBService.ExecuteStandaloneSql(data)` | `data` のモジュールの、タイミングが「単独」の ExecuteSql を、`data` の値をパラメータにして実行します |

- 保存が失敗したときはジョブの失敗になります（[6. トランザクションと失敗](#6-トランザクションと失敗)）
- タイミングが新規作成時・更新時・削除時の ExecuteSql は、`Add` / `Update` / `Delete` で一緒に走ります

### 更新・削除のデータ

読んだ行の `CreateUpdateData()` から始めます。Id と楽観ロックの版だけを写した新しいデータが返るので、変える項目だけを足します。

```csharp
var change = row.CreateUpdateData();       // Id と楽観ロックの版だけが載っている
change.Status = new TextFieldData("Closed"); // 書く項目を載せる
DBService.Update(change);                  // 載せた Status だけが書かれる

DBService.Delete(row.CreateUpdateData());  // 削除も同じ
```

- 載せなかった項目の列は書かれません。`new TextFieldData(null)` を載せると、列が null になります
- 楽観ロックのあるモジュール（[OptimisticLockingField](../fields/OptimisticLocking.md) を持つモジュール）では、読んだ行の楽観ロックの項目が要ります。`CreateUpdateData()` が写すので、そのまま使えば足ります。読んだ後に他で更新されていたら、ジョブの失敗になります

### 追加のデータ

`new モジュール名.Data()`（項目が 1 つも載っていない空のデータ）から始めます。

```csharp
var add = new Order.Data();
add.Name = new TextFieldData("山田商店");
add.Qty = new NumberFieldData(3);
DBService.Add(add);
```

親と子を追加するときは、親を先に `Add` して、採番された親の Id を子の項目に入れます。

```csharp
var invoice = new Invoice.Data();
invoice.Customer = new TextFieldData("山田商店");
DBService.Add(invoice);                              // invoice.Id.Value は採番された Id になる

var line = new InvoiceItem.Data();
line.InvoiceId = new IdFieldData(invoice.Id.Value);  // 親を指す
DBService.Add(line);
```

### 値を入れて作れる FieldData

| FieldData | 値 |
|---|---|
| `TextFieldData` / `SelectFieldData` / `RadioGroupFieldData` / `LinkFieldData` / `IdFieldData` / `PasswordFieldData` / `JsonFieldData` | 文字列 |
| `NumberFieldData` | 数値 |
| `BooleanFieldData` | `true` / `false` |
| `DateFieldData` / `DateTimeFieldData` / `DateTimeOffsetFieldData` / `TimeFieldData` | 日付・日時・時刻 |

- 値はスクリプトのいつもの規則で変換されます（`new NumberFieldData("5")` は 5）
- 読むときは今までどおり `.Value` です（`row.Status.Value`）
- 上の表に無い FieldData（添付など）は、`new XxxFieldData()` で作ってからメンバーに代入します

### 単独の ExecuteSql

```csharp
//「単独」の ExecuteSql (ストアド) の戻り値の受け皿が Total 項目
var calc = new Recalc.Data();
calc.Target = new TextFieldData("A001");
DBService.ExecuteStandaloneSql(calc);
var total = calc.Total.Value;   // 出力パラメータの値が項目に戻る
```

---

## 6. トランザクションと失敗

### 失敗させる (Fail)

`Fail("文言")` を呼ぶと、そこで実行をやめてジョブは失敗になります。文言が失敗の理由です（画面のボタンから起動したときはエラーの通知に出ます）。

```csharp
if (rows.Count == 0) Fail("対象がありません");
```

実行時エラーと、保存（`DBService` の書き込み）の失敗も、ジョブの失敗になります。

アプリのテンプレートで作ったアプリでは、画面に出る失敗の文言のうち、そのまま出るのは `Fail` の理由と、検証・楽観ロックの競合・権限の文言だけです。それ以外（実行時エラー・DB のエラー）は本番では「ジョブでエラーが発生しました。」になり、元の文言は `Logger` と同じ出力先に出ます。

### トランザクション

**ジョブの 1 回の実行全体が 1 つのトランザクション**です。

| 場面 | 書き込みの扱い |
|---|---|
| `Run` が正常に終わる | 確定します |
| ジョブが失敗する（`Fail`・実行時エラー・保存の失敗） | 最後の `DBService.Commit()`（無ければ実行の始め）以降の書き込みを取り消します |
| `DBService.Commit()` を呼ぶ | そこまでの書き込みを確定します。この後に失敗しても、確定した分は残ります |

- 同じ実行の中の検索は、まだ確定していない書き込みも見えます
- 添付ファイルも同じトランザクションに入ります。取り消されたら、添付は保存前の状態に戻ります

大量のデータを少しずつ確定する例です（途中で失敗しても、確定済みの分は残ります）。

```csharp
void Run(JobArguments args)
{
    var search = new ModuleSearcher<Order>();
    search.AddEquals(e => e.Status.Value, "Open");
    var rows = search.ExecuteRaw();
    for (var i = 0; i < rows.Count; i++)
    {
        var change = rows[i].CreateUpdateData();
        change.Status = new TextFieldData("Closed");
        DBService.Update(change);
        if ((i + 1) % 1000 == 0) DBService.Commit();
    }
}
```

---

## 7. 起動する

Job スクリプトは C# のジョブ（`IJob`）と同じ「ジョブ」として扱われ、起動のしかたも同じです。

### 画面のボタンから起動する

モジュールに [ジョブトリガー（JobTriggerField）](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/JobTriggerField.md)（拡張ライブラリ Extras のフィールド）を置き、ジョブ名を設定します。ボタンを押すとジョブが動き、`Run` の戻り値が成功の通知に出ます。

**画面の値を受け取る**: JobTriggerField の「画面の内容を渡す」が `true`（既定）なら、画面のモジュールの現在値（未保存の値を含む）が `args.Data` に入ります。無ければ `null` です。

```csharp
string Run(JobArguments args)
{
    Order.Data data = args.Data;   // 型付きで受けると項目の補完と設計チェックが効く
    Logger.Log(data.Name.Value);
    return "完了しました";
}
```

- `args.Data` は画面が送った値そのままです（サーバーは検証していません）。一覧の項目には、画面が読み込んでいる行だけが入ります
- 型付きで受けても、中身がそのモジュールのデータかは確かめられません

**画面に結果を返す**: `JsonObject` を作って `args.Result` に入れます。画面のスクリプトでは、JobTriggerField の `Result` で読めます。

```csharp
// Job
string Run(JobArguments args)
{
    var invoice = new Invoice.Data();
    invoice.Customer = new TextFieldData("山田商店");
    DBService.Add(invoice);

    var result = new JsonObject();
    result.InvoiceId = invoice.Id.Value;
    result.Total = 12800;
    args.Result = result;
    return "請求書を作りました";
}
```

```csharp
// 画面のスクリプト (CreateJob は JobTriggerField)
void CreateButton_OnClick()
{
    if (!CreateJob.Run()) return;
    var r = CreateJob.Result;
    Total.Value = r.Total;
}
```

- 返るのはジョブが成功したときだけです
- キーは大文字で始めます（`result.total` で入れても、画面では `r.Total` で読みます）
- 画面で読むキーは必ず入れます（値が無いときは `null` を入れます。入れていないキーを読むと、`null` ではなく空の `JsonObject` になります）
- 画面では、数値は `decimal`、`Guid`・日付は文字列で届きます

ジョブを動かす場所は JobTriggerField の `ProcessType` で選べます（既定の `NewProcess` = サーバーの別プロセス / `InProcess` = Web サーバーのプロセスの中）。

### コマンドライン・定期実行で起動する

```
dotnet LowCodeApp.Server.dll job <ジョブ名>
```

タスクスケジューラ・Azure WebJobs からの定期実行の設定は [バッチ処理（ジョブ）](../user_code/batch_job.md) を参照してください。

- コマンドラインの引数（ジョブ名の後ろ）は Job スクリプトには渡りません。引数が要る処理は C# のジョブ（`IJob`）で書きます
- コマンドラインから動かすジョブは、サービスアカウントを指します（[8. 権限とサービスアカウント](#8-権限とサービスアカウント)）

---

## 8. 権限とサービスアカウント

読み書きの権限は、ジョブを実行するユーザーのものです（画面から読み書きするのと同じ判定）。

| 起動 | 権限 |
|---|---|
| 画面のボタン | ボタンを押したユーザー |
| コマンドライン | ユーザーなし（匿名）。ログインが要るアプリでは読み書きできません |
| サービスアカウントを指したジョブ | 起動のしかたによらず、サービスアカウントのユーザー |

サービスアカウントは、Web サーバーの `appsettings.json` で名前とユーザー（ログイン ID）を対応させます。

```json
"ServiceAccounts": [
  { "Name": "System", "User": "<ログイン ID>" }
]
```

ジョブの設定（`名前.job.json`。デザイナでは設定のタブ）の `ServiceAccount` に、その名前を書きます。

```json
{
  "ServiceAccount": "System"
}
```

- デザインに書くのは名前だけです。どのユーザーで動かすかは `appsettings.json` で決まります
- 指した名前が `ServiceAccounts` に無いとき、ログイン ID のユーザーが見つからない・停止中のときは、ジョブは失敗します
- すべての権限を持つ特別なユーザーはありません。ジョブに要る権限は、サービスアカウントのユーザーの権限で調整します
- 画面のボタンから起動できるかは、JobTriggerField に書き込めるかだけで決まります

スクリプトからは、ユーザーのデータを次の 2 つで読めます（`CurrentUser.Name.Value` のように読みます）。

| 名前 | 内容 |
|---|---|
| `CurrentUser` | ジョブを実行するユーザー（読み書きの権限のユーザー） |
| `Invoker` | ジョブを起動したユーザー。サービスアカウントを指していないジョブでは `CurrentUser` と同じです。起動したユーザーがいないときは `null` |

> **サービスアカウントを指した Job では、`args.Data`（型付きで受けたものも）をそのまま `DBService.Add` / `Update` / `Delete` / `ExecuteStandaloneSql` に渡さないでください。**
> 保存は押したユーザーではなくサービスアカウントの権限で通るので、ボタンを押せるだけのユーザーが、別のモジュールや項目への書き込みを通せてしまいます。保存するデータは Job が `new モジュール名.Data()` で作り、`args.Data` からは必要な値だけを読んで入れます。
> 画面に返す結果（`args.Result`）と渡すファイルも、起動した人に見せてよいものだけにします。

---

## 9. ファイル・Excel・メール

### リソースを読む (Resources)

デザインのリソース（`Resources/` の下のファイル）を読みます。書き方は画面のスクリプトと同じです。

| メソッド | 説明 |
|---|---|
| `Resources.GetMemoryStream(path)` | ファイルの内容（`MemoryStream`）。無ければ `null` |
| `Resources.GetText(path)` | ファイルの内容を UTF-8 の文字列で。無ければ空 |

### 添付ファイル (FileService)

| メソッド | 説明 |
|---|---|
| `FileService.Save(moduleName, fieldName, fileName, content)` | 内容（`MemoryStream`）を添付の項目に入れる値にします。レコードの項目に代入して `DBService.Add` / `Update` で保存すると行に入ります |
| `FileService.GetMemoryStream(moduleName, fieldName, id)` | 保存済みの添付の内容。無い・読めないなら `null` |
| `FileService.SaveTemporary(moduleName, fieldName, fileName, content)` | 起動したユーザーに渡すファイルを作ります（下の「作ったファイルを画面に渡す」） |

- `Save` は画面のアップロードと同じ判定（項目の書き込み権限・許可する拡張子・最大サイズ）を通ります
- `Save` しただけでは行に入りません。保存しなかったファイルは期限で消えます

```csharp
var terms = Resources.GetMemoryStream("templates/terms.pdf");
if (terms == null) Fail("約款がありません");
var change = row.CreateUpdateData();
change.Attachment = FileService.Save("Invoice", "Attachment", "terms.pdf", terms);
DBService.Update(change);
```

### 作ったファイルを画面に渡す

Job が作ったファイルは `FileService.SaveTemporary` で起動したユーザーに渡すファイルにし、戻り値の `FileGuid` / `FileName` / `FileSize` を `args.Result` で返します。

```csharp
// Job
var file = FileService.SaveTemporary("Report", "Attachment", "売上.xlsx", content);
var result = new JsonObject();
result.FileGuid = file.FileGuid;
result.FileName = file.FileName;
result.FileSize = file.FileSize;
args.Result = result;
```

画面のスクリプトでは、次のどちらかで受けます。

```csharp
// 画面のスクリプト (r は JobTriggerField の Result)
FileService.DownloadTemporaryFile("Report", "Attachment", r.FileGuid, r.FileName);   // 保存せずダウンロードだけする
Attachment.SetTemporaryFile(r.FileGuid, r.FileName, r.FileSize);                     // 添付の項目に入れる (レコードを保存すると行に入る)
```

- モジュール名・項目名は、画面で入れる先の FileField を指します
- サービスアカウントを指したジョブから画面に渡すファイルは、`Save` ではなく `SaveTemporary` で作ります
- DB の列に内容を格納する添付には使えません

### Excel

画面のスクリプトと同じ `Excel` で、テンプレートに検索したデータを書き込めます。

| メソッド | 説明 |
|---|---|
| `OverWrite(data)` | テンプレートを検索で読んだデータ（`ExecuteRaw` 等の行）で埋めます。記法は画面の `OverWrite` と同じです |
| `ToMemoryStream()` / `ToPdfMemoryStream()` | 書き込んだ xlsx / PDF に変換した内容。`FileService.Save` や `SaveTemporary` に渡します |
| `ToAttachment(fileName)` / `ToPdfAttachment(fileName)` | 書き込んだ xlsx / PDF をメールの添付にします |

- `Download()` / `DownloadPdf()` は使えません（ダウンロードするブラウザがありません）。ファイルはメールの添付・レコードの添付・`SaveTemporary` で画面に渡します

```csharp
var change = order.CreateUpdateData();
using (var excel = new Excel(Resources.GetMemoryStream("Reports/Order.xlsx"), "Order.xlsx"))
{
    excel.OverWrite(order);
    change.Sheet = FileService.Save("Order", "Sheet", "注文書.pdf", excel.ToPdfMemoryStream());
}
```

### メール (MailService)

`MailService.Send(message)` でメールを送ります。設定（送信インフラ・送信履歴など）は画面のメールと同じです。戻り値の `IsSuccess` / `Failures` で結果を確かめます（送れなくても実行時エラーにはなりません）。

```csharp
var mail = new MailMessage();
mail.To.Add(row.AssigneeEmail.Value);
mail.Subject = "期限が近いタスク: " + row.Title.Value;
mail.Body = row.Title.Value + " の期限は " + row.DueDate.Value + " です。";
var result = MailService.Send(mail);
if (!result.IsSuccess) Logger.Error(result.Failures[0].To + ": " + result.Failures[0].Error);
```

- **送ったメールは取り消せません**。ジョブが後で失敗すると書き込みは取り消されますが、送ったメールは戻りません。データを書くジョブでは、**書き込みを `DBService.Commit()` で確定してから送ります**
- 差出人は送信インフラの設定のシステム送信者です

`Excel` と `MailService` は、アプリのテンプレートで作ったアプリでは使えるようになっています。

---

## 10. 他のスクリプトの呼び出し

| 呼べるもの | 書き方 |
|---|---|
| モジュールの const / static 関数 | `Order.StatusOpen` / `Order.Label("x")` |
| 別の Job の const / static 関数 | `ジョブ名.関数()` |

- モジュールのスクリプトから Job は呼べません
- Job の名前はモジュール名・列挙型名と重ねないでください（重なると `名前.` はモジュール・列挙型を指します）

---

## 11. 注意

- **デザイナのブレークポイントでは止まりません**。動きは `Logger` の出力と、失敗の文言（スクリプトの位置つき）で追います。`Logger` の出力先は、コマンドライン・別プロセスでは標準出力、プロセス内実行ではアプリのログです
- 変数は実行ごとに初期化されます。実行と実行の間で値は残りません
- 同じジョブが同時に動くことがあります（二人がボタンを押す・前回の定期実行が終わっていない等）。JobTriggerField の「同時実行を許す」が `false`（既定）なら、1 台のサーバーの中では画面のボタンからの重ねての起動は止まります。厳密な排他が要るならジョブの中で取ります
- ジョブがデータを保存しても、開いている画面には反映されません

---

## 12. サンプル

[サンプルギャラリー](https://lowcodedemo.azurewebsites.net/) に、在庫の発注候補表（Excel）を作る Job があります。

- [ReorderReport.job.cs](../../Samples/LowCodeSamples/Design/Jobs/ReorderReport.job.cs) — 発注点を下回った商品を検索し、仕入先ごとにまとめて Excel のテンプレートに書き込み、`SaveTemporary` で画面に渡します。仕入先別の小計を `args.Result` で返します
- [JobDemo.mod.cs](../../Samples/LowCodeSamples/Design/Modules/Job/JobDemo.mod.cs) — 上の Job を JobTriggerField で動かし、返ってきた結果を画面に出して Excel をダウンロードする画面のスクリプト

---

## 関連項目

- [バッチ処理（ジョブ）](../user_code/batch_job.md) — C# で書くジョブ・コマンドラインと定期実行
- [ジョブトリガー（JobTriggerField）](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/JobTriggerField.md) — 画面のボタンからジョブを実行する
- [スクリプト概要](script.md)
- [ModuleSearcher / BatchSearcher](script_module_searcher.md)
- [ModuleAggregator / BatchAggregator](script_module_aggregator.md)
