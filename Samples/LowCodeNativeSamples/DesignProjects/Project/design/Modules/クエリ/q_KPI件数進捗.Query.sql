-- 対象月の件数進捗（受注済 / 残り の 2 行を返す、Donut 用）
WITH base AS (
  SELECT
    SUM(CASE WHEN status = '受注' THEN 1 ELSE 0 END) AS won,
    SUM(CASE WHEN status NOT IN ('受注', '失注') THEN 1 ELSE 0 END) AS remaining
  FROM deal
  WHERE to_char(expected_close_date, 'YYYY-MM') = @target_month::text
)
SELECT '受注済' AS category, won AS count FROM base
UNION ALL
SELECT '残り'   AS category, remaining AS count FROM base
