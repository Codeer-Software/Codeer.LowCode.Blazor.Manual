-- 商品 × 倉庫 ごとの現在庫（inventory テーブル直読み、楽観ロック対応）
SELECT
    p.id              AS product_id,
    p.code            AS product_code,
    p.name            AS product_name,
    p.category        AS category,
    p.unit_price      AS unit_price,
    p.reorder_point   AS reorder_point,
    p.safety_stock    AS safety_stock,
    p.supplier_id     AS supplier_id,
    sup.name          AS supplier_name,
    w.id              AS warehouse_id,
    w.name            AS warehouse_name,
    inv.current_stock AS current_stock,
    inv.current_stock * p.unit_price AS stock_amount
FROM inventory inv
JOIN product p ON p.id = inv.product_id
JOIN warehouse w ON w.id = inv.warehouse_id
LEFT JOIN supplier sup ON sup.id = p.supplier_id
WHERE (NULLIF(@p_warehouse_id::text, '') IS NULL OR inv.warehouse_id = NULLIF(@p_warehouse_id::text, '')::integer)
  AND (NULLIF(@p_supplier_id::text,  '') IS NULL OR p.supplier_id   = NULLIF(@p_supplier_id::text,  '')::integer)
  AND (NULLIF(@p_code::text, '') IS NULL OR p.code LIKE '%' || @p_code::text || '%')
  AND (NULLIF(@p_name::text, '') IS NULL OR p.name LIKE '%' || @p_name::text || '%')
ORDER BY w.id, p.code
