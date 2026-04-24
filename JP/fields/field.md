# Field — 一覧と概念

**Field は Module を構成する部品**です。TextField・NumberField のような入力部品、Label・Button のような UI 部品、List や Module のような構造部品まで、画面と値の最小単位として振る舞います。

## Field の性質

- 大部分の Field は**値を持つ**
- DB とマッピングすると、モジュールが ORM の Entity のように振る舞う
- 多くの Field は**UI を持ち**、レイアウト（Grid / Canvas / Flow）に配置できる
- UI を持たずデータ保持だけに使う Field もある
- スクリプトから**名前で直接参照**でき、プロパティ・メソッドを呼び出せる

---

## System Field — 特別な役割を持つ Field

名前で役割が決まる Field です。ツールボックスからドラッグで作成し、名前は変更できません。DB 業務アプリで必要になる機能に対応します。

| 名前 | 種別 | 役割 |
|---|---|---|
| **Id** | Id | モジュールのデータを特定する主キー（追加・更新・削除に必須） |
| **LogicalDelete** | Boolean | 論理削除のフラグ |
| **CreatedAt** | DateTime | 作成日時 |
| **UpdatedAt** | DateTime | 更新日時 |
| **Creator** | Link | 作成者（CurrentUserModule 設定時） |
| **Updater** | Link | 更新者（CurrentUserModule 設定時） |
| **OptimisticLocking** | OptimisticLocking | 楽観ロック |

---

## DB Field — DB のカラムから自動生成

Module に Data Source を設定すると、ツールボックスに「DB フィールド」リストが出ます。そこからドラッグ＆ドロップで Field を追加すると、DB 列の型と名前に応じた Field が自動生成されます。

- 名前は DB 定義名をパスカルケースに変換したもの
- 名前は変更可能（ただし System Field の名前は使えない）
- System Field の名前と DB 列名が合わない時は、先に System Field を作って `DbColumn` で列を指定

---

## Field 一覧（系統別）

### 共通事項

- [Field 共通プロパティ](common_properties.md) — すべての Field で使えるプロパティ・スクリプト API

### 入力系

| Field | 用途 |
|---|---|
| [Text](Text.md) | 文字列入力（1 行／複数行） |
| [Number](Number.md) | 数値入力（最小・最大・スライダー対応） |
| [Boolean](Boolean.md) | 真偽値（チェック／スイッチ／トグル） |
| [Date](Date.md) | 日付のみ |
| [DateTime](DateTime.md) | 日時 |
| [Time](Time.md) | 時刻のみ |
| [Password](Password.md) | パスワード（確認入力チェック付き） |
| [File](File.md) | ファイルアップロード（画像プレビュー対応） |

### 選択系

| Field | 用途 |
|---|---|
| [Select](Select.md) | プルダウン選択（他モジュールから候補取得可） |
| [RadioGroup](RadioGroup.md) | ラジオボタンのコンテナ |
| [RadioButton](RadioButton.md) | 個別のラジオボタン |
| [Link](Link.md) | 他モジュール 1 件を検索ダイアログで選択 |

### 表示系

| Field | 用途 |
|---|---|
| [Label](Label.md) | 文字列の表示（見出し・キャプション） |
| [AnchorTag](AnchorTag.md) | ハイパーリンク |
| [Id](Id.md) | 主キー／外部キー |
| [ImageViewer](ImageViewer.md) | 画像表示 |
| [MarkupString](MarkupString.md) | HTML 直接表示 |

### 構造系

| Field | 用途 |
|---|---|
| [List](List.md) | 一覧（テーブル形式） |
| [DetailList](DetailList.md) | 一覧（カード形式、縦並び） |
| [TileList](TileList.md) | 一覧（タイル形式、折り返し） |
| [ListNumber](ListNumber.md) | 一覧内の行番号列 |
| [Module](Module.md) | 他モジュールを画面内に埋め込む |
| [Search](Search.md) | 検索バー |

### 操作系

| Field | 用途 |
|---|---|
| [Button](Button.md) | 独自スクリプトを実行 |
| [SubmitButton](SubmitButton.md) | 登録・更新・削除の標準ボタン |

### 特殊

| Field | 用途 |
|---|---|
| [ProCode](ProCode.md) | 独自の Blazor コンポーネントを埋め込み |
| [OptimisticLocking](OptimisticLocking.md) | 楽観ロック（System Field） |

### DB 系（データベースセクション参照）

| Field | 用途 |
|---|---|
| [Query](../db/query_field.md) | カスタム SQL で一覧を作る |
| [ExecuteSql](../db/execute_sql_field.md) | 任意の SQL を実行する |

### 廃止

| Field | 備考 |
|---|---|
| [ModuleSelect](ModuleSelect.md) | 最新バージョンには存在しません（代替については各ページ参照） |

---

## 関連項目

- [Field 共通プロパティ](common_properties.md)
- [Module](../module/module.md)
- [レイアウト](../module/layout.md)
- [スクリプト概要](../overview/script.md)
