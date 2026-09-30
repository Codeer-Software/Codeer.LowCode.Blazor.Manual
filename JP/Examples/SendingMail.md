# メールを送信する

メール送信は拡張ライブラリ [Codeer.LowCode.Blazor.Extras](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras) の **MailField** で行います。
フィールドを置くだけで送信ボタンになり、宛先・件名・本文はレコードの値から組み立てられます。アプリテンプレートで作成したプロジェクトには最初から組み込まれています。

ここでは最短の手順だけを説明します。一斉送信 (BulkMailField)・送信履歴・プレビュー・送信インフラごとの設定などの詳細は、Extras のドキュメント [メール送信](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/Mail.md) を参照してください。

## ステップ

### 1. サーバーの送信設定を書く

Server プロジェクトの `appsettings.json`（開発環境では `appsettings.Development.json`）に、共通設定の `Mail` と、使う送信インフラの設定を書きます。
SMTP サーバーから送る場合の例:

```json
"Mail": {
  "DefaultInfraName": "Smtp",
  "DefaultBulkInfraName": "Smtp",
  "HistoryModuleName": ""
},
"Smtp": {
  "SenderMailAddress": "notify@your-domain.example",
  "SenderDisplayName": "業務システム",
  "Host": "smtp.your-domain.example",
  "Port": "587",
  "SSL": "true",
  "UserName": "",
  "Password": ""
}
```

- 送信インフラは SMTP のほか Microsoft Graph (Microsoft 365)・SendGrid・Gmail API を用意しています。`DefaultInfraName` に使うものの名前を書きます
- パスワードなどの秘密情報は環境変数やユーザーシークレットに置いてください（例: 環境変数 `Smtp__Password`）
- 差出人は常にこの設定の送信者（システム送信者）です。担当者本人のアカウントから送りたい場合は Windows アプリの [MailSender](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/MailSender.md) を使います

### 2. モジュールに MailField を置く

送信元にしたいモジュールの詳細レイアウトに **MailField** を置き、プロパティで宛先と文面を設定します。

| プロパティ | 設定例 |
|---|---|
| 宛先変数 | `Email.Value`（自レコードのフィールド。リンク先なら `Customer.Email.Value`） |
| 件名 | `注文確認: {OrderNo.Value}` |
| 本文 | `{Customer.Name.Value} 様 ご注文を受け付けました。` |

`{変数}` は自レコードの値で置き換えられます。詳細画面に送信ボタンが表示され、押すと確認ダイアログのあとにメールが送られます。

### 3. (必要なら) スクリプトから送る

文字付きのボタンにしたい場合や、宛先・文面を動的に決めたい場合は、ButtonField の OnClick などから MailField の `Send()` を呼びます。

```csharp
void ButtonSend_OnClick()
{
    ReceiptMail.To = TextRecipient.Value;
    ReceiptMail.Subject = TextSubject.Value;
    ReceiptMail.Body = TextContent.Value;
    var result = ReceiptMail.Send();
    if (result.IsSuccess) MessageBox.Show("送信しました。");
    else MessageBox.Show("送信できませんでした: " + result.Failures[0].Error);
}
```

MailField はレイアウトに置かなくても、モジュールのフィールドとして定義してあればスクリプトから使えます。

## 旧方式 (MailService) について

以前のテンプレートにあった、`appsettings.json` の `MailSettings` とスクリプトの `MailService.SendEmail(...)` で送る方式は、Extras 0.12.0 で削除されました（スクリプトから権限を通らずに送れてしまうため）。
現在のテンプレートには含まれていません。MailField / BulkMailField に置き換えてください。移行の詳細は [メール送信](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/Mail.md) の「0.5.0 のメール API から移行する」を参照してください。

## 関連情報
- [メール送信 (Extras)](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/Mail.md)
- [サーバー API の権限チェック (Extras)](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras/blob/main/docs/ServerApiAuthorization.md)
- [Module](../module/module.md)
- [スクリプト概要](../script/script.md)
