# 検索のパターン

リスト画面に検索条件を付けて結果を絞り込む各種パターン。基本的に 1 つの SearchLayout (検索レイアウト) と検索結果リストの組み合わせで実現します。

---

## 全フィールド検索

<!-- 画像参照: Manual の Image/web/patterns/search_all.png (ここではコメントアウト) -->

Text / Number / Date / DateTime / Time / Select / Radio / Boolean / Link / Id を 1 つの SearchLayout に並べた基本サンプル。Number/Date 系は標準で **From〜To 範囲入力 UI** に、Boolean は **3 状態 (true/false/未指定)** に自動切替されます。

**標準パターン集の対応**: サイドバー **`検索/全フィールド → `AllFieldsSearch``**

---
## 簡易検索パラメータ

<!-- 画像参照: Manual の Image/web/patterns/search_simple.png (ここではコメントアウト) -->

フィールドの `IsSimpleSearchParameter: true` を付けると、SearchLayout に置かなくても URL パラメータや一覧画面の簡易検索 UI で絞り込めます。「サイドバーから直接特定の条件で開きたい」ケースに便利。

**標準パターン集の対応**: サイドバー **`検索/簡易検索パラメータ → `SimpleSearch``**

---
## 空検索 (常時全件表示)

<!-- 画像参照: Manual の Image/web/patterns/search_empty.png (ここではコメントアウト) -->

SearchLayout を空にして、開いた瞬間に全件表示するパターン。マスタ表のように検索よりも一覧表示が目的の画面で使います。`AllowEmptySearch: true` で「条件未入力でも検索ボタンを押せる」設定にできます。

**標準パターン集の対応**: サイドバー **`検索/空検索 → `EmptySearch``**

---
## AND・OR 切替検索

<!-- 画像参照: Manual の Image/web/patterns/search_andor.png (ここではコメントアウト) -->

SearchLayout の `SearchOperator` を `UserSpecified` にすると、画面で **AND/OR を切り替えるラジオボタン**が出現。さらに 3 階層までネストして複雑な論理式 (例: `(A AND B) OR (C AND D)`) を組み立てられます。

**標準パターン集の対応**: サイドバー **`検索/AND・OR → `AndOrSearch``**

---
## 検索条件の Select 連動

<!-- 画像参照: Manual の Image/web/patterns/search_select_cascade.png (ここではコメントアウト) -->

検索条件の Select 同士を連動させるパターン (例: プロジェクト → フェーズ)。子の `SelectField.SearchCondition` に `FieldVariableMatchCondition` で「親フィールドの値で候補を絞り込む」を宣言します。

ポイントは、**検索条件の入力は `SearchValue` に入るが、候補絞り込みの参照先は `Value`** という点。親の `OnSearchDataChanged` スクリプトで 1 行コピーすると、親を変えた瞬間に子の候補がリアルタイムに取り直され、候補から外れた子の選択値は自動でクリアされます。

```csharp
void SelectedProject_OnSearchDataChanged()
{
    SelectedProject.Value = SelectedProject.SearchValue;
}
```

詳細画面の入力欄の連動は宣言だけで動きます ([連動入力 (検索条件で宣言)](input_patterns.md#連動入力-検索条件で宣言) 参照)。このスクリプトが要るのは**検索条件として置いた場合だけ**です。

**標準パターン集の対応**: サイドバー **`検索/Select連動 → `CascadeSearch``**

---
## 検索条件の初期化

<!-- 画像参照: Manual の Image/web/patterns/search_init.png (ここではコメントアウト) -->

サイドバーリンクから一覧を開いたとき、検索条件に初期値が入っている状態にしたい場合。`OnSearchInitialization` スクリプトで `SearchValue` / `SearchMin` / `SearchMax` をセットします。サイドバーリンク経由で URL に `?initialize_search=true` が自動付与されたときだけ発火する仕様。

**標準パターン集の対応**: サイドバー **`検索/初期化 → `SearchInit``**

---

## 関連ドキュメント

- [アプリ作成パターン一覧](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../ModuleDesign.md)
- [Field リファレンス](../Fields/)
