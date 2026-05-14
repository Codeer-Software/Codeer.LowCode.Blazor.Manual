INSERT INTO inventory(product_id, warehouse_id, current_stock, version, created_at, updated_at)
SELECT
  @product_id::integer,
  s.warehouse_id,
  -@quantity::integer,
  0,
  CURRENT_TIMESTAMP,
  CURRENT_TIMESTAMP
FROM shipping s
JOIN shipping_detail sd ON sd.shipping_id = s.id
WHERE sd.id = currval(pg_get_serial_sequence('shipping_detail', 'id'))
ON CONFLICT(product_id, warehouse_id)
DO UPDATE SET
  current_stock = inventory.current_stock + excluded.current_stock,
  version = inventory.version + 1,
  updated_at = excluded.updated_at;
