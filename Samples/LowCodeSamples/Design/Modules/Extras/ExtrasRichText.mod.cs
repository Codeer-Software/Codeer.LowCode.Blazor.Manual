void DetailLayoutDesign_OnAfterInitialization()
{
    Content.Value = """
        <h1 style="color:#1a73e8;">RichTextField デモ</h1>
        <p><span style="font-size:18px;">書式付きテキスト</span>の編集に対応したフィールドです。</p>
        <h2 style="color:#d93025;">文字装飾</h2>
        <p>
            <b>太字</b> / <i>斜体</i> / <u>下線</u> / <s>取り消し線</s> /
            <span style="color:#188038;">緑色の文字</span> /
            <span style="background-color:#fff59d;">マーカー</span>
        </p>
        <h2 style="color:#d93025;">サイズ違い</h2>
        <p>
            <span style="font-size:10px;">10px の小さな文字</span><br>
            <span style="font-size:14px;">14px の通常サイズ</span><br>
            <span style="font-size:24px;">24px の大きな文字</span><br>
            <span style="font-size:32px;color:#e37400;">32px のオレンジ</span>
        </p>
        <h2 style="color:#d93025;">リスト</h2>
        <ul>
            <li>順序なしリストの項目 1</li>
            <li style="color:#188038;">緑色の項目</li>
            <li><b>太字の項目</b></li>
        </ul>
        <ol>
            <li>順序ありリストの項目 1</li>
            <li>項目 2</li>
            <li>項目 3</li>
        </ol>
        <h2 style="color:#d93025;">リンク</h2>
        <p>
            <a href="https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras" target="_blank">Codeer.LowCode.Blazor.Extras (GitHub)</a>
        </p>
        <p style="color:#5f6368;font-size:12px;">ツールバーから書式を変更できます。編集内容はこのページでは保存されません。</p>
        """;
}
