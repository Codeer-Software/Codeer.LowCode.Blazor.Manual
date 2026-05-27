# 別フレームのパターン

アプリ全体のナビゲーション構造を切り替える `PageFrame` 単位のサンプル。サイドバー型・ヘッダー型・自動ズーム・色テーマなど、フレーム全体の見た目を別パターンで提示します。

---

## ヘッダー型ナビゲーション

<!-- 画像参照: Manual の Image/web/patterns/frame_header.png (ここではコメントアウト) -->

通常の左サイドバー型と違い、画面上部のヘッダーにナビゲーションを配置するパターン。`PageFrame` の `Header.IsVisible` を有効にして `Left.IsVisible` を無効にする構成。

**標準パターン集の対応**: サイドバー **`別フレーム/ヘッダー型ナビ → `HeaderFrame` (別 PageFrame)`**

---
## 自動ズーム (基準幅追従)

<!-- 画像参照: Manual の Image/web/patterns/frame_zoom.png (ここではコメントアウト) -->

`PageFrame.AutoZoom: FitToWidth` + `BaseWidth` を指定すると、画面幅に応じて UI 全体を**ブラウザズーム風に拡大縮小**。タブレット表示で固定幅レイアウトをきれいに収めたいときに使う。

**標準パターン集の対応**: サイドバー **`別フレーム/自動ズーム → `ZoomFrame` (別 PageFrame)`**

---
## 色テーマ変更

<!-- 画像参照: Manual の Image/web/patterns/frame_color.png (ここではコメントアウト) -->

`PageFrame.Color` / `BackgroundColor` / `FontFamily` を切り替えて、サイドバー・ヘッダーの配色や全体のフォントを別テーマに。同じモジュール群でも『管理者用は暗色、一般ユーザーは明色』のような分け方ができる。

**標準パターン集の対応**: サイドバー **`別フレーム/色テーマ変更 → `ColorFrame` (別 PageFrame)`**

---

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../ModuleDesign.md)
- [Field リファレンス](../Fields/)
