# SearchField (検索)

## これは何か

**検索条件の入力欄と実行ボタンを提供するフィールド**。[List](List.md) / [DetailList](DetailList.md) / [TileList](TileList.md) などと組み合わせて、画面上部の検索バーを構成します。

検索の中核を担う Field なので、以下に**検索の仕組みそのもの**もまとめてあります。Field 個別の検索 UI のふるまいは各 Field のドキュメントを参照してください。

## いつ使うか

- 一覧画面に検索条件を付けたい時（通常は一覧画面に自動で配置される）
- 独自の場所に検索バーを追加したい時

SearchField は `結果表示フィールド名` で指定した List 系 Field と連動し、そのモジュールの検索レイアウトに従って検索 UI を構築します。

---

## デザイナでの設定

<img src="../../Image/designer/fields/search/SearchSample_properties_panel.png" alt="SearchFieldのプロパティパネル" style="border: 1px solid;" width="400">

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `検索` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |
| **ResultsViewFieldName** | 結果表示フィールド名 | string | `""` | 検索結果を表示する List / DetailList / TileList の Name |
| **LayoutName** | レイアウト名 | string | `""` | 結果表示モジュールの Search レイアウト名 |
| **OnSearched** | 検索イベント | string | `""` | 検索実行後のスクリプト |
| **UserUrlParameter** | URLパラメータを使う(q, p) | bool | `false` | 検索条件・ページ番号を URL に反映する |
| **SearchUrlParameterKey** | 検索URLパラメータ(未指定場合はq) | string | `""` | 検索条件の URL パラメータ名（既定 `q`） |
| **PageIndexUrlParameterKey** | ページングURLパラメータ(未指定の場合はp) | string | `""` | ページ番号の URL パラメータ名（既定 `p`） |
| **SearchInitializationTriggerUrlParameter** | スクリプトでの条件初期化用URLパラメータ | string | `""` | 指定した URL パラメータがあるとき、スクリプトでの検索条件初期化をトリガ |
| **IgnoreModification** | 変更判定から除外 | bool | `false` | 変更検知から除外 |

> SearchField には `表示名` / `必須` はありません。値を持つのは子の入力 Field 側です。

---

## 検索の仕組み

### 検索レイアウト（SearchLayout）

モジュールデザインの **SearchLayouts** に、検索に使う Field を並べます。配置した各 Field がそのまま検索フォームとして描画されます。

- `GridLayout` で置けば 2 列・3 列のグリッド検索フォームに
- `FlowLayout` で置けば横並び／折り返しのコンパクトな検索バーに
- `TabLayout` で置けば タブで切り替える検索パネルに

検索ボタンを押すと、入力されている全 Field の値で DB クエリが組み立てられ、結果が一覧に差し替わります。

### AND / OR の切り替え

SearchLayout の **Operator** で、入力された全条件を `AND` で結ぶか `OR` で結ぶかを指定します。

- **And**（既定）: 全条件を満たすデータ
- **Or**: どれか 1 条件でも満たすデータ

### 簡易検索 と 詳細検索（IsSimpleSearchParameter）

各 Field の `IsSimpleSearchParameter`（簡易検索条件）プロパティで、検索 UI のふるまいを切り替えます。

| 設定 | 用途 | UI |
|---|---|---|
| **`true`（簡易検索）** | 比較演算子を Field 固有のデフォルトに固定し、ユーザーには入力欄だけを見せる | コンパクト |
| **`false`（詳細検索）** | 比較演算子・空／空以外モードをユーザーに選ばせる | 入力欄 + 比較ドロップダウン |

> ⚠ **ハマりどころ**: 簡易にした Field は、**常にその Field のデフォルト演算子**で検索されます（TextField は部分一致、NumberField は ≥、Boolean は True/False のみ）。完全一致や範囲などをユーザーに切り替えさせたい時は詳細にしておく必要があります。

> **例外**: BooleanField と LinkField は **`IsSimpleSearchParameter` の影響を受けません**（簡易と詳細で UI が同じ）。空検索を使いたい時は `AllowEmptySearch=true` だけ設定してください。

