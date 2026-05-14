-- ダッシュボードの KPI 8 項目を 1 行で返す
-- total_stock_amount     : 総在庫金額（inventory.current_stock × product.unit_price の合計）
-- stock_out_count        : 在庫切れ件数（current_stock <= 0 の組合せ数）
-- reorder_count          : 要発注件数（inventory.current_stock < product.reorder_point の組合せ数）
-- month_receiving_count  : 今月の入庫件数
-- month_shipping_count   : 今月の出庫件数
-- pending_order_count    : 発注中件数（status='発注中' or '一部入庫'）
-- pending_order_amount   : 発注残金額（未完了発注の残数量×単価合計）
-- delayed_order_count    : 遅延発注件数（希望納期超過 かつ 未完了）
SELECT
    (SELECT COALESCE(SUM(inv.current_stock * p.unit_price), 0)
     FROM inventory inv
     JOIN product p ON p.id = inv.product_id) AS total_stock_amount,
    (SELECT COUNT(*)
     FROM inventory inv
     WHERE inv.current_stock <= 0) AS stock_out_count,
    (SELECT COUNT(*)
     FROM inventory inv
     JOIN product p ON p.id = inv.product_id
     WHERE p.reorder_point IS NOT NULL
       AND inv.current_stock < p.reorder_point) AS reorder_count,
    (SELECT COUNT(*) FROM receiving WHERE date_trunc('month', receiving_date) = date_trunc('month', CURRENT_DATE)) AS month_receiving_count,
    (SELECT COUNT(*) FROM shipping WHERE date_trunc('month', shipping_date) = date_trunc('month', CURRENT_DATE)) AS month_shipping_count,
    (SELECT COUNT(*) FROM purchase_order
     WHERE status IN ('発注中', '一部入庫')) AS pending_order_count,
    (SELECT COALESCE(SUM((COALESCE(pod.quantity, 0) - COALESCE(pod.received_quantity, 0)) * COALESCE(pod.unit_price, 0)), 0)
     FROM purchase_order_line pod
     JOIN purchase_order po ON po.id = pod.purchase_order_id
     WHERE po.status IN ('発注中', '一部入庫')) AS pending_order_amount,
    (SELECT COUNT(*) FROM purchase_order
     WHERE status IN ('発注中', '一部入庫')
       AND desired_delivery_date IS NOT NULL
       AND desired_delivery_date < CURRENT_DATE) AS delayed_order_count
