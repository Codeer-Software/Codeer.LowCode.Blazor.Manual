-- 対象年月の KPI（予定 vs 実績の対比）
-- target_count   : 予定総件数（進行中 + 受注確定）
-- won_count      : 受注確定数
-- target_amount  : 予測総金額（進行中の予測 + 受注確定金額）
-- won_amount     : 受注確定金額
-- count_progress : 件数達成率 (0-100)
-- amount_progress: 金額達成率 (0-100)
WITH base AS (
  SELECT
    SUM(CASE WHEN status NOT IN ('受注', '失注') AND to_char(expected_close_date, 'YYYY-MM') = @target_month::text THEN 1 ELSE 0 END) AS active_count,
    SUM(CASE WHEN status = '受注' AND to_char(expected_close_date, 'YYYY-MM') = @target_month::text THEN 1 ELSE 0 END) AS won_count,
    SUM(CASE WHEN status NOT IN ('受注', '失注') AND to_char(expected_close_date, 'YYYY-MM') = @target_month::text THEN expected_amount ELSE 0 END) AS expected_total,
    SUM(CASE WHEN status = '受注' AND to_char(expected_close_date, 'YYYY-MM') = @target_month::text THEN expected_amount ELSE 0 END) AS won_amount
  FROM deal
)
SELECT
  @target_month::text AS target_month,
  active_count + won_count AS target_count,
  won_count,
  expected_total + won_amount AS target_amount,
  won_amount,
  CASE WHEN (active_count + won_count) = 0 THEN 0
       ELSE CAST(won_count * 100.0 / (active_count + won_count) AS INTEGER)
  END AS count_progress,
  CASE WHEN (expected_total + won_amount) = 0 THEN 0
       ELSE CAST(won_amount * 100.0 / (expected_total + won_amount) AS INTEGER)
  END AS amount_progress
FROM base
