# スクリプト作成ガイドライン

Claude Code でスクリプト (`.mod.cs`) を作成する際の規約・注意事項。

---

## 明細の集計はサーバー問い合わせではなく画面上のデータを使う

DetailListField / ListField の行データを集計する場合、`ModuleSearcher` でDBに問い合わせるのではなく、フィールドの `Rows` プロパティから現在の画面上のデータを使って計算する。

### 理由

- `ModuleSearcher` はDBに保存済みのデータしか返さない
- ユーザーが編集中（未保存）の明細行が反映されない
- 不要なサーバーアクセスが発生する

### 正しい例

```csharp
void Items_OnDataChanged()
{
    decimal total = 0;
    foreach (var row in Items.Rows)
    {
        var item = (ExpenseItem)row;
        if (item.Amount.Value != null)
        {
            total += item.Amount.Value;
        }
    }
    TotalAmount.Value = total;
}
```

### 誤った例

```csharp
// ❌ DBから取得し直すのは不適切
void Items_OnDataChanged()
{
    var search = new ModuleSearcher<ExpenseItem>();
    search.AddEquals(e => e.ExpenseReportId.Value, Id.Value);
    var items = search.Execute();

    decimal total = 0;
    foreach (var item in items)
    {
        if (item.Amount.Value != null)
        {
            total += item.Amount.Value;
        }
    }
    TotalAmount.Value = total;
}
```

### 補足

- `Items.Rows` は `List<Module>` 型。具象型にキャストして使う（例: `(ExpenseItem)row`）
- `ModuleSearcher` は、画面上に無いデータを検索する場合（マスタ参照、別モジュールの検索等）に使う

---

## スクリプトから使えるフィールドプロパティ

スクリプト (`.mod.cs`) から設定可能なフィールドプロパティは以下のみ:

- `Color`, `BackgroundColor` - 表示色
- `IsEnabled` - 有効/無効
- `IsVisible` - 表示/非表示
- `IsViewOnly` - 閲覧専用
- `IsValid`, `ErrorText` - バリデーション状態
- `ClassName` - CSSクラス
- `IgnoreModification` - 変更検知除外

### ⚠️ `IsRequired` はスクリプトから設定できない

`IsRequired` はデザイン時プロパティ（JSON で定義）であり、ランタイムスクリプトからはアクセスできない。

条件付きで必須にしたい場合は、`IsVisible` で表示/非表示を切り替える方法で対応する。

```csharp
// ❌ エラー: IsRequired はスクリプトから使えない
CustomerId.IsRequired = true;

// ✅ 正しい: IsVisible で表示切り替え + 値クリア
CustomerId.IsVisible = isEntertainment;
if (!isEntertainment)
{
    CustomerId.Value = null;
}
```

---

## this でモジュールメソッドを呼ぶ

モジュール自体のメソッド（Submit, Delete, Reload, ValidateInput 等）は `this.` を付けて呼ぶ。

```csharp
// ❌ 誤り: this がないと見つからない
Submit();
ValidateInput();

// ✅ 正しい
this.Submit();
this.ValidateInput();
this.Reload();
this.Delete();
this.NewModule();
this.CopyModule();
```

フィールドへのアクセスは `this.` 不要（暗黙的にモジュールスコープ）。

```csharp
Name.Value = "テスト";    // OK
this.Name.Value = "テスト"; // これもOK（明示的）
```

---

## モジュール変数名をフィールド名と被らせない

モジュールのトップレベル変数（`int counter;` のような宣言）を、同じモジュールの **フィールド名と同じ名前**にしてはいけない。デザインチェックでエラーになり、実行時もモジュール作成時にエラーになる。

裸の識別子は **「変数 → フィールド」の順**で解決されるため、同名だと変数がフィールドを隠してしまう（フィールドへは `this.フィールド名` でしか到達できなくなる）。

```csharp
// ❌ Field "Total" があるのに同名のモジュール変数を宣言 → エラー
decimal Total;

// ✅ 別名にする
decimal totalCache;
```

関数内の **ローカル変数**はフィールドと同名でも可（C# と同じシャドーイング。`this.フィールド名` でフィールドに到達できる）。ただし紛らわしいので避けるのが無難。

