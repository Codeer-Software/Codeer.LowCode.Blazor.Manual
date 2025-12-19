SELECT
  g.name,
  COALESCE(SUM(sh.amount), 0) AS total_sales_amount
FROM goods AS g
LEFT JOIN sales_history sh
  ON sh.goods_code = g.goods_code
 AND sh.sales_date BETWEEN @start_date AND @end_date
GROUP BY
  g.goods_code, g.name
ORDER BY
  total_sales_amount DESC