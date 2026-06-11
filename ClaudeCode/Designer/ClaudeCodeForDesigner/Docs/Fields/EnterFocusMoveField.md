# EnterFocusMoveField - Enter キーでフォーカス移動

**TypeFullName:** `Codeer.LowCode.Blazor.Extras.Designs.EnterFocusMoveFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Blazor.Extras`

モジュール内で Enter キーを押すと、次の入力要素へフォーカスを移動させるユーティリティフィールド。レイアウトに 1 つ配置するだけでモジュール全体に機能が適用される。実行時の UI は持たない（非表示の span を 1 つ出力するだけ。デザインモードでは「EnterFocusMove」というグレーのプレースホルダが表示される）。

DB列へのマッピングは持たず（`CreateData()` は `null`）、検索画面用コンポーネントも持たない。

## C# クラス定義 (真実の源)

このフィールドは外部ライブラリ `Codeer.LowCode.Blazor.Extras` で定義されているため、`Codeer.LowCode.Blazor` リポジトリ内に C# 定義は存在しない。`FieldDesignBase` を直接継承するため、共通プロパティ (Name / IgnoreModification / OnValidateInput) を持つ。

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

**固有のプロパティはない。** モジュールの Fields に定義し、レイアウトに配置するだけで有効になる。

## JSON例

```json
{
  "Name": "EnterFocusMove",
  "IgnoreModification": false,
  "OnValidateInput": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Extras.Designs.EnterFocusMoveFieldDesign"
}
```

見た目を持たないため、レイアウト上の配置場所はどこでもよい。同一モジュールに 2 つ以上配置してもよいが、キーイベントが多重にバインドされるため 1 つで十分。

## ランタイム動作

JavaScript（`enterfocusmove-interop.js`）でモジュールのルート要素に `keydown` リスナーを登録し、Enter キーで次のフォーカス可能要素へ移動する。

### 動作範囲（イベント登録先）

自身の出力位置から最も近い以下の祖先要素にイベントを登録する。

1. `[data-module]` または `[data-module-design]` 要素（詳細ページ / ダイアログ / パネルのモジュールルート）
2. 上記がなければ祖先の `<form>`
3. それもなければ親要素

このため、ページ内・ダイアログ内・パネル内いずれのモジュールでも、そのモジュール内に限定してフォーカス移動が行われる。

### フォーカス移動対象

以下のセレクタに該当する要素が移動対象になる。

| 要素 | 条件 |
|---|---|
| `<input>` | `type="hidden"` / `disabled` / `readonly` を除く |
| `<select>` | `disabled` を除く |
| `<button>` | `disabled` を除く |
| `[contenteditable="true"]` | 常に対象 |
| `[tabindex]` | `tabindex="-1"` を除く |

- 非表示要素（`offsetParent === null` かつ `position` が `fixed` でない）は自動的に除外される。
- 移動順序: 正の `tabindex` を持つ要素が `tabindex` 昇順で先、その後は DOM 順。
- 最後の要素で Enter を押すと先頭の要素に戻る（ループ）。
- 移動先が `<input>` の場合は値を全選択し、上書き入力しやすくする。

### Enter キーが移動しないケース

以下の場合は Enter の既定動作を優先し、フォーカス移動を行わない。

| ケース | 理由 |
|---|---|
| IME 変換中（`isComposing`） | 変換確定の Enter と競合しないため |
| `<textarea>` にフォーカスがある | 改行入力を優先 |
| `contenteditable` 要素にフォーカスがある | 改行入力を優先 |
| 要素（または祖先）に `data-consumes-enter` 属性がある | カスタムフィールドが Enter を利用している宣言 |
| `<button type="submit">` / `<input type="submit">` | フォーム送信の既定動作を維持 |

### data-consumes-enter による除外

ProCodeField 等の独自 UI が Enter キーを候補選択・確定などに使う場合、ルート要素に `data-consumes-enter` 属性を付けると EnterFocusMoveField の処理から除外できる。

```razor
<div data-consumes-enter>
  <!-- Enter を独自処理する UI -->
</div>
```

## スクリプトAPI

このフィールド固有のスクリプト API は公開していない（`IsModified` は常に `false`、データ系メソッドはすべて非公開）。