---

## 予約名フィールド (`Creator` / `CreatedAt` / `Updater` / `UpdatedAt`) はスクリプトで代入しない

`SystemFieldNames` 予約名 (`Creator` / `CreatedAt` / `Updater` / `UpdatedAt`) のフィールドは、
**CLB の保存処理 (Submit) で自動的に値が入る** — `Creator`/`Updater` には現在ユーザーの Id、
`CreatedAt`/`UpdatedAt` には現在時刻がセットされる。

したがってスクリプトの初期化処理で以下のように代入する**必要はない**:

```csharp
// NG (冗長): CLB が自動でセットするので不要
void OnAfterInitialization()
{
    if (!IsNewData) return;
    Creator.Value = CurrentUser.Id.Value;
    CreatedAt.Value = DateTime.Now;
    Order.AddRow(...);
}
```

```csharp
// OK: 予約名フィールドは触らない
void OnAfterInitialization()
{
    if (!IsNewData) return;
    Order.AddRow(...);
}
```

書いても結果は同じ (同じ値で上書きするだけ) だが、保守する人に「特別な処理が必要なのか」と
誤解させる。**何もしないのが正解**。子モジュールの予約名も同様で、親が `flow.Creator.Value = ...` を
書く必要はない。

ただしカスタム値を入れたい場合 (例: 「他人代理申請で `Creator` を別人に固定」「`CreatedAt` を業務基準時刻に
強制」) は当然代入してよい。デフォルト挙動 (= 現在ユーザー / 現在時刻) に従う場合だけ書かない。

詳細は CLAUDE.md の #47 `SystemFieldNames` を参照。

---

## IsNewData で新規/既存を判定する

新規レコード（まだDBに保存されていない）かどうかは `this.IsNewData` で判定する。

```csharp
void Detail_OnAfterInit()
{
    if (this.IsNewData)
    {
        // 新規データの場合のデフォルト値
        Status.Value = "Draft";
        CreatedDate.Value = DateOnly.FromDateTime(DateTime.Today);
    }
    else
    {
        // 既存データの場合
        EditPanel.IsViewOnly = true;
    }
}
```

---

## 保存前にバリデーションを行う

カスタム保存処理を書く場合、`this.ValidateInput()` で全フィールドを検証してから `this.Submit()` する。

```csharp
void SaveButton_OnClick()
{
    // 全フィールドのバリデーション
    if (!this.ValidateInput())
    {
        MessageBox.Show("入力エラーがあります。修正してください。");
        return;
    }

    // カスタムバリデーション
    if (StartDate.Value != null && EndDate.Value != null && StartDate.Value > EndDate.Value)
    {
        EndDate.SetError("終了日は開始日以降にしてください");
        return;
    }

    this.Submit();
    MessageBox.Show("保存しました");
}
```

---

## ModuleSearcher の正しい使い方

`ModuleSearcher` は別モジュールのDBデータを検索するためのもの。画面上のデータではなくDB保存済みデータを返す。

### 正しい用途

- マスタ参照（別モジュールのデータ取得）
- SelectField/LinkField の候補フィルタリング
- 他モジュールのデータ検索

```csharp
// マスタ参照: 商品の単価を取得
var searcher = new ModuleSearcher<Product>();
searcher.AddEquals(e => e.Code.Value, ProductCode.Value);
var products = searcher.Execute();
if (products.Count > 0)
{
    UnitPrice.Value = products[0].Price.Value;
}

// SelectFieldの候補フィルタリング
var catSearcher = new ModuleSearcher<SubCategory>();
catSearcher.AddEquals(e => e.CategoryId.Value, Category.Value);
SubCategory.SetAdditionalCondition(catSearcher);
SubCategory.ReloadCandidates();
```

### 誤った用途

- 画面上の明細行の集計 → `Rows` プロパティを使う（[CommonMistakes.md](CommonMistakes.md) の #4 参照）

---

## 帳票出力パターン

Excelテンプレートを使った帳票出力は2パターンある。`using (var memory = ...)` ブロック形式を使い、Excelコンストラクタの第2引数はファイル名（拡張子なし）。

