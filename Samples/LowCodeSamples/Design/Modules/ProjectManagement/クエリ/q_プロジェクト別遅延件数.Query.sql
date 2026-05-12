SELECT
  p.name       AS project_name,
  COUNT(t.id)  AS task_count
FROM project p
LEFT JOIN task t ON p.id = t.project_id
  AND t.status <> '完了'
  AND t.end_date < CURRENT_DATE
GROUP BY p.id, p.name
ORDER BY p.id
