SELECT
	'OperativeEvent_$(ope_date_yyyymmdd)_$(ope_guid)_$(patient_id).xml' AS file_name
	, (
		SELECT
			CAST(ope_guid as varchar(36)) + '_' + FORMAT(data_time, 'yyyyMMddHHmmss') + '_' + CAST(data_idx AS varchar(16)) AS [id]	
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
				FOR XML PATH(''),TYPE).value('.','varchar(max)') AS [description.items]
		FROM (
			SELECT *
			FROM VPSDB01.VIPROS_NAGOYA_UNIV_OPE.dbo.T_OPE_イベント
			WHERE ope_guid = '$(ope_guid)'
		) TBL
		ORDER BY data_time, data_idx
		FOR XML PATH('ope'), ROOT('root')) AS file_data