### パターン1: Resources のテンプレートを使用

```csharp
void PdfButton_OnClick()
{
    using (var memory = Resources.GetMemoryStream("Invoice.xlsx"))
    {
        var excel = new Excel(memory, "Invoice");
        excel.OverWrite(this);
        excel.DownloadPdf();
    }
}
```

### パターン2: FileField に登録されたファイルを使用

```csharp
void ExcelButton_OnClick()
{
    using (var memory = Template.GetMemoryStream())
    {
        var excel = new Excel(memory, "Report");
        excel.OverWrite(this);
        excel.Download();
    }
}
```

### 出力メソッド

| メソッド | 出力形式 |
|---|---|
| `excel.Download()` | Excel (.xlsx) |
| `excel.DownloadPdf()` | PDF |

### OverWrite のプレースホルダー書式

`excel.OverWrite(this)` は、Excelテンプレート内の `{{FieldName.Property}}` 形式のプレースホルダーをモジュールのフィールド値で置換する。

```
{{Name.Value}}           → TextField の値
{{Price.Value}}          → NumberField の値
{{CreatedDate.Value}}    → DateField の値
{{Category.DisplayText}} → LinkField の表示テキスト
```

### セルの個別操作

`OverWrite` ではカバーできない場合、セルを個別に操作できる。

```csharp
void CustomExport_OnClick()
{
    using (var memory = Resources.GetMemoryStream("Template.xlsx"))
    {
        var excel = new Excel(memory, "CustomReport");

        // セルをテキストで検索して値を設定
        var cell = excel.FindCellByText("{{Title}}");
        if (cell != null)
        {
            excel.SetCellValue(cell, Title.Value);
        }

        // 行のコピー（テンプレート行を複製して明細を展開）
        var startCell = new ExcelCellIndex();
        startCell.RowIndex = 5;
        startCell.ColumnIndex = 0;
        var destCell = startCell.GetNext(1, 0);
        excel.CopyCells(startCell, destCell, 1, 5);

        excel.DownloadPdf();
    }
}
```

### 注意事項

- `Excel` は `IDisposable` のため、**必ず `using` ブロック内で使う**こと
- コンストラクタの第2引数（ファイル名）に**拡張子は不要**（`.xlsx` / `.pdf` は自動付与）
- `Download()` と `DownloadPdf()` は非同期メソッドだが、スクリプトエンジンが自動で await するため明示的な `await` は不要

### Excel メソッド一覧

全メソッドの詳細は [ScriptExtensions.md](ScriptExtensions.md) の Excel セクションを参照。

---

## Null条件演算子（?.）は使えない

スクリプトエンジンはNull条件演算子（`?.`、`?[]`）をサポートしていない。代わりに明示的なnullチェックを書く。

```csharp
// ❌ エラー: ?. はサポートされない
var name = product?.Name.Value;

// ✅ 正しい: 明示的にnullチェック
string name = null;
if (product != null)
{
    name = product.Name.Value;
}

// ✅ Null合体演算子（??）は使える
var value = Price.Value ?? 0;
```

---

## Rows の行を具象型にキャストする

ListField / DetailListField の `Rows` は `List<Module>` 型。各行を具象モジュール型にキャストしてフィールドにアクセスする。

```csharp
void Items_OnDataChanged()
{
    decimal total = 0;
    foreach (var row in Items.Rows)
    {
        var item = (OrderItem)row;  // 具象型にキャスト
        if (item.Amount.Value != null)
        {
            total += item.Amount.Value;
        }
    }
    TotalAmount.Value = total;
}
```

## 式メソッド / 複数引数ラムダは使えない

CLB スクリプトインタプリタは C# の以下を**サポートしない**。ロード時に「不正な構文です」エラーになる。

```csharp
// NG: 式メソッド (expression-bodied method)
string GetName() => "alice";

// OK: ブロック体
string GetName()
{
    return "alice";
}
```

```csharp
// NG: 複数引数ラムダ
list.Sort((a, b) => a.OrderNo.CompareTo(b.OrderNo));

// OK: 手動バブルソート等
for (int i = 0; i < list.Count; i++) {
    for (int j = i + 1; j < list.Count; j++) {
        if (list[i].OrderNo > list[j].OrderNo) {
            var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
    }
}
```

