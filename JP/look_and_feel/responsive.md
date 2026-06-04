# レスポンシブ対応の実現方法

Codeer.LowCode.Blazor では、CSS のメディアクエリを直接書き込まなくても、**PageFrame レベル**と**レイアウトレベル**の機能を組み合わせて画面幅・デバイスの違いに追従する画面を作れます。このページはレスポンシブ対応で使う機能のまとめです。

アプローチは大きく 2 つあります。

1. **画面ごと作り分けて切り替える** — PC とスマホで構成が大きく違うときは、PageFrame を分けてルート URL で出し分ける
2. **同じ画面を追従させる** — 折り返し・比率・伸縮・ズームで、1 つのレイアウトを画面幅に合わせる

---

## 機能一覧

| やりたいこと | 使う機能 | 設定場所 | 詳細 |
|---|---|---|---|
| PC とスマホで**画面構成ごと**切り替える | 対象デバイス (`TargetDevice`) / 適用開始幅 (`WidthFrom`) | PageFrame | [画面幅・デバイスでアプリケーションルートを切り替える](../designer/page_frame.md#画面幅デバイスでアプリケーションルートを切り替える) |
| 狭い画面でサイドバーを折りたたむ | モバイル時の動作 = `CollapseToHamburger` | PageFrame のサイドバー | [モバイル時の動作](../designer/page_frame.md#モバイル時の動作) |
| 固定幅で作った画面を**そのまま縮めて**収める | 自動ズーム (`AutoZoom: FitToWidth` + 基準幅 `BaseWidth`) | PageFrame | [自動ズーム](../patterns/frame_patterns.md#自動ズーム-基準幅追従) |
| 列が入りきらないときに折り返す | `IsWrap` | Row | [Wrap 系の使い分け](../module/layout.md#wrap-系の使い分けisflowlayout--isautofillwrap--iswrap) |
| カードを幅に応じて 2 列・3 列…と**均等に**並べ替える | `IsAutoFillWrap` + `MinWidth` | Grid または Row | 同上 |
| 行・列を区切らず並べて折り返す | フローレイアウト (`IsFlowLayout`) | Grid | 同上 |
| 列幅の**配分を保ったまま**伸縮させる | 列幅を比率で拡大縮小 (`IsProportionalScale`) | Row | [列幅を比率で拡大縮小](../module/layout.md#列幅を比率で拡大縮小isproportionalscale) |
| 伸縮する列に下限・上限を付ける | `MinWidth` / `MaxWidth` | Column | [列幅の決定ルール](../module/layout.md#列幅の決定ルール) |
| タイルを幅に応じて折り返して並べる | `TileListField` (+ `TileWidth`) | Field | [TileListField](../fields/TileList.md) |
| 縦方向の残り領域に追従させる | `IsFillAvailable` | Grid | [残り領域を埋める](../patterns/layout_patterns.md#残り領域を埋める-fillavailable) |

---

## どれを使うか（シーン別）

### スマホでは画面の作りを変えたい

折り返しで収まるレベルを超えて「スマホは縦一列の専用 UI にしたい」場合は、コンパクト用の PageFrame を別に作り、**適用開始幅** で出し分けます（例: `Main` に適用開始幅 900、コンパクト側は条件なし）。タッチ端末かどうかで分けたいなら **対象デバイス** (`PC` / `Touch`) を使います。

→ 標準パターン集 **`別フレーム/画面幅で切替`**

### PC 用の画面をタブレットでそのまま使いたい

レイアウトを作り直さず、**自動ズーム** (`FitToWidth`) で全体をブラウザズーム風に縮めて収める割り切りが手早いです。基準幅 (`BaseWidth`) に PC 設計時の幅を指定します。

→ 標準パターン集 **`別フレーム/自動ズーム`**

### 1 つの画面を幅に追従させたい

- **ダッシュボードのカード群**: `IsAutoFillWrap` + `MinWidth` で「広い画面は 4 列、狭くなったら 2 列」のような均等折り返し。カード 1 枚ずつなら `TileListField` も同じ発想
- **入力フォーム**: 行に `IsWrap` を付け、各列に `MinWidth` を指定して、狭くなったら折り返す
- **表・帳票風の列配分**: `IsProportionalScale` で列幅を比率扱いにして、幅が変わっても配分 (例 1:2:1) を維持
- **高さ方向**: 一覧を画面下端まで広げたいなら `IsFillAvailable`

→ 標準パターン集 **`レイアウト/折り返し対応`** / **`レイアウト/列幅を比率で拡大縮小`** / **`レイアウト/残り領域を埋める`**

### サイドバーだけ何とかしたい

PageFrame のサイドバーで **モバイル時の動作** を `CollapseToHamburger` にすると、狭い画面ではサイドバーが折りたたまれてハンバーガーメニューになります。多くの業務画面は「ハンバーガー + 行の折り返し」で十分実用になります。

---

## 組み合わせの目安

- まずは **`CollapseToHamburger` + 折り返し系 (`IsWrap` / `IsAutoFillWrap`)** で 1 画面のまま対応できないかを検討する
- それで崩れる画面だけ **自動ズーム** で逃がすか、**PageFrame 切替** で専用画面を用意する
- 自動ズームは「全体を縮める」機能なので、折り返し系とは役割が重なります。1 つの PageFrame ではどちらかに寄せるのがおすすめです
- 機能で足りない細かい調整は、`app.css` に通常の CSS（メディアクエリ含む）を書いて補えます（[CSS](css.md) / [カスタムスタイル](custom_styles.md) 参照）

---

## 標準パターン集で動きを確認する

デザイナの新規プロジェクトで「標準パターン集」テンプレートを展開すると、以下のサンプルで実際の動きを確認できます。

| サイドバー | 内容 |
|---|---|
| `別フレーム/画面幅で切替` | 適用開始幅によるルート URL の PageFrame 出し分け |
| `別フレーム/自動ズーム` | `FitToWidth` + 基準幅 |
| `レイアウト/折り返し対応` | `IsWrap` + `MinWidth` によるカードの折り返し |
| `レイアウト/列幅を比率で拡大縮小` | 比率行と固定幅行の比較 |
| `レイアウト/残り領域を埋める` | `IsFillAvailable` で一覧が画面下端まで広がる |

---

## 関連ドキュメント

- [レイアウト（Grid / Canvas / Flow）](../module/layout.md) — 折り返し・列幅・比率の全仕様
- [PageFrame](../designer/page_frame.md) — フレーム切替・サイドバー・自動ズームの全仕様
- [画面レイアウトのパターン](../patterns/layout_patterns.md) / [別フレームのパターン](../patterns/frame_patterns.md)
- [CSS](css.md) / [カスタムスタイル](custom_styles.md)
