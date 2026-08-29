-- 承認待ち: ログインユーザーが今待たれている承認メンバー行 (回覧の未確認も含む)。
-- current_user_id はサーバーが束縛する予約パラメータ (クライアントから変えられない)。
-- 並び (申請日の降順) とページングはフレームワーク側 (QuerySortType/QueryPagingType = System) なので ORDER BY は書かない。
SELECT
  m.id AS id,
  f.id AS flow_id,
  f.target_module_name AS target_module_name,
  f.target_id AS target_id,
  f.applicant AS applicant,
  u.name AS applicant_name,
  m.step_no AS step_no,
  m.step_name AS step_name,
  m.step_type AS step_type,
  (SELECT MIN(h.acted_at) FROM approval_histories h
    WHERE h.flow_id = f.id::text AND h.attempt_no = f.attempt_no AND h.action = 'Submit') AS submitted_at
FROM approval_flow_members m
JOIN approval_flows f ON f.id::text = m.flow_id AND f.attempt_no = m.attempt_no
LEFT JOIN app_users u ON u.id::text = f.applicant
WHERE m.approver_user = @current_user_id
  AND m.status = 'Waiting'
