# PasswordField (パスワード)

## これは何か

**パスワード入力用のフィールド**。入力内容は伏せ字で表示され、確認入力との一致チェック機構も備えています。

> **重要**: PasswordField は単体では DB に保存されません。**PasswordHashField** と組み合わせて、ハッシュ化された値を DB に書き込む仕組みになっています。詳細は [PasswordHashField との組み合わせ](#passwordhashfield-との組み合わせ) を参照。

## いつ使うか

- ユーザー登録時のパスワード入力
- パスワード変更画面
- 再入力（確認）との一致チェック

---

## デザイナでの設定

<img src="../../Image/designer/fields/password/PasswordBasic_properties_panel.png" alt="PasswordFieldのプロパティパネル" style="border: 1px solid;" width="400">

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `パスワード` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |
| **DisplayName** | 表示名 | string | `""` | 画面表示用の名前 |
| **IsRequired** | 必須 | bool | `false` | 入力必須 |
| **OnDataChanged** | データ変更イベント | string | `""` | 値変更時のスクリプトイベント |
| **IgnoreModification** | 変更判定から除外 | bool | `false` | 変更検知（IsModified）から除外 |

> `DbColumn` は PasswordField にはありません。保存は次章の PasswordHashField が担います。

---

## スクリプトから

### プロパティ・メソッド

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `Value` | string? | パスワード値 |
| `CheckPassword()` | bool | 画面上の本入力と確認入力欄の値が一致するかを返す |
| `Clear()` | Task | 本入力・確認入力の両方をクリア |

> 確認入力欄の値（内部の `ConfirmPassword`）はスクリプトから直接参照できません。`CheckPassword()` を通じて一致判定だけ行えます。

共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

### よく使う例

```csharp
// 登録時にパスワードと確認入力が一致するかチェック
void SaveButton_OnClick()
{
    if (!Password.CheckPassword())
    {
        Password.SetError("確認入力と一致しません");
        return;
    }

    if (await Submit())
    {
        Password.Clear();
        Toaster.Success("登録しました");
    }
}
```

---

## 保存の仕組み (ログインアカウント契約 / PasswordHashField)

PasswordField が扱うのは**画面での入力**のみで、DB への保存・検証はサーバー側 (**CustomizedModuleDataIO** / **PasswordHashHelper**) で行われます。ハッシュ / ソルトをどの列に書くかは、ユーザーモジュールでは **LoginAccountContractField** (ログインアカウント契約) の `PasswordField`、それ以外のモジュール (パスワード変更ダイアログなど) では **PasswordHashField** が宣言します。

この構成は Visual Studio テンプレート（`Codeer.LowCode.Blazor`）で新規作成したソリューションに最初から含まれています。デザイナの各テンプレートに入っている `AppUser` モジュールも、この仕組みでパスワードを保存しています。

> ログイン機能を外したホストでは PasswordHashField はどの認証とも結び付かないため、自前の認証と繋ぎ込まない限り意味を持ちません。

### 仕組み

```
画面                    Module                          DB
──────                  ─────────────────               ────────────────
[ パスワード入力 ]  ──▶  PasswordField.Value  ──┐
                                               │
                                               ├──▶ PasswordHashHelper
                                               │    ・ Salt をランダム生成（32byte）
                                               │    ・ PBKDF2 + SHA256 で 10 万回ハッシュ化
                                               │
                                               ▼
                        PasswordHashField ──────────▶  DbColumnHash (Hash)
                                              ──────▶  DbColumnSalt (Salt)
```

保存時の流れ:

1. ユーザーが PasswordField に入力
2. Submit → サーバー側の `CustomizedModuleDataIO.AddAsync` / `UpdateAsync` が呼ばれる
3. `PasswordHashHelper.ApplyPasswordHash` が Module 内の `LoginAccountContractField` (PasswordField 指定あり) または `PasswordHashField` を見つける
4. 対応する PasswordField の値をハッシュ化してそのフィールドのデータに格納
5. 宣言されたハッシュ / ソルトの列に書き込まれる

### 配置手順

ユーザーモジュール (`AppUser`):

1. Module に **PasswordField** を配置（画面入力用）
2. 同じ Module の **LoginAccountContractField** の `PasswordField` にその名前を指定し、`DbColumnPasswordHash` / `DbColumnPasswordSalt` にハッシュ・ソルトの列を指定（テンプレートの AppUser は設定済み）

その他のモジュール (同じテーブルを参照するパスワード変更ダイアログなど):

1. Module に **PasswordField** を配置
2. 同じ Module に **PasswordHashField** を配置（DB 保存用、画面には表示されない）し、`PasswordFieldName` / `DbColumnHash` / `DbColumnSalt` を指定

どちらもサーバー側で `CustomizedModuleDataIO` が登録されていることが前提です（テンプレート出力のまま OK）。

### ログイン時の検証

パスワードの検証は `PasswordHashHelper.VerifyHash(password, hash, salt)` を使います。
Cookie 認証テンプレートのログイン処理に組み込まれているので、通常は追加実装不要です。

### 独自認証に差し替える場合

- 契約 / PasswordHashField と CustomizedModuleDataIO の仕組みをそのまま使える場合はそのままで OK
- 独自のハッシュアルゴリズムを使いたい場合は、`CustomizedModuleDataIO.AddAsync` / `UpdateAsync` と `PasswordHashHelper` を書き換えます（Extras は MIT なのでコピーして改変できます）
- ログインの仕組み全体は [Extras の認証ドキュメント](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/Authentication.md) を参照

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [Text](Text.md) — 通常の文字入力
- [認証・認可](../authorization/authorization.md)
- [チュートリアル: 認可を設定する](../tutorials/tutorial_auth.md)
