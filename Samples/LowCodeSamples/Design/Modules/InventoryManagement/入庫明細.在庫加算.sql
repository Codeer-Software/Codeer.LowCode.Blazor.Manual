INSERT INTO inventory(product_id, warehouse_id, current_stock, version, created_at, updated_at)
SELECT
  @product_id::integer,
  r.warehouse_id,
  @quantity::integer,
  0,
  CURRENT_TIMESTAMP,
  CURRENT_TIMESTAMP
FROM receiving r
JOIN receiving_detail rd ON rd.receiving_id = r.id
WHERE rd.id = currval(pg_get_serial_sequence('receiving_detail', 'id'))
ON CONFLICT(product_id, warehouse_id)
DO UPDATE SET
  current_stock = inventory.current_stock + excluded.current_stock,
  version = inventory.version + 1,
  updated_at = excluded.updated_at;
