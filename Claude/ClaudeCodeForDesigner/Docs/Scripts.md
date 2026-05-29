# スクリプト (*.mod.cs)

モジュールのイベントハンドラをC#ライクな構文で記述するスクリプトファイルのリファレンス。

スクリプトエンジンは Roslyn (Microsoft.CodeAnalysis) でC#構文木を解析し、インタープリタ方式で実行する。
C#のサブセットをサポートしており、コンパイル不要でビジネスロジックを記述できる。

## ファイル命名規則

```
{ModuleName}.mod.cs
```

- モジュール定義ファイル (`{ModuleName}.mod.json`) と同じフォルダに配置
- ファイル名のモジュール名部分は `.mod.json` と完全に一致させる

---

## スクリプトの基本構造

スクリプトファイルにはクラス定義は不要。メソッド（イベントハンドラ）とモジュールレベル変数を直接記述する。

```csharp
// モジュールレベル変数（全関数で共有）
int counter = 0;

// イベントハンドラ
void SaveButton_OnClick()
{
    counter++;
    Name.Value = "テスト";
}

// 引数付きイベントハンドラ
void Detail_OnFieldChanged(string fieldName)
{
    if (fieldName == "Price")
    {
        Total.Value = Price.Value + Tax.Value;
    }
}

// 戻り値ありイベントハンドラ
bool Detail_OnLeaving()
{
    return true; // false でページ遷移キャンセル
}

// ユーザー定義関数
decimal CalcTax(decimal price)
{
    return Math.Round(price * 0.1, 0, MidpointRounding.AwayFromZero);
}
```

---

## スクリプト内で利用可能なオブジェクト

### モジュールフィールド

モジュール内の全フィールドに、フィールド名で直接アクセスできる。

```csharp
// 値フィールドの値にアクセス
Name.Value = "テスト";
Price.Value = 1000;
IsActive.Value = true;

// フィールドの表示制御
Name.IsEnabled = false;
Price.IsVisible = false;
```

フィールドはアプリケーション側で独自に追加可能。標準フィールドおよびフィールド共通のスクリプトAPIは [Fields/_ScriptApi.md](Fields/_ScriptApi.md) を参照。各フィールド型固有のプロパティ・メソッド・イベントハンドラ例は `Docs/Fields/` の各フィールドドキュメントを参照。

### レイアウト要素

名前付きレイアウト要素にアクセスし、表示制御が可能。

```csharp
// GridLayoutDesign の Name で指定した名前でアクセス
GridTarget.IsEnabled = false;   // 無効化
GridTarget.IsVisible = false;   // 非表示
GridTarget.IsViewOnly = true;   // 読み取り専用
```

### this キーワード

`this` は現在のモジュールインスタンスを参照する。

```csharp
this.IsEnabled = true;
this.IsViewOnly = false;
this.PageTitle = "新しいタイトル";

// モジュールレベル変数へのアクセス（thisは省略可）
int yyy = 100;
void Func1()
{
    this.yyy++;  // thisで明示的に参照
    yyy++;       // 省略も可
}
```

---

## 文法リファレンス

### 変数宣言と型

```csharp
// 型指定
int x = 100;
string text = "abc";
bool flag = true;
decimal amount = 10.5;

// 型推論
var y = 200;

// Nullable
int? nullableInt = null;
int? nullableInt2 = 100;

// 複数宣言
int a, b;
a = 3;
b = 3;

// 連続代入
int a, b;
a = b = 3;  // 両方 3
```

### サポートされるプリミティブ型

| 型 | 説明 |
|---|---|
| `bool` | 真偽値 |
| `byte`, `short`, `ushort` | 小数部なし整数 |
| `int`, `uint` | 32ビット整数 |
| `long`, `ulong` | 64ビット整数 |
| `float`, `double` | 浮動小数点数 |
| `decimal` | 高精度数値 |
| `char` | 文字 |
| `string` | 文字列 |
| `object` | 汎用オブジェクト |

### サポートされるシステム型

