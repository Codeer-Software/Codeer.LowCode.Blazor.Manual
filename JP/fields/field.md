# Field

FieldはModuleを構成する部品です。わかりやすいものはTextFieldなどのUIを持つ部品です。これもWinFormsなどでFormクラスがTextContolをプロパティとして持つことをイメージしてもらうとわかりやすいと思います。UIに表示せずにデータの入出力だけに使うことも可能です。
- 大部分のFieldはデータを持ちます。
- ModuleをDBとマッピングしたときにカラムを割り当てて入出力することができます。
- 大部分のFieldはUIを持ちレイアウトに配置することができます。
- Webのフロントで表示する場合はレイアウトに配置されてるかもしくはDataOnlyFieldsに配置しているFieldのみサーバーからデータを取得します。
- メソッドプロパティを持ちスクリプトから操作することもできます。

## System Fields

System Fieldはシステム内で特別な役割を持つFieldです。名前で判別しているので名前を変更することはできません。Desingerでツールボックスからドロップして作成します。DBを使うアプリで一般的に使われる機能に対応しています。

| 名前 | 種別 | 内容|
|------|------|-----|
|Id|Id|Moduleのデータのキーとなるフィールドです。重複しないデータである必要があります。|
|LogicalDelete|Boolean|論理削除のフラグです。このフィールドがある場合は論理削除になります。|
|CreatedAt|DateTime|作成時間を保持するフィールドです。|
|UpdatedAt|DateTime|更新時間を保持するフィールドです。|
|Creator|Link|作成者です。認証があるときでなければ利用できません。CurrentUserModuleを設定してください。|
|Updater|Link|更新者です。認証があるときでなければ利用できません。CurrentUserModuleを設定してください。|
|OptimisticLocking|OptimisticLockingField|楽観ロック用のデータです。それぞれのDBで更新があったときに値が変わるカラムを指定してください。DBにその機能がない場合はトリガなどで代用してください。|

## Fields
- [AnchorTag](AnchorTag.md)
- [Boolean](Boolean.md)
- [Button](Button.md)
- [Date](Date.md)
- [DateTime](DateTime.md)
- [DetailList](DetailList.md)
- [File](File.md)
- [Id](Id.md)
- [ImageViewer](ImageViewer.md)
- [Label](Label.md)
- [Link](Link.md)
- [List](List.md)
- [ListNumber](ListNumber.md)
- [MarkupString](MarkupString.md)
- [Module](Module.md)
- [ModuleSelect](ModuleSelect.md)
- [Number](Number.md)
- [OptimisticLocking](OptimisticLocking.md)
- [Password](Password.md)
- [Search](Search.md)
- [Select](Select.md)
- [SubmitButton](SubmitButton.md)
- [Text](Text.md)
- [TileList](TileList.md)
- [Time](Time.md)
