-- 直近6ヶ月の月次入庫量・出庫量
-- 月は 'YYYY-MM' 形式
SELECT month, in_qty, out_qty
FROM (
    SELECT
        month,
        SUM(in_qty) AS in_qty,
        SUM(out_qty) AS out_qty
    FROM (
        SELECT to_char(r.receiving_date, 'YYYY-MM') AS month, rd.quantity AS in_qty, 0 AS out_qty
        FROM receiving r JOIN receiving_detail rd ON rd.receiving_id = r.id
        UNION ALL
        SELECT to_char(s.shipping_date, 'YYYY-MM') AS month, 0 AS in_qty, sd.quantity AS out_qty
        FROM shipping s JOIN shipping_detail sd ON sd.shipping_id = s.id
    ) AS combined
    GROUP BY month
    ORDER BY month DESC
    LIMIT 6
) AS top6
ORDER BY month ASC
