-- 承認状況: 全フローの一覧。検索 (状態 / 申請者 / 申請種別) は出力列への通常検索としてフレームワークが WHERE を足す。
-- 現在担当者 = 現在ステップで Waiting のメンバー名 (GROUP_CONCAT は生成時に DB の方言へ置き換わる)。
-- 並び (申請日の降順) とページングはフレームワーク側 (QuerySortType/QueryPagingType = System) なので ORDER BY は書かない。
SELECT
  f.id AS id,
  f.status AS status,
  f.applicant AS applicant,
  u.name AS applicant_name,
  f.target_module_name AS target_module_name,
  f.target_id AS target_id,
  f.current_step_no AS current_step_no,
  (SELECT MIN(m.step_name) FROM approval_flow_members m
    WHERE m.flow_id = f.id::text AND m.attempt_no = f.attempt_no AND m.step_no = f.current_step_no) AS current_step_name,
  (SELECT STRING_AGG(u2.name, '、') FROM approval_flow_members m2
    JOIN app_users u2 ON u2.id::text = m2.approver_user
    WHERE m2.flow_id = f.id::text AND m2.attempt_no = f.attempt_no AND m2.step_no = f.current_step_no
      AND m2.status = 'Waiting') AS waiting_names,
  (SELECT MIN(h.acted_at) FROM approval_histories h
    WHERE h.flow_id = f.id::text AND h.attempt_no = f.attempt_no AND h.action = 'Submit') AS submitted_at,
  f.attempt_no AS attempt_no
FROM approval_flows f
LEFT JOIN app_users u ON u.id::text = f.applicant