単一引数ラムダ (`x => x.Value`) は `ModuleSearcher` の条件メソッドでのみ使える ([Docs/Scripts.md](Scripts.md) のラムダ式セクション参照)。

---

## ChildModule の LinkField / SelectField は `.Value` がロード遅延で空のことがある

`DetailListField` の中の Module (孫レコード等) は、親の `OnAfterInitialization` タイミングでまだ完全にロードされていないことがある。具体的には:

- ChildModule の **LinkField** の `.Value` が空 (`""`)
- ChildModule の **SelectField** の `.Value` が空 (`""`)

これにより `if (member.ApproverUser.Value == CurrentUser.Id.Value)` のような比較が常に false になり、判定ロジックが壊れる。

```csharp
// NG: ChildModule の LinkField/SelectField を直接見て判定する
void OnAfterInitialization()
{
    foreach (var o in Orders.Rows)
    {
        if (o.Status.Value != "Active") continue;   // ← 空文字なので常に continue
        foreach (var m in o.Members.Rows)
        {
            if (m.ApproverUser.Value == CurrentUser.Id.Value) { ... }  // ← 空文字なので常に false
        }
    }
}
```

**対処**: `ModuleSearcher` で DB から直接検索する。

```csharp
// OK: 親モジュール経由の Id (= Order.Id) を使って ModuleSearcher で検索
void OnAfterInitialization()
{
    foreach (var o in Orders.Rows)  // Rows と Id.Value は取れる
    {
        var s = new ModuleSearcher<ApprovalFlowMember>();
        s.AddEquals(m => m.ApprovalFlowOrderId.Value, o.Id.Value);
        s.AddEquals(m => m.ApproverUser.Value, CurrentUser.Id.Value);
        s.AddEquals(m => m.Status.Value, "Waiting");
        if (s.Execute().Count > 0) { /* 自分宛の Waiting Member あり */ }
    }
}
```

トップレベルの親モジュール (Detail Page のメインモジュール) の LinkField/SelectField は `OnAfterInitialization` で問題なく取れる。**ChildModule (DetailListField 経由) だけがこの罠**。

---

## 新規データの Id (`@temporary:guid`) を ModuleSearcher で数値列に渡さない

新規モジュールには `@temporary:<guid>` のようなテンポラリ ID 文字列が入っている (実Id確定前)。これを `ModuleSearcher` で数値カラムにぶつけると **`The input string '@temporary:...' was not in a correct format.` 実行時エラー**になる。

```csharp
// NG: 新規時は this.Id.Value が @temporary:guid で数値列検索が失敗する
void UpdateFlowSummary()
{
    var hs = new ModuleSearcher<ApprovalHistory>();
    hs.AddEquals(h => h.ApprovalFlowId.Value, this.Id.Value);  // ← エラー
    ...
}

// OK: IsNewData ガードで早期 return
void UpdateFlowSummary()
{
    if (GetParentModule().IsNewData) { FlowSummary.Value = ""; return; }
    var hs = new ModuleSearcher<ApprovalHistory>();
    hs.AddEquals(h => h.ApprovalFlowId.Value, this.Id.Value);  // 既存なら実 Id
    ...
}
```

`IsNewData` 判定は `Id.Value == null` ではなく `Module.IsNewData` プロパティを使う (テンポラリ ID は非 null なので)。

---

## 動的型は `var` で受ける

`GetParentModule()` / `Rows[i]` 等で返るオブジェクトは動的型 (`object`)。`bool` や `int` で明示的に型宣言すると **「bool = var 不正な代入です」**エラーになる。

```csharp
// NG
void OnAfterInitialization()
{
    var parent = GetParentModule();
    bool isNew = parent.IsNewData;  // ← bool = var 不正な代入
}

// OK
void OnAfterInitialization()
{
    var parent = GetParentModule();
    var isNew = parent.IsNewData;
}
```

例外: フィールドの `.Value` のように明示的に型がある場合は `int x = field.Value ?? 0;` 等 OK。

---