| 型 | 説明 | コンストラクタ例 |
|---|---|---|
| `DateTime` | 日時 | `new DateTime(2025, 1, 2)` |
| `DateOnly` | 日付 | `new DateOnly(2025, 1, 2)` |
| `TimeOnly` | 時刻 | `new TimeOnly(13, 30)` |
| `TimeSpan` | 時間間隔 | `new TimeSpan(1, 30, 0)` |
| `DateTimeOffset` | タイムゾーン付き日時 | `new DateTimeOffset(...)` |
| `Guid` | GUID | `new Guid("...")` |
| `MemoryStream` | メモリストリーム | `new MemoryStream()` |
| `StreamContent` | HTTPコンテンツ | `new StreamContent(stream)` |
| `JsonObject` | JSON動的オブジェクト | `new JsonObject()` |
| `Stopwatch` | 計測タイマー | `new Stopwatch()` |

### コレクション型

```csharp
// 配列
int[] ary = new int[10];
ary[0] = 100;

// 配列の初期化子
int[] ary = new int[]{1, 2, 3};

// List<T>
List<int> list = new List<int>();
list.Add(100);
list.Add(200);
var count = list.Count;
var first = list[0];

// Dictionary<K,V>
Dictionary<string, int> dict = new Dictionary<string, int>();
dict.Add("A", 100);
var val = dict["A"];

// HashSet<T>
HashSet<string> set = new HashSet<string>();
set.Add("item");

// モジュール配列
Home[] modules = new Home[10];
modules[0] = new Home();

// ネストされたList
var nested = new List<List<int>>();
var inner = new List<int>();
inner.Add(100);
nested.Add(inner);
```

> **注意**: 多次元配列 (`int[,]`, `int[][]`) はサポートされない。

### 算術演算子

```csharp
var result = (100 + 200) * (1 + 3);  // 四則演算
var mod = 10 % 3;                      // 剰余
var neg = -1;                          // 単項マイナス

// 複合代入
result += 5;   // 加算代入
result -= 1;   // 減算代入
result *= 2;   // 乗算代入
result /= 2;   // 除算代入
result %= 3;   // 剰余代入

// インクリメント/デクリメント
var x = 100;
x++;   // 後置インクリメント
++x;   // 前置インクリメント
x--;   // 後置デクリメント
--x;   // 前置デクリメント
```

> **注意**: 異なる数値型の演算は自動的に `decimal` に変換される。

### 比較演算子

```csharp
if (x == 100) { }     // 等価
if (x != 100) { }     // 非等価
if (x < 100) { }      // 未満
if (x > 100) { }      // 超過
if (x <= 100) { }     // 以下
if (x >= 100) { }     // 以上

// DateTime / DateOnly の比較も可能
var a = new DateTime(2025, 1, 2);
var b = new DateTime(2025, 1, 3);
var isEarlier = a < b;  // true
```

### 論理演算子

```csharp
var a = true;
var b = false;

if (a && b) { }     // 論理AND（短絡評価）
if (a || b) { }     // 論理OR（短絡評価）
var result = !false; // 論理NOT

// 短絡評価の挙動
if (true || HeavyFunc()) {}   // HeavyFunc() は呼ばれない
if (false && HeavyFunc()) {}  // HeavyFunc() は呼ばれない
```

### ビット演算子

```csharp
var result = 0xFF;
result &= 0x80;    // ビットAND代入
result |= 0x08;    // ビットOR代入

var abc = 0x08 | 0x80;  // ビットOR
var def = 0x08 & 0x80;  // ビットAND
```

### Null合体演算子

```csharp
int? a = null;
var result = a ?? 100;  // a が null なら 100

int? b = 200;
var result2 = b ?? 100; // b が null でないので 200
```

### Null条件演算子（`?.`）

nullの可能性があるオブジェクトに安全にアクセスする。左辺がnullの場合、式全体がnullを返す。

```csharp
// プロパティアクセス
var value = product?.Name.Value;  // product が null なら null

// メソッド呼び出し
var text = name?.ToString();  // name が null なら null

// チェーン
var upper = product?.Name.Value?.ToUpper();  // 途中で null なら null

// Null合体演算子との組み合わせ
var displayName = product?.Name.Value ?? "未設定";
```

