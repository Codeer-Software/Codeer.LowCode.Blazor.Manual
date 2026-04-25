# アプリ作成パターン — 着手前メモ（TODO）

ユーザーマニュアルの新セクション `JP/patterns/`（アプリ作成パターン）の構成案と、
書く対象のパターン洗い出し。時間ができたタイミングから順次着手する。

> 注: このセクションは**チュートリアル・Tips・Field リファレンスと内容が重複してよい**。
> 同じ機能でも「アプリの作り方」の文脈で改めて触れることに価値がある（ユーザー指示）。

---

## 提案する構成

```
JP/patterns/
  patterns.md                # 入口・パターン一覧（インデックス）
  data_shape.md              # A. データ構造パターン
  screen_flow.md             # B. 画面構成パターン
  input_assist.md            # C. 入力支援パターン
  data_io.md                 # D. データアクセス・出力パターン
  auth_patterns.md           # E. 認証・認可パターン
  domain_examples.md         # F. ドメイン例（任意）
```

各パターンは「**いつ使う / CLB ではこう作る / 落とし穴**」の3点セットで簡潔に。
既存の Tips（DoubleList、SendingMail、ModuleSearcher 等）からはリンクで誘導。

完成後は `README.md` の「ガイド」セクションに導線を追加。

---

## A. データ構造のパターン（モジュールの形）

| パターン | 説明 | CLB の道具 |
|---|---|---|
| 単一 CRUD | 1テーブル＝1モジュール。基本形 | Module + DataSource |
| ヘッダ詳細 | 親モジュール + 明細リスト（注文+明細、請求書+行） | Module + ListField/DetailList、`OnSubmit` 連携保存 |
| マスタ参照（Link） | 他モジュールから 1件選んで紐付け | LinkField |
| 多対多（中間テーブル） | タグ・属性などの紐付け | 中間モジュール + LinkField |
| ツリー（自己参照） | カテゴリ階層・組織図 | Module + LinkField（自モジュール参照） |
| 状態遷移を持つレコード | ステータス管理（下書き→申請中→承認済） | フィールド + スクリプトでガード |
| 履歴・監査 | 変更履歴の自動保存 | DataChangeHistory（テンプレ標準実装あり） |
| 論理削除 | 物理削除せず削除フラグ | LogicalDelete System Field |

---

## B. 画面構成のパターン

| パターン | 説明 | CLB の道具 |
|---|---|---|
| 一覧 → 詳細 | クリックで詳細へ遷移（最も基本） | ListToDetail PageType |
| 検索付き一覧 | 検索バー + 結果リスト | SearchField + ListField / SearchLayout |
| 詳細内に関連リスト | 詳細画面内で子レコードを並べる | ListField を詳細に配置 |
| 詳細内に検索付きリスト | 詳細内のリストに絞り込みバー | ListField + SearchField |
| 2リスト連動 | 親リストで選択→子リスト絞り込み | `SetAdditionalCondition` + `Reload`（既存 DoubleList Tips） |
| マルチタブ編集 | 大きいフォームをタブで分割 | TabLayout |
| タイル/カード表示 | 表形式以外の並べ方 | DetailList / TileList |
| ダッシュボード | 複数モジュールを 1画面に集約 | ModuleField を並べる + Canvas |
| Wizard（ステップ入力） | 段階的入力 | TabLayout + 切替制御 / 別画面遷移 |

---

## C. 入力支援パターン

| パターン | 説明 |
|---|---|
| カスケーディング選択 | A 選択→ B の候補が絞られる |
| オートサブミット | 変更したらすぐ保存 |
| 読取/編集モード切替 | 表示モードと編集モードを 1画面で切替（ViewEditToggleButton） |
| 行内編集 | 一覧の行を直接編集 |
| コピー新規作成 | 既存データをコピーして新規（CopyModuleButton） |
| 値の自動計算 | 他項目から自動計算 |

---

## D. データアクセス・出力パターン

| パターン | 説明 |
|---|---|
| 生 SQL でリスト | Query Field |
| 集計表示 | SUM/COUNT を Query で |
| 任意 SQL の実行 | ExecuteSql Field（更新系も） |
| Excel 帳票 | テンプレート流し込み（既存チュートリアルあり） |
| PDF 帳票 | Excel→PDF 変換 |
| CSV/Excel 一括ダウンロード | 標準機能 |
| CSV/Excel 一括アップロード | 標準機能 |
| WebAPI 連携 | WebApiService（既存チュートリアルあり） |
| メール送信 | MailService（既存 Tips あり） |
| ファイル添付 | FileField |

---

## E. 認証・認可パターン

| パターン | 説明 |
|---|---|
| ログインユーザー連動 | CurrentUserModule、Creator/Updater |
| ロール別権限 | UserRead/UserWrite |
| データ単位権限 | DataRead/DataWrite（作った本人のみ等） |
| マルチテナント | テナント別データ分離 |

---

## F. ドメイン例（業務アプリの実装例として、任意）

| 例 | 含まれるパターン |
|---|---|
| 顧客管理（CRM 風） | 単一CRUD + Link + 検索 + 履歴 |
| 注文管理 | ヘッダ詳細 + 状態遷移 + 帳票 |
| 在庫管理 | 単一CRUD + 集計 + ExecuteSql |
| 課題/タスク管理 | 状態遷移 + Link + 検索 |
| 予約管理 | カレンダー視点 + 時間帯衝突チェック |

---

## 着手時の進め方メモ

1. まず `patterns.md`（入口）と A・B（最も基本）から書く
2. 各パターンは 1 セクション = 数行の説明 + 簡単な構成図 or コード断片 + 既存ドキュメントへのリンクで OK
3. 新しいスクショを撮るより、既存の Field/チュートリアル/Examples の図を流用する
4. 抜けてるパターンに気づいたらこのファイルに追記してから書く
5. 全部書き終わったら `README.md` のガイドセクションに導線を追加し、このファイルは削除
