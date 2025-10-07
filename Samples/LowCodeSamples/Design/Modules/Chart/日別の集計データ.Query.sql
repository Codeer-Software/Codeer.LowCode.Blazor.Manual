-- 日別の実績と目標値を取得
SELECT 
    dates.date AS date,
    COALESCE(SUM(pr.quantity), 0) AS actual_quantity,
    COALESCE(SUM(pt.target_quantity), 0) AS target_quantity
FROM 
    (
        SELECT DISTINCT production_date AS date FROM production_records
        UNION
        SELECT DISTINCT target_date AS date FROM production_targets
    ) dates
LEFT JOIN production_records pr ON dates.date = pr.production_date
LEFT JOIN production_targets pt ON dates.date = pt.target_date
GROUP BY 
    dates.date
ORDER BY 
    dates.date;