> **注意**: Null条件インデクサ（`?[]`）はサポートされない。

### 三項演算子

```csharp
var x = 100;
var y = 200;
var result = x < 100 ? 1 : 200 <= y ? 2 : 3;  // ネスト可能
```

### 型キャスト

```csharp
int x = 100;
var d = (double)x;     // int → double

double y = 100.9;
var i = (int)y;        // double → int は切り捨て (100。四捨五入ではない)

int z = 100;
var dec = (decimal)z;  // int → decimal

// Nullableキャスト
int? nx = null;
var nd = (double?)nx;  // null のまま

int? nx2 = 100;
var nd2 = (double?)nx2; // 100.0

// モジュールキャスト
Module x = new SearchTestAB();
var typed = (SearchTestAB)x;
```

### 制御構文

#### if / else

```csharp
var abc = 100;
if (abc == 100)
{
    return true;
}
else if (abc == 200)
{
    return false;
}
else
{
    return false;
}
```

#### for

```csharp
var result = 0;
for (var i = 0; i < 10; i++)
{
    result++;
}

// 無限ループ
for (;;)
{
    result++;
    if (10 <= result) break;
}
```

#### while

```csharp
var sum = 0;
while (sum < 10)
{
    sum++;
}
```

#### foreach

```csharp
var search = new ModuleSearcher<SearchTestAB>();
var list = search.Execute();
var sum = 0;
foreach (var row in list)
{
    sum += row.B.Value;
}
```

#### switch / case

```csharp
var x = 2;
var y = 0;
switch (x)
{
    case 1:
        y = 1;
        break;
    case 2:
        y = 2;
        break;
    default:
        y = 10;
        break;
}
```

#### break / continue

```csharp
for (var i = 0; i < 10; i++)
{
    if (i == 2) continue;  // 次のイテレーションへ
    if (i == 5) break;     // ループ終了
    result++;
}
```

#### return

```csharp
int Func(int x)
{
    if (x < 0) return -1;
    return x * 2;
}
```

### 関数定義

```csharp
// 型指定の関数
int Add(int a, int b)
{
    return a + b;
}

// var による型推論
var Multiply(var a, var b)
{
    return a * b;
}

// void 関数
void DoSomething()
{
    Name.Value = "updated";
}

// 関数呼び出し
var result = Add(100, 200);
```

### using 文

```csharp
// using ブロック
using (var memory = file.GetMemoryStream())
using (var excel = new Excel(memory, file.FileName))
{
    excel.OverWrite(this);
    excel.Download();
}

// using 宣言（C# 8スタイル）
using var memory = file.GetMemoryStream();
using var excel = new Excel(memory, file.FileName);
excel.OverWrite(this);
excel.Download();
```

### out / ref パラメータ

```csharp
// out パラメータ
int x = 0;
int.TryParse("100", out x);

// out var（暗黙型推論）
int.TryParse("100", out var x);

// ref パラメータ
int x = 0;
int.TryParse("100", ref x);
```

### コメント

```csharp
// 行コメント
var x = 100; // 末尾コメント

/* ブロックコメント */
/*
  複数行の
  コメント
*/
```

### スコープ規則

```csharp
// ブロックスコープ - 同名変数を異なるスコープで使用可能
for (var i = 0; i < 10; i++) { }
for (var i = 0; i < 10; i++) { }  // OK：別スコープ

// foreach も同様
foreach (var row in list1) { }
foreach (var row in list2) { }  // OK：別スコープ

// 同一スコープ内での重複はエラー
var x = 1;
var x = 2;  // エラー：変数名重複
```

### ラムダ式（限定的）

ラムダ式は `ModuleSearcher` のメソッド内でのみ使用可能。

```csharp
var search = new ModuleSearcher<Product>();
search.AddEquals(e => e.CategoryId.Value, "CAT001");
search.OrderBy(e => e.Name);
search.Select(e => e.Name, e => e.Price);
```

