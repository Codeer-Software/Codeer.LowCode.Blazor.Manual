# 列挙型 (Enum) リファレンス

デザインファイルのJSONで使用する共通列挙型の一覧。JSON内では文字列として記述する。

> **注意:** フィールド型固有の列挙型（BooleanUIType, ButtonVariant, LabelStyle 等）は、`Docs/Fields/` 内の各フィールドドキュメントに記載されている。

## MatchComparison
検索条件の比較演算子。

| 値 | 説明 |
|---|---|
| `Equal` | 等しい (=) |
| `NotEqual` | 等しくない (!=) |
| `LessThan` | 未満 (<) |
| `LessThanOrEqual` | 以下 (<=) |
| `GreaterThan` | 超過 (>) |
| `GreaterThanOrEqual` | 以上 (>=) |
| `Like` | パターン一致 (LIKE) |
| `In` | リスト内に存在 (IN) |
| `NotIn` | リスト内に存在しない (NOT IN) |
| `Exists` | 存在する |
| `NotExists` | 存在しない |

## HorizontalAlignment
水平方向の配置。

| 値 | 説明 |
|---|---|
| `Start` | 左寄せ |
| `Center` | 中央 |
| `End` | 右寄せ |
| `Stretch` | 伸縮 |

## VerticalAlignment
垂直方向の配置。

| 値 | 説明 |
|---|---|
| `Top` | 上寄せ |
| `Middle` | 中央 |
| `Bottom` | 下寄せ |
| `Stretch` | 伸縮 |

## ScrollDirection
スクロール方向。フラグ型（組み合わせ可能）。

| 値 | 説明 |
|---|---|
| `Unset` | 設定なし |
| `Vertical` | 縦スクロール |
| `Horizontal` | 横スクロール |

## TextWrap
テキストの折り返し。

| 値 | 説明 |
|---|---|
| `Unset` | 設定なし（デフォルト動作） |
| `BreakAll` | 任意の位置で折り返す |
| `Ellipsis` | はみ出す部分を省略（...） |

## PagerPosition
一覧フィールドのページャー位置。ListFieldDesignBase の `PagerPosition` プロパティで使用。

| 値 | 説明 |
|---|---|
| `None` | ページャーを表示しない |
| `Top` | 上部に表示（デフォルト） |
| `Bottom` | 下部に表示 |

## GridRowType
グリッド行の種別。

| 値 | 説明 |
|---|---|
| `Normal` | 通常行 |
| `Header` | ヘッダー行（固定表示） |
| `Footer` | フッター行（固定表示） |

## CssFontWeight
フォントの太さ。CSS font-weight に対応。

| 値 | 数値 |
|---|---|
| `Thin` | 100 |
| `ExtraLight` | 200 |
| `Light` | 300 |
| `Normal` | 400 |
| `Medium` | 500 |
| `SemiBold` | 600 |
| `Bold` | 700 |
| `ExtraBold` | 800 |
| `Black` | 900 |

## CssFontStyle
フォントスタイル。

| 値 | 説明 |
|---|---|
| `Normal` | 標準 |
| `Italic` | イタリック |
| `Oblique` | 斜体 |

## HomeLabelType
PageFrame のホームラベル種別。

| 値 | 説明 |
|---|---|
| `Text` | テキスト表示 |
| `Image` | 画像表示 |

## ModulePageType
ページのレイアウト種別。

| 値 | 説明 |
|---|---|
| `Auto` | モジュール定義に基づいて自動判定 |
| `ListToDetail` | 一覧→詳細の遷移型 |
| `List` | 一覧のみ |
| `Detail` | 詳細のみ |
