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
## 画面幅で切替 (デバイス別ルーティング)

<!-- 画像参照: Manual の Image/web/patterns/frame_device.png (ここではコメントアウト) -->

複数のアプリケーションルートに **対象デバイス** (`TargetDevice`: Any/PC/Touch) と **適用開始幅** (`WidthFrom`) を設定すると、ルート URL (`/`) を開いたときの PageFrame を画面サイズやデバイスで出し分けできる。PC は通常のサイドバー型、スマホはコンパクト型、のような構成に。

標準パターン集では `Main` に適用開始幅 900 を設定し、条件なしの `Compact` を追加。900px 未満でルート URL を開くと Compact 側が選ばれる。選択ルールや補足は [PageFrame のプロパティ](../PageFrame.md) を参照。

**標準パターン集の対応**: サイドバー **`別フレーム/画面幅で切替 → `DeviceFrameSample`** (説明) + `Compact` (別 PageFrame) + `CompactHome`

---

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [レスポンシブ対応の実現方法](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Manual/blob/main/JP/look_and_feel/responsive.md) ─ 画面幅・デバイスへの追従に使う機能のまとめ
- [モジュール定義の全体構造](../ModuleDesign.md)
- [Field リファレンス](../Fields/)