> **制限**: 一般的なラムダ式（`list.Where(x => x > 0)` 等のLINQ）はサポートされない。

---

## 組み込みサービス

スクリプト内でサービス名で直接アクセスできる。

### MessageBox

```csharp
// 単純なメッセージ
MessageBox.Show("処理が完了しました");

// 選択肢付き（戻り値は選択されたボタン文字列）
var result = MessageBox.Show("削除しますか？", "はい", "いいえ");
if (result == "はい")
{
    // 削除処理
}

// タイトル付き
var result = MessageBox.ShowWithTitle("確認", "データを保存しますか？", "OK", "キャンセル");

// カスタムボタンスタイル
var result = MessageBox.Show("操作を選択", new DangerButton("削除"), new SecondaryButton("キャンセル"));
```

**メソッド:**

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Show(string message, params string[] buttons)` | string | メッセージ表示。ボタン省略時は「OK」 |
| `ShowWithTitle(string title, string message, params string[] buttons)` | string | タイトル付きメッセージ |
| `Show(string message, params DialogButton[] buttons)` | string | カスタムスタイルボタン |
| `ShowWithTitle(string title, string message, params DialogButton[] buttons)` | string | タイトル付き + カスタムボタン |

**DialogButton 型:**
`DialogButton`, `PrimaryButton`, `SecondaryButton`, `SuccessButton`, `DangerButton`,
`WarningButton`, `InfoButton`, `LightButton`, `DarkButton`, `LinkButton`
および各Outlineバリアント (`OutlinePrimaryButton`, `OutlineSecondaryButton` 等)

### Logger

```csharp
Logger.Log("情報メッセージ");
Logger.Warn("警告メッセージ");
Logger.Error("エラーメッセージ");
```

### Toaster (App側拡張サービス)

画面右下にトースト通知を表示する。

```csharp
Toaster.Success("保存しました");
Toaster.Info("確認してください");
Toaster.Warn("注意が必要です");
Toaster.Error("失敗しました");
```

**メソッド:**

| メソッド | 説明 |
|---|---|
| `Success(string s)` | 緑色の成功トースト |
| `Info(string s)` | 青色の情報トースト |
| `Warn(string s)` | 黄色の警告トースト |
| `Error(string s)` | 赤色のエラートースト |

> **注意**: `Toaster.Warning` は存在しない。警告は `Toaster.Warn` を使う。

実装: `Source/App/WebApp.Client.Shared/ScriptObjects/Toaster.cs`

### LoadingService

ローディングオーバーレイの表示制御。`StartLoading(int? delayTime)` で `LoadingScope (IDisposable)` を返し、`using` スコープ内で重なりを管理する。

**2 つの使い方**:

```csharp
// 使い方 1: 短時間処理のチラつき防止 (1000ms 経たないと表示しない)
void SubmitButton_OnClick()
{
    using var scope = LoadingService.StartLoading(1000);
    var ret = this.Submit();
    if (ret != true) { Toaster.Error("保存失敗"); return; }
    Toaster.Success("保存しました");
}

// 使い方 2: 複数通信を 1 つのインジケータでまとめる (delayTime=0)
void Approve_OnClick()
{
    using var loading = LoadingService.StartLoading(0);
    parent.Submit();           // 1回目通信
    OtherList.Reload();         // 2回目通信
    parent.Submit();           // 3回目通信
    // ↑ using スコープ全体で 1 つのローディング表示にまとまる
}
```

挙動:
- `_loadingCount` で scope の重なりを管理。複数 scope を入れ子にしても 1 つの状態
- `delayTime` は **最初の呼び出しの値が記録される** (以後の値は無視)
- `delayTime` 経過前に全 scope が Dispose されれば一度も表示されない
- 内部の `Submit()` が短時間ローディング表示を試みても、外側 scope が生きている限り維持される

実装: `Source/Codeer.LowCode.Blazor/Components/AppParts/Loading/LoadingService.cs`

詳細な使い分けは [Docs/ScriptGuidelines.md](ScriptGuidelines.md) の「複数フィールド更新 + 通信は SuspendNotifyStateChanged + StartLoading でまとめる」セクション参照。

### NavigationService

```csharp
// ページ遷移
NavigationService.NavigateTo("/products");

