SELECT '$(patient_no)_1.txt' AS file_name
, (SELECT A.patient_no, section.name
    FROM ope_t A
        INNER JOIN dbo.section_t B
        ON B.ope_t_id = A.id
        INNER JOIN dbo.section_m section
        ON section.id = B.section_m_id
    WHERE A.id = OT.id
    FOR JSON AUTO) AS file_data
FROM dbo.ope_t OT WHERE OT.id = '$(ope_t_id)'
