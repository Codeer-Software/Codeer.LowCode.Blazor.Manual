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
// 型指定（明示型）: 型が固定される
int x = 100;
string text = "abc";
bool flag = true;
decimal amount = 10.5;

// var: 動的（型を固定しない）
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

#### `var` は動的（C# の型推論 var とは別物）

CLB スクリプトの `var` は **JavaScript の `var` に近い動的な変数**で、型を固定しない。C# の `var`（初期化子から型を推論して固定する静的型）とは挙動が違う。

```csharp
var x = 0;
x = DateTime.Now;   // OK（違う型でも代入できる。デザインチェックもエラーにしない）
x = 1.2;            // OK。int に固定されないので 1.2 のまま（切り捨てられない）
```

**ローカル変数は基本 `var` を使えばよい。** 型を固定しないので代入で型エラーにならず、補完も最後に代入した型に追従する。`Field` / `Module` / 動的な戻り値を受けるときも `var` が無難。

明示型を書く主な場面は **関数定義（引数・戻り値の型）** くらい。ローカル変数で明示型を使うのは「整数で受けて小数を切り捨てたい」など型を固定したい特定ケースだけ。

- 違う型の再代入が許される（明示型 `int x` なら型固定なので、代入できない型はエラー）。
- 入力補完は「最後に代入された型」に追従する（`x = DateTime.Now` のあとは `DateTime` のメンバが出る）。

> 関連: 動的型（`GetParentModule()` の戻り値や `Rows[i]` のメンバ等）を受けるときは、明示型ではなく `var` で受ける（`bool isNew = parent.IsNewData;` は「bool = var 不正な代入です」エラー、`var isNew = parent.IsNewData;` が正）。[ScriptGuidelines.md](ScriptGuidelines.md) 参照。

### サポートされるプリミティブ型

| 型 | 説明 |
|---|---|
| `bool` | 真偽値 |
| `sbyte`, `byte`, `short`, `ushort` | 小数部なし整数 |
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

// 初期化子つき配列
int[] arr = new int[]{1, 2, 3};

// 暗黙型配列（型を省略）。要素の値から型が決まる
var a = new[]{1, 2, 3};        // int[]
var b = new[]{1, 2.5};         // decimal[]（数値混在）
var c = new[]{1, "a", null};   // object[]

// ネストされたList
var nested = new List<List<int>>();
var inner = new List<int>();
inner.Add(100);
nested.Add(inner);
```

> **注意**: 多次元配列 (`int[,]`, `int[][]`) はサポートされない。

#### 暗黙型配列 `new[]{ ... }` の要素型

型を省略した `new[]{ ... }` は、要素の値から配列の型が決まる（C# の「最適共通型」とは異なる規則）。

| 要素 | 配列の型 | 例 |
|---|---|---|
| 全要素が同じ型 | その型の配列 | `new[]{1, 2, 3}` → `int[]` |
| 数値で型が混在 | `decimal[]`（数値演算の decimal 統一と整合） | `new[]{1, 2.5}` → `decimal[]` |
| `null` を含む | `object[]` | `new[]{1, null}` → `object[]` |
| 上記以外（非数値の混在など） | `object[]` | `new[]{1, "a"}` → `object[]` |

要素の型を固定したいときは `new int[]{ ... }` のように明示する。

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

> **注意**: `decimal` と `double` / `float` の `==` / `!=` 比較も可能（数値フィールドの値とリテラル `1.5` の比較などでエラーにならない）。

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

### 文字列補間 `$"..."`

`$"..."` で文字列の中に式を埋め込める。`+` での連結の代わりに使える。

```csharp
var name = "Bob";
var count = 3;
var msg = $"Hello {name}! ({count} 件)";   // "Hello Bob! (3 件)"
var sum = $"合計 {price * qty} 円";          // 式も書ける
```

- 埋め込む値が `null` のときは空文字になる。
- 書式指定 `:fmt` と桁揃え `,N` が使える（内部で `string.Format` に委譲、現在カルチャで整形）。
- `{` / `}` そのものを出したいときは `{{` / `}}`。

```csharp
var a = $"{1234.5:N2}";                 // "1,234.50"（書式指定）
var b = $"{DateTime.Now:yyyy/MM/dd}";   // 日付の書式
var c = $"[{id,5}]";                    // 桁揃え（幅5・右寄せ）
var d = $"{{{value}}}";                 // "{...}"（波括弧のエスケープ）
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

