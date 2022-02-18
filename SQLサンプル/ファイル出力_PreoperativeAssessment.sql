---------------------------------
-- <PreoperativeAssessment>ASA
---------------------------------
SELECT 
	'PreoperativeAssessment_$(ope_date_yyyymmdd)_$(ope_guid).json' AS file_name
	, (
		SELECT 
			CAST(ope_guid as varchar(36))  AS [_id]
			, 'ASA Physical Status Classification' AS [name.value]
			, 'Vipros_ASA_No' AS [name.mappings.target.terminology_id.value]
			, CAST(ope_guid as varchar(36))  AS [name.mappings.target.code_string]
			, 'openEHR-EHR-OBSERVATION.asa_status.v0' AS [archetype_node_id]
			, CAST(dbo.fnc_conv_name_uuid('JP-23|9900110|Vipros_ASA_No|' + CAST(ope_guid as varchar(36))) AS varchar(36)) + '::Philips.Japan.Vipros::1' AS [uid.value]
			, 'openEHR-EHR-OBSERVATION.asa_status.v0' AS [archetype_details.archetype_id.value]
			, '1.0.4' AS [archetype_details.rm_version]
			, 'Japan' AS [feeder_audit.originating_system_item_ids.issuer]
			, 'Philips.Japan' AS [feeder_audit.originating_system_item_ids.assigner]
			, '1010401025874' AS [feeder_audit.originating_system_item_ids.id]
			, 'Corporate Number' AS [feeder_audit.originating_system_item_ids.type]
			, 'ISO_639-1' AS [language.terminology_id.value]
			, 'ja' AS [language.code_string]
			, 'IANA_character-sets' AS [encoding.terminology_id.value]
			, 'UTF-8' AS [encoding.code_string]
			, 'NagoyaUniv_PT' AS [subject.external_ref.namespace]
			, 'PARTY' AS [subject.external_ref.type]
			, CAST(dbo.fnc_conv_name_uuid('NagoyaUniv_PT' + '|' +'$(patient_id)') AS varchar(36)) + '::Philips.Japan.Vipros::1' AS [subject.external_ref.id.value]
			, FORMAT(CAST('$(ope_t_date)' AS datetime), 'yyyy-MM-ddTHH:mm:ss+09:00') AS [data.origin.value]
			, (SELECT *
				FROM (
					SELECT 'Classification' AS [name.value]
						, 'at0001' AS [archetype_node_id]
						, LEFT(value, 1) AS [ordinal.value]
					UNION ALL
					SELECT 'Emergency (E)' AS [name.value]
						, 'at0002' AS [archetype_node_id]
						, 'true' AS [bool.value]
					WHERE RIGHT(value, 1) = 'E'
						) X
				FOR JSON PATH) AS [data.summary.items]
		FROM (
			SELECT TOP 1 *
			FROM VPSDB01.VIPROS_NAGOYA_UNIV_OPE.dbo.T_CITA_カンファレンス
			WHERE ope_guid = '$(ope_guid)'
			AND CITAカンファレンスID = '012570'
			ORDER BY history_idx DESC, data_idx DESC -- 履歴の1番新しいものを対象にする
		) TBL
		FOR JSON PATH
	) AS file_data
