-- 対象年月の月末時点での要対応商談件数（遅延または停滞）
-- @target_month: "yyyy-MM" 形式
WITH params AS (
  SELECT
    -- 月末日: target_month + 1ヶ月 - 1日
    (to_date(@target_month::text || '-01', 'YYYY-MM-DD') + interval '1 month' - interval '1 day')::date AS month_end_date,
    -- 停滞判定の境界（月末 - 30日）
    (to_date(@target_month::text || '-01', 'YYYY-MM-DD') + interval '1 month' - interval '31 days')::date AS stale_date
),
latest_activity AS (
  SELECT deal_id, MAX(activity_date_time) AS last_activity_at
  FROM activity
  GROUP BY deal_id
),
warned AS (
  SELECT
    d.id,
    CASE WHEN d.expected_close_date <= (SELECT month_end_date FROM params) THEN 1 ELSE 0 END AS is_overdue,
    CASE WHEN la.last_activity_at IS NULL
              OR la.last_activity_at::date < (SELECT stale_date FROM params)
         THEN 1 ELSE 0 END AS is_stale
  FROM deal d
  LEFT JOIN latest_activity la ON la.deal_id = d.id
  WHERE d.status NOT IN ('受注', '失注')
)
SELECT
  @target_month::text AS target_month,
  SUM(CASE WHEN is_overdue = 1 OR is_stale = 1 THEN 1 ELSE 0 END) AS total_count
FROM warned
