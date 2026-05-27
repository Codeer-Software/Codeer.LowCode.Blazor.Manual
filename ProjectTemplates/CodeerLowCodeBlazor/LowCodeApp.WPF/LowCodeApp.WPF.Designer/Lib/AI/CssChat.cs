using System.ClientModel;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.Json;
using OpenAI.Chat;

namespace LowCodeApp.WPF.Designer.Lib.AI
{
    public class CssChat : IAIChat
    {
        readonly ICssEditor _editor;
        readonly ChatClient _chatClient;
        readonly DesignerEnvironment _designerEnvironment;
        readonly List<ChatMessage> _conversationHistory = new();

        public string Explanation => "cssを編集するためのチャットです";

        public CssChat(DesignerEnvironment designerEnvironment, AISettings settings, ICssEditor editor)
        {
            _designerEnvironment = designerEnvironment;
            _editor = editor;
            var azureClient = new AzureOpenAIClient(
                new Uri(settings.OpenAIEndPoint),
                new ApiKeyCredential(settings.OpenAIKey));
            _chatClient = azureClient.GetChatClient(settings.ChatModel);
        }

        public void Clear() => _conversationHistory.Clear();

        public async Task<string> ProcessMessage(string message)
        {
            var currentCss = _editor.GetCss();
            var prompt = BuildPrompt(message, currentCss);

            var result = await _chatClient.CompleteChatAsync(prompt,
                new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() });
            var resultText = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

            try
            {
                var response = JsonConverterEx.DeserializeObject<AIResponse>(resultText)!;
                _editor.Update(response.Css);

                _conversationHistory.Add(new UserChatMessage(message));
                _conversationHistory.Add(new AssistantChatMessage(resultText));
                TrimConversationHistory();

                return string.IsNullOrEmpty(response.Explanation)
                    ? "CSSを変更しました"
                    : response.Explanation;
            }
            catch (Exception ex)
            {
                return $"エラーリトライしてください\r\n{ex.Message}";
            }
        }

        List<ChatMessage> BuildPrompt(string message, string currentCss)
        {
            var prompt = new List<ChatMessage>();

            prompt.Add(new SystemChatMessage(SystemPrompt));
            prompt.Add(new SystemChatMessage(BuildDesignContextInfo()));
            prompt.Add(new SystemChatMessage(
                $"現在のCSS:\n```css\n{currentCss}\n```"));

            prompt.AddRange(_conversationHistory);
            prompt.Add(new UserChatMessage(message));

            return prompt;
        }

        string BuildDesignContextInfo()
        {
            var lines = new List<string> { "## 現在のアプリケーション情報" };

            try
            {
                var designData = _designerEnvironment.GetDesignData();

                var moduleNames = designData.Modules.GetModuleNames();
                if (moduleNames.Any())
                {
                    lines.Add("\n### モジュール一覧（data-module / list-module セレクタで使用可能）");
                    foreach (var name in moduleNames)
                    {
                        var mod = designData.Modules.Find(name);
                        if (mod == null) continue;

                        var fieldNames = mod.Fields.Select(f => f.Name).ToList();
                        var classNames = new List<string>();

                        foreach (var kvp in mod.DetailLayouts)
                        {
                            if (!string.IsNullOrEmpty(kvp.Value.ClassName))
                                classNames.Add($"DetailLayout(\"{kvp.Key}\"): {kvp.Value.ClassName}");

                            CollectClassNames(kvp.Value.Layout, classNames);
                        }

                        lines.Add($"- {name}");
                        if (fieldNames.Any())
                            lines.Add($"  フィールド: {string.Join(", ", fieldNames)}");
                        if (classNames.Any())
                            lines.Add($"  ClassName: {string.Join(", ", classNames)}");
                    }
                }

                var pageFrameNames = designData.PageFrames.GetPageFrameNames();
                if (pageFrameNames.Any())
                {
                    lines.Add("\n### ページフレーム一覧（data-pageframe セレクタで使用可能）");
                    foreach (var name in pageFrameNames)
                    {
                        lines.Add($"- {name}");
                    }
                }
            }
            catch
            {
                lines.Add("（デザインデータの取得に失敗しました）");
            }

            return string.Join("\n", lines);
        }

