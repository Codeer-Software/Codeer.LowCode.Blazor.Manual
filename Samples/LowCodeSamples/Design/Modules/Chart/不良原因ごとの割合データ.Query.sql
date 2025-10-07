SELECT 
    dc.name AS defect_cause,
    SUM(dr.quantity) AS total_defects,
    ROUND(SUM(dr.quantity) * 100.0 / (SELECT SUM(quantity) FROM defect_records), 2) AS percentage
FROM 
    defect_records dr
JOIN 
    defect_causes dc ON dr.defect_cause_id = dc.id
GROUP BY 
    dc.name
ORDER BY 
    total_defects DESC;
