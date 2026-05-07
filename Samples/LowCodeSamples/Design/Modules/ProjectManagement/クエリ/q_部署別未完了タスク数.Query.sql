-- 部署別の未完了タスク数（今月が終了日のタスクに絞って、担当者の所属部署で集計）
SELECT
  d.name AS department_name,
  COUNT(*) AS task_count
FROM task t
INNER JOIN members m ON t.assignee_member_id = m.id
INNER JOIN department d ON m.department_id = d.id
WHERE t.status <> '完了'
  AND date_trunc('month', t.end_date) = date_trunc('month', CURRENT_DATE)
GROUP BY d.id, d.name
ORDER BY task_count DESC
