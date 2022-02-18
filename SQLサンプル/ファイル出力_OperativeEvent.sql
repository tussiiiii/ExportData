SELECT
	'OperativeEvent_$(ope_date_yyyymmdd)_$(ope_guid).json' AS file_name
	, (
		SELECT
			CAST(ope_guid as varchar(36)) + '_' + FORMAT(data_time, 'yyyyMMddHHmmss') + '_' + CAST(data_idx AS varchar(16)) AS [_id]
			, 'Procedure' AS [name.value]
			, 'Vipros_Procedure_No' AS [name.mappings.target.terminology_id.value]
			, CAST(ope_guid as varchar(36)) + '_' + FORMAT(data_time, 'yyyyMMddHHmmss') + '_' + CAST(data_idx AS varchar(16)) AS [name.mappings.target.code_string]
			, 'openEHR-EHR-ACTION.procedure.v1' AS [archetype_node_id]
			, CAST(dbo.fnc_conv_name_uuid('JP-23|9900110|Vipros_Procedure_No|' + CAST(ope_guid as varchar(36)) + '_' + FORMAT(data_time, 'yyyyMMddHHmmss') + '_' + CAST(data_idx AS varchar(16))) AS varchar(36)) + '::Philips.Japan.Vipros::1' AS [uid.value]
			, 'openEHR-EHR-ACTION.procedure.v1'AS [archetype_details.archetype_id.value]
			, '1.0.4' AS [archetype_details.rm_version]
			, 'Japan' AS [feeder_audit.originating_system_item_ids.issuer]
			, 'Philips.Japan' AS [feeder_audit.originating_system_item_ids.assigner]
			, '1010401025874' AS [feeder_audit.originating_system_item_ids.id]
			, 'Corporate Number' AS [feeder_audit.originating_system_item_ids.type]
			, 'Philips.Japan.Vipros' AS [feeder_audit.originating_system_audit.system_id]
			, 'NagoyaUniv_PT' AS [feeder_audit.originating_system_audit.subject.external_ref.namespace]
			, 'PARTY' AS [feeder_audit.originating_system_audit.subject.external_ref.type]
			, CAST(dbo.fnc_conv_name_uuid('NagoyaUniv_PT' + '|' + '$(patient_id)') AS varchar(36)) + '::Philips.Japan.Vipros::1' AS [feeder_audit.originating_system_audit.subject.external_ref.id.value]
			, 'NagoyaUniv_USER' AS [feeder_audit.originating_system_audit.provider.external_ref.namespace]
			, 'PARTY' AS [feeder_audit.originating_system_audit.provider.external_ref.type]
			, '' AS [feeder_audit.originating_system_audit.provider.external_ref.id.value]
			, 'ISO_639-1' AS [language.terminology_id.value]
			, 'ja' AS [language.code_string]
			, 'IANA_character-sets' AS [encoding.terminology_id.value]
			, 'UTF-8' AS [encoding.code_string]
			, 'NagoyaUniv_PT' AS [subject.external_ref.namespace]
			, 'PARTY' AS [subject.external_ref.type]
			, CAST(dbo.fnc_conv_name_uuid('NagoyaUniv_PT' + '|' + '$(patient_id)') AS varchar(36)) + '::Philips.Japan.Vipros::1' AS [subject.external_ref.id.value]
			, 'NagoyaUniv_USER' AS [provider.external_ref.namespace]
			, 'PARTY' AS [provider.external_ref.type]
			, ''AS [provider.external_ref.id.value]
			, (SELECT *
				FROM (
					SELECT 'Procedure name' AS [name.value]
						, 'at0002' AS [archetype_node_id]
						, イベント名 AS [text.value]
						, 'NagoyaUniv_OpeEvent' AS [text.mappings.target.terminology_id.value]
						, イベントID AS [text.mappings.target.code_string]
						, NULL AS [date_time.value]
					UNION ALL
					SELECT 'Final end date/time' AS [name.value]
						, 'at0060' AS [archetype_node_id]
						, NULL AS [text.value]
						, NULL AS [text.mappings.target.terminology_id.value]
						, NULL AS [text.mappings.target.code_string]
						, FORMAT(data_time, 'yyyy-MM-ddTHH:mm:ss+09:00') AS [date_time.value]
						) X
				FOR JSON PATH) AS [description.items]
		FROM (
			SELECT *
			FROM VPSDB01.VIPROS_NAGOYA_UNIV_OPE.dbo.T_OPE_イベント
			WHERE ope_guid = '$(ope_guid)'
		) TBL
		ORDER BY data_time, data_idx
		FOR JSON PATH) AS file_data
