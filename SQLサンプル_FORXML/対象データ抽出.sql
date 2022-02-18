SELECT A.ope_guid
	, B.����ID AS patient_id
	, A.��p�� AS ope_date
	, FORMAT(A.��p��, 'yyyyMMdd') AS ope_date_yyyymmdd
FROM VPSDB01.VIPROS_NAGOYA_UNIV_OPE.dbo.T_OPE_��p��� A
	INNER JOIN VPSDB01.VIPROS_NAGOYA_UNIV_OPE.dbo.T_COM_���Ҋ�{ B
	ON B.patient_guid = A.patient_guid
WHERE A.��p�� >= '$(from_date)' AND A.��p�� <= '$(to_date)'