// URL置換（履歴に残さない）
NavigationService.ReplaceTo("/products");

// モジュールURLの生成
var url = NavigationService.GetModuleUrl("Product");
var detailUrl = NavigationService.GetModuleDataUrl("Product", recordId);

// PageFrame指定
var url = NavigationService.GetModuleUrl("Main", "Product");
var detailUrl = NavigationService.GetModuleDataUrl("Main", "Product", recordId);

// クエリパラメータ取得
var qs = NavigationService.GetQueryString();
var params = NavigationService.GetQueryParameters();        // Dictionary<string, List<string>>
var unique = NavigationService.GetUniqueQueryParameters();  // Dictionary<string, string>

// 現在のURL情報
var pageFrame = NavigationService.PageFrameUrlSegment;
var module = NavigationService.ModuleUrlSegment;

// ログアウト
NavigationService.Logout();
```

### Resources

```csharp
// テキストリソース取得
var text = Resources.GetText("templates/email.html");

// バイナリリソース取得
var stream = Resources.GetMemoryStream("images/logo.png");

// ローカライズ
var localized = Resources.Localize("保存しました");
```

### CurrentUser

`app.clprj` の `CurrentUserModuleDesignName` が設定されている場合のみ使用可能。ログインユーザーのモジュールインスタンスとしてアクセスできる。

```csharp
var userName = CurrentUser.Name.Value;
var role = CurrentUser.Role.Value;
```

### Math（静的クラス）

.NET の `System.Math` 静的メソッドがそのまま使用可能。

```csharp
var abs = Math.Abs(-100);
var rounded = Math.Round(10.456, 2);
var roundedUp = Math.Round(Price.Value * 0.1, 0, MidpointRounding.AwayFromZero);
var max = Math.Max(a, b);
var min = Math.Min(a, b);
```

---

## ModuleSearcher - データ検索

`ModuleSearcher<T>` でDB内のモジュールデータを検索する。

### 基本的な使い方

```csharp
var search = new ModuleSearcher<Product>();
search.AddEquals(e => e.Category.Value, "食品");
search.OrderBy(e => e.Name);
var results = search.Execute();  // List<Module>

foreach (var product in results)
{
    Logger.Log(product.Name.Value);
}
```

### フィルタメソッド

| メソッド | 説明 |
|---|---|
| `AddEquals(lambda, value)` | 等価 |
| `AddLessThan(lambda, value)` | 未満 |
| `AddLessThanOrEqual(lambda, value)` | 以下 |
| `AddGreaterThan(lambda, value)` | 超過 |
| `AddGreaterThanOrEqual(lambda, value)` | 以上 |
| `AddLike(lambda, value)` | LIKE検索 |
| `AddIn(lambda, params values)` | IN検索 |
| `AddIn(lambda, enumerable)` | IN検索（リスト指定） |
| `AddNotIn(lambda, params values)` | NOT IN検索 |
| `AddNotIn(lambda, enumerable)` | NOT IN検索（リスト指定） |
| `AddParameter(lambda, value)` | パラメータ設定 |
| `AddConditions(searcher)` | 他のSearcherの条件を追加 |

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `IsOrMatch` | bool | `true` で条件をOR結合（デフォルト: AND） |
| `IsNot` | bool | `true` で条件を反転 |

### ソートメソッド

```csharp
search.OrderBy(e => e.Name);
search.OrderByDescending(e => e.CreatedAt);
search.ThenBy(e => e.Code);
search.ThenByDescending(e => e.Price);
```

### 列選択

```csharp
// 特定の列のみ取得（パフォーマンス最適化）
search.Select(e => e.Name, e => e.Price);
```

### 実行メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Execute()` | `List<Module>` | モジュールインスタンスのリスト |
| `ExecuteRaw()` | `List<ModuleData>` | 生のデータリスト |