## 複数フィールド更新 + 通信は SuspendNotifyStateChanged + StartLoading でまとめる

承認/却下/キャンセルのようなボタンハンドラで「**複数フィールド変更 + 1回以上の Submit**」を行うとき、何もしないと:

- 各 `Field.Value = ...` で内部の `NotifyStateChanged()` が走り、画面が**チラつく**
- Submit の通信中にローディングオーバーレイが**短時間 ON/OFF を繰り返してチラつく**

対策は 2 つの `using` スコープを最初に張ること:

```csharp
void Approve_OnClick()
{
    var member = GetCurrentMemberForUserStrict();
    if (member == null) { Toaster.Error("承認権限がありません"); return; }

    using var suspend = GetParentModule().SuspendNotifyStateChanged();
    using var loading = LoadingService.StartLoading(0);

    member.Status.Value = "Approved";
    member.ActorUser.Value = CurrentUser.Id.Value;
    member.ApprovedAt.Value = DateTime.Now;
    // ... 集計・遷移計算 (途中再描画されない)
    AddHistory("Approve", Comment.Value);

    var ret = GetParentModule().Submit();
    if (ret == true) Toaster.Success("承認しました");
}
```

### `SuspendNotifyStateChanged` の挙動

- 呼ぶと `Module._blockNotifyStateChangedCount++`、戻り値 (IDisposable) を `using` で受ける
- `using` スコープ内では `NotifyStateChanged()` を呼んでも実際の StateHasChanged は走らず、Pending キューに溜まる
- Dispose で `ResumeNotifyStateChanged()` が呼ばれカウント減、0 になると溜まった通知が一括 flush → 1 回だけ再描画
- 効果: 複数フィールド変更の途中で画面が再描画されず、最終状態だけ一度に描画される
- 親モジュールに対して呼ぶのが基本 (子の状態も巻き込んでサスペンドされる)

### `LoadingService.StartLoading(int? delayTime)` の 2 つの使い方

**使い方 1: 複数の通信処理をまとめてローディングインジケータを 1 回にする (delayTime=0)**

```csharp
using var loading = LoadingService.StartLoading(0);
parent.Submit();          // 1 回目の通信
SomeOtherFlow.Reload();   // 2 回目の通信
parent.Submit();          // 3 回目
// using スコープ全体で 1 つのローディング表示
```

複数 scope が `_loadingCount` でカウントされ、最後の Dispose で `OnLoadingChanged(false)` が発火。途中で別の `Submit()` が内部で勝手にローディング開閉しても、外側 scope が生きている限り表示は維持される。

**使い方 2: 短時間処理のチラつき防止 (delayTime を大きく)**

```csharp
using var scope = LoadingService.StartLoading(1000);  // 1秒経たないと表示しない
var ret = this.Submit();
// 100ms で終わる Submit ならローディング表示されないまま終わる
```

- `delayTime` は **最初の `StartLoading()` 呼び出し時の値が記録される** (以後の呼び出しは無視)
- 数値 ms 経過後に `OnLoadingChanged(true)`、それより前に Dispose されれば表示されない
- 短時間で終わる Submit/Reload のチラつきを完全に消せる

### 使い分けの目安

| シナリオ | 推奨 |
|---|---|
| 通信が 1 回だけ、短時間で終わる可能性が高い | `StartLoading(1000)` (チラつき防止) |
| 通信が複数回 (Submit → Reload → Submit 等) | `StartLoading(0)` + 全体を using スコープで囲む |
| 複数フィールドを連続で書き換える | `SuspendNotifyStateChanged()` で再描画を 1 回に |

承認フロー系ハンドラ (Approve/Reject/Cancel/Resubmit) では **両方を組み合わせる**のがベストプラクティス。

### 通信を含むヘルパー関数も `using` の中で呼ぶ

`ModuleSearcher.Execute()` を内部で呼ぶヘルパー (例: `IsCurrentUserCreator()` / `GetCurrentMemberForUserStrict()`) は**通信**。`using var loading = LoadingService.StartLoading(0);` より**外**で呼ぶと、その通信が独立してインジケータを ON/OFF してチラつく。

