# RadioButtonField (ラジオボタン)

## これは何か

**1 つのラジオボタン**。単独で使うことはなく、[RadioGroup](RadioGroup.md) の配下に配置して使います。

## いつ使うか

- [RadioGroup](RadioGroup.md) と組み合わせて、選択肢の 1 つを表すボタンとして

> 候補数が決まっていて自動生成したい場合は、RadioGroupField の `ラジオボタンの選択肢設定`（PopulateRadioButtons）を使う方法もあります。

---

## デザイナでの設定

<img src="../../Image/designer/fields/radiobutton/RankA_properties_panel.png" alt="RadioButtonFieldのプロパティパネル" style="border: 1px solid;" width="400">

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `ラジオボタン` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |
| **Text** | テキスト | string | `"Radio"` | ラジオボタンの表示文字 |
| **Value** | 値 | string | `""` | このラジオが選択された時にグループに設定される値 |
| **GroupField** | ラジオボタングループフィールド | string | `""` | 親の RadioGroupField の Name |
| **IgnoreModification** | 変更判定から除外 | bool | `false` | 変更検知（IsModified）から除外 |

> RadioButtonField は `DisplayName` / `DbColumn` / `IsRequired` などを持ちません。これらは親の [RadioGroup](RadioGroup.md) 側で管理されます。

---

## スクリプトから

### プロパティ・メソッド

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `Value` | bool | このラジオが選択されているか（親の値と一致すれば true） |
| `SetCheck()` | void | このラジオをプログラム的に選択する |
| `GetRadioGroupField()` | RadioGroupField | 親の RadioGroupField を取得 |

共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

### よく使う例

```csharp
// 条件に応じて特定のラジオを選択
if (someCondition)
{
    HighRank.SetCheck();
}
```

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [RadioGroup](RadioGroup.md) — 親のコンテナ
