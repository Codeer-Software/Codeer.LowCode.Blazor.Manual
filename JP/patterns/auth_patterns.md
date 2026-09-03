# 認証・権限・承認のパターン

業務アプリで欠かせない「**ログインユーザーの管理**」「**自分のデータだけ見せる**」「**申請 → 承認のワークフロー**」「**一般ユーザーと管理者の画面分離**」といったパターンをまとめたページです。

## 「標準パターン集」テンプレートの「認証・権限」「承認」グループで実機確認できる

デザイナで新規プロジェクトを作るときに、テンプレートから **「標準パターン集」** (内部名 `PatternShowcase`) を選ぶと、他のパターンと一緒にこれらのパターンの実装サンプルも展開されます。サイドバーの **「認証・権限」** と **「承認」** グループに並んでいます。

<img src="../../Image/web/patterns/create_new.png" alt="新規プロジェクト作成ダイアログのテンプレート選択" style="border: 1px solid #ccc;" width="500">

- alice / bob / carol / dave / admin の 5 ユーザーが登録済み (パスワード: `test`、admin は `admin`)
- 一般ユーザー画面 (`Main` フレーム) と管理者画面 (`AdminFrame`) の 2 つの PageFrame で構成

> 認証・権限・承認は、ログインのないアプリでは成り立ちません (申請者・承認者・データの所有者を区別できない)。デザイナのテンプレートはすべてログイン付きなので、Visual Studio 側で特別なテンプレートを選ぶ必要はありません。立ち上げ方は [認証付きプロジェクトの始め方](../authorization/auth_getting_started.md) を参照してください。

---

## パターン一覧

| パターン | 内容 |
|---|---|
| [ユーザーモジュールと認証連動](auth_user_module.md) | `AppUser` 定義、ログイン中ユーザー (`CurrentUser`) の参照、パスワード変更 |
| [個人データのフィルタと権限](auth_personal_data.md) | 自分が作ったレコードだけ見せる (`DataReadCondition`)、検索初期値で自分のデータ絞り込み |
| [承認フローのワークフロー](auth_workflow.md) | 申請 (経費/休暇) → `ApprovalFlowField` による承認 → 履歴のワークフロー |
| [一般画面と管理画面の分離 (複数 PageFrame)](auth_admin_frame.md) | `Main` フレームと `AdminFrame` の使い分け、`UserReadCondition` で管理者だけ入れる画面 |

関連: [論理削除](soft_delete.md) の「削除者を自動記録する」バリエーション (`Deleter`) も「認証・権限」グループに収録しています。

---

## 関連ドキュメント

- [認証付きプロジェクトの始め方](../authorization/auth_getting_started.md) ─ ソリューション作成からテンプレート展開・サンプルの動かし方まで
- [アプリ作成パターン 一覧](patterns.md) ─ 全パターンの入口
- [認証 / 認可の概要](../authorization/authorization.md)
- [PageFrame の設定](../designer/page_frame.md)
