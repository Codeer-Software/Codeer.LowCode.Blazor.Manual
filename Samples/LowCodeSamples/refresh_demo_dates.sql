-- デモデータの日付を「今日」基準にずらす (PostgreSQL)。何度流してもよい。
-- demo_date_base に「データが今どの日を今日として作られているか」を持ち、その差だけずらす。
--   extras    : カレンダー / ガントチャート (月単位でずらす = 今月に予定が並ぶ)
--   sfa       : 営業支援 (週単位でずらす = 曜日を保つ)
--   pm        : プロジェクト管理 (週単位)
--   inventory : 在庫管理 (週単位)

CREATE TABLE IF NOT EXISTS demo_date_base (
    dataset   VARCHAR(50) PRIMARY KEY,
    base_date DATE NOT NULL
);

-- 初回だけ入る (元の ddl_*.txt のデータを作ったときの「今日」)
INSERT INTO demo_date_base (dataset, base_date) VALUES
    ('extras',    '2026-04-01'),
    ('sfa',       '2026-05-22'),
    ('pm',        '2026-05-10'),
    ('inventory', '2026-05-13')
ON CONFLICT (dataset) DO NOTHING;

DO $$
DECLARE
    m INT;
    d INT;
BEGIN
    -- カレンダー / ガントチャート
    SELECT (EXTRACT(YEAR FROM CURRENT_DATE) * 12 + EXTRACT(MONTH FROM CURRENT_DATE))
         - (EXTRACT(YEAR FROM base_date) * 12 + EXTRACT(MONTH FROM base_date))
      INTO m FROM demo_date_base WHERE dataset = 'extras';
    IF m <> 0 THEN
        UPDATE extras_calendar_event SET "start" = "start" + make_interval(months => m), "end" = "end" + make_interval(months => m);
        UPDATE extras_gantt_task     SET "start" = "start" + make_interval(months => m), "end" = "end" + make_interval(months => m);
        UPDATE demo_date_base SET base_date = (base_date + make_interval(months => m))::DATE WHERE dataset = 'extras';
    END IF;

    -- 営業支援 (SFA)
    SELECT ((CURRENT_DATE - base_date) / 7) * 7 INTO d FROM demo_date_base WHERE dataset = 'sfa';
    IF d <> 0 THEN
        UPDATE deal     SET expected_close_date = expected_close_date + d,
                            created_at = created_at + make_interval(days => d), updated_at = updated_at + make_interval(days => d);
        UPDATE activity SET activity_date_time = activity_date_time + make_interval(days => d),
                            created_at = created_at + make_interval(days => d), updated_at = updated_at + make_interval(days => d);
        UPDATE demo_date_base SET base_date = base_date + d WHERE dataset = 'sfa';
    END IF;

    -- プロジェクト管理
    SELECT ((CURRENT_DATE - base_date) / 7) * 7 INTO d FROM demo_date_base WHERE dataset = 'pm';
    IF d <> 0 THEN
        UPDATE project        SET start_date = start_date + d, end_date = end_date + d,
                                  created_at = created_at + make_interval(days => d), updated_at = updated_at + make_interval(days => d);
        UPDATE project_member SET joined_date = joined_date + d,
                                  created_at = created_at + make_interval(days => d), updated_at = updated_at + make_interval(days => d);
        UPDATE task           SET start_date = start_date + d, end_date = end_date + d,
                                  created_at = created_at + make_interval(days => d), updated_at = updated_at + make_interval(days => d);
        UPDATE demo_date_base SET base_date = base_date + d WHERE dataset = 'pm';
    END IF;

    -- 在庫管理
    SELECT ((CURRENT_DATE - base_date) / 7) * 7 INTO d FROM demo_date_base WHERE dataset = 'inventory';
    IF d <> 0 THEN
        UPDATE receiving      SET receiving_date = receiving_date + d,
                                  created_at = created_at + make_interval(days => d), updated_at = updated_at + make_interval(days => d);
        UPDATE shipping       SET shipping_date = shipping_date + d,
                                  created_at = created_at + make_interval(days => d), updated_at = updated_at + make_interval(days => d);
        UPDATE stocktaking    SET stocktake_date = stocktake_date + d,
                                  created_at = created_at + make_interval(days => d), updated_at = updated_at + make_interval(days => d);
        UPDATE purchase_order SET order_date = order_date + d, desired_delivery_date = desired_delivery_date + d,
                                  created_at = created_at + make_interval(days => d), updated_at = updated_at + make_interval(days => d);
        UPDATE demo_date_base SET base_date = base_date + d WHERE dataset = 'inventory';
    END IF;
END $$;
