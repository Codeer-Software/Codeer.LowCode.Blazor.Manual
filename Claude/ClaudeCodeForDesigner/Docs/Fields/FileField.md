# FileField - ファイルアップロード/ダウンロード

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.FileFieldDesign`

ファイルのアップロード・ダウンロード機能を提供するフィールド。ファイルメタデータを3つのDBカラムに分けて保存する。
`FieldDesignBase` を直接継承し、`IDisplayName` と `IUpdateProtected` を実装する。

## C# クラス定義 (真実の源)

```csharp
public class FileFieldDesign : FieldDesignBase, IDisplayName, IUpdateProtected
{
    public bool IsUpdateProtected { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool ShowPreview { get; set; }
    public string StorageName { get; set; } = string.Empty;
    public string DbColumnFileName { get; set; } = string.Empty;   // ファイルメタを 3 つの列に分けて保存
    public string DbColumnFileSize { get; set; } = string.Empty;
    public string DbColumnFileGuid { get; set; } = string.Empty;
    public ObjectFit ObjectFit { get; set; } = ObjectFit.Contain;  // enum: Contain / Cover / Fill / None / ScaleDown
    public string OnDataChanged { get; set; } = string.Empty;
    public string OnSearchDataChanged { get; set; } = string.Empty;
    public long? MaxAllowedSize { get; set; }                       // null=デフォルト 500MB
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}
```

## プロパティ

> 共通プロパティは [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `DisplayName` | string | `""` | 表示ラベル名。 |
| `DbColumnFileName` | string | `""` | ファイル名を保存するDBカラム名。 |
| `DbColumnFileSize` | string | `""` | ファイルサイズを保存するDBカラム名。 |
| `DbColumnFileGuid` | string | `""` | ファイルGUID（ストレージ参照キー）を保存するDBカラム名。 |
| `StorageName` | string | `""` | `designer.settings.json` で定義されたストレージ名（例: `"Local"`, `"Azure"`）。 |
| `MaxAllowedSize` | long? | `null` | アップロード可能な最大ファイルサイズ（バイト）。`null` の場合はデフォルト上限 500MB（`1024 * 1024 * 500`）。**整数のみ（`10485760.0` は不可、`10485760` と書くこと）** |
| `ShowPreview` | bool | `false` | `true` にすると、アップロードされたファイルが画像の場合にプレビュー表示する。 |
| `ObjectFit` | ObjectFit | `"Contain"` | プレビュー画像のフィットモード。CSS object-fit に対応。 |
| `IsUpdateProtected` | bool | `false` | `true` にすると、レコード作成後はファイルを変更できなくなる（読み取り専用）。 |
| `OnDataChanged` | string | `""` | ファイル変更時のスクリプトイベント名。 |
| `OnSearchDataChanged` | string | `""` | 検索値変更時のスクリプトイベント名。 |

## 列挙型

### ObjectFit

| 値 | 説明 |
|---|---|
| `None` | リサイズしない |
| `Contain` | アスペクト比を保持してコンテナ内に収める |
| `Cover` | アスペクト比を保持してコンテナを満たす |
| `Fill` | コンテナに合わせて引き伸ばす |
| `ScaleDown` | None と Contain の小さい方 |

## JSON例

```json
{
  "StorageName": "Local",
  "DbColumnFileName": "file_name",
  "DbColumnFileSize": "file_size",
  "DbColumnFileGuid": "file_guid",
  "ObjectFit": "Contain",
  "ShowPreview": false,
  "MaxAllowedSize": null,
  "IsUpdateProtected": false,
  "DisplayName": "添付ファイル",
  "OnDataChanged": "",
  "OnSearchDataChanged": "",
  "Name": "Attachment",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FileFieldDesign"
}
```

### 画像プレビュー付きの例

```json
{
  "StorageName": "Local",
  "DbColumnFileName": "photo_name",
  "DbColumnFileSize": "photo_size",
  "DbColumnFileGuid": "photo_guid",
  "ObjectFit": "Cover",
  "ShowPreview": true,
  "MaxAllowedSize": 10485760,
  "IsUpdateProtected": false,
  "DisplayName": "写真",
  "OnDataChanged": "Photo_OnDataChanged",
  "OnSearchDataChanged": "",
  "Name": "Photo",
  "IgnoreModification": false,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FileFieldDesign"
}
```

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `FileName` | string (読み取り専用) | 現在のファイル名 |
| `SearchFileName` | string? | ファイル名で検索 |
| `SearchFileSizeMin` | decimal? | 最小ファイルサイズで検索 |
| `SearchFileSizeMax` | decimal? | 最大ファイルサイズで検索 |
| `SearchFileNameComparison` | MatchComparison | ファイル名比較モード |

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `GetMemoryStream()` | MemoryStream? | ファイル内容をメモリストリームとして取得 |
| `Download()` | void | ファイルをブラウザでダウンロード |
| `SetFile(string fileName, StreamContent content)` | void | ファイルをアップロード設定 |
| `ClearFile()` | void | ファイルを削除 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [Scripts.md](../Scripts.md) を参照。

### イベントハンドラ例

```csharp
// ファイルをExcelとして読み込み・編集
void ProcessFile_OnClick()
{
    using (var memory = Attachment.GetMemoryStream())
    using (var excel = new Excel(memory, Attachment.FileName))
    {
        excel.OverWrite(this);
        excel.Download();
    }
}
```

## サーバ側設定が必須 (これがないと動かない)

`FileField` は **サーバ側に一時ファイル管理テーブルと `TemporaryFileTableInfo` 設定が必要**。デザインプロジェクトの JSON だけでは動かず、サーバの `appsettings.json` 設定と DB テーブル作成がセット。

1. **`appsettings.json` の `TemporaryFileTableInfo` 配列**:
   ```json
   "TemporaryFileTableInfo": [
     {
       "DataSourceName": "Main",
       "Table": "temporary_files",
       "GuidColumn": "guid",
       "CreatedDateTimeColumn": "created_date_time"
     }
   ]
   ```
   - `DataSourceName` は `designer.settings.json` の DataSource 名に揃える
   - DataSource ごとに 1 エントリ必要

2. **対応するテーブル作成** (各 DataSource):
   ```sql
   CREATE TABLE temporary_files (
     guid TEXT PRIMARY KEY,
     created_date_time DATETIME NOT NULL
   );
   ```
   - 列名・型は `appsettings.json` で指定した名前に合わせる

3. **`designer.settings.json` の `FileStorages`**:
   ```json
   "FileStorages": [
     { "Name": "Local", "FileStorageType": "FileSystem" }
   ]
   ```
   - 本番では `AzureBlobStorage` 等も選べる

これらが未設定だと、`FileField` のアップロード/ダウンロードがランタイム例外で失敗する (ファイル選択ダイアログ自体は出るが、サーバが受け取れない)。

サンプル/デモプロジェクトに `FileField` / `ImageField` 系を入れるときは、上の3点が揃っているか必ず確認すること。

## ランタイム動作

- ファイルアップロード時、設定されたストレージ（`StorageName`）にファイルを保存する。
- 3つのDBカラムにメタデータが記録される:
  - `DbColumnFileName`: 元のファイル名
  - `DbColumnFileSize`: ファイルサイズ（バイト）
  - `DbColumnFileGuid`: ストレージ上の参照キー（GUID）
- `DownloadAsync()` でストレージからファイルをダウンロードできる。
- `ShowPreview` が `true` の場合、画像ファイルのプレビューが表示される。`ObjectFit` でプレビュー画像のサイズ調整方式を制御する。
- `MaxAllowedSize` を超えるファイルはアップロードが拒否される。

## 検索

- ファイル名（FileName）に対して Equal / Like で検索可能。
- ファイルサイズ（FileSize）に対して範囲検索（min/max）が可能。

---

## DOM構造（CSS用）

### ファイル未選択時

```html
<input type="file" class="form-control" style="[インラインスタイル]" />
```

### ファイル選択済み（プレビューなし）

```html
<div>
  <a href="[ダウンロードURL]">ファイル名</a>
  <button class="btn btn-sm btn-outline-danger">削除</button>
</div>
<input type="file" class="form-control" style="[インラインスタイル]" />
```

### ファイル選択済み（プレビューあり: ShowPreview = true）

```html
<div>
  <img class="img-fluid" src="[プレビューURL]" style="object-fit: [ObjectFit値]" />
  <a href="[ダウンロードURL]">ファイル名</a>
  <button class="btn btn-sm btn-outline-danger">削除</button>
</div>
<input type="file" class="form-control" style="[インラインスタイル]" />
```

### 表示モード

```html
<a href="[ダウンロードURL]">ファイル名</a>
<!-- プレビューあり時 -->
<img class="img-fluid" src="[プレビューURL]" style="object-fit: [ObjectFit値]" />
```

### CSSセレクタ例

```css
/* ファイルプレビュー画像 */
[data-name="Photo"] .img-fluid {
  max-height: 200px;
  border-radius: 0.5rem;
}

/* ファイル入力のスタイル */
[data-name="Attachment"] .form-control {
  max-width: 400px;
}
```
