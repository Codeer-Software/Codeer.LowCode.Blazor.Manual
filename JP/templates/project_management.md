# プロジェクト管理テンプレート

中小規模のプロジェクトチームが、タスクの計画・進捗・負荷状況をひと目で把握できるようにするための雛形アプリ。一覧 / ガントチャート / タスクボード / ダッシュボード を一つのアプリにまとめたたたき台です。

内部名: `ProjectManagementTemplate`

<img src="../../Image/web/templates/template_project_home.png" alt="プロジェクト管理ホーム（進捗率 / 遅延件数 / 消化率 / 部署別負荷）" style="border: 1px solid #ccc;" width="900">

---

## 画面構成

| メニュー | 用途 |
|---|---|
| ホーム | 進捗率・遅延件数・今月の消化率・部署別負荷を ApexCharts で可視化 |
| プロジェクト | プロジェクトの CRUD。詳細画面にメンバー・タスクを内包表示 |
| マスタ（メンバー / 部署） | 担当者情報・組織の部署マスタ |

ガント / タスクボード は **プロジェクト一覧の各行のボタン** から、対象プロジェクトに絞った状態で開きます（URL: `プロジェクトガント/{Id}`, `プロジェクトボード/{Id}`）。実装上は独立モジュールではなく、プロジェクトモジュールの **別 DetailLayout** として作られています。

### ホーム（ダッシュボード）

4 つのウィジェットで、管理者が朝一で「プロジェクト全体の健康度」を把握できる構成:

| ウィジェット | 種類 | 内容 |
|---|---|---|
| プロジェクト別 進捗率 | 横棒グラフ | 各プロジェクトの平均進捗率 |
| プロジェクト別 遅延件数 | 棒グラフ | プロジェクトごとの期限超過タスク数 |
| 今月のタスク消化率 | ドーナツ | 今月期限のタスクのうち「完了 / 未完了」の構成比 |
| 部署別 今月の未完了タスク数 | 棒グラフ | 今月期限で未完了のタスクを、担当者の所属部署ごとに集計 |

### プロジェクト一覧（ボード／ガントへの導線）

<img src="../../Image/web/templates/template_project_list.png" alt="プロジェクト一覧（行頭の「ボード」「ガント」ボタンで対象プロジェクトのビューへ）" style="border: 1px solid #ccc;" width="900">

各行の先頭に「ボード」「ガント」ボタン。クリックで対象プロジェクトに絞った状態のタスクボード / ガントが開きます。

### タスクボード

<img src="../../Image/web/templates/template_project_board.png" alt="タスクボード（未着手 / 進行中 / レビュー / 完了 の 4 列。カードドラッグでステータス変更）" style="border: 1px solid #ccc;" width="900">

カードを別列へドラッグするとステータスが変わり、自動保存でそのままDBに反映されます。

### ガントチャート

<img src="../../Image/web/templates/template_project_gantt.png" alt="ガントチャート（タスクの期間バー + 進捗率 + 上部に遅延件数/構成比チャート）" style="border: 1px solid #ccc;" width="900">

タスク行ごとに期間バー + 進捗率を表示。上部の 3 チャート（担当者別遅延件数 / ステータス別遅延件数 / タスク状態構成比）はガント画面でも常時表示され、期間バーのドラッグやカード編集と同じ自動保存サイクルで再読み込みされます。

---

## データモデル

組織側とプロジェクト側の 2 系統で **5 テーブル構成**。

| テーブル | 主な列 |
|---|---|
| `project`（プロジェクト） | name / start_date / end_date / status / owner_id（→ members） |
| `task`（タスク） | project_id（→ project）/ name / assignee_id（→ members）/ start_date / end_date / progress（進捗率）/ status / priority |
| `project_member`（プロジェクトメンバー） | project_id / member_id（→ members）/ role / joined_at |
| `members`（メンバー） | name / email / title / department_id（→ department） |
| `department`（部署） | name |

### 関係図

```
department ←── members ─┬── project (owner_id)
                        ├── project_member ──→ project
                        └── task (assignee_id)
                                   │
                                   └── project (project_id)
```

`project_member` は「メンバー × プロジェクト」の多対多を解決する中間テーブル。タスクの担当者はプロジェクトメンバーに限定したい場合、`task.assignee_id` の SelectField 候補を `project_member.member_id WHERE project_id = task.project_id` で絞り込む拡張が可能（標準では全メンバーから選択）。

---

## 業務フロー

### 導入時

1. マスタでメンバー・部署を登録
2. プロジェクト画面で新規作成 → 参加メンバーとタスクを追加

### 日々の運用

- **進捗更新**: タスクボードでカードをドラッグしてステータスを変更。進捗率は自動連動
- **遅延・負荷の把握**: ホームのダッシュボードで遅延件数・今月の消化率・部署別負荷をチェック
- **スケジュール調整**: タスクガントで全体の期間バランスを確認。期間バーをドラッグで開始日 / 終了日を変更

---

## 自動保存の仕組み

プロジェクト詳細・タスクボード・ガントの各画面では **`AutoSubmitField`** を組み込んでおり、フィールド編集後 300ms で自動的に保存されます（Submit ボタン操作不要）。

対象操作:

- タスクボードでのカードのドラッグ＆ドロップ（ステータス列の移動）
- ガント上の期間バーのドラッグ（開始日 / 終了日の変更）
- カードのインライン編集（タスク名・進捗率・担当者など）

保存完了後にダッシュボードのチャート（QueryField）も再読み込みされるため、ボード上の操作が即座に上部の集計チャートに反映されます。

---

## 1 モジュール × 3 DetailLayout の構造

このテンプレ最大の構成上の特徴。**プロジェクトモジュールに 3 つの DetailLayout** を持たせ、それぞれを別 URL にマップしています。

