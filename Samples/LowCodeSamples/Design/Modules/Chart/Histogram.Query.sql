SELECT 
    CONCAT(FLOOR(quantity / 10) * 10, '-', (FLOOR(quantity / 10) * 10) + 9) AS range, -- 販売数量の範囲 (カテゴリ)
    COUNT(*) AS frequency                                                          -- 頻度 (件数)
FROM sales
GROUP BY FLOOR(quantity / 10)
ORDER BY FLOOR(quantity / 10);
