SELECT
  CASE
    WHEN t.status = '完了' THEN '完了'
    WHEN t.end_date < strftime('%Y/%m/%d', 'now', 'localtime') THEN '遅延'
    ELSE t.status
  END AS category_name,
  COUNT(*) AS task_count
FROM task t
WHERE @project_id IS NOT NULL
  AND t.project_id = @project_id
GROUP BY category_name
