# C# でのデータ取得ヘルパ

C# でデータを読むときの基本は `IModuleDataService`（フロント）または `ModuleDataIO`（サーバー）に
[`SearchCondition`](../module/module.md) を渡す形です。`ModuleDataIOExtensions` はこの記述を
**モジュールに対応した C# クラス + LINQ ライクな書き方**で簡単に行うための拡張メソッド群です。

```csharp
using Codeer.LowCode.Blazor.DataIO;          // 拡張メソッド / MatchComparisonHelper
using Codeer.LowCode.Blazor.Repository.Match; // SearchCondition<T>
```

## モジュールに対応したクラスを用意する

取得対象のモジュールを表す C# クラスを用意します。手書きせずデザイナから生成できます。

- ソリューションエクスプローラーでモジュールを右クリック → **Create FieldData Class**

生成されるクラスは次のような形です。

```csharp
public class Customer
{
    public IdFieldData? Id { get; set; }
    public TextFieldData? Name { get; set; }
    public NumberFieldData? Age { get; set; }
    public SelectFieldData? Rank { get; set; }
    public LinkFieldData? Company { get; set; }

    [Link]
    public TextFieldData? Company_Name { get; set; } // 関連先 Company の Name フィールド
}
```

- クラス名 = モジュール名
- プロパティ = フィールド（型は `TextFieldData` などの各フィールドのデータ型、値は `.Value` で読みます）
- 関連先（リンク先）のフィールドは `[Link]` 属性を付け、`関連フィールド名_フィールド名` のように `_` 区切りで書きます（例: `Company_Name` は関連先 `Company` の `Name`）

## いちばん簡単な取得

条件にマッチする全データを取得します。

```csharp
// 全件
var all = await service.GetListAsync<Customer>();

// 条件付き（式で書く）
var adults = await service.GetListAsync<Customer>(e => e.Age!.Value >= 20);
```

`service` はフロントなら `IModuleDataService`、サーバーなら `ModuleDataIO` です。どちらも同じ書き方ができます。

## 条件を組み立てる（SearchCondition&lt;T&gt;）

複雑な条件や並び替えが必要なときは `SearchCondition<T>` をメソッドチェーンで組み立ててから渡します。

```csharp
var condition = new SearchCondition<Customer>()
    .Where(e => e.Age!.Value >= 20)
    .Where(e => e.Company_Name!.Value == "Codeer")   // Where を重ねると AND 連結
    .OrderByDescending(e => e.Age!.Value)
    .ThenBy(e => e.Name!.Value)
    .Take(50);

var list = await service.GetListAsync(condition);
```

| メソッド | 説明 |
|----|-----|
| `Where(式)` | 条件を追加。複数回呼ぶと AND で連結されます |
| `AddParameter(フィールド, 値)` | 指定フィールドに値をセット（後述） |
| `Select(フィールド, ...)` | 取得するフィールドを限定します（既定は全フィールド） |
| `OrderBy` / `OrderByDescending` | 並び替え |
| `ThenBy` / `ThenByDescending` | 2 番目以降の並び替えキー |
| `Take(件数)` | 取得件数の上限 |

### Where で使える条件

- 比較演算子: `==` `!=` `<` `<=` `>` `>=`
- 論理演算子: `&&` `||` `!`
- 右辺には定数・変数・配列の要素・メソッドの戻り値などを使えます

```csharp
var keyword = "山田";
var ranks = new[] { "A", "B" };

var condition = new SearchCondition<Customer>()
    .Where(e => MatchComparisonHelper.Like(e.Name!.Value, keyword)) // 部分一致
    .Where(e => MatchComparisonHelper.In(e.Rank!.Value, ranks));    // いずれかに一致
```

`MatchComparisonHelper` には `Like` / `In` / `NotIn` があります。

> **式の中で使えない書き方**
> 条件式はフィールドと値の比較を組み立てるためのもので、次のような書き方はできません（エラーになります）。代わりに用意されたヘルパを使ってください。
>
> | 書きたいこと | 使えない書き方 | 代わりに |
> |----|----|----|
> | 部分一致 | `e.Name!.Value!.Contains("山田")` / `StartsWith` / `EndsWith` | `MatchComparisonHelper.Like(e.Name!.Value, "山田")` |
> | いずれかに一致 | `list.Contains(e.Rank!.Value)` | `MatchComparisonHelper.In(e.Rank!.Value, list)` |
> | フィールド同士の比較 | `e.A!.Value == e.B!.Value` | （未対応。値との比較のみ） |

### AddParameter（クエリのパラメータをセットする）

`QueryField` のように、あらかじめ用意した SQL にパラメータを持たせている場合は、
複雑な条件を組むのではなく**パラメータに値をセットするだけ**です。その用途には `AddParameter` を使います。

```csharp
// 例: where 句に @Month を持つクエリに値を渡す
var condition = new SearchCondition<SalesSummary>()
    .AddParameter(e => e.Month!.Value, "2026-05");

var rows = await service.GetListAsync(condition);
```

## まとめて取得する（フロント向け）

フロントからは、複数のクエリを 1 回の通信でまとめて実行できます。往復回数が減り通信コストを抑えられます。

```csharp
var result = await service.GetListAsync(
    new SearchCondition<Customer>().Where(e => e.Age!.Value >= 20),
    new SearchCondition<Order>().Where(e => e.Status!.Value == "open"));

var customers = result.Find<Customer>();  // 型で取り出す
var orders    = result.GetAt<Order>(1);   // 渡した順番（インデックス）で取り出す
```

- `Find<T>()` … 渡したクエリの中から型 `T` に一致するものを取り出します（無ければ空リスト）
- `GetAt<T>(index)` … 渡した順番で取り出します

> このまとめ取得は通信を伴うフロント（`IModuleDataService`）向けの機能です。サーバー側の `ModuleDataIO` は通信を伴わないため提供していません。

## 補足

- これらのヘルパは**読み取り**用です。ページングは扱わず「条件にマッチする全データ」を取得します。件数を抑えたいときは `Take` を使ってください。細かいページ制御が必要な場合は `ModuleDataIO.GetListAsync(condition, pageIndex)` を直接使います。
- 書き込み（追加・更新・削除）は [`ModuleDataIO`](user_code.md) の保存処理で行います。
- 検索条件に一致するデータをまとめて削除したい場合は [検索条件でまとめて削除する（SearchDelete）](search_delete.md) を参照してください。