### IN検索の例

```csharp
// params で直接指定
search.AddIn(e => e.Status.Value, "Active", "Pending");

// 配列で指定
var statuses = new string[] { "Active", "Pending" };
search.AddIn(e => e.Status.Value, statuses);

// Listで指定
var list = new List<string>();
list.Add("Active");
list.Add("Pending");
search.AddIn(e => e.Status.Value, list);
```

---

## BatchSearcher - 一括検索

複数の `ModuleSearcher` を一括で実行する。

```csharp
var search1 = new ModuleSearcher<Product>();
search1.AddEquals(e => e.Category.Value, "食品");

var search2 = new ModuleSearcher<Product>();
search2.AddEquals(e => e.Category.Value, "飲料");

var response = BatchSearcher.Execute(search1, search2);
var foods = response.GetAt(0);    // List<Module>
var drinks = response.GetAt(1);   // List<Module>
```

---

## Module API

モジュールインスタンスの操作メソッド。`this` またはフィールドから取得したモジュールに対して使用。

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `IsNewData` | bool | 新規レコードか |
| `IsDeleted` | bool | 削除されたか |
| `IsModified` | bool | データが変更されたか |
| `PageTitle` | string | ページタイトル |
| `ClassName` | string | CSSクラス名 |
| `Color` | string | テキスト色 |
| `BackgroundColor` | string | 背景色 |
| `IsEnabled` | bool | 有効/無効 |
| `IsViewOnly` | bool | 読み取り専用 |
| `DialogTitle` | string | ダイアログタイトル |

### データ操作

```csharp
// 新規レコード
this.NewModule();

// レコードコピー
this.CopyModule();

// データ再読み込み
this.Reload();

// 保存
this.Submit();

// 同時保存（トランザクション）
this.Submit(simultaneousWriteModules);

// 削除
this.Delete();

// バリデーション
var isValid = this.ValidateInput();

// 変更フィールド名取得
var names = this.GetModifiedFieldNames();
```

### 再描画抑止 (SuspendNotifyStateChanged)

複数フィールドを連続で書き換える間、画面の再描画 (StateHasChanged) を抑止する。

```csharp
void Approve_OnClick()
{
    using var suspend = GetParentModule().SuspendNotifyStateChanged();

    member.Status.Value = "Approved";       // ← 画面更新走らず
    member.ActorUser.Value = ...;            // ← 画面更新走らず
    member.ApprovedAt.Value = ...;           // ← 画面更新走らず
    // ... using スコープ終了で 1 回だけ再描画
}
```

挙動:
- `Module.SuspendNotifyStateChanged()` で `ResumeNotifyStateChangedInvoker (IDisposable)` 返却
- `_blockNotifyStateChangedCount` で重なりを管理 (入れ子可)
- 期間中の `NotifyStateChanged()` は Pending キューに溜まる
- Dispose で `ResumeNotifyStateChanged()` → カウント減 → 0 で flush して 1 回だけ実 StateHasChanged

**使う場面**: ボタンハンドラ等で複数フィールドを連続更新するとき。**親モジュールで呼ぶ**のが基本 (子モジュールも巻き込んで抑止される)。

### JSON操作

```csharp
// JSON変換
var json = this.ToJsonObject();

// JSONから読み込み
this.SetJsonObject(json);

// フィールドデータ取得
var data = this.GetData();
```

### ダイアログ表示

```csharp
// モーダルダイアログ
var dlg = new PersonalInfoDialog();
var result = dlg.ShowDialog("OK", "Cancel");
if (result == "OK")
{
    Name.Value = dlg.Name.Value;
}

// カスタムスタイルボタンのダイアログ
var result = dlg.ShowDialog(new PrimaryButton("保存"), new DangerButton("削除"), new SecondaryButton("キャンセル"));

// ポップアップ（座標指定）
var result = dlg.ShowPopup(x, y, "編集", "削除");

// パネル（サイドパネル）
var result = dlg.ShowPanel("保存", "キャンセル");
var result = dlg.ShowPanel(PanelAlignment.Right, "保存", "キャンセル");

// ダイアログを閉じる（ダイアログ内から）
this.CloseDialog("OK");
this.ClosePanel("保存");
this.ClosePopup("選択");
```

