# ListUpDownButtonField (リスト上下ボタン)

## これは何か

**一覧の行を上下に移動する ▲▼ ボタン**。一覧（[List](List.md) / [DetailList](DetailList.md) / [TileList](TileList.md)）の行に表示するモジュールのレイアウトに置くと、押した行を一覧の中で 1 行上・1 行下へ移動します。

## いつ使うか

- 明細の並び順をユーザーが入れ替えられるようにしたい
- 行移動のボタンを、削除ボタンなどと一緒に自分で決めた位置に置きたい

> `UseIndexSort`（インデックスソート）と `CanUpdate`（更新）を有効にした一覧には、行の操作列に同じ ▲▼ が標準で表示されます。ListUpDownButton は、その ▲▼ を行のレイアウトの好きな位置に置きたいときに使います。

---

## デザイナでの設定

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `リスト上下ボタン` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |
| **Variant** | ボタンのスタイル | enum | `OutlineSecondary` | [Button の Variant](Button.md#variantボタンのスタイル) 参照 |

> ListUpDownButtonField は値を持たないため、`表示名` / `必須` / `DBカラム` はありません。

---

## 動作

- ▲ で 1 行上へ、▼ で 1 行下へ移動します。先頭の行では ▲、末尾の行では ▼ が無効になります
- フィールドが無効（`IsEnabled = false`）または閲覧表示のときは、両方のボタンが無効になります
- 一覧の行以外の場所に置いた場合は何もしません（デザイナ上では配置確認のため両方有効な見本が表示されます）
- 移動すると一覧の `OnDataChanged` が発生します。ページをまたいで移動した場合は、移動先の行が見えるページに切り替わります（`IsInMemoryPaging` のとき）

### 並び順の保存

移動は画面上の並び替えです。並び順を DB に保存するには、一覧の `UseIndexSort`（インデックスソート）を有効にし、`SearchCondition.SortConditions` の先頭に並び順を保存する Number フィールドを指定します。移動のたびに、そのフィールドへ行の順番（0 始まり）が設定されます。

---

## スクリプトから

スクリプト公開メンバーは共通プロパティ（`IsEnabled` / `IsVisible` / `Color` など）のみです。
[Field 共通プロパティ](common_properties.md) を参照。

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [List](List.md) / [DetailList](DetailList.md) / [TileList](TileList.md)
- [Button](Button.md) — ボタンの Variant
