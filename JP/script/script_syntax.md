# スクリプト構文リファレンス

スクリプトの構文・型変換・名前解決のリファレンスです。
C# 経験者が「C# ならこう動くはず」と思って書くと結果が変わる箇所もあるため、
**C# との差分**を明示しています。

> 凡例
> - ⚠ **C# と挙動が異なる項目**
> - ✗ **未対応の構文**

---

## 1. C# との差分 早見表

| # | 差分 | 詳細 |
|---|---|---|
| ⚠ 1 | 数値同士の二項算術は `decimal` で計算（`int / int` の結果は `decimal`）。数値以外（`DateTime` など）は通常通り | [§5](#5-数値演算は-decimal) |
| ⚠ 2 | 明示型は再代入で型保持、`var` は素通し | [§4.2](#42-明示型ローカル変数-vs-var) |
| ⚠ 3 | 代入・引数渡しで緩い型変換（`"123"` → `int`、`1` → `bool` など） | [§4.1](#41-緩い型変換) |
| ⚠ 4 | クラス型のキャストは検証なし（パススルー） | [§4.5](#45-クラス型キャストはパススルー) |
| ⚠ 5 | 真偽値評価が緩い（`if (1)`、`if ("True")` などが有効） | [§4.4](#44-真偽値評価の緩さ) |
| ⚠ 6 | `Nullable<T>` の `.Value` / `.HasValue` は使えない（値はそのまま使う） | [§4.3](#43-nullable-の扱い) |
| ⚠ 7 | 参照型引数への変換失敗は `null` で渡る（実行時エラーにならない） | [§4.6](#46-メソッド引数の-null-縮退) |
| ⚠ 8 | 三項演算子 `?:` の両辺型違いを許容 | [§4.7](#47-三項演算子の両辺型違い) |
| ⚠ 9 | 識別子の解決順は ローカル/モジュール変数 → モジュール宣言メンバー(Field/Layout/CurrentUser) → サービス/型 | [§7](#7-名前解決順) |
| ⚠ 10 | `switch` は C / C++ / Java と同じくフォールスルー可（C# だけが特殊） | [§3.4](#34-switch-のフォールスルー挙動) |
| ⚠ 11 | スクリプト関数の同名重複は実行時エラー、複数候補はベストマッチ | [§6](#6-オーバーロード解決) |
| ✗ | `try` / `catch` / `throw` / `typeof` / `is` / `as` | [§2](#2-未対応の-c-構文) |

---

## 2. 未対応の C# 構文

以下は書くと実行時エラーまたは解析時エラーになります。

| 構文 | 備考 |
|---|---|
| ✗ `try` / `catch` / `finally` | 例外はランタイムが捕捉してログ化 |
| ✗ `throw` | |
| ✗ `typeof(T)` | |
| ✗ `is` / `as` | 型判定の構文は使えない |
| ✗ switch 式 (`expr switch { ... }`) | |
| ✗ パターンマッチ (`case int x`、`is X p` 等) | |
| ✗ 本体付きラムダ `() => { ... }` | 単項ラムダ `e => e.X` のみ |
| ✗ 匿名メソッド `delegate { ... }` | |
| ✗ `goto` / `checked` / `unchecked` | |
| ✗ `yield return` | |
| ✗ `default(T)` / `default` リテラル | |
| ✗ `nameof(x)` | |
| ✗ 多次元配列・ジャグ配列 | 明示エラー |
| ✗ ジェネリック関数の定義 | `List<T>` 等の利用は可 |
| ✗ LINQ クエリ構文 | メソッド呼び出し（`Where` 等）は可 |

---

## 3. 利用できる構文

### 3.1 リテラル

C# のリテラル文法をほぼそのまま受け付けます。

| 種別 | 例 |
|---|---|
| 整数 | `5`, `5L`, `5u`, `5UL`, `0x10`, `0b1010`, `1_000_000` |
| 浮動小数点 | `5.0`, `5.0f`, `5m`, `1e10` |
| 文字列 | `"abc"`（エスケープ `\n` `\t` `\"` `\\` `\uXXXX` 等） |
| char | `'a'`, `'\n'` |
| bool | `true` / `false` |
| null | `null` |

### 3.2 変数宣言と代入

```csharp
var x = ...;       // 型を明示しない
int x = ...;       // 明示型
int? x = null;     // Nullable
int a, b;          // 多重宣言
a = b = 3;         // 多重代入
```

複合代入: `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `|=`

### 3.3 演算子

| 種別 | 演算子 | メモ |
|---|---|---|
| 算術 | `+` `-` `*` `/` `%` | ⚠ 数値同士は decimal で計算。`DateTime` 等は通常通り（`DateTime + TimeSpan` も可） |
| 文字列連結 | `+` | 一方が string なら連結 |
| 比較 (大小) | `<` `<=` `>` `>=` | 数値同士は decimal、`DateTime` 等は通常通り |
| 比較 (等価) | `==` `!=` | 数値同士は decimal に揃えて比較（`null` は 0 扱いしない） |
| 論理 | `&&` `\|\|` `!` | null は false 扱い |
| 単項 | `+x` `-x` `~x` `!x` | |
| インクリメント | `x++` `++x` `x--` `--x` | 前置は新値、後置は旧値を返す（C# 準拠） |
| ビット | `&` `\|` `^` `<<` `>>` | 数値同士は long で計算 |
| null 合体 | `??` | |
| null 条件 | `?.` | チェーン可 |
| 三項 | `?:` | ⚠ 両辺の型違いを許容 |
| キャスト | `(T)x` | ⚠ 数値・文字列等は変換（小数→整数型は切り捨て）、クラス型はパススルー |

### 3.4 制御フロー

- `if` / `else`
- `while (cond) { ... }`
- `for (init; cond; inc)` — 各部省略可
- `foreach (var x in enumerable)`
- `switch` — `case` は int / string / null / default
- `break` / `continue` / `return`

#### Switch のフォールスルー挙動

本実装の `switch` は **C / C++ / Java と同じく**、`break` / `return` を書かないと**次の case の文も続けて実行**します。

```csharp
switch (x)
{
case 1:
    y = 1;        // break なし
case 2:           // ← x == 1 のときここにも落ちる
    y += 10;
    break;
default:
    y = 999;
    break;
}
// x == 1 のとき y = 11
```

> ⚠ **C# 経験者向け注意**: C# の感覚で書くと「`break` 書き忘れ → 次の case も実行」という意図しない挙動になりやすい。終端文を忘れずに。

### 3.5 関数の定義

モジュール内に関数を定義できます。**戻り値・引数の型は `var` でも明示型でも OK**。

```csharp
int Add(int a, int b) { return a + b; }
var Greet(var name) { return "Hello " + name; }
```

> ⚠ **C# 差分**: スクリプト内で定義する関数は以下を**サポートしません**:
> - デフォルト引数 (`void F(int a = 10)`)
> - params (`void F(params int[] args)`)
>
> .NET 側のメソッドを呼ぶ場合は両方サポートされます。

### 3.6 モジュール変数（モジュール内のトップレベル変数）

モジュールの Scripts にトップレベル変数を宣言できます。初期値は `default(T)`（型に応じた既定値）。

```csharp
int counter;       // モジュール内で共有
string lastUser;
```

他モジュールからは `new MyMod().counter` のようにインスタンスを介して参照できます。

### 3.7 オブジェクト生成

```csharp
new Customer()                 // モジュール
new Customer.Data()            // ModuleData
new ModuleSearcher<Customer>() // 検索
new MemoryStream()             // .NET 型
new int[10]                    // 配列
new int[]{1, 2, 3}             // 初期化子つき
new[]{1, 2, 3}                 // 型を省略（暗黙型配列）
new List<int>()
new Dictionary<string, int>()
```

> ✗ **未対応**: 多次元配列 `new int[1,2]`、ジャグ配列 `new int[10][]` は明示エラーになります。

#### 暗黙型配列 `new[]{ ... }` の要素型

型を省略した `new[]{ ... }` は、要素の値から配列の型が決まります。判定規則は次の通りです（C# の「最適共通型」とは異なります）。

| 要素 | 配列の型 | 例 |
|---|---|---|
| 全要素が同じ型 | その型の配列 | `new[]{1, 2, 3}` → `int[]` |
| 数値で型が混在 | `decimal[]`（数値演算の decimal 統一と整合） | `new[]{1, 2.5}` → `decimal[]` |
| `null` を含む | `object[]` | `new[]{1, null}` → `object[]` |
| 上記以外（非数値の混在など） | `object[]` | `new[]{1, "a"}` → `object[]` |

数値が混在すると `decimal[]` になるのは、本スクリプトの数値演算が `decimal` で行われること（[§5](#5-数値演算は-decimal)）と揃えるためです。要素の型を固定したいときは `new int[]{ ... }` のように明示してください。

### 3.8 using 文

```csharp
using (var x = ...) { ... }
using var x = ...;
using (expr) { ... }
```

### 3.9 ref / out

```csharp
int x;
int.TryParse(s, out x);
int.TryParse(s, out var y);
```

### 3.10 文字列補間 `$"..."`

`$"..."` で文字列の中に式を埋め込めます。`+` での連結の代わりに使えます。

```csharp
var name = "Bob";
var count = 3;
return $"Hello {name}! ({count} 件)";   // "Hello Bob! (3 件)"
return $"合計 {price * qty} 円";          // 式も書ける
```

- 埋め込む値が `null` のときは空文字になります。
- 書式指定 `:fmt` と桁揃え `,N` が使えます（内部的に `string.Format` に委譲、現在のカルチャで整形）。
- `{` / `}` そのものを出したいときは `{{` / `}}` と書きます。

```csharp
return $"{1234.5:N2}";                     // "1,234.50"（書式指定）
return $"{DateTime.Now:yyyy/MM/dd}";       // 日付の書式
return $"[{id,5}]";                        // 桁揃え（幅5・右寄せ）
return $"{{{value}}}";                     // "{...}"（波括弧のエスケープ）
```

### 3.11 その他

- コメント: `//` / `/* */`
- `int.Parse(...)` / `decimal.Zero` などの型のメンバアクセス
- 単項ラムダ `e => e.B.Value`（`ModuleSearcher` の条件指定など限定用途）

---

## 4. C# との意図的な差分

ローコード向けにユーザが型を細かく意識しなくて済むよう、**C# よりかなり緩い型変換**を行います。

### 4.1 緩い型変換

代入・引数渡し時に、宣言された型（または仮引数の型）に合わせて値が**自動変換**されます。

| 変換 | C# | 本スクリプト |
|---|---|---|
| `string "123"` → `int` | 不可 | ○（`123` になる） |
| `double 1.5` → `int` | 要キャスト | ○（**切り捨て**。`1.5` → `1`） |
| `string "1.9"` → `int` | 不可 | ○（**切り捨て**。`1.9` → `1`） |
| `int 1` → `bool` | 不可 | ○（0=false、他=true） |
| `bool true` → `int` | 不可 | ○（true=1、false=0） |
| `int` → `string` | 要 `ToString()` | ○ |
| `string "2024-01-01"` → `DateTime` | 要 `Parse` | ○ |
| `string` → `Guid` | 要 `Parse` | ○ |
| `string` / `int` → `Enum` | 要キャスト / Parse | ○ |
| 変換できない値（`string "abc"` → `int` 等） | 不可 | **非 null 許容型は実行時エラー / null 許容型 (`int?` 等) は `null`** |

C# ならコンパイルエラーになるケースの大半が、実行時に「いい感じに」変換されて動きます。

> **小数 → 整数型は切り捨て**（0 方向）です。数値でも小数形式の文字列でも同じで、代入・初期化・キャスト・関数の引数・戻り値すべてで一貫します（例: `(int)1.9`、`int x = 1.9`、`int x = "1.9"` はいずれも `1`）。
>
> **変換できない値**を入れたときは、入れ先が **null を取れない型なら実行時エラー**、**null 許容型なら `null`** になります（初期化・再代入・キャストで共通）。
>
> ただし **`null` 自体はエラーになりません**。`null` を null 許容型に入れれば `null`、**null を取れない値型に入れると既定値**になります（例: `int x = null;` → `0`、`DateTime x = null;` → `DateTime.MinValue`。[§4.5](#45-クラス型キャストはパススルー) も参照）。

### 4.2 明示型ローカル変数 vs `var`

ローカル変数への代入挙動は**宣言の仕方によって違います**。

#### 明示型 (`int x`、`double y` 等)

初期化と再代入の両方で §4.1 の変換が適用され、**宣言型が保持**されます。

```csharp
int x = "5";       // "5" → 5
x = "10";          // "10" → 10
x = 1.7;           // 1.7 → 1（切り捨て）
x = "abc";         // 変換不能 → 実行時エラー
x = true;          // true → 1
x = null;          // null → 0（既定値。エラーにはならない）

double y = 1;      // 1 → 1.0
y = 2;             // 2 → 2.0（double 維持）
```

#### `var` 宣言

`var` は**素通し**。右辺の値がそのまま入り、変数の実行時型は代入のたびに変わります。

```csharp
var x = 100;       // x は Int32
x = "abc";         // x は String
x = 3.14;          // x は Double
```

> ⚠ **C# の `var` とは別物**。C# の `var` は「初期化子から型を推論して固定する（実体は静的型）」ため、上のように違う型を再代入するとコンパイルエラーになります。本実装の `var` は **JavaScript の `var` に近い動的な変数**で、型を固定せず何でも代入できます（デザインチェックでもエラーになりません）。型を固定したいときは明示型（`int x` など）を使ってください。
>
> 入力補完は **「最後に代入された型」** に追従します。`var x = 0; x = DateTime.Now;` のあとに `x.` と打つと `DateTime` のメンバが候補に出ます。

#### 使い分け

**ローカル変数は基本 `var`** でかまいません。型を固定しないので代入で型エラーになることがなく、補完も最後に代入した型に追従します。`Field` や `Module`、動的な戻り値を受けるときも `var` が無難です。

明示型を書く主な場面は **関数定義（引数・戻り値の型）**（[§3.5](#35-関数の定義)）です。ローカル変数で明示型を使うのは、整数で受けたい（小数を切り捨てたい）など「結果の型を固定したい」特定のケースくらいです。

迷ったら `var`。

### 4.3 Nullable の扱い

C# の `Nullable<T>` API（`.Value` / `.HasValue` / `.GetValueOrDefault()`）は **使えません**。

#### 仕様

- `int? x = null;` は素の `null`
- `int? x = 5;` は素の `Int32`
- `x.Value` / `x.HasValue` などにアクセスすると**実行時エラー**

```csharp
int? x = 5;
// C# :    x.Value → 5、x.HasValue → true
// 本実装: x.Value → エラー、x.HasValue → エラー
//        x      → そのまま 5 として使える
```

#### 推奨の書き方

| C# での書き方 | 本実装での推奨 |
|---|---|
| `x.Value` | `x` そのまま |
| `x.HasValue` | `x != null` |
| `x.GetValueOrDefault()` | `x ?? 0`（型に応じたデフォルトを明示） |
| `x.GetValueOrDefault(5)` | `x ?? 5` |
| `if (x.HasValue) use(x.Value);` | `if (x != null) use(x);` |

`?.` / `??` を組み合わせれば `Nullable<T>` API に相当する処理はすべて書けます。

### 4.4 真偽値評価の緩さ

`if`、`while`、`for(cond)`、`?:`、`&&`、`||` の条件式は、C# より緩く評価されます。

| 値 | 評価 |
|---|---|
| `bool` | そのまま |
| `null` | false |
| 数値 `0` | false |
| 数値 非0 | true |
| `"True"` | true |
| `"False"` | false |
| 上記以外の文字列 | 実行時エラー |

```csharp
if (1) { ... }         // C#: コンパイルエラー / 本実装: 実行される
if ("True") { ... }    // C#: コンパイルエラー / 本実装: 実行される
if (null) { ... }      // C#: コンパイルエラー / 本実装: 実行されない
```

### 4.5 クラス型キャストはパススルー

クラス型のキャストは**型検証を行わず**そのまま返します。

```csharp
Module x = new Customer();
var y = (Order)x;
// C#: InvalidCastException
// 本実装: y の中身は Customer のまま（実行時エラーにならない）
```

実際には別の型なので、その後のメソッド呼び出しで失敗します。型を確認してから扱うのが安全です。

ただし**パススルーになるのはクラス型どうしのキャスト**です。`string` から数値型など、§4.1 の変換が効くキャスト（例: `(int)"5"` → `5`）はそのまま変換されます。`(int)null` のように `null` を非 null 許容の値型へキャストした場合は既定値（`(int)null` → `0`）になります。

### 4.6 メソッド引数の null 縮退

参照型（クラス / `Nullable<T>`）の仮引数に変換不能な値を渡すと、**`null` として呼び出されます**。

```csharp
public static void SaveAnimal(AnimalBase animal) { ... }

var list = new List<int>();
SaveAnimal(list);
// C#: コンパイルエラー
// 本実装: animal = null として呼ばれる
//        （メソッド内で null を触ると NullReferenceException）
```

非 null 値型（`int`、`bool` 等）は従来どおりオーバーロード候補から除外されます。

### 4.7 三項演算子の両辺型違い

両辺の型が違っても、選ばれた辺の値がそのまま返ります。

```csharp
return true  ? "a" : 1;   // 本実装: "a"（string）
return false ? "a" : 1;   // 本実装: 1（int）
```

戻り値の型が条件で変わるので、後段で型を仮定した処理（算術など）に繋ぐと意図しない結果になりやすい。両辺の型を揃えるのが無難です。

---

## 5. 数値演算は decimal

数値同士の二項算術 (`+` `-` `*` `/` `%`) は、**型に関わらずすべて `decimal` に変換してから計算**されます（`int` どうしでも `decimal` で計算）。
`/` も例外ではなく **decimal で割る**ため、`7 / 2` は `3.5`、`2 / 3` は `0.6666…` のように割り切らずに小数のまま求まります（C# の整数除算とは異なる）。
ビット (`&` `|` `^` `<<` `>>`) は同様に **`long` に変換**。

| 式 | C# の結果 | 本実装の結果 |
|---|---|---|
| `7 / 2` | `3` (int) | `3.5` (decimal) |
| `int a=2, b=3; a / b` | `0` (int) | `0.6666…` (decimal) |
| `null + 5` | コンパイルエラー | `5`（null は 0 扱い） |
| `null * 2` | コンパイルエラー | `0` |

ただし計算結果を **整数の明示型変数で受け取ると、結果は整数になります**。計算自体は `decimal` で行われ、代入時に小数部が切り捨て（0 方向）られて `int` に収まります。

```csharp
int q = 7 / 2;     // 計算は 3.5m → int で受けて切り捨て → q = 3
int z = 2 / 3;     // 計算は 0.6666…m → int で受けて切り捨て → z = 0
var r = 7 / 2;     // r = 3.5m（decimal のまま）
```

### 数値以外は変換されない

数値型（`int` / `long` / `short` / `byte` / `uint` / `ulong` / `ushort` / `sbyte` / `float` / `double` / `decimal`）以外の値は変換されず、**実行時に C# のオペレータがそのまま呼ばれます**（内部的には `dynamic` でディスパッチ）。
そのため `DateTime + TimeSpan` や `DateTime < DateTime` のような演算は C# と同じように動きます。

```csharp
var a = new DateTime(2025, 1, 2);
var b = new DateTime(2025, 1, 3);
return a < b;                  // true（DateTime のまま比較される）

var t = new DateTime(2025, 1, 1) + TimeSpan.FromDays(7);
                               // DateTime + TimeSpan が普通に動く
```

### 等価比較（`==` / `!=`）

`==` / `!=` は、**両辺がともに数値なら `decimal` に揃えて比較**します。
そのため `decimal` と `double` の比較もそのまま行えます（例: 数値フィールド `== 1.5`）。

- `1 == 1m`、`1 == 1.0`、`1m == 1.0` はいずれも `true`
- **ただし `null` は 0 扱いしません**（算術・大小比較とは違う点）。`null == 0` は **false**、`null == null` は `true`
- 数値と文字列のように型が合わず比較できない組み合わせは実行時エラーになります（例: `"1" == 1`）

---

## 6. オーバーロード解決

同名のメソッドが複数あるとき、引数の型に**最も近いもの**が選ばれます。

### スコアリングの考え方

各候補に対し、引数ごとに「どれだけ理想からずれているか」のスコアを付けて、合計の最小値が選ばれます。

| 状態 | スコア |
|---|---|
| 完全一致 | 0 |
| 数値拡大変換（`int` → `long`） | 1 |
| 派生 → 基底 | 2 + 継承距離 |
| 適合不可 | 1000（実質候補外） |
| `null` → 値型 | +10 |
| `null` → 参照型 | +1 |

### ⚠ C# との差分

- **同点の場合は、宣言順の先頭が勝つ**（C# なら ambiguous エラー）
- **スクリプト関数の完全同型重複**は実行時エラー
- 参照型の適合不可は §4.6 の `null` 縮退で適合扱いされ、複数候補が残ることがある

```csharp
int F(int a) { return 1; }
int F(string a) { return 2; }
return F("5");
// 本実装: 2（F(string) が完全一致で勝つ）
```

---

## 7. 名前解決順

識別子（`x`、`Text`、`Math` などの裸の名前）は次の順で解決されます。
**先に見つかったものが勝ち**、後の階層は隠れます。

```
1. ローカル変数（内側スコープ優先） → モジュール変数
2. モジュール宣言メンバー（Field / Layout / CurrentUser）
3. サービス / 型（Logger、DateTime、Math、ユーザ登録の AddService 等）
4. .NET フォールバック
```

「ユーザが書いたものが、外部の登録物より常に優先」というルールです。
モジュール変数はローカル変数と同じ段（1）で解決されるため、`CurrentUser` などのモジュール宣言メンバー（2）より優先されます。

### 主な衝突パターン

| パターン | 挙動 |
|---|---|
| ローカル変数 × モジュールメンバー（Field / Layout / モジュール変数 / CurrentUser） | ローカルが勝つ（シャドーイング） |
| モジュール変数 × CurrentUser | モジュール変数が勝つ（同 1 段で先に解決） |
| モジュール変数 × モジュール変数 | **実行時エラー**（モジュール作成時にも警告） |
| モジュール変数 × Field 名 | **実行時エラー** |
| モジュール変数 × 型/サービス名 | モジュール変数が勝つ |
| Field 名 × 型/サービス名（`Math`、`DateTime` 等） | **Field が勝つ**（型/サービスは隠れる） |
| スクリプト関数 × Field 名（呼び出し `Text()`） | **関数が勝つ**（呼び出し時は関数が先に検査される） |
| スクリプト関数 × Module の公開メソッド | スクリプト関数が適合するなら勝つ |
| スクリプト関数 × スクリプト関数（完全同型） | **実行時エラー** |
| スクリプト関数 × モジュール変数（同名） | 宣言は通るが、裸の名前は**変数が勝つ**ため関数を呼べない（`F()` は実行時エラー）。同名は避ける |
| 同名のローカル変数を同一/外側スコープで再宣言 | **実行時エラー**（C# 準拠） |

> ローカル変数が Field を隠している場合でも、`this.フィールド名` と書けば Field に到達できます（`this` はモジュール自身を指すため、識別子のシャドーイングと無関係）。なお **モジュール変数を Field と同名にするのは禁止**（デザインチェック・実行時の両方でエラー）です。被らせてよいのは関数内のローカル変数だけです。

### モジュール名は別系統

アプリのモジュール名（`Home`、`Customer` など）は識別子解決の対象外で、**型として扱う場面でのみ**使われます。

```csharp
new Home()              // ○（型として）
Home x = ...;           // ○（型として）
new List<Home>()        // ○
ModuleSearcher<Home>    // ○
var v = Home;           // モジュール名としては解決されない（同名のローカル/Field がなければエラー）
```

そのため、モジュール名と同名のローカル変数や Field を作っても `new X()` の型解析は壊れません。

### 修飾アクセス（`式.メンバー`）は別系統

上記の解決順は**裸の識別子**に対するものです。`mod.X` のように `.` を介したメンバーアクセスは、左辺の対象から直接メンバーを引くため、解決対象が異なります。

```csharp
var h = new Home();
h.counter;     // Home の Field / Layout / モジュール変数 のみが対象（サービスや型は対象外）
```

他モジュールのインスタンス経由でアクセスできるのは、そのモジュールの **Field / Layout / モジュール変数**です。`Logger` や `DateTime` のようなサービス・型は修飾アクセスの対象になりません。

### CurrentUser を変数名にしない

`CurrentUser` はモジュール宣言メンバー扱いの特殊識別子です。同名のローカル変数やモジュール変数を宣言すると（1 段で先に解決されるため）システムの `CurrentUser` が隠れて参照できなくなります。`CurrentUser` という名前は変数に使わないでください。

---

## 8. 利用できる型

### プリミティブ

`object` / `bool` / `byte` / `char` / `ushort` / `short` / `uint` / `int` / `ulong` / `long` / `float` / `double` / `decimal` / `string`

### 標準で登録されている型

| カテゴリ | 型 |
|---|---|
| 数値・時刻 | `Math`、`DateTime`、`DateOnly`、`TimeOnly`、`TimeSpan`、`DateTimeOffset`、`MidpointRounding` |
| ID・列挙 | `Guid`、`DayOfWeek`、`StringSplitOptions` |
| ストリーム・JSON | `MemoryStream`、`StreamContent`、`JsonObject` |
| 計測 | `Stopwatch` |
| 非同期・入力 | `Task`、`KeyboardEventArgs` |
| コレクション | `IEnumerable`、`List<T>`、`HashSet<T>`、`Dictionary<K,V>`、配列 |
| ローコード列挙 | `ModuleLayoutType`、`MatchComparison`、`TransactionMode`、`SideBarState`、`PanelAlignment` |
| レイアウト型 | `Layout`、`GridLayout`、`SearchGridLayout`、`TabLayout` |
| ダイアログボタン | `DialogButton` / `PrimaryButton` / `SecondaryButton` / `SuccessButton` / `DangerButton` / `WarningButton` / `InfoButton` / `LightButton` / `DarkButton` / `LinkButton`（および `*OutlineButton`） |
| その他 | `ResumeNotifyStateChangedInvoker`、`Rect`、`ProCodeComponentBase`、`ModuleData`、`FieldDataBase`、各 `XxxFieldData` |
| 検索 | `ModuleSearcher`、`ModuleSearcher<T>`、`BatchModuleSearchResponse`、`IModuleDataSyncHandler` |

### アプリで定義したもの

- 各モジュール名を**型として**使える（`new Home()` など）
- 各 Field 型（`TextField`、`NumberField` など）

### サービス

[組み込みサービスとテンプレート由来サービス](script_services.md) を参照。

### 主な列挙

#### `ModuleLayoutType`

| 値 | 説明 |
|---|---|
| `None` | なし |
| `Detail` | 詳細 |
| `List` | 一覧 |
| `Search` | 検索 |

#### `MatchComparison`

| 値 | 説明 |
|---|---|
| `Equal` / `NotEqual` | 一致 / 不一致 |
| `LessThan` / `LessThanOrEqual` | 未満 / 以下 |
| `GreaterThan` / `GreaterThanOrEqual` | より大きい / 以上 |
| `Like` | あいまい |
| `In` / `NotIn` | 含まれる / 含まれない |
| `Exists` / `NotExists` | 存在する / しない |

#### `TransactionMode`

| 値 | 説明 |
|---|---|
| `Insert` | 登録 |
| `Update` | 更新 |
| `Delete` | 削除 |

---

## 関連項目

- [スクリプト概要](script.md)
- [組み込みサービスとテンプレート由来サービス](script_services.md)
- [ModuleSearcher / BatchSearcher](script_module_searcher.md)
- [スクリプトの拡張](script_extend.md)
- [破壊的変更（1.2.57）](../breaking_changes/1.2.57.md)
- [破壊的変更（1.3.0）](../breaking_changes/1.3.0.md)
