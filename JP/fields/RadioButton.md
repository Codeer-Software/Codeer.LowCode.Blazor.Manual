# RadioButtonField (ラジオボタン)

## これは何か

**グループ内の選択肢 1 つを表す UI 要素**。RadioButton 自体は値を持たず、クリックすると親の [RadioGroupField](RadioGroup.md) に自分の `Value` がセットされます。

> **RadioButtonField は自身では値を保持しません**（`IsModified` は常に `false`、DB 保存もされない）。値は必ず親の [RadioGroupField](RadioGroup.md) に集約されます。

## いつ使うか

- [RadioGroup](RadioGroup.md) と組み合わせて、グループ内の選択肢の 1 つを表すボタンとして

> 候補数が決まっていて自動生成したい場合は、RadioGroupField の `ラジオボタンの選択肢設定`（PopulateRadioButtons）を使う方法もあります。

---

## 動作の仕組み

1. ユーザーが RadioButton をクリック
2. RadioButton が親の [RadioGroupField](RadioGroup.md)（`GroupField` で指定）を探す
3. 親の RadioGroup に**自分の `Value`** が設定される
4. 同じグループの他の RadioButton は自動的に未選択状態になる

```
ユーザー操作               内部の動き
───────                    ─────────
[○ ランクA] ← クリック     RankA.SetCheck()
                          └─ RankGroup.SetValueAsync("A")
[○ ランクB]
[○ ランクC]
```

RadioButton 自体の `Value` プロパティ（スクリプト上で参照できる）は **`bool` 型**で、
「自分が選択されているか」= 「親グループの値と自分の `Design.Value` が一致するか」を示します。

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
| **Value** | 値 | string | `""` | このラジオが選択された時に**親グループに設定される値** |
| **GroupField** | ラジオボタングループフィールド | string | `""` | 親の RadioGroupField の Name |
| **IgnoreModification** | 変更判定から除外 | bool | `false` | 変更検知から除外 |

> RadioButtonField は `DisplayName` / `DbColumn` / `IsRequired` などを持ちません。値の保持・DB 保存・必須チェックは親 [RadioGroup](RadioGroup.md) の責務です。

---

## スクリプトから

### プロパティ・メソッド

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `Value` | bool | このラジオが現在選択されているか（親の値と `Design.Value` が一致すれば true） |
| `SetCheck()` | void | このラジオを選択状態にする（親の `SetValueAsync` を呼び出す） |
| `GetRadioGroupField()` | RadioGroupField? | 親の RadioGroupField を取得 |

共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

### よく使う例

```csharp
// このラジオが選択されているかチェック
if (HighRank.Value)
{
    // 選択中の処理
}

// プログラム的に選択する
void SomeButton_OnClick()
{
    HighRank.SetCheck();
    // → 親 RankGroup に "HighRank.Design.Value" がセットされる
}

// 親グループを取得
var group = HighRank.GetRadioGroupField();
var currentValue = group?.Value;
```

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [RadioGroup](RadioGroup.md) — 値を保持する親
