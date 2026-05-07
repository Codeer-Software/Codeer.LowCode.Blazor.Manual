-- 今月が期限のタスクを 完了 / 未完了 に分類して件数を返す（Donut 表示用）
SELECT '完了' AS category_name,
       COALESCE(SUM(CASE WHEN status = '完了' THEN 1 ELSE 0 END), 0) AS task_count
FROM task
WHERE date_trunc('month', end_date) = date_trunc('month', CURRENT_DATE)
UNION ALL
SELECT '未完了' AS category_name,
       COALESCE(SUM(CASE WHEN status <> '完了' THEN 1 ELSE 0 END), 0) AS task_count
FROM task
WHERE date_trunc('month', end_date) = date_trunc('month', CURRENT_DATE)
