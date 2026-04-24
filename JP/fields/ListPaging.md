# ListPagingField (リストページング)

## これは何か

**[List](List.md) / [DetailList](DetailList.md) / [TileList](TileList.md) のページャー（ページ送り UI）を、リスト本体とは別の場所に独立配置するためのフィールド**。

通常、ページャーは List 系 Field の `PagerPosition`（Top / Bottom）で表示位置を指定しますが、**画面の離れた位置にもう一つのページャーを置きたい**、あるいは**カスタムレイアウトでリストとページャーを分離したい**場合にこの Field を使います。

## いつ使うか

- 長いリストの上下両方にページャーを置きたい
- 画面のフッター部などリストと離れた位置にページャーを配置したい
- カスタムの画面デザインでページャーとリスト本体を別々のセルに分けたい

---

## デザイナでの設定

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `リストページング` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |
| **ListFieldName** | リストフィールド名 | string | `""` | 対象の List / DetailList / TileList の Name |
| **IgnoreModification** | 変更判定から除外 | bool | `false` | 変更検知から除外 |

> `ListFieldName` は `IListFieldDesign` を実装する Field（List / DetailList / TileList）の名前である必要があります。

---

## 動作の仕組み

1. Module 内の `ListFieldName` に対応する ListField を探す
2. ListField の現在のページ情報（`Page` / `PageCount` / `TotalCount` / `LimitCount`）を参照して UI を描画
3. ユーザーがページ番号をクリックすると ListField の `PagingAsync(page)` を呼び出し、リスト本体と同期してページを変更

---

## スクリプトから

スクリプト公開メンバーはありません（全 `[ScriptHide]` または `internal`）。

ページ操作をスクリプトから行いたい場合は、[ListField 側のメソッド](List.md#スクリプトから) を使ってください。

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [List](List.md) / [DetailList](DetailList.md) / [TileList](TileList.md)
