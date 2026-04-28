# css

見た目は通常のBlazorアプリと同様にcssで変更できます。ご参考までにサンプルを用意いたしました。

- [Material Design](material_design.md)
- [Fluent Design](fluent_design.md)
- [Custom Styles](custom_styles.md)

こちらのサイトでサイドバーのcssの項目で確認することができます。

https://lowcodedemo.azurewebsites.net

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

各コンポーネントは共通のClassNameプロパティを持っています。
このプロパティを設定することで、各コンポーネントへCSSクラスを追加することができます。

このCSSクラスは `div.field-layout` に設定されるので設定した任意のクラスを起点にフィールドから生成されるDOM要素に対して
任意のスタイルを適用することができます。

例えば、以下のように設定した場合、`div.field-layout` に `my-class` が追加されます。

![image](https://github.com/user-attachments/assets/8a48231c-8544-409c-bf88-788f2e21d810)

この時のDOMは以下のようになります。実際のフィールドがどのように生成されるかはフィールドによって異なるのでブラウザの実行結果から確認してください。

```html
<div class="field-layout my-class">
  <!-- ここに実際のフィールドが生成されます -->
</div>
```

たとえば、`my-class` に対して以下のようなCSSを設定した場合、フィールドの背景色が赤くなります。

```css
.my-class {
  background-color: red;
}
```

ClassNameプロパティはスクリプトから設定することもできます。このフィールドが `myField` という名前の場合、以下のように設定できます。

```cs
void OnClick()
{
  myField.ClassName = "my-class";
}
```

スクリプトで設定することで動的にクラスを変更することができるので、ユーザの操作に応じてフィールドのスタイルを変更することができます。

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
