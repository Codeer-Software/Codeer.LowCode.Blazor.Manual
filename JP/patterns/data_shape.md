# データ構造のパターン (モジュールの形)

業務アプリで繰り返し登場するデータの「形」と、それを Codeer.LowCode.Blazor でどう作るかをまとめたページです。各パターンには [PatternShowcase テンプレート](patterns.md#デザイナの標準テンプレート-patternshowcase-で全パターンを実機確認できる) の **対応モジュール名** を併記しています。デザイナで実機サンプルを開いて読みながらこのページを参照すると理解が早いです。

---

## 1. 単一 CRUD

**いつ使う**: 商品マスタ、社員マスタなど **1テーブル＝1画面で完結する基本形**。

### DB の形

```
products
├── id          PK
├── code        TEXT
├── name        TEXT
└── price       NUMBER
```

### CLB ではこう作る

1モジュール = 1テーブル。`DataSourceName` + `DbTable` を指定し、各カラムに対応する `Field` を並べる。`Id` フィールド (`IdFieldDesign`) + 入力フィールド + `SubmitButton` の構成。

### PatternShowcase の対応モジュール

サイドバー **`データ操作/CRUD`** → `Product` モジュール

---

## 2. ヘッダ詳細 (1:N)

**いつ使う**: 注文+明細、請求書+明細行、プロジェクト+タスクなど、**1件の親レコードに複数の子レコードがぶら下がる**典型構造。

### DB の形

```
orders                    order_details
├── id        PK    ←─┐   ├── id          PK
├── customer  TEXT    │   ├── order_id    FK → orders.id
└── order_date DATE   └── ├── product     TEXT
                          ├── qty         INT
                          └── price       NUMBER
```

### CLB ではこう作る

- 親モジュール (`Order`) と子モジュール (`OrderDetail`) を別々に作る
- 子モジュール側に外部キー列 (`order_id`) + `LinkField` で親を参照
- 親モジュールの DetailLayout に `DetailListField` を配置し、子モジュールを内包
- 親 Submit 時に子の Add/Update/Delete が **1 トランザクションでまとめて保存** される (CLB が面倒みる)

### PatternShowcase の対応モジュール

サイドバー **`データ操作/親子 (1:N)`** → `Order` + `OrderDetail`
サイドバー **`データ操作/多段ネスト`** → `Project` + `Phase` + `Task` (親→子→孫の3段)

### 落とし穴

- 子の `DetailListField.DeleteTogether` を `true` にすると、親レコード削除時に子も物理削除される (論理削除との併用は別途検討)
- 子の `LimitCount` は親詳細画面では `null` (= 全件) にする。一覧ページでは `50` 等が普通

---

## 3. マスタ参照 (多対1)

**いつ使う**: 注文に紐づく顧客、商品に紐づくカテゴリなど、**他モジュールから 1件選んで紐付ける**構造 (多対1)。

### DB の形

```
products                  categories
├── id                    ├── id        PK
├── name                  ├── name      TEXT
├── category_id  FK ───→  └── ...
└── ...
```

### CLB ではこう作る

参照する側 (`Product`) に `LinkField` を置く:
- `DbColumn`: 外部キー列名 (`category_id`)
- `SearchCondition.ModuleName`: 参照先モジュール名 (`Category`)
- `ValueVariable`: `Id.Value` (参照先の ID 取得式)
- `DisplayTextVariable`: `Name.Value` (画面表示用テキスト取得式)

ユーザーは検索ダイアログで参照先レコードを選ぶ。データとしては FK 列に参照先 ID を保存。

### PatternShowcase の対応モジュール

`LookupCustomer` (汎用的なマスタ参照)。`Article` モジュールでも `LinkField` の使用例あり。

### 落とし穴

- 参照先モジュールも PageFrame に登録 (またはサイドバー表示) されてないと、検索ダイアログで開けない場合がある
- 表示用に参照先の他フィールドも見たいときは、親モジュールの `LinkFieldNames` にパス追加 + レイアウトで参照する

---

## 4. 多対多

**いつ使う**: 記事とタグ、ユーザーとロールなど、**両側が複数のレコードと関連する**構造。中間テーブルで実現。

### DB の形

```
articles            article_tags                  tags
├── id    PK ←─────┤ ├── id           PK         ├── id    PK
├── title          │ ├── article_id   FK → articles.id  ├── name
└── ...            ├─┤ tag_id        FK → tags.id      └── ...
                   └─└── ...
```

### CLB ではこう作る

- 3モジュール: 本体 (`Article`) + 中間 (`ArticleTag`) + 相手 (`Tag`)
- 中間モジュールは `LinkField` を2つ持つ (両側への FK)
- 本体モジュールに `DetailListField` で中間モジュールを内包すると、本体の詳細画面でタグ付け/解除ができる

### PatternShowcase の対応モジュール

サイドバー **`データ操作/多対多`** → `Article` + `ArticleTag` + `Tag`

---

## 5. ツリー (自己参照)

**いつ使う**: カテゴリの階層構造、組織図、コメントのスレッドなど、**同じテーブル内で親子関係を表現**する構造。

### DB の形

```
departments
├── id          PK
├── name        TEXT
└── parent_id   FK → departments.id (自己参照、NULL 許容)
```

### CLB ではこう作る

`LinkField` で **同じモジュール (自分自身)** を参照する設定にする (`SearchCondition.ModuleName` に自モジュール名)。`parent_id` が NULL のレコードがツリーのルート。

### PatternShowcase の対応モジュール

サイドバー **`データ操作/階層構造`** → `Department`

### 落とし穴

- 階層表示 (ツリービュー) は標準ビルトインでは無いので、Query フィールド + 再帰 CTE などで表示する
- 自分が自分の祖先になる循環を防ぐバリデーションはアプリ側で書く

---

## 6. 双方向 ID 持ち合い (1:1 相互参照)

**いつ使う**: 1:1 で対応する2つのテーブルが **互いに ID を持つ** 相互参照構造。

具体例:
- 会社 (Company) ↔ 代表者 (President)
- 注文 (Order) ↔ 領収書 (Receipt)
- ユーザー (User) ↔ 拡張プロフィール (Profile)
- 申請 (LeaveRequest) ↔ 承認フロー (ApprovalFlow)

純粋な DB 設計だけ見ると 1:1 関係は片方向 FK が定石ですが、CLB の `LinkField` / `ModuleField` が **単方向 FK 前提** で動くため、**両側から相手を参照したい**ケースでは双方向 FK にしておく方が CLB の機能を素直に活かせます。

例えば申請 ↔ 承認フローの場合:
- 申請画面に承認フローを `ModuleField` で内包 (申請 → 承認フローの方向)
- 承認待ち一覧から申請に `LinkField` で戻る (承認フロー → 申請の方向)

両方向のアクセスが必要なデザインでは双方向 FK が一番素直。`LinkField` の `DisplayTextVariable` で相手側の値を引いて表示するのも、両側に FK があると単純に書けます。

「親が子の ID を持ち、子も親の ID を持つ」状態は新規同時作成時に通常デッドロックになる (どちらも相手の ID が未確定なので INSERT できない) ところを、CLB のランタイムが **片方を NULL Insert → 後追いで UPDATE で実 ID を埋める** ことでサイクル解決します。アプリ側コードに特別な配慮は不要。

### DB の形

```
mutual_main                      mutual_sub
├── id        PK ←──────────────┤ ├── id        PK
├── name                          ├── name
└── sub_id    FK → mutual_sub.id  └── main_id    FK → mutual_main.id  ←──┐
                                                                          │
        (mutual_main.sub_id) ─────────────────────────────────────────────┘
```

### CLB ではこう作る

- 親モジュール (`MutualMain`) に **`ModuleField`** (`DbColumn = "sub_id"`, `ModuleName = "MutualSub"`) を置いて子を内包
- 子モジュール (`MutualSub`) に **`LinkField`** (`DbColumn = "main_id"`, `ModuleName = "MutualMain"`) を置いて親を逆参照 (`DataOnlyFields` に入れて UI 非表示でも OK)
- 親の `OnAfterInitialization` で **新規時に子の親参照に自分のテンポラリ ID をセット** する:

```csharp
void OnAfterInitialization()
{
    if (IsNewData)
    {
        // 子の Main に「自分 (= 親) のテンポラリ Id」を設定
        // Submit 時に CLB が双方向サイクルを解決する
        SubSlot.ChildModule.Main.Value = this.Id.Value;
    }
}
```

- 親の `SubmitButton` を1回押すと、親と子が **同一トランザクションで保存** される (子の `main_id` には NULL Insert 後の UPDATE で実 ID が入る)

### DB の前提条件

- 双方向の FK 列のどちらか (もしくは両方) が **NULL 許容**であること (片方を NULL Insert するため)。NOT NULL 制約が両方に付いてると CLB の解決ロジックでも DB エラーになる
- 通常の SQLite/PostgreSQL/SQL Server 等のデフォルト (列の NULL 許容) で問題なし

### PatternShowcase の対応モジュール

サイドバー **`データ操作/双方向ID (1:1)`** → `MutualMain` + `MutualSub`

### 落とし穴

- 「親 (Main) の Submit ボタン」だけで両方保存される。子側に Submit ボタンを置く必要なし
- 子の `Main` フィールドを `DataOnlyFields` に入れず UI に出してしまうと、ユーザーが手動で親を選ぶ必要があるように見えて混乱しやすい

---

## 7. 状態遷移を持つレコード

**いつ使う**: 受注の進行状態 (受注 → 出荷 → 完了)、申請の状態 (下書き → 申請中 → 承認) など、**特定の状態しか取らず、状態間の遷移にルールがある**レコード。

### CLB ではこう作る

- `SelectField` で状態を持つ (例: `Status` フィールド、Candidates `下書き,Draft` / `申請中,Submitted` / `承認済,Approved`)
- 状態遷移ボタン (`ButtonField` の `OnClick` スクリプト) で `Status.Value` を書き換える
- ボタンの `IsVisible` を現在状態によって出し分け、不正な遷移を UI レベルで防ぐ
- スクリプト側で `if (Status.Value != "Draft") { return; }` のようなガードも入れる (UI を回避された場合の保険)

### PatternShowcase の対応モジュール

PatternShowcase には専用サンプルはまだない。承認フロー付きの完全例は **PatternShowcaseAuth テンプレート** (認証付き) の `LeaveRequest` / `ExpenseRequest` + `ApprovalFlow` を参照。

---

## 8. 履歴・監査 (DataChangeHistory)

**いつ使う**: 「誰が・いつ・どのフィールドを・どの値からどの値に変えたか」を全レコード自動で残したい場合。

### DB の形

別テーブル (`data_change_history` 等) に変更ログが蓄積される。CLB が自動で INSERT する。

### CLB ではこう作る

サーバー側 `appsettings.json` の `DataChangeHistoryTableInfo` で対象テーブルを指定。CLB が Submit のたびに変更差分を自動記録する (アプリ側で書く必要なし)。

### PatternShowcase の対応モジュール

(現状 PatternShowcase 単体には専用サンプルなし。詳細は [認証 / 認可の概要](../authorization/authorization.md) や Help セクション参照)

---

## 9. 論理削除

**いつ使う**: 削除ボタンを押しても **物理削除せず削除フラグを立てる**だけ。後から復活できる、または監査要件で履歴が必要な場合。

### DB の形

```
soft_delete_items
├── id            PK
├── name          TEXT
└── LogicalDelete BOOLEAN  ← CLB 予約名 (この綴りでないと動かない)
```

### CLB ではこう作る

- `BooleanFieldDesign` を作って、**`Name` を必ず `"LogicalDelete"`** にする (CLB の予約名)
- これだけで CLB の削除ボタンが **`UPDATE SET LogicalDelete = true`** に変わる
- 一覧表示時も `LogicalDelete = false` で自動フィルタされる
- UI には出さず Fields にだけ定義する (一覧で空セルになるので)

### PatternShowcase の対応モジュール

- サイドバー **`データ操作/論理削除`** → `SoftDeleteItem` (一般ユーザー画面)
- サイドバー **`データ操作/論理削除(管理)`** → `SoftDeleteItemAdmin` (削除済みも含めて見える管理画面)

### 落とし穴

- フィールド名は必ず `LogicalDelete` の綴り。`IsDeleted` 等の任意名だと CLB の自動動作が効かない
- 管理画面 (削除済も見える) を作るときは、別モジュール (= 別 `Module`) で同じテーブルを参照 + Boolean 名を `LogicalDelete` 以外 (例: `DeletedFlag`) にして自動フィルタを回避する

---

## 10. 作成日時・更新日時 (システムフィールド)

**いつ使う**: 「いつ作られた / 最後に更新されたか」「誰が作った / 更新したか」を自動で記録したい。

### CLB ではこう作る

CLB の **予約名システムフィールド**を Fields に追加する (CLB が自動セットするのでスクリプト不要):

| フィールド名 (予約名) | 型 | 動作 |
|---|---|---|
| `CreatedAt` | DateTime | レコード作成時に自動セット |
| `UpdatedAt` | DateTime | レコード更新時に自動セット |
| `Creator` | Link → ユーザー | 作成者を自動セット |
| `Updater` | Link → ユーザー | 更新者を自動セット |

### PatternShowcase の対応モジュール

サイドバー **`データ操作/作成日時・更新日時`** → `SystemFieldsSample`

### 落とし穴

- フィールド名は予約名の **綴りそのまま**。`CreatedDate` などの任意名だと自動セットされない
- スクリプトで `Creator.Value = CurrentUser.Id.Value` のように代入する必要はない (CLB の保存処理が自動で入れる)

---

## パターンの組み合わせ

実際のアプリは複数のパターンを組み合わせて作ります。例:

| ドメイン例 | 使うパターン |
|---|---|
| 顧客管理 (CRM) | 単一CRUD + マスタ参照 + 検索 + 履歴 |
| 注文管理 | ヘッダ詳細 (1:N) + マスタ参照 (顧客/商品) + 状態遷移 + 帳票 |
| 在庫管理 | 単一CRUD + 集計 (Query) + ExecuteSql で在庫更新 |
| 課題/タスク管理 | 状態遷移 + マスタ参照 (担当者) + 階層構造 (親タスク → 子タスク) |
| 会員管理 (1:1 拡張プロフィール) | 単一CRUD + **双方向 ID 持ち合い** (基本情報 ↔ 詳細プロフィール) |

「これを実現したい」と思ったら、まず PatternShowcase で似た構造のサンプルを開いて、自分のプロジェクトのモジュールに**コピー/参考にしながら組み替える** のが最短ルートです。

---

## 関連ドキュメント

- [パターン入口](patterns.md) ─ 全パターンのインデックス
- [モジュール定義の全体構造](../module/module.md)
- [Field リファレンス](../fields/) ─ LinkField / ListField / DetailListField / ModuleField 等の詳細
- [認証 / 認可の概要](../authorization/authorization.md) ─ Creator/Updater 等の動作含む
