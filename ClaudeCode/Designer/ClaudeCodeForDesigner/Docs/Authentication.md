# 認証の仕組み (Cookie 認証 / 既定)

CLB の**既定の認証バリアントは Cookie 認証**で、サーバ実装は `Source/App/Cookie/Server` にある。このドキュメントは、デザインファイル作成者が「ログイン・現在のユーザー・ユーザー管理」を正しく組むために、**サーバが何を期待しているか (＝デザイン側が満たすべき契約)** をまとめたもの。実機サンプルとパターンは [AppPatterns/auth_patterns.md](AppPatterns/auth_patterns.md) / [AppPatterns/auth_user_module.md](AppPatterns/auth_user_module.md) を参照。

> ⚠ **ASP.NET Identity ではない。** 既定の Cookie 認証は `AspNetUsers` も `UserManager` も使わない。**プレーンなユーザーテーブル (既定 `app_users`) を Dapper で直接引き、独自のハッシュ照合 (`PasswordHashHelper`) で検証して Cookie を発行する**シンプルな実装。一部ドキュメントの「ASP.NET Identity / AspNetUsers」表現は不正確なので、テーブル/列はこのドキュメントを正とする。

## ログインの流れ (サーバ実装の要約)

1. クライアントが `POST /api/account/login` に `{ Id, Password, IsPersistent }` を送る (`Id` が**ログイン識別名**、`Password` が平文)。
2. サーバは `PasswordCheckUserTableInfo` (appsettings) の設定に従い、ユーザーテーブルから `WHERE user_name = @Id` の 1 件を取得 (`id` / `user_name` / `hash` / `salt`)。
3. `PasswordHashHelper.VerifyHash(平文, hash, salt)` で照合。失敗なら 401。
4. 成功したら Cookie 認証でサインイン。Claim に **`NameIdentifier = ユーザーの id 列の値`**、`Name = ログイン識別名` を入れる。
5. 以降、`GET /api/account/current_user` は `NameIdentifier` (＝ユーザーの `id`) を返す。これが CLB の `CurrentUser` の正体。
6. ログアウトは `POST /api/account/logout`。CSRF 対策で全 POST に `X-ANTIFORGERY-TOKEN` ヘッダが要る (フレームワークの `HttpService` が自動付与するのでデザイン側の考慮は不要)。

**ログイン画面そのものはフレームワーク (クライアント) が提供する。**デザイン側でログインフォームを作る必要はない。デザイン側が用意するのは「ユーザーを格納するモジュール/テーブル」と「ユーザー管理・マイプロフィール等の画面」。

## サーバ側の契約: `PasswordCheckUserTableInfo`

サーバの `appsettings.json` がユーザーテーブルの場所と列名を持つ。Cookie テンプレートの既定値:

```json
"PasswordCheckUserTableInfo": {
  "TableName": "app_users",
  "IdColumn": "id",
  "UserNameColumn": "user_name",
  "HashColumn": "hash",
  "SaltColumn": "salt"
}
```

- **デザイン側のユーザーテーブル/列はこの設定と一致させる**。既定の Cookie バリアントをそのまま使うなら、テーブル `app_users` / 列 `id` `user_name` `hash` `salt` で作る (下記 DDL)。
- データソースは `app.clprj` の `CurrentUserModuleDesignName` が指すモジュールの `DataSourceName` が使われる (ログイン時もそのデータソースに接続する)。

## デザイン側がやること

### 1. `app.clprj` で「現在のユーザーモジュール」を指定

```json
{ "CurrentUserModuleDesignName": "AppUser" }
```

これで「ログイン中ユーザー＝この `AppUser` モジュールの、`id` が一致するレコード」と紐づき、スクリプト/条件から `CurrentUser` で参照できる。**認証ありプロジェクトでは必須**。

### 2. ユーザーモジュール (`AppUser`) を作る

`app_users` テーブルに紐づく**通常の CRUD モジュール**。必須・推奨フィールド:

| フィールド | 型 | DB 列 | 役割 |
|---|---|---|---|
| `Id` | `IdFieldDesign` | `id` | 主キー。`NameIdentifier` claim と突き合わされる (**必須**) |
| ログイン識別名 | `TextFieldDesign` | `user_name` | ログイン ID。`UserNameColumn` と一致 (**必須**) |
| パスワード (入力) | `PasswordFieldDesign` | (DB 列なし) | 平文入力欄。画面でパスワードを入れるため |
| ハッシュ | `PasswordHashFieldDesign` (Extras) | `hash` / `salt` | `PasswordFieldName` で上のパスワード欄を指定。`DbColumnHash`/`DbColumnSalt` に書き込み (**必須**) |
| 表示名 | `TextFieldDesign` | `name` | `CurrentUser.表示名` 等で使う表示用 (任意) |
| `Role` | `SelectFieldDesign` | `role` | 権限の出し分けに使う (任意) |
| `IsActive` | `BooleanFieldDesign` | `is_active` | 有効/無効 (任意) |

