# RadioGroupField (ラジオボタングループ)

## これは何か

**複数のラジオボタンをまとめるコンテナ**。選択された値を保持します。実際のラジオボタンは [RadioButton](RadioButton.md) を配置するか、`ラジオボタンの選択肢設定`（PopulateRadioButtons）で自動生成します。

> **RadioGroupField 自体はレイアウトに配置しなくても動作します**。値を保持する役割だけでよい場合（画面には RadioButton だけを並べて、選択状態は RadioGroupField で管理したい場合）は、Fields に追加しておくだけで OK です。レイアウトに配置する必要があるのは、一覧（List）などで `PopulateRadioButtons` を使ってラジオボタンを自動展開する時などです。

## いつ使うか

- 3〜5 個程度の選択肢から 1 つを選ばせる
- 選択肢が常に画面に見えている方がよい場合（[Select](Select.md) はクリックで開く必要がある）
- List 内で各行のラジオ選択を並べる場合（`PopulateRadioButtons`）

---

## デザイナでの設定

<img src="../../Image/designer/fields/radiogroup/RankGroup_properties_panel.png" alt="RadioGroupFieldのプロパティパネル" style="border: 1px solid;" width="400">

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `ラジオボタングループ` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |
| **DisplayName** | 表示名 | string | `""` | 画面表示用の名前 |
| **DbColumn** | DBカラム | string | `""` | 対応する DB 列名 |
| **IsRequired** | 必須 | bool | `false` | 入力必須 |
| **IsUpdateProtected** | 更新無効 | bool | `false` | 更新時に値を変更できないようにする |
| **OnDataChanged** | データ変更イベント | string | `""` | 値変更時のスクリプトイベント |
| **IgnoreModification** | 変更判定から除外 | bool | `false` | 変更検知（IsModified）から除外 |

#### 検索設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **IsSimpleSearchParameter** | 簡易検索条件 | bool | `false` | 簡易検索の対象にする |
| **AllowOrSearch** | 直接入力または選択 | bool | `false` | 検索時の複数選択（OR）を許可 |
| **AllowEmptySearch** | 空検索を許可 | bool | `false` | 空での検索を許可する |
| **OnSearchDataChanged** | 検索モードデータ変更イベント | string | `""` | 検索条件が変更された時のスクリプトイベント |

#### LIST（一覧での表示）

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **PopulateRadioButtons** | ラジオボタンの選択肢設定 | bool | `false` | List 内で候補を自動でラジオボタンに展開 |
| **ShowRadioButtonText** | ラジオボタンのテキストを表示 | bool | `false` | 自動生成するラジオボタンにテキスト表示 |

---

## 使い方の 2 パターン

### パターン A: RadioButton を個別配置

RadioGroupField を 1 つ用意し（画面への配置は任意）、その配下に複数の [RadioButton](RadioButton.md) を配置します。
RadioButton の `GroupField` プロパティで親の RadioGroupField を指定します。

### パターン B: PopulateRadioButtons で自動生成

List の中で使う場合、`PopulateRadioButtons: true` にすると列単位でラジオボタンが自動展開されます。

---

## スクリプトから

### プロパティ

| 名前 | 型 | 説明 |
|---|---|---|
| `Value` | string? | 選択値 |
| `DisplayText` | string? | 表示テキスト |
| `DisplayTextAndValue` | IReadOnlyDictionary\<string, string\> | 候補 |
| `SearchValue` | string? | 検索値 |
| `SearchValues` | List\<string\> | 複数選択検索値 |
| `SearchIsEmpty` | bool? | 空検索 |
| `IsInverted` | bool | NOT 検索 |

共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

### よく使う例

```csharp
// 選択値を取得
var status = RankGroup.Value;

// 検索条件を設定
await RankGroup.SetSearchValueAsync("A");
```

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [RadioButton](RadioButton.md)
- [Select](Select.md) — プルダウン