```csharp
// NG: 権限チェックの通信が独立してローディング表示する
void Approve_OnClick()
{
    var member = GetCurrentMemberForUserStrict();  // ← この通信が独立してインジケータ点滅
    if (member == null) { Toaster.Error("..."); return; }
    using var loading = LoadingService.StartLoading(0);
    // ... Submit
}

// OK: 通信を全部 using スコープ内に
void Approve_OnClick()
{
    using var suspend = GetParentModule().SuspendNotifyStateChanged();
    using var loading = LoadingService.StartLoading(0);
    var member = GetCurrentMemberForUserStrict();  // ← まとまる
    if (member == null) { Toaster.Error("..."); return; }
    // ... Submit
}
```

「即時 return 用の純粋な値チェック」(`if (Status.Value != "Pending") return;` 等、通信しないもの) は `using` の上に置いて OK。基準は「**通信が走るかどうか**」。

---

## 名前付き引数 (`name: value`) は使えない

CLB のスクリプトインタプリタは C# の **名前付き引数構文をサポートしない**。引数は必ず**位置引数で渡す**こと。

```csharp
// NG: ロード時に "isDefault: 不正なシンタックスです" エラー
new PrimaryButton("確定", isDefault: true)

// OK: 位置で渡す
new PrimaryButton("確定", true)

// NG
MessageBox.ShowWithTitle(title: "確認", message: "削除しますか?", "はい", "いいえ")

// OK
MessageBox.ShowWithTitle("確認", "削除しますか?", "はい", "いいえ")
```

オーバーロードや引数の意味は呼び出し先 C# シグネチャ (Source/Codeer.LowCode.Blazor/... または Source/App/WebApp.Client.Shared/ScriptObjects/...) で確認する。


## Submit() / Reload() を呼んだ後は結果を確認する

`Module.Submit()` は `bool?` を返すので、必ず戻り値を受けて分岐する。**結果を確認せずに連続処理 (Reload 等) を書くのは NG**。

### 戻り値の意味

| 値 | 意味 | スクリプトでの扱い |
|---|---|---|
| `null` | 送信データなし (Module が変更されていない) | 成功扱い (何もせず後続処理に進める) |
| `false` | 通信失敗 / サーバ側エラー | ユーザに通知 (`Toaster.Error(...)`) して後続を止める |
| `true` | 成功 | 後続処理 (`Reload`, 画面遷移など) を実行 |

### 推奨パターン

```csharp
void SaveButton_OnClick()
{
    using var scope = LoadingService.StartLoading(1000);  // 1秒以上かかる時だけ Loading 表示
    if (Editor.ChildModule == null) return;
    if (Editor.ChildModule.Id.Value == null) return;

    var ret = Editor.ChildModule.Submit();
    if (ret == false) {
        Toaster.Error("更新失敗");
        return;  // 失敗時は後続を止める判断もアリ
    }
    Items.Reload();  // 名前変更などを一覧側に反映
}
```

### ローディングインジケータの抑制

`Module.Submit()` / `Module.Reload()` は内部で HTTP 通信するため、CLB 標準では即座にローディングオーバーレイ (中央に「Loading」スピナー) が表示される。瞬時に完了する処理 (SQLite ローカル等) では「一瞬チラつく」だけになりうっとうしい。

`LoadingService.StartLoading(int delayMs)` で **N ms 以上かかった場合のみインジケータを出す** スコープを張れる:

```csharp
void Foo_OnClick()
{
    using var scope = LoadingService.StartLoading(1000);  // 1秒以上で初めて Loading 表示
    SomeModule.Submit();
    SomeModule.Reload();
}
```

スコープが Dispose されるまでの間に発火する Loading は `delayMs` の遅延を経てから表示される。短時間で済む処理は無音、本当に重い処理だけインジケータが出るので UX が良い。

### NG パターン

```csharp
// NG: 戻り値を捨てて常に Reload。失敗していても何の通知も出ない
Editor.ChildModule.Submit();
Items.Reload();

// NG: ret == false の通知が無い
var ret = Editor.ChildModule.Submit();
if (ret == true) Items.Reload();
// 失敗時は無言なのでユーザ気づけない
```
