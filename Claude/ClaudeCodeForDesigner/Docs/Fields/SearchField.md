# SearchField - 検索フォームフィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.SearchFieldDesign`

検索フォーム。SearchLayout で検索条件の入力フォームを構成し、ユーザーの入力から検索条件を組み立てて ListField 等の結果表示フィールドに適用する。

## C# クラス定義 (真実の源)

```csharp
public class SearchFieldDesign : FieldDesignBase
{
    public string ResultsViewFieldName { get; set; } = string.Empty;       // List/DetailList/TileList の Name
    public string LayoutName { get; set; } = string.Empty;
    public string OnSearched { get; set; } = string.Empty;
    public bool UserUrlParameter { get; set; }
    public string SearchUrlParameterKey { get; set; } = string.Empty;
    public string PageIndexUrlParameterKey { get; set; } = string.Empty;
    public string SearchInitializationTriggerUrlParameter { get; set; } = string.Empty;
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `ResultsViewFieldName` | string | `""` | 検索結果を表示するフィールド名。`ISearchResultsViewFieldDesign` を実装するフィールド（ListField, DetailListField, TileListField 等）を指定する。 |
| `LayoutName` | string | `""` | 使用するSearchLayout名。空の場合はデフォルトレイアウト。`ResultsViewFieldName` で指定したフィールドの対象モジュールのSearchLayoutを参照する。 |
| `OnSearched` | string | `""` | 検索実行後に呼ばれるスクリプトイベント名。`.mod.cs` にメソッドを定義する。 |
| `UserUrlParameter` | bool | `false` | `true` にすると、検索条件をURLクエリパラメータにエンコードする。ブックマークやURLの共有が可能になる。 |
| `SearchUrlParameterKey` | string | `""` | 検索条件のURLパラメータキー名。`UserUrlParameter = true` 時に使用。 |
| `PageIndexUrlParameterKey` | string | `""` | ページインデックスのURLパラメータキー名。`UserUrlParameter = true` 時に使用。 |
| `SearchInitializationTriggerUrlParameter` | string | `""` | 検索初期化をトリガーするURLパラメータ名。このパラメータがURLに含まれる場合、自動的に検索が実行される。 |

## JSON例

### 基本的な検索フォーム

```json
{
  "ResultsViewFieldName": "ProductList",
  "LayoutName": "",
  "OnSearched": "",
  "UserUrlParameter": false,
  "SearchUrlParameterKey": "",
  "PageIndexUrlParameterKey": "",
  "SearchInitializationTriggerUrlParameter": "",
  "IgnoreModification": false,
  "Name": "Search",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchFieldDesign"
}
```

### URL検索パラメータ対応の検索フォーム

```json
{
  "ResultsViewFieldName": "OrderList",
  "LayoutName": "DetailSearch",
  "OnSearched": "Search_OnSearched",
  "UserUrlParameter": true,
  "SearchUrlParameterKey": "q",
  "PageIndexUrlParameterKey": "page",
  "SearchInitializationTriggerUrlParameter": "autoSearch",
  "IgnoreModification": false,
  "Name": "OrderSearch",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchFieldDesign"
}
```

## スクリプトAPI

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `InvokeSearchInitialization()` | void | 検索初期化スクリプトを実行 |
| `ExecuteSearch()` | void | 検索を実行 |
| `ExecuteClear()` | void | 検索条件をクリアして結果をリセット |

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `Condition` | SearchCondition (読み取り専用) | 現在の検索条件 |
| `SearchModule` | Module (読み取り専用) | 検索フォームのモジュールインスタンス |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [Scripts.md](../Scripts.md) を参照。

### イベントハンドラ: OnSearched

`OnSearched` プロパティに指定したメソッド名が、検索実行後に呼ばれる。

```csharp
void Search_OnSearched()
{
    Toaster.Success("検索が完了しました");
}
```

### 検索画面初期化イベント: OnSearchInitialization

SearchLayoutDesign の `OnSearchInitialization` プロパティに指定したメソッド名が、検索画面初期化時に呼ばれる。

```csharp
void Search_OnInit()
{
    // 検索条件の初期値設定
    var clientSearcher = new ModuleSearcher<Client>();
    SelectClient.SetSearchCondition(clientSearcher);
    SelectClient.ReloadCandidates();
}
```

## ランタイム動作

- `SearchLayout` に基づいて検索フォームをレンダリングする。SearchLayout 内の各フィールドは `ISearchableField` として検索条件の入力を受け付ける。
- ユーザーが検索フォームに条件を入力し、検索ボタンをクリックすると以下の処理が実行される:
  1. SearchLayout 内の全 `ISearchableField` から検索条件（`SearchCondition`）を構築する。
  2. `ResultsViewFieldName` で指定された ListField 等に検索条件を適用する。
  3. ListField がデータを再取得して一覧を更新する。
  4. `OnSearched` スクリプトイベントが実行される（定義されている場合）。
- **UserUrlParameter = true** の場合、検索条件がURLクエリパラメータにエンコードされる。ページリロードやURLの共有時に検索状態が復元される。
- **SearchInitializationTriggerUrlParameter** で指定したURLパラメータが存在する場合、ページ読み込み時に自動的に検索が実行される。
- データの永続化は行わない。`CreateData()` は `null` を返す。

## 検索

検索には対応しない（SearchField 自体が検索機能を提供するフィールドであり、他のフィールドから検索されるものではない）。DB列マッピングなし。

---

## DOM構造（CSS用）

### 検索フォーム

```html
<form>
  <!-- SearchGridLayoutDesign による検索フォームレイアウト -->
  <div data-layout="grid">
    <!-- ... GridLayoutDesign と同じ構造 ... -->
  </div>

  <!-- 検索ボタン行（ShowDefaultSearchButtons = true 時） -->
  <div class="row mt-3">
    <div class="col">
      <input type="submit" class="btn btn-primary" value="検索" data-system="search" />
      <input type="reset" class="btn btn-secondary" value="クリア" data-system="clear" />
    </div>
  </div>
</form>
```

### CSSセレクタ例

```css
/* 検索ボタンのスタイル */
[data-name="Search"] [data-system="search"] {
  min-width: 100px;
}

[data-name="Search"] [data-system="clear"] {
  margin-left: 0.5rem;
}
```
