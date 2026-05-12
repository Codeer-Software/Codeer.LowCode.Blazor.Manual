-- 対象月の金額進捗（受注済 / 残り の 2 行を返す、Donut 用）
WITH base AS (
  SELECT
    SUM(CASE WHEN status = '受注' THEN expected_amount ELSE 0 END) AS won,
    SUM(CASE WHEN status NOT IN ('受注', '失注') THEN expected_amount ELSE 0 END) AS remaining
  FROM deal
  WHERE to_char(expected_close_date, 'YYYY-MM') = @target_month::text
)
SELECT '受注済' AS category, won AS amount FROM base
UNION ALL
SELECT '残り'   AS category, remaining AS amount FROM base
