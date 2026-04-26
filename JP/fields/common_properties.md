# Field 共通プロパティ

すべて、または多くの Field に共通するプロパティをここにまとめます。
各 Field の個別ページでは、**固有プロパティだけ**を記載しています。共通部分を確認したいときはこのページを参照してください。

---

## デザイナでの共通プロパティ

### すべての Field に共通

| プロパティ | 型 | 既定値 | 説明 |
|---|---|---|---|
| **Name** | string | `""` | フィールド識別子。スクリプトや DB 列の参照で使う |
| **IgnoreModification** | bool | `false` | 変更検知（IsModified）から除外する |
| **OnValidateInput** | string | `""` | 入力検証スクリプト。Submit 時に呼ばれ、`bool` を返す（[入力検証](#入力検証-onvalidateinput) 参照） |

### 値を持つ Field（ValueField 系）に共通

| プロパティ | 型 | 既定値 | 説明 |
|---|---|---|---|
| **DisplayName** | string | `""` | 画面表示用の名前（ラベル・ヘッダー）。空なら Name が使われる |
| **IsRequired** | bool | `false` | 入力必須にする |
| **OnDataChanged** | string | `""` | 値が変更された時のスクリプトイベント |

### DB 連動 Field（DbValueField 系）に共通

| プロパティ | 型 | 既定値 | 説明 |
|---|---|---|---|
| **DbColumn** | string | `""` | 対応する DB 列名 |
| **IsUpdateProtected** | bool | `false` | 更新時に値を変更できないようにする |

### 検索画面での共通プロパティ（Search Settings カテゴリ）

| プロパティ | 型 | 既定値 | 説明 |
|---|---|---|---|
| **IsSimpleSearchParameter** | bool | `false` | 簡易検索の対象にする |
| **AllowEmptySearch** | bool | `false` | 空での検索を許可する |
| **OnSearchDataChanged** | string | `""` | 検索条件が変更された時のスクリプトイベント |

---

## スクリプトでの共通プロパティ・メソッド

### すべての Field で使える

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `ClassName` | string? | CSS クラス名を取得・設定 |
| `IsValid` | bool | バリデーション結果 |
| `ErrorText` | string | エラーメッセージ |
| `SetError(string)` | void | エラーを設定 |
| `ClearError()` | void | エラーをクリア |
| `Color` | string? | 文字色 |
| `BackgroundColor` | string? | 背景色 |
| `FontFamily` | string? | フォント名 |
| `FontSize` | string? | フォントサイズ |
| `IsEnabled` | bool | 有効／無効 |
| `IsVisible` | bool | 表示／非表示 |
| `IsViewOnly` | bool | 編集可／読み取り専用 |
| `IsModified` | bool | 変更されたか |
| `IgnoreModification` | bool | 変更検知から除外 |
| `Focus()` | Task | フォーカスを当てる |
| `GetClientRect()` | Task<Rect> | 画面上の矩形を取得 |
| `NotifyStateChanged()` | void | 再描画を促す |

### 値を持つ Field で使える

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `Value` | T? | 値を取得 |
| `SetValueAsync(T?)` | Task | 値を設定（`Value = ...` と書いても同じ） |

### 範囲検索 Field (Number / Date / DateTime / Time) で使える

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `SearchMin` | T? | 検索の最小値 |
| `SearchMax` | T? | 検索の最大値 |
| `SearchIsEmpty` | bool? | 「空」を検索条件にするか |
| `SetSearchMinAsync(T?)` | Task | 最小値を設定 |
| `SetSearchMaxAsync(T?)` | Task | 最大値を設定 |
| `SetSearchIsEmptyAsync(bool?)` | Task | 空検索フラグを設定 |

### 単一値の検索 Field (Text / Boolean / Select / Id など) で使える

| 名前 | 型・戻り値 | 説明 |
|---|---|---|
| `SearchValue` | T? | 検索値 |
| `SearchComparison` | MatchComparison | 比較方法（Equal, Like など） |
| `SearchIsEmpty` | bool? | 「空」を検索条件にするか |
| `SetSearchValueAsync(T?)` | Task | 検索値を設定 |
| `SetSearchComparisonAsync(MatchComparison)` | Task | 比較方法を設定 |
| `SetSearchIsEmptyAsync(bool?)` | Task | 空検索フラグを設定 |

---

## MatchComparison 列挙体

検索条件の比較方法。`SearchComparison` プロパティで使います。

| 値 | 意味 |
|---|---|
| `Equal` | 一致 |
| `NotEqual` | 不一致 |
| `LessThan` | 未満 |
| `LessThanOrEqual` | 以下 |
| `GreaterThan` | より大きい |
| `GreaterThanOrEqual` | 以上 |
| `Like` | あいまい検索 |
| `Exists` | 存在する |
| `NotExists` | 存在しない |

---

## 入力検証 (OnValidateInput)

各 Field は **Submit 時に検証** が走り、不正な値があると Submit が中断されます。

### 検証が走るタイミング

| 契機 | 説明 |
|---|---|
| **SubmitButton クリック** | `Module.ValidateInput()` が走り、配下の全 Field を検証。失敗で Submit 中断 |
| **AutoSubmit による自動保存** | 上と同じ。検証失敗でエラー表示し保存しない |
| **`普通のButton` + スクリプトで `Module.Submit()` 呼ぶ** | スクリプトでの呼び出しは検証を走らせない（ユーザー責任）。必要なら自分で `Module.ValidateInput()` を呼ぶ |

### 組込検査と OnValidateInput の流れ

検証は次の順で走ります:

1. **組込検査**（`IsRequired` / `MaxLength` / Number の `Min`/`Max` 等）が失敗したら `SetError` + 中断
2. 全部通れば **`OnValidateInput` スクリプト** が呼ばれる（Field 共通プロパティ）
3. スクリプトが `false` を返したら、その時点のエラー状態を保持して中断
4. スクリプトが `true` を返したら（または未指定なら）、`ClearError` してパス

### OnValidateInput スクリプト

戻り値 `bool` のスクリプトを書きます。`false` を返すと Submit が中断されます。エラー文を出したい場合は `SetError` を併用します。

```csharp
bool MyField_OnValidateInput()
{
    if (StartDate.Value > EndDate.Value)
    {
        EndDate.SetError("開始日より後の日付を入力してください");
        return false;
    }
    return true;
}
```

### リアルタイム検証 (`OnDataChanged`) との使い分け

| 用途 | 推奨フック |
|---|---|
| 値変更時の即時フィードバック（編集中の警告表示） | `OnDataChanged` で `SetError` |
| Submit 時の最終検証（Submit を止める判断） | `OnValidateInput` |

両方で同じ検証ロジックを走らせたい場合は、**ユーザー定義の関数を 1 つ作って両方から呼ぶ**のが定型です。

```csharp
bool DoCrossFieldCheck()
{
    if (StartDate.Value > EndDate.Value)
    {
        EndDate.SetError("開始日より後に");
        return false;
    }
    EndDate.ClearError();
    return true;
}

void StartDate_OnDataChanged() { DoCrossFieldCheck(); }
void EndDate_OnDataChanged() { DoCrossFieldCheck(); }
bool MyField_OnValidateInput() { return DoCrossFieldCheck(); }
```

### Submit 時の `SetError` 消失について

`OnDataChanged` の中で `SetError` した内容は、Submit が走った瞬間に **`ValidateInput` の組込検査が通った場合** にクリアされます。Submit 時にもエラーを保持したい場合は、`OnValidateInput` スクリプトで再判定してください。

`普通のButton` + スクリプトの `Module.Submit()` ルートを取れば `ValidateInput` は走らないので、ユーザーの SetError はそのまま残せます。

---

## 関連項目

- [Field 一覧](field.md)
- [スクリプト概要](../script/script.md)
- [Module](../module/module.md)
