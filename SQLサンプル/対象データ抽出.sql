SELECT A.ope_guid
	, B.Š³ÒID AS patient_id
	, A.èp“ú AS ope_date
	, FORMAT(A.èp“ú, 'yyyyMMdd') AS ope_date_yyyymmdd
FROM VPSDB01.VIPROS_NAGOYA_UNIV_OPE.dbo.T_OPE_èpî•ñ A
	INNER JOIN VPSDB01.VIPROS_NAGOYA_UNIV_OPE.dbo.T_COM_Š³ÒŠî–{ B
	ON B.patient_guid = A.patient_guid
WHERE A.èp“ú >= '$(from_date)' AND A.èp“ú <= '$(to_date)'