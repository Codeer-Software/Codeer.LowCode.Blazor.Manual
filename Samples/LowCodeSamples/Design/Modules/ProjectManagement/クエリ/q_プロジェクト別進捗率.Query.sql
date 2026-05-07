SELECT
  p.name                                            AS project_name,
  COALESCE(CAST(AVG(t.progress_rate) AS INTEGER), 0) AS average_progress
FROM project p
LEFT JOIN task t ON p.id = t.project_id
GROUP BY p.id, p.name
ORDER BY p.id
