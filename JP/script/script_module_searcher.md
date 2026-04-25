# ModuleSearcher / BatchSearcher

スクリプトから他モジュールのデータを検索するためのクラスです。

| クラス | 用途 |
|---|---|
| `ModuleSearcher<T>` | 1 モジュールを 1 回検索 |
| `ModuleSearcher` | ジェネリックなしの版（モジュール名を文字列で指定） |
| `BatchSearcher` | 複数の `ModuleSearcher` を 1 回のリクエストで実行 |

---

## 1. 基本

### 作成と実行

```csharp
var searcher = new ModuleSearcher<Customer>();
searcher.AddEquals(c => c.Email.Value, "user@example.com");
searcher.AddLike(c => c.Name.Value, "山田");
searcher.OrderBy(c => c.Id.Value);

var customers = await searcher.Execute();   // List<Module>
foreach (var c in customers)
{
    Logger.Log(c.Name.Value);
}
```

ラムダ式は `c => c.<Field名>.Value` の単項ラムダ形式に限ります。
ネストして `c => c.Author.Name.Value` のように書くこともできます。

### Module ではなく ModuleData で取りたい

軽量な `ModuleData`（生のデータ）として取りたい場合は `ExecuteRaw` を使います。

```csharp
var rawList = await searcher.ExecuteRaw();   // List<ModuleData>
```

---

## 2. 条件追加メソッド

`Add*` 系は AND で結ばれます。

| メソッド | 比較 |
|---|---|
| `AddEquals(lambda, value)` | 一致（`=`） |
| `AddLessThan(lambda, value)` | 未満（`<`） |
| `AddLessThanOrEqual(lambda, value)` | 以下（`<=`） |
| `AddGreaterThan(lambda, value)` | より大きい（`>`） |
| `AddGreaterThanOrEqual(lambda, value)` | 以上（`>=`） |
| `AddLike(lambda, value)` | あいまい一致（`LIKE`） |
| `AddIn(lambda, params object[] values)` | リスト内に含まれる |
| `AddIn(lambda, IEnumerable values)` | リスト内に含まれる |
| `AddNotIn(lambda, params object[] values)` | リスト内に含まれない |
| `AddNotIn(lambda, IEnumerable values)` | リスト内に含まれない |

```csharp
searcher.AddEquals(c => c.Status.Value, "Active");
searcher.AddGreaterThan(c => c.Age.Value, 20);
searcher.AddIn(c => c.Region.Value, "東京", "大阪", "名古屋");
```

---

## 3. AND / OR の組み合わせ

`AddConditions` で別の `ModuleSearcher` の条件をまとめて取り込めます。
これを利用して AND と OR を組み合わせます。

| プロパティ | 型 | 説明 |
|---|---|---|
| `IsOrMatch` | `bool` | 条件を OR で結ぶ（既定 false = AND） |
| `IsNot` | `bool` | 条件全体を NOT で囲う |

```csharp
// (Region = "東京" OR Region = "大阪") AND Status = "Active"
var orCondition = new ModuleSearcher<Customer>();
orCondition.IsOrMatch = true;
orCondition.AddEquals(c => c.Region.Value, "東京");
orCondition.AddEquals(c => c.Region.Value, "大阪");

var searcher = new ModuleSearcher<Customer>();
searcher.AddEquals(c => c.Status.Value, "Active");
searcher.AddConditions(orCondition);

var list = await searcher.Execute();
```

---

## 4. ソート

```csharp
searcher.OrderBy(c => c.Id.Value);              // 昇順
searcher.OrderByDescending(c => c.Date.Value);  // 降順
searcher.ThenBy(c => c.Name.Value);             // 第 2 ソートキー
searcher.ThenByDescending(c => c.Sub.Value);
```

---

## 5. 取得列の絞り込み

`Select` で取得する Field を絞ると通信量が減ります。

```csharp
searcher.Select(c => c.Id.Value, c => c.Name.Value);
```

---

## 6. ジェネリックなしの ModuleSearcher

モジュール名が動的に決まる場合は文字列で指定します。

```csharp
var searcher = new ModuleSearcher("Customer");
// 条件式は型補完が効かないので注意
```

---

## 7. BatchSearcher — 複数検索を一括

複数の `ModuleSearcher` を 1 回のリクエストにまとめます。**サーバーラウンドトリップが減ります**。

```csharp
var s1 = new ModuleSearcher<Customer>();
s1.AddEquals(c => c.Status.Value, "Active");

var s2 = new ModuleSearcher<Order>();
s2.AddGreaterThan(o => o.CreatedAt.Value, DateTime.Today.AddDays(-7));

var response = await BatchSearcher.Execute(s1, s2);

var customers = response.GetAt(0);   // s1 の結果
var orders = response.GetAt(1);      // s2 の結果
```

---

## 8. パターン集

### 1 件だけ取りたい

```csharp
var searcher = new ModuleSearcher<Customer>();
searcher.AddEquals(c => c.Email.Value, EmailField.Value);
var list = await searcher.Execute();
if (list.Count == 0) return;
var customer = list[0];
```

### 件数だけ知りたい

```csharp
var list = await searcher.ExecuteRaw();
var count = list.Count;
```

### List コンポーネントの絞り込みに使う

```csharp
var searcher = new ModuleSearcher<Order>();
searcher.AddEquals(o => o.CustomerId.Value, this.Id.Value);

OrderListField.SetAdditionalCondition(searcher);
OrderListField.Reload();
```

---

## 関連項目

- [スクリプト概要](script.md)
- [スクリプト構文リファレンス](script_syntax.md)
- [組み込みサービスとテンプレート由来サービス](script_services.md)
- [チュートリアル: モジュール連携](../tutorials/tutorial_modules.md)
- [Tips: ModuleSearcher で他モジュールにアクセス](../Examples/Tips_ModuleSearcher.md)
- [Tips: リスト同士の連携](../Examples/DoubleList.md)
