-- @target_month の活動履歴を担当者×種別で集計（活動0件の担当者も含む）
SELECT
  s.name AS salesperson_name,
  SUM(CASE WHEN a.type = '訪問'    THEN 1 ELSE 0 END) AS visit_count,
  SUM(CASE WHEN a.type = '電話'    THEN 1 ELSE 0 END) AS phone_count,
  SUM(CASE WHEN a.type = 'メール'  THEN 1 ELSE 0 END) AS email_count,
  SUM(CASE WHEN a.type = 'Web会議' THEN 1 ELSE 0 END) AS web_count
FROM salesperson s
LEFT JOIN activity a
  ON a.salesperson_id = s.id
  AND to_char(a.activity_date_time, 'YYYY-MM') = @target_month::text
GROUP BY s.id, s.name
ORDER BY (SUM(CASE WHEN a.type = '訪問' THEN 1 ELSE 0 END)
        + SUM(CASE WHEN a.type = '電話' THEN 1 ELSE 0 END)
        + SUM(CASE WHEN a.type = 'メール' THEN 1 ELSE 0 END)
        + SUM(CASE WHEN a.type = 'Web会議' THEN 1 ELSE 0 END)) DESC, s.id ASC
