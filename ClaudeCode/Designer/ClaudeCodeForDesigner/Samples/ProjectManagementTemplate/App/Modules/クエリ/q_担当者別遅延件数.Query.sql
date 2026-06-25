SELECT
  m.name AS assignee_name,
  COUNT(*) AS task_count
FROM task t
LEFT JOIN members m ON m.id = t.assignee_member_id
WHERE @project_id IS NOT NULL
  AND t.project_id = @project_id
  AND t.status != '完了'
  AND t.end_date < strftime('%Y/%m/%d', 'now', 'localtime')
GROUP BY m.id, m.name
ORDER BY task_count DESC