| DetailLayout | URL | 内容 |
|---|---|---|
| `""`（標準） | `/Main/プロジェクト/{Id}` | プロジェクト詳細（基本情報 + メンバー + タスク） |
| `ボード行` | `/Main/プロジェクトボード/{Id}` | タスクボード画面（カンバン） |
| `ガント行` | `/Main/プロジェクトガント/{Id}` | ガントチャート画面 |

切り替えは `app.clprj` の `OtherPageModuleDesigns` で URL と DetailLayout を対応付けるだけ。**1 つのテーブル / モジュールで 3 種類のビュー** を切り替える構造になっています。

プロジェクト一覧の行頭ボタンは、それぞれ対応する URL に遷移する `AnchorTagField` です。

---

## 作るときのポイント

CLB で同等のテンプレートを 1 から組むときに重要な設計判断:

### 1 モジュール × 3 DetailLayout でビュー切替

ボードとガントは「同じ task テーブルを別の見た目で表示する」だけなので、独立モジュールにせず **プロジェクトモジュールの別 DetailLayout** にします。テーブル / フィールド / スクリプトを共有できるので、自動保存ロジックも 1 箇所で済みます。詳細は [モジュールページ種別](../designer/page_types.md)。

### AutoSubmitField で「保存ボタン無し」UX

ボード操作・ガント操作のたびに保存ボタンを押させると操作感が落ちます。`AutoSubmitField` を 1 つ置けば、内部の変更検知から 300ms デバウンスで自動 Submit が走ります。詳細は [自動保存パターン](../patterns/auto_save.md)。

### Submit 後にチャートを Reload

`AutoSubmitField` の `OnAfterSubmitAsync` でホームのチャート（QueryField）を `Reload()` すると、ボード上の操作が即座に上部チャート（担当者別遅延件数 / 構成比 Donut 等）に反映されます。**保存と UI 反映を切り離さない** のがリアルタイム感のコツ。

### ガント / タスクボード Field は Extras パッケージ

ガント・タスクボードは `Codeer.LowCode.Blazor.Extras` パッケージで提供される拡張 Field。CLB 標準には入っていないので、`app.clprj` の使用ライブラリに `Codeer.LowCode.Blazor.Extras` を追加して使います。インストール手順は [プロコード概要](../overview/procode.md)。

### ステータスと進捗率の連動

「完了」ステータスにしたら進捗率 100%、「未着手」にしたら 0% など、ステータスと進捗率を連動させたい場合は `task.mod.cs` の `Status.OnAfterValueChangedAsync` で `Progress.SetValueAsync(...)` を呼びます。逆に、進捗率が 100% になったら自動で「完了」にしたい場合は `Progress.OnAfterValueChangedAsync` 側で対応。

### 中間テーブル（多対多）

`project_member` は「メンバー × プロジェクト」の多対多。プロジェクト詳細画面では `ListField` で並べ、`project_id` は自動セット（親プロジェクトの ID を継承）。詳細は [多対多パターン](../patterns/many_to_many.md)。

### ApexCharts のレスポンシブ

ダッシュボードのウィジェットは画面幅に応じてレイアウトが変わるよう、`GridLayout` の `IsAutoFillWrap` + `MinWidth` で「横並び → 縦並び」の折り返しを設定。詳細は [レイアウトパターン](../patterns/layout_patterns.md)。

---

## カスタマイズのポイント

- 項目追加はデザイナ GUI から（コーディング不要）
- ステータス色・タスクボード列の定義は各モジュールの JSON。会社の運用ルールに合わせて編集可能
- ApexCharts の種類（棒／円／折れ線）も差し替え容易
- 工数入力・実績集計を追加したい場合は `task` に `estimated_hours` / `actual_hours` を追加 → 集計クエリを 1 つ足す

---

## 拡張ライブラリ

- **`Codeer.LowCode.Bindings.ApexCharts`** — ダッシュボードのチャート（棒 / 円 / Donut / 横棒）
- **`Codeer.LowCode.Blazor.Extras`** — ガントチャート Field / タスクボード Field

---

## 想定業種

タスクに「担当者」「期限」「進捗」がある業務なら業種を問わず適用可能。

| 業種 | 使用ケース |
|---|---|
| ソフトウェア開発・SIer | 開発案件の工程・タスク管理、リリーススケジュール可視化 |
| Web 制作・広告代理店 | 制作進行、クライアント別案件管理、納期管理 |
| コンサルティング | 複数顧客の案件並行管理、工数見える化 |
| 建設・不動産 | 工事案件のフェーズ管理、職人・業者の割当状況 |
| 製造業（設計・試作） | 設計タスク・試作工程のスケジュール管理 |
| 総務・情報システム部門 | 社内プロジェクト（DX 推進、システム入替等）の進行管理 |
| 研究・教育機関 | 研究プロジェクトのタスク割当、論文執筆スケジュール |
| 士業（会計・法務） | 顧問先ごとの案件・期限管理 |

規模感としては **5〜50 名程度のチーム、同時 10〜30 プロジェクト程度** に最適。

---

## 関連ドキュメント

- [業務テンプレート一覧](templates.md)
- [アプリ作成パターン一覧](../patterns/patterns.md)
- [多対多パターン](../patterns/many_to_many.md) — メンバー × プロジェクトの中間テーブル
- [ヘッダ詳細 (1:N) パターン](../patterns/header_detail.md) — プロジェクト＋タスクの作り方
- [自動保存パターン](../patterns/auto_save.md) — `AutoSubmitField` の使い方
- [モジュールページ種別](../designer/page_types.md) — 1 モジュール × 複数 DetailLayout の構成
- [プロコード概要](../overview/procode.md) — Extras パッケージの導入
