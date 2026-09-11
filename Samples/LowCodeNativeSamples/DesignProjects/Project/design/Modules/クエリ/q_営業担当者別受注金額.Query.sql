-- 営業担当者別の受注金額（@target_month の月に受注予定だった案件の expected_amount 合計、降順）
SELECT
  s.name AS salesperson_name,
  COALESCE(SUM(d.expected_amount), 0) AS total_amount
FROM salesperson s
LEFT JOIN deal d ON d.owner_salesperson_id = s.id
  AND d.status = '受注'
  AND to_char(d.expected_close_date, 'YYYY-MM') = @target_month::text
GROUP BY s.id, s.name
ORDER BY total_amount DESC, s.id ASC
