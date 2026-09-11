-- 要対応商談リスト
-- 遅延: 受注予定日 < 基準日 かつ 未受注未失注
-- 停滞: 30日以上活動なし かつ 未受注未失注
WITH params AS (
  SELECT
    @today_date::date AS today_date,
    (@today_date::date - interval '30 days')::date AS stale_date
),
latest_activity AS (
  SELECT deal_id, MAX(activity_date_time) AS last_activity_at
  FROM activity
  GROUP BY deal_id
),
warned AS (
  SELECT
    d.id,
    d.name AS deal_name,
    c.name AS customer_name,
    s.name AS owner_name,
    d.status,
    d.expected_close_date,
    la.last_activity_at::date AS last_activity_date,
    CASE WHEN d.expected_close_date < (SELECT today_date FROM params) THEN 1 ELSE 0 END AS is_overdue,
    CASE WHEN la.last_activity_at IS NULL
              OR la.last_activity_at::date < (SELECT stale_date FROM params)
         THEN 1 ELSE 0 END AS is_stale
  FROM deal d
  LEFT JOIN customer c ON d.customer_id = c.id
  LEFT JOIN salesperson s ON d.owner_salesperson_id = s.id
  LEFT JOIN latest_activity la ON la.deal_id = d.id
  WHERE d.status NOT IN ('受注', '失注')
)
SELECT
  id,
  deal_name,
  customer_name,
  owner_name,
  status,
  expected_close_date,
  last_activity_date,
  CASE
    WHEN is_overdue = 1 AND is_stale = 1 THEN '遅延・停滞'
    WHEN is_overdue = 1 THEN '遅延'
    ELSE '停滞'
  END AS warning_type
FROM warned
WHERE is_overdue = 1 OR is_stale = 1
ORDER BY is_overdue DESC, is_stale DESC, expected_close_date, id