- **パスワードのハッシュ化はサーバが自動でやる。** サーバの `CustomizedModuleDataIO` が Add/Update のたびに `PasswordHashHelper.ApplyPasswordHash` を呼び、`PasswordField` の平文から `hash`+`salt` を生成して `PasswordHashField` の 2 列に書き込む。**デザイン側にハッシュ化スクリプトは要らない**。`PasswordField` と `PasswordHashField` をペアで Fields に置くだけ (`PasswordHashField` はレイアウトに出さなくてよい＝UI なし)。詳細は [Extras の PasswordHashField ドキュメント](Fields/PasswordHashField.md)。
- **初期ユーザーは自動作成される。** ユーザーテーブルが空のとき、サーバ起動時に `admin` / パスワード `admin` を 1 件 INSERT する。

#### ユーザーテーブル DDL (SQLite 例)

```sql
CREATE TABLE app_users (
    id         INTEGER PRIMARY KEY AUTOINCREMENT,
    user_name  TEXT NOT NULL,
    name       TEXT,
    hash       TEXT,    -- サーバが書き込む (初期は空でよい)
    salt       TEXT,    -- 同上
    role       TEXT,
    is_active  INTEGER
);
```

`hash` / `salt` は base64 で 44〜64 文字程度。可変長文字列列を 2 つ用意する。

### 3. マイプロフィール / パスワード変更 / ユーザー管理 (任意)

- **マイプロフィール**: 表示専用モジュール (`DbTable: ""`)。`CurrentUser.表示名.Value` 等を Label に流して表示。
- **パスワード変更ダイアログ**: `app_users` を参照する別モジュール (Password + PasswordHash を持つ) を `ShowDialog` で開き、自分のレコードだけ更新。
- **ユーザー管理**: `AppUser` を管理者画面で CRUD。実装サンプルは [auth_user_module.md](AppPatterns/auth_user_module.md)。

## `CurrentUser` の参照

ログインで `NameIdentifier = app_users.id` がセットされ、CLB はそれを使って `CurrentUserModuleDesignName` のモジュールから該当レコードを引く。スクリプト・条件から:

```csharp
var who = CurrentUser.表示名.Value;      // 現在ユーザーの表示名
var role = CurrentUser.Role.Value;       // 権限
```

`CurrentUser` の活用パターン (検索初期値を自分に / `SideBarDesign.UserName` に表示名 等) は [Scripts.md](Scripts.md) / 認証パターン集を参照。

> ⚠ **`AppUser` に `UserReadCondition` / `UserWriteCondition` を付けない。** `AppUser` は `CurrentUser` のソースなので、読み取り/書き込みを制限すると**マイプロフィール・パスワード変更・他モジュールでの LinkField 表示が全部壊れる**。「管理者だけユーザー管理に入れる」は**モジュールではなく PageFrame 側 / リンクの `UserReadCondition`** で絞る → [auth_admin_frame.md](AppPatterns/auth_admin_frame.md)。

## 権限による画面の出し分け

- 一般ユーザー画面と管理者画面を別 `PageFrame` に分け、管理フレームに `UserReadCondition` を付けて `Role` で絞る ([auth_admin_frame.md](AppPatterns/auth_admin_frame.md))。
- サイドバーのリンクを出し分けたいときは、**遷移先モジュール側の `UserReadCondition`** で絞る (PageFrame の条件だけだとリンクは見えてアクセス時に拒否される)。
- 自分が作ったレコードだけ見せるのは `DataReadCondition` ([auth_personal_data.md](AppPatterns/auth_personal_data.md))。

## エンドポイント早見 (参考)

| メソッド / パス | 役割 |
|---|---|
| `POST /api/account/login` | `{ Id, Password, IsPersistent }` でログイン。`Id`＝ログイン識別名 |
| `POST /api/account/logout` | ログアウト |
| `GET /api/account/current_user` | 現在ユーザーの `id` を返す (要認証) |

> デザイン側はこれらを直接叩かない (フレームワークが処理する)。挙動理解用。

## 関連ドキュメント

- [認証パターン集 一覧](AppPatterns/auth_patterns.md) ─ 実機サンプル (`PatternShowcaseAuth` テンプレート)
- [ユーザーモジュールと認証連動](AppPatterns/auth_user_module.md) ─ AppUser / マイプロフィール / パスワード変更の作り
- [一般画面と管理画面の分離](AppPatterns/auth_admin_frame.md) ─ 複数 PageFrame と権限ゲート
- [PasswordHashField (Extras)](Fields/PasswordHashField.md) ─ パスワードハッシュ用フィールドの詳細
- [ProjectSettings.md](ProjectSettings.md) ─ `app.clprj` の `CurrentUserModuleDesignName`
