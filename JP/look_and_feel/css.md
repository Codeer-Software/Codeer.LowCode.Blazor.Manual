# CSS

見た目は通常の Blazor アプリと同様に CSS で変更できます。

## どこに書くか

アプリ全体のスタイルは **`wwwroot/css/app.css`** に書きます。Bootstrap・このフレームワーク本体・テンプレートが用意した既定スタイルの**上から**当たるので、ここに書いた CSS が最終的に勝ちます。

- 個別の見栄えを上書きしたい → `app.css` に普通の CSS / Bootstrap セレクタで上書き
- 特定の Module / Field だけスタイルを変えたい → 後述の `data-` 属性セレクタや `ClassName` プロパティ
- アプリ全体の余白・サイドバー色などの**既定値**を変えたい → 後述の [カスタマイズ可能な CSS 変数](#カスタマイズ可能な-css-変数) を `:root` で上書き

## サンプル

- [Material Design](material_design.md) — Bootstrap テーマを差し替える例
- [Fluent Design](fluent_design.md) — 同上
- [Custom Styles](custom_styles.md) — 自前で書くスタイル例

実物を見るには[デモサイト](https://lowcodedemo.azurewebsites.net) のサイドバー「CSS」項目が参考になります。

## 特定のモジュール・フィールドに対するCSSセレクタの利用

モジュールや、フィールドはそれらを識別するためにdata属性を公開しています。
これらの属性を利用することで、特定のモジュールやフィールドに対してCSSを適用することができます。

例えば、MyModule というモジュールに対してCSSを適用したい場合、以下のように記述します。

```css
[data-module="MyModule"] {
  background-color: red;
}
```

この場合、MyModule というモジュールの場合のみ背景色が赤くなります。

また、フィールドに対しても同様に、data属性を利用することができます。
例えば、MyField というフィールドに対してCSSを適用したい場合、以下のように記述します。

```css
[data-name="MyField"] {
  background-color: blue;
}
```

この場合、MyField というフィールドの場合のみ背景色が青くなります。

## コンポーネントのClassNameプロパティの利用

Field・Layout・Module は共通の `ClassName` プロパティを持ち、ここに任意の CSS クラス名を設定できます。

Field の場合、設定したクラスは `div.field-layout` に追加されるので、そのクラスを起点に Field 全体のスタイルを変えられます。

例えば、Field の `ClassName` に `my-class` を指定すると DOM は次のようになります（中身は Field 種別によって異なります）:

```html
<div class="field-layout my-class">
  <!-- ここに実際のフィールドが生成されます -->
</div>
```

`my-class` に対して以下のような CSS を書くと背景色が赤になります。

```css
.my-class {
  background-color: red;
}
```

`ClassName` はスクリプトからも設定でき、ユーザー操作に応じてスタイルを動的に切り替えられます:

```cs
void OnClick()
{
  myField.ClassName = "my-class";
}
```

Layout（Grid / Canvas / Tab）や Module の `ClassName` も同様に、それぞれのレイアウト・モジュールを表す要素に追加されます。設定したクラスを起点に CSS を書くことで、Field と同じ要領でスタイルを適用できます。実際にどの要素にクラスが付くかは、ブラウザの開発者ツールで確認してください。

## カスタマイズ可能な CSS 変数

レイアウトの余白などはあらかじめ CSS 変数で定義されているので、`app.css` で値を上書きすればアプリ全体に反映できます。
個別の Grid・Row・Column・Tab の `Padding` / `Margin` プロパティを使えば**そのレイアウト単体**で上書きでき、CSS 変数を使えば**アプリ全体の既定値**を変更できます。

### レイアウト（Grid / Row / Column / Tab）の余白

| 変数 | 既定値 | 用途 |
|---|---|---|
| `--default-card-padding-top` | `1rem` | Grid `IsBordered` オン時のカード内側 上 |
| `--default-card-padding-right` | `0.625rem` | 同 右 |
| `--default-card-padding-bottom` | `1rem` | 同 下 |
| `--default-card-padding-left` | `0.625rem` | 同 左 |
| `--default-row-margin-top` | `1rem` | Row の上マージン |
| `--default-row-margin-bottom` | `1rem` | Row の下マージン |
| `--default-column-padding-left` | `0.375rem` | Column の左パディング |
| `--default-column-padding-right` | `0.375rem` | Column の右パディング |
| `--default-column-border-padding-top` | `0.5rem` | `BorderStyle` 指定ありの Column の上パディング |
| `--default-column-border-padding-bottom` | `0.5rem` | 同 下 |
| `--default-tab-padding-top` | `0.625rem` | Tab パネル内側 上 |
| `--default-tab-padding-right` | `1rem` | 同 右 |
| `--default-tab-padding-bottom` | `0.625rem` | 同 下 |
| `--default-tab-padding-left` | `1rem` | 同 左 |

`app.css` で `:root` または `body` に書けばアプリ全体に効きます。

```css
:root {
  --default-row-margin-top: 1.5rem;
  --default-row-margin-bottom: 1.5rem;
  --default-column-padding-left: 0.5rem;
  --default-column-padding-right: 0.5rem;
}
```

### サイドバー背景グラデーション

| 変数 | 既定値 | 用途 |
|---|---|---|
| `--background-start` | `rgb(5, 39, 103)` | サイドバー背景グラデーション 上 |
| `--background-end` | `#3a0647` | サイドバー背景グラデーション 下 |

```css
:root {
  --background-start: #1a1a2e;
  --background-end: #16213e;
}
```

### Bootstrap の CSS 変数

このフレームワークは Bootstrap をベースに描画しています。色や枠線の基本トーンは Bootstrap の CSS 変数（`--bs-body-bg` / `--bs-body-color` / `--bs-border-color` / `--bs-primary` 等）でも調整できます。詳細は [Bootstrap 公式ドキュメント](https://getbootstrap.com/docs/5.3/customize/css-variables/) を参照してください。
