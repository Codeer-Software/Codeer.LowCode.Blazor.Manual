WITH group_stats AS (
    SELECT 
        sample_group,
        AVG(measurement) AS sample_mean,
        MAX(measurement) - MIN(measurement) AS sample_range
    FROM measurement_samples
    GROUP BY sample_group
),
overall_stats AS (
    SELECT 
        AVG(sample_mean) AS x_bar,
        AVG(sample_range) AS R_bar
    FROM group_stats
),
control_limits AS (
    SELECT
        x_bar,
        R_bar,
        x_bar + 0.577 * R_bar AS xbar_ucl, -- A_2 = 0.577 (グループサイズ5の場合)
        x_bar - 0.577 * R_bar AS xbar_lcl,
        R_bar AS R_cl,
        2.114 * R_bar AS R_ucl,           -- D_4 = 2.114
        0 * R_bar AS R_lcl                -- D_3 = 0
    FROM overall_stats
)
SELECT 
    g.sample_group,
    g.sample_mean,
    g.sample_range,
    c.x_bar AS xbar_cl,
    c.xbar_ucl,
    c.xbar_lcl,
    c.R_cl,
    c.R_ucl,
    c.R_lcl
FROM group_stats g
CROSS JOIN control_limits c
ORDER BY g.sample_group;
