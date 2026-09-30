# DateTimeOffsetField (タイムゾーン付き日時)

## これは何か

**タイムゾーンのオフセット（`+09:00` など）付きの日時を入力・表示するフィールド**。日時と、その日時がどのオフセットで入力されたかを両方保存する DB 列に対応します。

## いつ使うか

- DB の列がオフセット付きの日時型のとき（SQL Server の `datetimeoffset`、Oracle の `TIMESTAMP WITH TIME ZONE`）
- 複数のタイムゾーンから入力され、「どの地域の時刻で入力されたか」も残したいとき

オフセットを保存しない通常の日時列には [DateTime](DateTime.md) を使います。

| | DateTime | DateTimeOffset |
|---|---|---|
| DB 列 | 日時型（オフセットなし） | オフセット付き日時型 |
| 保存される値 | 日時（`SaveAsUtc` で UTC に変換して保存も可） | 日時 + オフセット |
| 表示 | 現地時刻（`SaveAsUtc` のときは UTC から変換） | **保存されたオフセットのまま**表示 |

> DB フィールドから作成すると、SQL Server の `datetimeoffset`・Oracle の `TIMESTAMP WITH TIME ZONE` の列は DateTimeOffsetField になります。PostgreSQL の `timestamp with time zone` の列は [DateTime](DateTime.md) になります。

---

## デザイナでの設定

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `タイムゾーン付き日時` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |
| **DisplayName** | 表示名 | string | `""` | 画面表示用の名前 |
| **DbColumn** | DBカラム | string | `""` | 対応する DB 列名 |
| **Format** | フォーマット | string | `""` | 閲覧表示のフォーマット（例: `yyyy/MM/dd HH:mm zzz`。`zzz` でオフセットを表示） |
| **IsRequired** | 必須 | bool | `false` | 入力必須 |
| **IsUpdateProtected** | 更新無効 | bool | `false` | 更新時に値を変更できないようにする |
| **OnDataChanged** | データ変更イベント | string | `""` | 値変更時のスクリプトイベント |
| **IgnoreModification** | 変更判定から除外 | bool | `false` | 変更検知（IsModified）から除外 |

#### 検索設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **IsSimpleSearchParameter** | 簡易検索条件 | bool | `false` | 簡易検索の対象にする |
| **AllowEmptySearch** | 空検索を許可 | bool | `false` | 空での検索を許可する |
| **OnSearchDataChanged** | 検索モードデータ変更イベント | string | `""` | 検索条件が変更された時のスクリプトイベント |

---

## 入力と表示

- **入力**: 日時ピッカーには、値をブラウザのタイムゾーンの時刻に直して表示します。入力した日時には、ブラウザのタイムゾーンのオフセットが付きます（日本のブラウザなら `+09:00`）
- **閲覧表示**: 保存されたオフセットのまま表示します（`+00:00` で保存された値は UTC の時刻で表示）。`Format` が空のときは既定の書式で表示します

---

## スクリプトから

### プロパティ

| 名前 | 型 | 説明 |
|---|---|---|
| `Value` | DateTimeOffset? | 日時の値（代入で設定） |
| `SearchMin` | DateTimeOffset? | 検索の最小日時（代入で設定） |
| `SearchMax` | DateTimeOffset? | 検索の最大日時（代入で設定） |
| `SearchIsEmpty` | bool? | 「空」を検索条件にする（代入で設定） |

共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

### よく使う例

```csharp
// 現在日時（実行環境のオフセット付き）を設定
ReceivedAt.Value = DateTimeOffset.Now;

// 過去 24 時間を検索
ReceivedAt.SearchMin = DateTimeOffset.Now.AddDays(-1);
ReceivedAt.SearchMax = DateTimeOffset.Now;
```

---

## 検索での挙動

[DateTime](DateTime.md#検索での挙動) と同じ **範囲検索** です（簡易検索は指定日時**以降**、詳細検索は開始 ～ 終了、`AllowEmptySearch` で **空** / **空以外**）。検索条件の日時は、ブラウザのタイムゾーンの時刻として入力します。

検索全体の仕組みは [SearchField](Search.md#検索の仕組み) を参照。

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [DateTime](DateTime.md) — オフセットなしの日時
- [Date](Date.md) — 日付のみ
- [Time](Time.md) — 時刻のみ
