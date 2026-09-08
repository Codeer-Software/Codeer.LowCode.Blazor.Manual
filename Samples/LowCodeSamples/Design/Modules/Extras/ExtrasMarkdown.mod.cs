void DetailLayoutDesign_OnAfterInitialization()
{
    Content.Value = """
        # MarkdownField デモ

        Markdown のテキストをそのまま保存し、閲覧時は HTML に描画するフィールドです。
        上のツールバーから記法を挿入でき、[プレビュー] タブで見え方を確認できます。

        ## 文字装飾

        **太字** / *斜体* / ~~取り消し線~~ / `インラインコード`

        ## リスト

        - 順序なしリストの項目 1
        - 項目 2
          - 入れ子の項目
        1. 順序ありリストの項目 1
        2. 項目 2

        - [x] 完了したタスク
        - [ ] 未完了のタスク

        ## 表

        | 項目 | 数量 | 単価 |
        |---|---:|---:|
        | ノート PC | 2 | 180,000 |
        | モニター | 4 | 45,000 |
        | キーボード | 10 | 8,000 |

        ## コードブロック

        ```csharp
        void Approve_OnClick()
        {
            Notes.AppendLine("- " + DateTime.Now.ToString("yyyy/MM/dd HH:mm") + " 承認");
        }
        ```

        ## 引用とリンク

        > 備考・仕様・手順・議事メモなど、構造のある長文に向いています。
        > AI が書いた文章もそのまま保存できます。

        [Codeer.LowCode.Blazor.Extras (GitHub)](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras)

        ## 安全性

        Markdown 中の生の HTML は無効化され、文字として表示されます: <b>太字にはなりません</b> <script>alert(1)</script>

        ---

        編集内容はこのページでは保存されません。
        """;
}
