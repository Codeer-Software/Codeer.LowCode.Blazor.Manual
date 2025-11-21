SELECT
  days AS received_date,
  COUNT(*) FILTER (WHERE i.category_id = 1) AS category_1,
  COUNT(*) FILTER (WHERE i.category_id = 2) AS category_2,
  COUNT(*) FILTER (WHERE i.category_id = 3) AS category_3,
  COUNT(*) FILTER (WHERE i.category_id = 4) AS category_4
FROM generate_series(
       @start_date,
       @end_date,
       INTERVAL '1 day'
     ) AS days
LEFT JOIN inquiries AS i
  ON i.received_date = days
GROUP BY days
ORDER BY days