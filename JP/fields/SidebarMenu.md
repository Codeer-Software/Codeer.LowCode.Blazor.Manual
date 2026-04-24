# SidebarMenuField (サイドバーメニュー)

## これは何か

**[PageFrame](../designer/page_frame.md) のサイドバーに表示されるメニューを、カスタムモジュールで組み立てるための Field**。
左右のサイドバーを独自モジュールで置き換えたい時、そのモジュール内にこの Field を配置して、標準のメニュー機能を部分的に埋め込めます。

> 通常は PageFrame の「Left」「Right」サイドバー設定だけでメニューを構築できます。この Field を使うのは、**カスタムモジュールで独自のサイドバー UI を組む**ケースです。PageFrame の `Left.ModuleName` / `Right.ModuleName` に代替モジュールを指定した時に、この Field を通じて標準メニュー機能を取り込みます。

## いつ使うか

- サイドバーに独自のパーツ（ウィジェット・お知らせ・検索など）を挟みたい
- 標準のレイアウトでは実現できないサイドバー構成にしたい
- Placement（左／右）と組み合わせて、右サイドバーにも独自 UI を出したい

---

## デザイナでの設定

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `サイドバーメニュー` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |
| **Placement** | 配置 | enum | `Left` | 左右どちらのサイドバーを対象にするか（`Left` / `Right`） |
| **Type** | メニュー種別 | enum | `Items` | どの標準パーツを表示するか |
| **IgnoreModification** | 変更判定から除外 | bool | `false` | 変更検知から除外 |

### Type の選択肢

| 値 | 描画される内容 |
|---|---|
| **Home** | PageFrame の `Left.Home` / `Right.Home` の内容（テキスト / アイコン / 画像） |
| **Items** | PageFrame の `Left.Links` / `Right.Links` のメニュー項目群 |
| **UserName** | ログイン中のユーザー名 |
| **Logout** | ログアウトボタン |

同じモジュール内に複数配置すれば、それぞれの要素を別々の位置に並べられます。

---

## スクリプトから

スクリプト公開メンバーは共通プロパティのみ。[Field 共通プロパティ](common_properties.md) を参照。

---

## 関連項目

- [PageFrame](../designer/page_frame.md)
- [HeaderMenuField](HeaderMenu.md) — ヘッダー版
- [Field 共通プロパティ](common_properties.md)
