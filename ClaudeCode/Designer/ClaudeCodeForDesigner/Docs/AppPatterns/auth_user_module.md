# ユーザーモジュールと認証連動

業務アプリ全体の前提となる「ログインユーザーをアプリ内のレコードとして持つ」「現在のログインユーザーを画面・スクリプトから参照する」「パスワードを変更する」といった基本パターン。

## アプリの作り

<!-- 画像参照: Manual の Image/web/patterns/auth_my_profile.png (ここではコメントアウト) -->

- ユーザーがログインすると、サイドバーに「マイプロフィール」リンクが表示される
- マイプロフィールを開くとログイン中ユーザーの情報 (表示名・メール等) が読み取り専用で表示される
- 「パスワード変更」ボタンでダイアログが開き、その場でパスワードを変更できる
- 管理画面の「ユーザー管理」ではすべてのユーザーを CRUD できる (管理者のみ)

## 支えるデータ構造

```
app_users  (ASP.NET Identity の AspNetUsers をベースに拡張)
├── Id              PK
├── UserName        TEXT (ログイン ID)
├── PasswordHash    TEXT (Extras の PasswordHashField でハッシュ管理)
├── 表示名           TEXT
├── EmailAddress    TEXT
├── IsActive        BOOLEAN
└── ...
```

`AppUser` は Cookie 認証 (ASP.NET Identity) の `AspNetUsers` テーブルに紐づく。CLB の `app.clprj` の `CurrentUserModuleDesignName: "AppUser"` で「現在のログインユーザー = AppUser のレコード」と紐づけ、スクリプトから `CurrentUser.表示名.Value` のようにアクセスできるようになる。

## モジュールとテーブルの対応

| モジュール | テーブル | 主な役割 |
|---|---|---|
| `AppUser` | `app_users` (= AspNetUsers) | ユーザーマスタ。管理画面で CRUD |
| `MyProfile` | (なし、表示専用) | ログイン中ユーザーの自分用情報表示 + パスワード変更ボタン |
| `ChangePasswordDialog` | `app_users` (同じテーブル) | 自分のパスワードだけ更新できるダイアログ用モジュール |

## CLB ではこう作る

- **AppUser モジュール**: 通常の CRUD モジュールとして `app_users` テーブルに紐づける。`Codeer.LowCode.Blazor.Extras` の `PasswordHashField` でハッシュ管理
- **app.clprj** の `CurrentUserModuleDesignName: "AppUser"` を指定 → スクリプトの `CurrentUser` から AppUser インスタンスにアクセスできるようになる
- **MyProfile** は表示専用モジュール (`DbTable: ""`)。`CurrentUser.表示名.Value` 等を Label/Text に流し込んで表示
- **パスワード変更**は ChangePasswordDialog (同じ `app_users` テーブルを参照する別モジュール) を `ShowDialog` で開く

## 認証パターン集の対応

- サイドバー **`マイプロフィール`** → `MyProfile`
- サイドバー **`管理画面へ` → `ユーザー管理`** → `AppUser` (管理者のみアクセス)

## 落とし穴

- `AppUser` に `UserReadCondition` / `UserWriteCondition` を**つけてはいけない** (CurrentUser のソースになるため、制限すると マイプロフィール / パスワード変更 / LinkField 表示が全部壊れる)。管理者だけアクセスさせたい場合は PageFrame レベル (`AdminHome.UserReadCondition`) で絞る → [管理画面の分離パターン](auth_admin_frame.md)
- パスワードは平文保存しない。`PasswordHashField` (Extras パッケージ) を使う

## 関連ドキュメント

- [認証パターン集 一覧](auth_patterns.md)
- [認証 / 認可の概要](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Manual/blob/main/JP/authorization/authorization.md)
