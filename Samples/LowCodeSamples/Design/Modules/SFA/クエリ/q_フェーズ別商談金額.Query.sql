-- @target_month の月に受注予定だった商談のフェーズ別予測金額合計（失注は除く・全フェーズを 0 円含めて返す）
WITH phases AS (
  SELECT '初回接触' AS phase, 1 AS sort_order
  UNION ALL SELECT 'ヒアリング', 2
  UNION ALL SELECT '提案', 3
  UNION ALL SELECT '見積', 4
  UNION ALL SELECT '最終交渉', 5
  UNION ALL SELECT '受注', 6
)
SELECT
  p.phase,
  COALESCE(SUM(d.expected_amount), 0) AS total_amount
FROM phases p
LEFT JOIN deal d ON d.status = p.phase
  AND to_char(d.expected_close_date, 'YYYY-MM') = @target_month::text
GROUP BY p.phase, p.sort_order
ORDER BY p.sort_order
