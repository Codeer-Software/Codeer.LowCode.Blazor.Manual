-- 今月が期限のタスクを 完了 / 未完了 に分類して件数を返す
-- SQLite の strftime は YYYY/MM/DD をパースできないため、string 比較で判定
SELECT '完了' AS category_name,
       COALESCE(SUM(CASE WHEN status = '完了' THEN 1 ELSE 0 END), 0) AS task_count
FROM task
WHERE substr(end_date, 1, 7) = strftime('%Y/%m', 'now', 'localtime')
UNION ALL
SELECT '未完了' AS category_name,
       COALESCE(SUM(CASE WHEN status != '完了' THEN 1 ELSE 0 END), 0) AS task_count
FROM task
WHERE substr(end_date, 1, 7) = strftime('%Y/%m', 'now', 'localtime')
