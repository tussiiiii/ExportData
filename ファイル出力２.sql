SELECT '$(patient_no)_2.txt' AS file_name
, (SELECT A.patient_no, disease_name
    FROM ope_t A
        INNER JOIN dbo.disease_t disease
        ON disease.ope_t_id = A.id
    WHERE A.id = OT.id
    FOR JSON AUTO) AS file_data
FROM dbo.ope_t OT WHERE OT.id = '$(ope_t_id)'