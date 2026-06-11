# Defaults/ — デザイン型のデフォルト状態 JSON

各 `{型名}.json` は、その型を**新規追加したときにデザイナが書き出す JSON と同一の既定状態**。
フィールド／レイアウト／検索条件などを書くときは、対応するファイルをコピーして
**必要なプロパティだけ上書き**する。これにより:

- `TypeFullName`・プロパティ構造・ポリモーフィックな入れ子（条件の `Value` ラップ等）が常に正しい
- 既定値が保証され、`LimitCount: 0` のような「デフォルトでも妥当でもない値」を吐かない
- 手書きの JSON 例より完全（全プロパティが実物どおりの順序・既定で並ぶ）

> 注意: `null` のプロパティは出力されない（デザイナの保存仕様と同じ。`DefaultIgnoreCondition = WhenWritingNull`）。
> 例えば子リストの `SearchCondition.LimitCount` は既定 `null` なので**行ごと現れない**＝それが「全件」の正しい状態。
> 値の一覧と意味は各 `Docs/Fields/*.md` を参照。

ファイル種別の目安:
- `*FieldDesign.json` … フィールド定義（design ファイルで最もよく使う）
- `*LayoutDesign.json` … レイアウト（Grid / Field / Canvas / Tab / SearchGrid …）
- `*MatchCondition.json` / `*Value.json` … 検索条件とその値ラップ
- `*FieldData.json` … ランタイムデータ構造（design ファイルでは通常書かない。参考）

## 再生成（手で編集しないこと）

このフォルダは**生成物**。型の追加・プロパティの既定値変更があったら、手で直さず再生成する。

- 生成元: `Source/Test/Json/DefaultJsonGenerator.cs`（NUnit の `[Explicit]` テスト）
- 実行: `dotnet test Source/Test/Test.csproj --filter "FullyQualifiedName~DefaultJsonGenerator"`
- 製品と同じシリアライザ（`JsonConverterEx`）で、各型を既定構築してシリアライズしている。
  そのため出力はデザイナが書く design ファイルと一致する。

> 対象は `Codeer.LowCode.Blazor` 本体に加え、外部フィールドライブラリ
> `Codeer.LowCode.Blazor.Extras`（Calendar / TaskBoard / Gantt / RichText 等）と
> `Codeer.LowCode.Bindings.Blazor-ApexCharts`（ApexChart / ApexHBarChart / ApexRadialChart）。
> バージョンはテストプロジェクトが参照するパッケージ（= アプリ/デザイナと同じ版）に追従する。
> ライブラリの版を上げたら再生成すること。
