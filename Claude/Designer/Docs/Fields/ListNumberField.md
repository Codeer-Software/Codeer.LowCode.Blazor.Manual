# ListNumberField / ListPagingField - 一覧行番号・ページング制御

## ListNumberFieldDesign

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.ListNumberFieldDesign`

一覧表示における行番号を自動表示するフィールド。ページネーションのオフセットを考慮した連番を表示する。`FieldDesignBase` を直接継承する。

## ListPagingFieldDesign

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.ListPagingFieldDesign`

一覧フィールドに対するページング制御UIを表示するフィールド。対象のリストフィールドを指定してページナビゲーションを行う。`FieldDesignBase` を直接継承する。

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

### ListNumberField のプロパティ

追加プロパティなし。FieldDesignBase の共通プロパティのみ。

### ListPagingField のプロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `ListFieldName` | string | `""` | ページングを制御する対象のリストフィールド名。ListField / DetailListField / TileListField の `Name` を指定する。 |

## JSON例

### ListNumberField - 行番号表示

```json
{
  "IgnoreModification": false,
  "Name": "RowNumber",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListNumberFieldDesign"
}
```

### ListPagingField - ページングコントロール

```json
{
  "ListFieldName": "Items",
  "IgnoreModification": false,
  "Name": "ItemsPager",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListPagingFieldDesign"
}
```

### 一覧モジュールでの組み合わせ例

```json
"Fields": [
  {
    "IgnoreModification": false,
    "Name": "RowNumber",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListNumberFieldDesign"
  },
  {
    "LayoutName": "",
    "DisplayName": "注文一覧",
    "SearchCondition": {
      "LimitCount": 20,
      "ModuleName": "Order",
      "Condition": {
        "IsOrMatch": false,
        "IsNot": false,
        "Children": [],
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
      }
    },
    "PagerPosition": "Bottom",
    "CanCreate": false,
    "CanUpdate": false,
    "CanDelete": false,
    "CanUserSort": true,
    "Name": "OrderList",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
  },
  {
    "ListFieldName": "OrderList",
    "IgnoreModification": false,
    "Name": "OrderListPager",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListPagingFieldDesign"
  }
]
```

## ランタイム動作

### ListNumberField

- 一覧の各行に自動的に連番を表示する。
- ページネーションのオフセットを考慮する。例えば、1ページ20件でページ3の場合、行番号は41, 42, 43, ... と表示される。
- 一覧レイアウト（ListLayout）の `Elements` に配置して使用する。

### ListPagingField

- `ListFieldName` で指定されたリストフィールドに対するページナビゲーションUIをレンダリングする。
- 対象フィールドは `ListFieldDesign`、`DetailListFieldDesign`、`TileListFieldDesign` のいずれかである必要がある（`IListFieldDesign` を実装する型）。
- リストフィールド本体の `PagerPosition` によるビルトインページャーとは別に、任意の位置にページングコントロールを配置できる。

### 共通

- DB列マッピングなし。検索対象外。

---

## DOM構造（CSS用）

### ListNumberField

```html
<label class="m-0 form-label" style="[インラインスタイル]">行番号</label>
```

### ListPagingField

```html
<div class="d-flex mb-3 gap-3 align-items-center justify-content-end">
  <!-- 件数表示 -->
  <div class="text-end">(1-10 /100 items)</div>

  <!-- ページャー -->
  <nav role="navigation">
    <ul class="pagination m-0">
      <li class="page-item"><button class="page-link">1</button></li>
      <li class="page-item active"><button class="page-link">2</button></li>
      <!-- ... -->
    </ul>
  </nav>
</div>
```

### CSSセレクタ例

```css
/* 行番号のスタイル */
[data-name="RowNumber"] .form-label {
  font-variant-numeric: tabular-nums;
  text-align: center;
}

/* ページャーのスタイル */
.pagination .page-link {
  min-width: 2.5rem;
  text-align: center;
}

.pagination .page-item.active .page-link {
  background-color: #0d6efd;
  border-color: #0d6efd;
}
```
