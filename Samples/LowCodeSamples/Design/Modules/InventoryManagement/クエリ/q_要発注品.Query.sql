-- 現在庫 < 発注点 の商品×倉庫の組み合わせ（要発注品、inventory 直読み）
SELECT
    p.id              AS product_id,
    p.code            AS product_code,
    p.name            AS product_name,
    p.category        AS category,
    p.reorder_point   AS reorder_point,
    p.safety_stock    AS safety_stock,
    p.supplier_id     AS supplier_id,
    sup.name          AS supplier_name,
    w.id              AS warehouse_id,
    w.name            AS warehouse_name,
    inv.current_stock AS current_stock,
    p.reorder_point - inv.current_stock AS shortage,
    (p.reorder_point + COALESCE(p.safety_stock, 0)) - inv.current_stock AS recommended_quantity
FROM inventory inv
JOIN product p ON p.id = inv.product_id
JOIN warehouse w ON w.id = inv.warehouse_id
LEFT JOIN supplier sup ON sup.id = p.supplier_id
WHERE p.reorder_point IS NOT NULL
  AND inv.current_stock < p.reorder_point
  AND (NULLIF(@p_warehouse_id::text, '') IS NULL OR inv.warehouse_id = NULLIF(@p_warehouse_id::text, '')::integer)
  AND (NULLIF(@p_supplier_id::text,  '') IS NULL OR p.supplier_id   = NULLIF(@p_supplier_id::text,  '')::integer)
  AND (NULLIF(@p_code::text, '') IS NULL OR p.code LIKE '%' || @p_code::text || '%')
  AND (NULLIF(@p_name::text, '') IS NULL OR p.name LIKE '%' || @p_name::text || '%')
ORDER BY shortage DESC, p.code