各 Field がデフォルトでどんな演算子になり、詳細時にどんな選択肢が出るかは [Field 別の検索挙動](#field-別の検索挙動) を参照。

### 空検索（AllowEmptySearch）

Field の `AllowEmptySearch=true` で、詳細検索（`IsSimpleSearchParameter=false`）のモードドロップダウンに **「空」「空以外」** が追加されます。

- **空**: その列が `NULL` または空文字のデータ
- **空以外**: その列に何か値が入っているデータ

> **前提**: 簡易検索のままでは空検索モードは出ません。詳細検索に切り替える必要があります。

### 検索ボタン・クリアボタン

検索フォームには `検索` と `クリア` の 2 ボタンが自動で描画されます。

- **検索**: 入力されている条件で検索を実行
- **クリア**: すべての入力欄を空に戻す

検索結果の件数は表の下に「(m-n /合計件)」形式で表示されます。

### URL パラメータ（UserUrlParameter）

`UserUrlParameter=true`（既定）の時、ユーザーが検索した条件とページ番号が URL のクエリ文字列に反映されます。

- `?q=...` 検索条件（Base64 エンコード）
- `?p=2` 現在のページ（2 ページ目）

この URL を共有すれば、同じ検索結果を再現できます。`false` にすると URL は変化せず、検索条件は画面内のみに保持されます。

URL パラメータのキー名は `SearchUrlParameterKey` / `PageIndexUrlParameterKey` で変更できます（複数の検索フォームを 1 ページに同居させたい時に使用）。

---

## Field 別の検索挙動

各 Field の検索 UI はそれぞれの Field ドキュメントを参照してください。

| Field | 検索 UI | 詳細時に選べるもの |
|---|---|---|
| [TextField](Text.md#検索での挙動) | 入力欄（簡易=部分一致） | 部分一致 / 完全一致 / 空 / 空以外 |
| [NumberField](Number.md#検索での挙動) | 入力欄（簡易=≥） | 範囲 / 空 / 空以外 |
| [DateField](Date.md#検索での挙動) | ピッカー（簡易=≥） | 範囲 / 空 / 空以外 |
| [DateTimeField](DateTime.md#検索での挙動) | ピッカー（簡易=≥） | 範囲 / 空 / 空以外 |
| [TimeField](Time.md#検索での挙動) | ピッカー（簡易=≥） | 範囲 / 空 / 空以外 |
| [BooleanField](Boolean.md#検索での挙動) | ドロップダウン（簡易と詳細で UI 同じ） | True/False + 空 / 空以外 |
| [SelectField](Select.md#検索での挙動) | 候補ドロップダウン | 一致 / 不一致 / 空 / 空以外（+ Or 検索でチェックリスト化） |
| [RadioGroupField](RadioGroup.md#検索での挙動) | Select と同じ UI（ドロップダウン） | 同上 |
| [LinkField](Link.md#検索での挙動) | テキスト + ダイアログアイコン（簡易と詳細で UI 同じ） | 空 / 空以外 |
| [IdField](Id.md#検索での挙動) | TextField と同じ | 部分一致 / 完全一致 / 空 / 空以外 |
| [ListField](List.md#検索での挙動) | ドロップダウンのみ | 行を持つ / 行を持たない（親レコードを子の有無で絞り込む） |
| [DetailListField](DetailList.md#検索での挙動) | List と同じ UI（ドロップダウン） | 同上 |
| [TileListField](TileList.md#検索での挙動) | List と同じ UI（ドロップダウン） | 同上 |

---

## スクリプトから

### プロパティ・メソッド

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `SearchModule` | Module? | 検索入力用の子モジュール（`SearchModule.Xxx.SearchValue = ...` のようにアクセス） |
| `ExecuteSearch()` | Task | 検索を実行 |
| `ExecuteClear()` | Task | 検索条件をクリア（`InvokeOnClearAsync` 相当） |
| `InvokeSearchInitialization()` | Task | 検索入力の初期化スクリプトを発動 |

共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

### よく使う例

```csharp
// 検索バーの Field にアクセスして値を設定、検索実行
Search.SearchModule.Name.SearchValue = "山田";
await Search.ExecuteSearch();

// 条件クリア
await Search.ExecuteClear();
```

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [List](List.md) / [DetailList](DetailList.md) / [TileList](TileList.md) — 検索結果の表示先
- [Module 検索設定](../module/module_search.md)
