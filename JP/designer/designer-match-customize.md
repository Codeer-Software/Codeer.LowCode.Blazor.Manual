# デザイナ上の検索コンポーネントの実装

ここではデザイナ上で検索条件を設定するためのコンポーネントを実装します。
検索コンポーネントは、WPF で描画されるため、XAML を使用して実装する必要があります。

## 検索コンポーネントが持つべき機能

検索コンポーネントは、WPF で実装されたユーザーコントロールです。
このユーザーコントロールは、デザイナの内部実装からインスタンス化され DataContext として `IMatchConditionData` インターフェースを実装したクラスを受け取ります。

このインターフェースは次のメンバーを持っています。

```cs
public interface IMatchConditionData
{
    // 検索条件
    public MatchConditionBase? Condition { get; set; }
    // 検索対象のフィールド名
    public string SearchTargetField { get; set; }
    // 検索対象のプロパティ名の候補
    public IEnumerable<string> VariableCandidates { get; }
}
```

検索コンポーネントは、UI 上で検索条件を設定するためのユーザーインターフェースを提供し、操作に応じてこの `IMatchConditionData` インターフェースを通して検索条件を設定する必要があります。

## 検索コンポーネントの実装

検索コンポーネントの最小の実装例を示します。

```xml
<UserControl x:Class="SearchComponent.SearchComponent"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             mc:Ignorable="d"
             d:DesignHeight="450" d:DesignWidth="800">
    <Grid>
        <TextBox x:Name="Text" Changed="OnTextChanged" />
    </Grid>
```

```cs
class SearchComponent : UserControl
{
    public SearchComponent()
    {
        InitializeComponent();
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        // 検索条件を生成
        var condition = new FieldValueMatchCondition
        {
            SearchTargetVariable = data.SearchTargetField + ".Value",
            Comparison = MatchComparison.Equals,
        };
        condition.Value = Text.Text;
        // 検索条件を設定
        var data = (IMatchConditionData)DataContext;
        data.Condition = condition;
    }
}
```

このコードは、`TextBox` に入力された文字列を検索条件として設定する検索コンポーネントの最小の実装例です。
`FieldValueMatchCondition` は、検索対象のプロパティを特定の値と比較し条件を満たしたときにマッチする検索条件です。
他にも、`FieldVariableMatchCondition` を使用することで他のフィールドの値をもとに比較することもできます。
`FieldVariableMatchCondition` を使用した入力欄を作成する際には、`IMatchConditionData.VariableCandidates` を通して利用可能な候補の一覧が提供されるため、これを利用して入力補完を実装することができます。

## FieldDesignとの関連付け

ここまでで作成したユーザーコントロールをユーザー定義FieldDesignに関連付ける必要があります。
関連付けは、 `GetSearchControlTypeFullName` の実装で、先ほど作成したクラスの完全修飾名を返すことでできます。

```cs
public override string GetSearchControlTypeFullName() => "Your.Custom.SearchComponent.Name";
```
