# RecordPagingField (レコードページ送り)

## これは何か

**詳細画面に置く、前後のレコードへ移動するページ送り**（`‹ 3 / 120 ›`）。一覧から詳細画面を開いたとき、**一覧で見ていた並び（検索条件・ソート）のまま**、前後のレコードの詳細へ移動できます。

リストのページを切り替える [ListPaging](ListPaging.md) とは異なり、1 件ずつレコードを送ります。

## いつ使うか

- 一覧から詳細を開き、次のレコード・前のレコードを順番に確認・編集したい
- 一覧に戻らずに、検索結果を 1 件ずつ見ていきたい

---

## デザイナでの設定

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `レコードページ送り` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |

> 設定項目は名前だけです。モジュールの **Detail レイアウト** に配置します。

---

## 表示される条件

レコードページ送りは、**一覧から詳細画面へ遷移したときだけ**表示されます。

- List の `CanNavigateToDetail`（詳細画面へ遷移）で表示される行の `>` ボタンから詳細画面を開いたとき（モジュールの一覧画面から詳細を開く場合も含む）
- スクリプトで遷移の準備をしたとき（[下記](#スクリプトで組んだ一覧から遷移する場合)）

次の場合は何も表示されません。

- URL を直接開いた、ブラウザで再読み込みした、新規作成画面など、一覧から遷移していない詳細画面
- 表示中のレコードが、遷移元の一覧に含まれていない
- Detail レイアウト以外（List / Search レイアウト）に置いた場合

> デザイナ上では配置を確認できるよう、見本（`‹ 1 / 1 ›`）が表示されます。

---

## 動作

- 表示は「一覧の中での位置 / 総件数」です。先頭では `‹`、末尾では `›` が無効になります
- 一覧がページングしている場合も、ページの境目で隣のページを自動で読み込んで移動します（一覧と同じ検索条件・ソート順）
- 詳細画面に未保存の変更があるときは、移動する前に「変更が失われる」旨の確認が出ます
- 移動はブラウザの履歴を積みません。ブラウザの「戻る」で一覧に戻ります
- 入れ子のモジュール（[Module](Module.md) フィールドの中）に置いた場合も、詳細画面のレコードを送ります

---

## スクリプトから

スクリプト公開メンバーは共通プロパティ（`IsEnabled` / `IsVisible` / `Color` など）のみです。
[Field 共通プロパティ](common_properties.md) を参照。

### スクリプトで組んだ一覧から遷移する場合

一覧の `>` ボタン以外の方法（ボタンや行ダブルクリックのスクリプトなど）で詳細画面へ遷移する場合は、遷移の前に `NavigationService.SetRecordPagingContext` で「一覧の並び」を登録すると、遷移先の詳細画面でレコードページ送りが使えます。`ids` には詳細画面のモジュールの Id を一覧の並び順で渡します。

| メソッド | 用途 |
|---|---|
| `NavigationService.SetRecordPagingContext(List<string> ids)` | 一覧をページングしていない（`ids` が全件）場合 |
| `NavigationService.SetRecordPagingContext(ListField list, int pageIndex, int totalCount, List<string> ids)` | List の検索条件・ソートで、そのページの `ids` を登録する |
| `NavigationService.SetRecordPagingContext(SearchField search, int pageIndex, int totalCount, List<string> ids)` | 検索フォームの結果一覧の検索条件で登録する |
| `NavigationService.SetRecordPagingContext(ModuleSearcher searcher, int pageIndex, int totalCount, List<string> ids)` | `Limit` と `ExecutePage` でページングした `ModuleSearcher` の結果で登録する |

`pageIndex` は `ids` のページ番号（0 始まり）、`totalCount` は総件数です。ページングしている場合は、ページの境目で隣のページを同じ条件で読み込みます。

```csharp
// 行ダブルクリックで詳細画面を開く
void Orders_OnDoubleClickRow(int index)
{
    var ids = new List<string>();
    foreach (var row in Orders.Rows)
    {
        ids.Add(row.Id.Value);
    }
    NavigationService.SetRecordPagingContext(ids);
    NavigationService.NavigateTo(NavigationService.GetModuleDataUrl("Order", Orders.Rows[index].Id.Value));
}
```

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [List](List.md) — `CanNavigateToDetail`（詳細画面へ遷移）
- [ListPaging](ListPaging.md) — リストのページ送り
- [スクリプトのサービス](../script/script_services.md) — NavigationService
