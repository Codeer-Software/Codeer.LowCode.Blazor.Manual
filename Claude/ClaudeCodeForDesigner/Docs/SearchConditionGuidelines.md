# SearchCondition ガイドライン

SearchCondition の `LimitCount` 設定に関する規約。

---

## LimitCount のデフォルト値

| 利用箇所 | LimitCount | 説明 |
|---|---|---|
| 詳細画面の子リスト（DetailListField / ListField） | `null` | 件数制限なし。親に紐づく子レコードは全件表示する |
| PageFrame のリスト表示（ListPageDesign.ListFieldDesign） | `50` | 一覧ページでは50件ずつページング |
| LinkField / SelectField の候補検索 | `50` | 候補選択ダイアログでは50件ずつ |

### 理由

- 詳細画面の明細リストは親に紐づく全件を表示する必要があるため、件数制限をかけない（`null`）
- `0` を指定すると0件になるため **使わないこと**
- 一覧ページは大量データ対策としてページングを入れる（`50`）

### JSON例

```json
// ✅ 詳細画面の DetailListField — null で全件
"SearchCondition": {
    "LimitCount": null,
    "ModuleName": "ExpenseItem",
    ...
}

// ✅ PageFrame の ListFieldDesign — 50件ずつ
"ListFieldDesign": {
    "SearchCondition": {
        "LimitCount": 50,
        "ModuleName": ""
    },
    ...
}

// ❌ 0 は使わない
"SearchCondition": {
    "LimitCount": 0,
    ...
}
```
