# ProCodeField - カスタムBlazorコンポーネント埋め込み

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.ProCodeFieldDesign`

カスタム開発した Blazor コンポーネント（`ProCodeComponentBase` 継承）をローコードフォームに埋め込むフィールド。ローコードとプロコードを組み合わせたハイブリッド開発を実現する。`FieldDesignBase` を直接継承する。

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `ProCodeComponent` | string | `""` | カスタムコンポーネントクラスの完全修飾型名（例: `"MyApp.Components.CustomChart"`）。 |
| `OnComponentSet` | string | `""` | コンポーネント初期化完了時に呼ばれるスクリプトイベント名。`.mod.cs` にメソッドを定義する。 |

## JSON例

### カスタムチャートコンポーネントの埋め込み

```json
{
  "ProCodeComponent": "MyApp.Components.SalesChart",
  "OnComponentSet": "SalesChart_OnComponentSet",
  "IgnoreModification": false,
  "Name": "SalesChart",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ProCodeFieldDesign"
}
```

### カスタムマップコンポーネントの埋め込み

```json
{
  "ProCodeComponent": "MyApp.Components.MapViewer",
  "OnComponentSet": "",
  "IgnoreModification": false,
  "Name": "Map",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ProCodeFieldDesign"
}
```

### スクリプト例（.mod.cs）

```csharp
void SalesChart_OnComponentSet()
{
    // コンポーネント初期化後の処理
    // カスタムコンポーネントにデータを渡すなど
}
```

## ランタイム動作

- Blazor の `DynamicComponent` を使用して、`ProCodeComponent` で指定されたカスタムコンポーネントをレンダリングする。
- カスタムコンポーネントは `ProCodeComponentBase` を継承している必要がある。
- コンポーネントの初期化が完了すると `OnComponentSet` イベントが発火する。
- カスタムコンポーネントはアプリケーションのアセンブリにコンパイル済みである必要がある（ローコード定義のみでは使用できない）。
- DB列マッピングなし。検索対象外。

---

## FillAvailable（最終行フィットモード）での使用

ProCodeField を `IsFillAvailable: true` の GridLayout の最終行に配置する場合、カスタムコンポーネント側でスクロール制御を実装する必要がある。

### 背景

フレームワーク組み込みの ListField / DetailListField / TileListField は、コンポーネント内部にスクロール制御ラッパーを持っており、FillAvailable 行に置かれたときに自動的にビューポート内でスクロールする。

ProCodeField はユーザーが自由に実装するコンポーネントを埋め込む器であるため、フレームワーク側でスクロール制御を強制しない。FillAvailable 行で正しく動作させるには、カスタムコンポーネントのルート要素に以下の CSS を適用する。

### 実装方法

カスタムコンポーネントのルート要素（または適切なラッパー div）に以下のスタイルを設定する:

```css
.my-wrapper {
    overflow: auto;
    height: inherit;
    width: 0;
    min-width: 100%;
}
```

```razor
<div class="my-wrapper">
  @* コンテンツ（テーブル、リスト等） *@
</div>
```

### 各プロパティの役割

| プロパティ | 役割 |
|---|---|
| `overflow: auto` | コンテンツが高さ・幅を超えたときにラッパー内でスクロールする |
| `height: inherit` | 親要素（field-layout）の高さをチェーンで受け取る。FillAvailable 行では固定 px 値になり、通常行では auto（制約なし）に解決される |
| `width: 0` | CSS の min-content サイジング時にこの要素の幅寄与を 0 にする。これがないとテーブル等の広いコンテンツの intrinsic width が親要素に伝播し、ページに横スクロールバーが出る（その横スクロールバーの高さ分だけ縦の表示領域が減り、縦スクロールも誘発される） |
| `min-width: 100%` | 通常のレイアウトでは親要素の幅いっぱいに広がる。min-content サイジング時は percentage が 0 扱いになるため、width: 0 の効果を打ち消さない |

### なぜこの CSS が必要か

ProCodeField の field-layout は `display: block` であり、block の子要素は flex/grid item ではない。CSS 仕様上、`overflow: auto` が min-width を 0 にする効果は **flex item と grid item にのみ** 適用される。block の子では、コンテンツの intrinsic width がそのまま親に伝播する。

`width: 0; min-width: 100%` のトリックにより、min-content サイジング時の幅寄与を 0 にしつつ、通常レイアウトでは親幅に合わせることで、この伝播を断ち切る。

### 注意事項

- `height: 100%` ではなく **`height: inherit`** を使うこと。CSS の仕様上、親が `height: auto` の場合に `100%` は解決されないが、`inherit` は親の指定値をそのままチェーンする
- `VerticalAlignment: "Stretch"` の設定は不要。`width: 0; min-width: 100%` で横幅の伝播を防いでいるため、field-layout が `display: block` のままでも正しく動作する
- FillAvailable を使わない場合は、この CSS があっても害はない（height: inherit が auto に解決されるだけ）

---

## DOM構造（CSS用）

### カスタムコンポーネント

```html
<!-- DynamicComponent により、登録されたカスタム Blazor コンポーネントが描画される -->
<!-- DOM構造はカスタムコンポーネントの実装による -->
```

**注意:** ProCodeField の DOM 構造はカスタムコンポーネントの実装に依存する。app.css でスタイルを適用する場合は `data-name` 属性でフィールドを特定する。

### CSSセレクタ例

```css
/* ProCode フィールドのラッパー */
[data-name="CustomChart"].field-layout {
  min-height: 300px;
}
```
