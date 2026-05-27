# ダイアログ・ポップアップ・通知のパターン

別画面に遷移せず、その場でユーザー操作を求めるための各種 UI。`MessageBox` / `Module.ShowDialog` / `ShowPanel` / `ShowPopup` / `Toaster` / `ContextMenuField` を使い分けます。

---

## メッセージボックス (確認・通知)

<img src="../../Image/web/patterns/dialog_msg.png" alt="MessageBox による確認ダイアログ" style="border: 1px solid #ccc;" width="800">

`MessageBox.Show()` で確認/通知ダイアログを表示。本文は MarkupString として描画されるので HTML タグ (太字・色・改行・箇条書き) が使えます。ボタン数・文言・スタイルを変えるだけで削除確認/エラー表示/情報通知に応用できる定番の API。

**標準パターン集の対応**: サイドバー **`ダイアログ等/メッセージボックス → `ConfirmDialogSample``**

---
## ボタンスタイル

<img src="../../Image/web/patterns/dialog_buttonstyle.png" alt="ダイアログボタンのスタイル指定" style="border: 1px solid #ccc;" width="800">

`MessageBox.Show` / `Module.ShowDialog` のボタン引数に `DialogButton` 派生クラス (`DangerButton` / `WarningButton` / `SuccessButton` / `PrimaryOutlineButton` 等) を渡して色やバリアントを指定できる。確認系では Danger、成功系では Success と使い分ける。

**標準パターン集の対応**: サイドバー **`ダイアログ等/ボタンスタイル → `DialogButtonStyleSample``**

---
## Module 編集ダイアログ

<img src="../../Image/web/patterns/dialog_edit.png" alt="別 Module をダイアログとして開く" style="border: 1px solid #ccc;" width="800">

「コメント追加」ボタンで別の Module をダイアログとして開き、入力結果を画面下のリスト (メモリ) に追加するパターン。DB に書き込まず画面リロードで消える「一時メモ」用途や、複雑な編集を別画面に切り出すときに使う。

**標準パターン集の対応**: サイドバー **`ダイアログ等/編集 → `EditDialogSample``**

---
## スライドパネル

<img src="../../Image/web/patterns/dialog_panel.png" alt="右/左からスライドして出てくるパネル" style="border: 1px solid #ccc;" width="800">

`Module.ShowPanel()` で画面右側 (or 左側) からスライド表示するパネル。フィルタ条件や補助メニューなど、**画面を切り替えずに作業を続けたい**ケースに向きます。

**標準パターン集の対応**: サイドバー **`ダイアログ等/パネル → `ShowPanelSample``**

---
## ポップアップ (任意座標)

<img src="../../Image/web/patterns/dialog_popup.png" alt="ボタン直下にフローティング表示" style="border: 1px solid #ccc;" width="800">

`Module.ShowPopup(x, y, ...)` で**指定座標にフローティング小ウィンドウ**を表示。`OpenPopup.GetClientRect()` でボタンの矩形を取得して直下に出すパターンが定番。

**標準パターン集の対応**: サイドバー **`ダイアログ等/ポップアップ → `ShowPopupSample``**

---
## 右クリックメニュー

<img src="../../Image/web/patterns/dialog_context.png" alt="ContextMenuField によるコンテキストメニュー" style="border: 1px solid #ccc;" width="800">

`ContextMenuField` で右クリック時のメニュー項目 (ラベル + クリックハンドラ) を定義し、フィールドや行に割り当てるパターン。`ContextMenu` プロパティで参照する。

**標準パターン集の対応**: サイドバー **`ダイアログ等/右クリック → `ContextMenuSample``**

---
## トースト通知

<img src="../../Image/web/patterns/dialog_toast.png" alt="Toaster による画面右下通知" style="border: 1px solid #ccc;" width="800">

`Toaster.Show()` で操作結果を画面右下などに通知表示。Success / Info / Warning / Error の種別あり。「保存しました」「エラーが発生しました」などフローを止めずに知らせたいときに使う。

**標準パターン集の対応**: サイドバー **`ダイアログ等/トースト → `ToastSample``**

---

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../module/module.md)
- [Field リファレンス](../fields/)
