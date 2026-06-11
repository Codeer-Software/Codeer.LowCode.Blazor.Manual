# PasswordHashField - パスワードハッシュ保存フィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Extras.Designs.PasswordHashFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Blazor.Extras`

入力されたパスワードを**ソルト付きハッシュ（PBKDF2-HMAC-SHA256）**として DB に保存するためのフィールド。パスワード平文そのものは保存しない。同じモジュール内の `PasswordField`（平文入力欄）を `PasswordFieldName` で参照し、その入力値からハッシュ＋ソルトを計算して 2 つの DB カラム（`DbColumnHash` / `DbColumnSalt`）に書き込む。両カラムは**書き込み専用**（読み戻されない）。

> [PasswordField](PasswordField.md)（core）との違い: PasswordField は値を 1 カラムに保存する入力フィールド。PasswordHashField は**入力欄を持たず**、別の PasswordField の入力をハッシュ化して保存する仕組み。ログインユーザーのパスワード保管など、平文を残したくない用途で使う。

## ⚠ サーバ側の実装が必須（重要）

このフィールドを置くだけでは**ハッシュは計算されない**。ハッシュ化はサーバ側のヘルパ `Codeer.LowCode.Blazor.Extras.Services.PasswordHashHelper` を呼んで行う。

- 保存時にハッシュ化: `ModuleDataIO` の派生（通常 `CustomizedModuleDataIO.AddAsync` / `UpdateAsync`）で `PasswordHashHelper.ApplyPasswordHash(moduleDesign, data)` を呼ぶ。これでモジュール上の各 PasswordHashField について、参照先 PasswordField に値があればハッシュ＋ソルトを再計算して書き込む。
- ログイン照合: `PasswordHashHelper.VerifyHash(password, hash, salt)` で検証する。

ハッシュ仕様: PBKDF2-HMAC-SHA256 / 100,000 反復 / 32 バイトソルト / 32 バイトハッシュ / base64 文字列で保存。

## C# クラス定義 (真実の源)

このフィールドは外部ライブラリ `Codeer.LowCode.Blazor.Extras` で定義されている。`FieldDesignBase` を継承する。ランタイムの Field は UI も submit データも持たない（ハッシュ書き込みは上記サーバ側ヘルパが担う）。

## プロパティ

> 共通プロパティ（Name, IgnoreModification, OnValidateInput）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `PasswordFieldName` | string | `""` | ハッシュ元の平文を入力する、**同じモジュール内の PasswordField** のフィールド名。 |
| `DbColumnHash` | string | `""` | ハッシュ（base64）を保存する DB カラム名。**書き込み専用**（読み戻さない）。 |
| `DbColumnSalt` | string | `""` | ソルト（base64）を保存する DB カラム名。**書き込み専用**。 |

`DbColumnHash` / `DbColumnSalt` は実テーブルに存在するかデザインチェックで検証される。

## 必要な DB 構成

ハッシュ・ソルトはいずれも base64 文字列（32 バイト → 44 文字程度）。文字列カラムを 2 つ用意する。

```sql
password_hash TEXT NULL,
password_salt TEXT NULL
```

## JSON例

```json
{
  "PasswordFieldName": "Password",
  "DbColumnHash": "password_hash",
  "DbColumnSalt": "password_salt",
  "Name": "PasswordHash",
  "IgnoreModification": false,
  "OnValidateInput": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Extras.Designs.PasswordHashFieldDesign"
}
```

同じモジュールに、入力用の `PasswordField`（`Name: "Password"`、DB カラムなし＝平文を保存しない設計も可）と、この `PasswordHashField` を両方置く。

> 既定状態は [../../Defaults/PasswordHashFieldDesign.json](../../Defaults/PasswordHashFieldDesign.json) を参照。

## スクリプトAPI

ランタイム Field は値・データ系メソッドを公開しない（`IsModified` は常に `false`）。固有のスクリプト API は無い。