        static void CollectClassNames(Codeer.LowCode.Blazor.Repository.Design.LayoutDesignBase? layout, List<string> classNames)
        {
            if (layout == null) return;

            if (layout is Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign field)
            {
                if (!string.IsNullOrEmpty(field.ClassName))
                    classNames.Add(field.ClassName);
            }
            else if (layout is Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign grid)
            {
                foreach (var row in grid.Rows)
                    foreach (var col in row.Columns)
                        CollectClassNames(col.Layout, classNames);
            }
            else if (layout is Codeer.LowCode.Blazor.Repository.Design.TabLayoutDesign tab)
            {
                foreach (var tabLayout in tab.Layouts)
                    CollectClassNames(tabLayout, classNames);
            }
        }

        void TrimConversationHistory()
        {
            const int maxMessages = 20;
            if (_conversationHistory.Count > maxMessages)
            {
                _conversationHistory.RemoveRange(0, _conversationHistory.Count - maxMessages);
            }
        }

        private class AIResponse
        {
            public string Css { get; set; } = string.Empty;
            public string Explanation { get; set; } = string.Empty;
        }

        const string SystemPrompt = @"
あなたはローコードWebアプリケーションのCSSエディタです。
ユーザーの指示に基づいてapp.cssを編集し、結果をJSONで返してください。

## 基本ルール
- 元のCSSが渡されるので、ユーザーの指示に対して**必要最小限の変更**にしてください。
- 既存のCSSルールは指示がない限り変更・削除しないでください。
- 新しいルールは既存CSSの末尾に追加してください。
- コメントがある場合はそのまま保持してください。

## 出力JSON形式

以下の形式でJSONを返してください:
{
  ""Css"": ""/* 変更後のCSS全体 */"",
  ""Explanation"": ""変更内容の説明""
}

- Css: 変更後のCSS全体を文字列として返す。改行は \n で表現する。
- Explanation: 何を変更したかの簡潔な日本語説明。

## アプリケーションのDOM構造

### ページ全体構造
```html
<div class=""page"" data-pageframe=""ページフレーム名"">
  <header class=""topbar"" data-system=""topbar""><!-- トップバー --></header>
  <header class=""topbar mobile d-block d-sm-none"" data-system=""mobile_topbar""><!-- モバイルトップバー --></header>
  <div class=""content"">
    <div class=""sidebar"" data-system=""sidebar"" data-system-placement=""left""><!-- 左サイドバー --></div>
    <main>
      <header><!-- user-header --></header>
      <article><!-- ページコンテンツ --></article>
      <footer><!-- user-footer --></footer>
    </main>
    <div class=""sidebar"" data-system=""sidebar"" data-system-placement=""right""><!-- 右サイドバー --></div>
  </div>
</div>
```

### 詳細ページ構造
```html
<div class=""lowcode-page"" data-module=""モジュール名""><!-- ModuleRenderer --></div>
```

### 一覧ページ構造
```html
<div list-module=""モジュール名"">
  <div data-system=""search-field""><!-- 検索フォーム --></div>
  <div data-system=""list-field""><!-- リスト表示 --></div>
</div>
```

### グリッドレイアウト構造
```html
<div data-name=""レイアウト名"" data-layout=""grid"">
  <div class=""grid-row"">
    <div class=""grid-column"" data-border=""..."" data-ha=""..."" data-va=""..."">
      <div class=""field-layout [ClassName]"" data-name=""フィールド名"">
        <!-- フィールドコンポーネント -->
      </div>
    </div>
  </div>
</div>
```

### タブレイアウト構造
```html
<div class=""tab-host"" data-name=""タブ名"" data-layout=""tab"">
  <ul class=""nav nav-tabs"" data-system=""tab"">
    <li class=""nav-item""><a class=""nav-link"">タブ名</a></li>
  </ul>
  <div class=""tab-bordered""><!-- タブ内容 --></div>
</div>
```

### フィールドラッパー構造
```html
<div class=""field-layout [ClassName]"" data-name=""フィールド名"">
  <!-- フィールドコンポーネントの内容 -->
</div>
```

## 使用可能なCSSセレクタ

### data属性セレクタ
- [data-module=""モジュール名""] - 特定モジュールの詳細画面
- [list-module=""モジュール名""] - 特定モジュールの一覧画面
- [data-pageframe=""ページフレーム名""] - 特定ページフレーム
- [data-module-design=""モジュール名""] - ダイアログ/パネル内モジュール
- [data-name=""フィールド名""] - 特定フィールド
- [data-layout=""grid""] - グリッドレイアウト
- [data-layout=""tab""] - タブレイアウト
- [data-system=""topbar""] - トップバー
- [data-system=""sidebar""] - サイドバー
- [data-system=""sidebar""][data-system-placement=""left""] - 左サイドバー
- [data-system=""sidebar""][data-system-placement=""right""] - 右サイドバー
- [data-system=""sidebar-brand""] - サイドバーブランド
- [data-system=""search-field""] - 検索フォーム領域
- [data-system=""list-field""] - リスト表示領域
- [data-system=""create""] - 新規作成ボタン
- [data-system=""delete""] - 削除ボタン
- [data-system=""tab""] - タブナビゲーション
- [data-system=""search""] - 検索ボタン
- [data-system=""clear""] - クリアボタン
- [data-system=""expander""] - 折りたたみ/展開ボタン
- [data-system=""module-dialog""] - モジュールダイアログ
- [data-system=""message-box""] - メッセージボックス
- [data-system=""logout""] - ログアウトリンク
- [data-title=""メニュータイトル""] - メニュー項目

### レイアウトCSSクラス
- .grid-row - グリッド行
- .grid-column - グリッド列
- .field-layout - フィールドラッパー
- .tab-host - タブコンテナ
- .nav-tabs - タブナビゲーション
- .tile-container - タイルコンテナ
- .tile - 各タイル
- .canvas-root - キャンバスルート
- .flow - フローレイアウト
- .flow-column - フロー要素

### 上書き可能なCSS変数（:where()で詳細度低く定義済み）
- .grid-row: --default-row-margin-top, --default-row-margin-bottom
- .grid-column: --default-column-padding-left, --default-column-padding-right
- .grid-bordered: --default-card-padding-left, --default-card-padding-right, --default-card-padding-top, --default-card-padding-bottom
- .tab-host > div: --default-tab-padding-left, --default-tab-padding-right, --default-tab-padding-top, --default-tab-padding-bottom

### ページレベルCSS変数
- --background-start, --background-end - サイドバーグラデーション
- --sidebar-item-depth - サイドバーネスト階層

### フォント・色のスタイル出力
フィールドのフォント・色はCSS変数ではなく、直接CSSプロパティ(font-family, font-size, color等)がインラインstyleに出力されます。

### Bootstrapクラス（カスタマイズ可能）
- .form-control - 入力フォーム
- .form-select - セレクトボックス
- .btn, .btn-primary, .btn-outline-primary 等 - ボタン
- .table, .table-light, .table-striped - テーブル
- .card, .card-body - カード
- .navbar, .nav-link, .nav-item - ナビゲーション
- .modal-dialog - モーダルダイアログ
- .is-invalid, .invalid-feedback - バリデーション

### テーブル一覧のセレクタ
- table tr.can-select:hover - ホバー時の行
- table tr.can-select.selected - 選択中の行
- thead.table-light th - テーブルヘッダー

### DetailList / TileList の選択状態セレクタ
ListField はテーブル形式だが、DetailListField は各行を div で、TileListField はタイル要素として描画する。
それぞれ専用のセレクタを使う必要がある。
- div.can-select:hover - DetailList のホバー行
- div.can-select.selected - DetailList の選択中の行（背景色を直接指定可能）
- .tile - TileList の各タイル
- .tile.selected - TileList の選択中タイル

### .card 背景色のデフォルト透過
IsBordered: true の Grid は Bootstrap の .card クラスで描画されるが、
**フレームワーク側で `div.card { background-color: transparent }` をデフォルトにしている**。
理由:
- DetailList の `div.can-select.selected` の青背景がカード越しに見える
- app.css で article 等に background-image を設定したときカードが手前に出ない
カードに背景色を明示したい場合は、デザイン側の GridLayoutDesign.BackgroundColor を設定するか、
app.css で `[data-module=""...""] div.card { background-color: ... }` のように上書きする。

### 入力フィールド (input/select/.input-group-text) のフォント・色は inherit 済み
Bootstrap の .form-control / .form-select / .input-group-text は font-size, font-weight, font-style, color を
固定値で上書きするが、フレームワーク側の各フィールドのスコープ CSS で
親要素 (field-layout 等) からの inherit を強制している。
- 親側 (field-layout, grid-column, .lowcode-page 等) で font/color を設定すれば、入力欄にも反映される
- :focus 状態でも .form-control:focus が color を上書きしないように強制 inherit 済み
- app.css でフィールドのフォント・色を変更したいときは親セレクタに対して font-* / color を設定するだけで OK

## 最重要: Bootstrapテーブルのスタイリングルール

このアプリケーションはBootstrap 5を使用しています。
Bootstrapのテーブルはbackground-colorを直接指定しても効きません。
**必ずBootstrapのCSS変数を使用してください。**

### 行の背景色を変更する場合
```css
/* 正しい方法: CSS変数を使用 */
.table > tbody > tr:nth-of-type(even) > * {
  --bs-table-bg-type: #f0f4f8;
}

/* 間違い: background-colorは効かない */
table tbody tr:nth-child(even) {
  background-color: #f0f4f8;  /* ← これはBootstrapテーブルでは効かない */
}
```

### Bootstrapテーブルで使用可能なCSS変数
- --bs-table-bg-type: 行の背景色の上書き（ストライプ等に使用）
- --bs-table-bg: テーブル全体の背景色
- --bs-table-bg-state: ホバーや選択状態の背景色
- --bs-table-striped-bg: ストライプ行の背景色
- --bs-table-hover-bg: ホバー時の背景色
- --bs-table-active-bg: アクティブ行の背景色
- --bs-table-color: テーブルのテキスト色
- --bs-table-border-color: テーブルのボーダー色

### テーブルスタイリングの正しい例
```css
/* 行のストライプ（縞々） */
.table > tbody > tr:nth-of-type(even) > * {
  --bs-table-bg-type: #f0f4f8;
}

/* ホバー時の行の色 */
.table > tbody > tr:hover > * {
  --bs-table-bg-state: #e2e6ea;
}

/* ヘッダーの背景色（theadのthにはbackground-color直接指定が可能） */
.table thead th {
  background-color: #2c3e50;
  color: white;
}

/* 選択中の行 */
table tr.can-select.selected > * {
  --bs-table-bg-state: #d9ecff;
}

/* 特定モジュールの一覧テーブルのみストライプ */
[list-module=""Product""] .table > tbody > tr:nth-of-type(even) > * {
  --bs-table-bg-type: #f0f4f8;
}
```

## CSS記述の推奨事項
- Bootstrapベースのフレームワークなので、Bootstrapのクラスとその**CSS変数**をベースにカスタマイズする
- **テーブル行の背景色はbackground-colorではなく、必ず--bs-table-bg-type等のCSS変数を使う**
- 特定モジュール/フィールドのスタイルにはdata属性セレクタを使用する
- ClassNameで指定されたクラスと組み合わせて詳細なスタイリングが可能
- レスポンシブ対応には@mediaクエリを使用する
- CSS変数の上書きでグリッドの間隔やパディングを調整できる
- !importantの使用は最小限にする
";
    }
}
