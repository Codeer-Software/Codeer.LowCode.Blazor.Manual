# OptimisticLockingField

## これは何か

**楽観ロック**を実現する System Field。データ更新時に「自分が読み取った時点から、他の人が変更していないか」をチェックできます。

## いつ使うか

- 同じデータを複数ユーザーが同時に編集し得る業務アプリ
- 更新の競合検知が必要な場合

この Field を Module に追加し、DB 側の「更新のたびに値が変わる列」にマッピングするだけで、自動的に楽観ロックが働きます。

---

## デザイナでの設定

### 固有プロパティ

| プロパティ | 型 | 既定値 | 説明 |
|---|---|---|---|
| **DbColumn** | string | `""` | ロックに使う DB 列名（タイムスタンプやバージョン列） |
| **IncrementVersion** | bool | `false` | 保存時にバージョン値をインクリメントする（DB 側に仕組みがない場合） |

共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

---

## 動作の仕組み

1. データを読み込むと、現在の OptimisticLocking 値が記憶される
2. 更新時、DB 側の現在値と記憶した値を比較
3. 一致しなければ「他の人が更新しています」エラーとなり、更新は失敗する

### DB 側に必要なもの

| DB | 推奨列型 |
|---|---|
| PostgreSQL / SQL Server | 行更新のたびに変わる列（例: `xmin`、`rowversion`） |
| その他 | 数値列 + `IncrementVersion: true` |

DB に自動変更の仕組みがない場合は、トリガで代用するか `IncrementVersion` をオンにします。

---

## スクリプトから

この Field は UI を持たず、スクリプトから操作することもほぼありません。
共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [Field 全体（System Field の一覧）](field.md)
- [Module 全体設定](../module/module_general.md)
