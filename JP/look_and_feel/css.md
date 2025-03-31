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