#### 数値・文字列の変換ルール

C# のキャスト挙動に揃えてある。代入・初期化・キャスト・関数の引数・戻り値のいずれでも同じルールが適用される。

```csharp
// 小数 → 整数型は「切り捨て」(ゼロ方向)。四捨五入ではない
int a = 1.9;        // 1
var b = (int)1.9;   // 1
int c = -1.9;       // -1

// 文字列 → 数値型は変換される
var d = (int)"5";   // 5
int e = "1.9";      // 1 (小数形式の文字列も切り捨てで整数化)

// null → 非 nullable の数値型は 0
var f = (int)null;  // 0
int g = null;       // 0
```

> **注意**: 変換できない値を **非 nullable 型** に入れると**実行時エラー**になる（例: 数値に変換できない文字列を `int` に代入）。**nullable 型** (`int?` 等) への変換に失敗した場合は `null` になる。

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
| `AddNotEqual(lambda, value)` | 不等価（`value` に `null` を渡すと `is not null`） |
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

### 件数上限・ページング

`Limit(count)` で取得件数の上限を設定する。指定しないと**全件取得**になるため、大量データを扱うモジュールでは必ず設定する。
ページングは `Limit` と `ExecutePage(pageIndex)` を併用する（`pageIndex` は 0 始まり、1 ページのサイズは `Limit` の値）。
当該ページの一覧と総件数を **1 クエリ**で取得でき、`ModulePageResult.Items` / `.TotalCount` で受け取る。

```csharp
var search = new ModuleSearcher<Product>();
search.OrderBy(e => e.Code);
search.Limit(50);
var result = search.ExecutePage(0);   // 1 ページ目
var rows = result.Items;              // 当該ページ (最大 50 件)
var total = result.TotalCount;        // 条件に一致する全件数 (ページャー表示用)
```

`Module` 実体が不要な軽量処理では、生データ版の `ExecuteRawPage(pageIndex)`（`ModuleDataPageResult`）を使う。

```csharp
var result = search.ExecuteRawPage(0);
var rows = result.Items;          // List<ModuleData>
var total = result.TotalCount;
```

> `TotalCount` は `Limit` 設定時のみ `count(*)` で算出される。`Limit` 未設定（全件取得）のときは取得した件数がそのまま総件数になる。

### 実行メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Execute()` | `List<Module>` | 条件に一致するモジュール一覧（ページングしない） |
| `ExecutePage(pageIndex)` | `ModulePageResult` | 指定ページの `Items` と総件数 `TotalCount` を 1 クエリで取得 |
| `ExecuteRaw()` | `List<ModuleData>` | 生のデータリスト（`Module` 実体を作らない軽量版） |
| `ExecuteRawPage(pageIndex)` | `ModuleDataPageResult` | 生データの指定ページ `Items` と総件数 `TotalCount` |
| `ExecuteFirstOrDefault()` | `Module?` | 先頭 1 件。該当なしは `null`（マスタ参照向け） |

```csharp
// マスタ参照: 1 件だけ欲しいときは ExecuteFirstOrDefault
var search = new ModuleSearcher<Product>();
search.AddEquals(e => e.Code.Value, ProductCode.Value);
var product = search.ExecuteFirstOrDefault();
if (product != null)
{
    UnitPrice.Value = product.UnitPrice.Value;
}
```

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

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `GetAt(index)` | `List<Module>` | 渡した順番（0 始まり）で結果を取得 |
| `GetBy(searcher)` | `List<Module>` | 渡した `ModuleSearcher` を指定して結果を取得（インデックス管理不要、バッチ外の searcher は例外） |

```csharp
// インデックスではなく searcher 指定で取得
var foods = response.GetBy(search1);
var drinks = response.GetBy(search2);
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
- パターンマッチング (`x is Y y`, switch 式)
- 範囲演算子 `..`
- Nullable の `.Value` プロパティ
- Null条件インデクサ（`?[]`）
- Nullable の `.HasValue` → `!= null` で比較すること
- `bool x = dynamicValue;` のような明示型代入 — 動的型 (parent モジュールのプロパティ等) を bool に直接代入できない。`var x = dynamicValue;` で受ける必要

---

## 拡張サービス

アプリケーション側で追加登録されたサービス（Excel, WebApi, Toaster, Mail 等）については [ScriptExtensions.md](ScriptExtensions.md) を参照。
