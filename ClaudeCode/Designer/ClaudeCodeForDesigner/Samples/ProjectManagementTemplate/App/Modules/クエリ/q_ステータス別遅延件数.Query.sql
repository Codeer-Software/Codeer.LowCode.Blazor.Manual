SELECT
  p.name AS project_name,
  COALESCE(SUM(CASE WHEN t.status = '未着手' THEN 1 ELSE 0 END), 0) AS unstart_count,
  COALESCE(SUM(CASE WHEN t.status = '進行中' THEN 1 ELSE 0 END), 0) AS progress_count,
  COALESCE(SUM(CASE WHEN t.status = 'レビュー' THEN 1 ELSE 0 END), 0) AS review_count
FROM project p
LEFT JOIN task t ON t.project_id = p.id
  AND t.status != '完了'
  AND t.end_date < strftime('%Y/%m/%d', 'now', 'localtime')
WHERE @project_id IS NOT NULL
  AND p.id = @project_id
GROUP BY p.id, p.name