### トランザクション内処理

```csharp
// OnTransaction イベント内での在庫更新例
bool StockInOut_OnTransaction(TransactionMode mode)
{
    if (mode != TransactionMode.Insert) return false;

    var stock = new Stock();
    stock.Id.Value = StockId.Value;
    stock.Reload();
    stock.Count.Value += Count.Value;
    stock.Submit();
    return true;
}
```

---

## フィールドとスクリプトAPI

フィールドの共通スクリプトAPI（全フィールド共通・値フィールド共通のプロパティ/メソッド）は [Fields/_ScriptApi.md](Fields/_ScriptApi.md) を参照。

各フィールド型固有のプロパティ・メソッド・イベントハンドラ例は `Docs/Fields/` の各フィールドドキュメントの「スクリプトAPI」セクションを参照。レイアウトイベントは [Layouts.md](Layouts.md) を参照。

フィールド型はアプリケーション側で独自に追加可能であり、追加されたフィールドもスクリプトから同じようにアクセスできる。

---

## 列挙型

スクリプト内で使用可能な列挙型。

| 列挙型 | 値 | 説明 |
|---|---|---|
| `ModuleLayoutType` | `None`, `Detail`, `List`, `Search` | モジュールレイアウトタイプ（通常は Detail / List / Search のいずれか） |
| `MatchComparison` | `Equal`, `NotEqual`, `LessThan`, `LessThanOrEqual`, `GreaterThan`, `GreaterThanOrEqual`, `Like`, `In`, `NotIn`, `Exists`, `NotExists` | 検索比較演算子 |
| `TransactionMode` | `Insert`, `Update`, `Delete` | トランザクション操作モード |
| `SideBarState` | `Opened`, `Closed`, `Hidden` | サイドバー状態 |
| `PanelAlignment` | `Left`, `Right` | パネル位置 |
| `MidpointRounding` | `AwayFromZero`, `ToEven` 等 | 丸めモード |
| `DayOfWeek` | `Sunday`〜`Saturday` | 曜日 |
| `StringSplitOptions` | `None`, `RemoveEmptyEntries` | 文字列分割オプション |

---

## サポートされない構文

以下のC#構文はスクリプトエンジンでサポートされない。

- クラス/構造体/インターフェースの定義
- 名前空間
- using ディレクティブ（`using System;` 等）
- async/await キーワード（スクリプトエンジンが自動で同期処理するため不要。**スクリプト内で `await` を書いてはいけない**）
- デリゲート/イベント定義
- yield/イテレータ
- unsafeコード
- パターンマッチング
- LINQ（`.Where()`, `.Select()` 等）
- 多次元配列（`int[,]`, `int[][]`）
- 一般的なラムダ式（ModuleSearcher以外では使用不可）
- 複数引数ラムダ式 `(a, b) => ...` (SimpleLambda = 単一引数のみ)
- 式メソッド `=>` (expression-bodied method) — `string F() => "x";` は **不可**、ブロック体 `string F() { return "x"; }` を使う
- 名前付き引数 `method(name: value)` — 位置引数で渡す
- `try` / `catch` / `finally`
- `yield return` / `yield break`
- 文字列補間 `$"..."` → 文字列連結（`+` 演算子）を使う
- パターンマッチング (`x is Y y`, switch 式)
- 範囲演算子 `..`
- Nullable の `.Value` プロパティ
- Null条件インデクサ（`?[]`）
- Nullable の `.HasValue` → `!= null` で比較すること
- `bool x = dynamicValue;` のような明示型代入 — 動的型 (parent モジュールのプロパティ等) を bool に直接代入できない。`var x = dynamicValue;` で受ける必要

---

## 拡張サービス

アプリケーション側で追加登録されたサービス（Excel, WebApi, Toaster, Mail 等）については [ScriptExtensions.md](ScriptExtensions.md) を参照。
