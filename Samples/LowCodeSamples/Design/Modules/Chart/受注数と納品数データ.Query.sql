-- 日ごとの全製品の注文数と納品数を集計
SELECT 
    COALESCE(o.order_date, d.delivery_date) AS date,
    COALESCE(SUM(o.order_quantity), 0) AS total_orders,
    COALESCE(SUM(d.delivery_quantity), 0) AS total_deliveries
FROM 
    (SELECT DISTINCT order_date FROM orders
     UNION
     SELECT DISTINCT delivery_date FROM deliveries) dates
LEFT JOIN orders o ON dates.order_date = o.order_date
LEFT JOIN deliveries d ON dates.order_date = d.delivery_date
GROUP BY 
    date
ORDER BY 
    date;
