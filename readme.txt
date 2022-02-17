### ExportData.exe.configについて
- sqlFileDir
    - sqlファイルのディレクトリパスを指定してください。指定がない場合は、ExportData.exeと同じフォルダとなります。

- defaultOutPutDir
    - デフォルト出力フォルダのディレクトリパスをして下さい。

### 対象データ抽出クエリについて
- 対象データ抽出.sql
    - 対象データを抽出するクエリのsqlファイルです。ファイル名は「対象データ抽出.sql」としてください。
    > SQLの例(対象データ抽出.sql)
    >> SELECT FROM dbo.ope_t
    SELECT ope_t_id, patient_no
    WHERE ope_on >= '$(from_date)' AND ope_on <= '$(to_date)'
    - 対応している置換文字列について
        - $(from_date)
            > 期間指定の開始日時
        - $(to_date)
            > 期間指定の終了日時


- ファイル出力.sql
    - 対象データ分のファイル出力を行うクエリのsqlファイルです。ファイル名は「ファイル出力*.sql」としてください。（複数可）
    > SQLの例（ファイル出力１.sql）
    >> SELECT '$(patient_no)_1.txt' AS file_name, xxx_data AS file_data
    FROM dbo.ope_xxx_t
    WHERE ope_t_id = '$(ope_t_id)'

    > SQLの例（ファイル出力２.sql）
    >> SELECT '$(patient_no)_2.txt' AS file_name, yyy_data AS file_data
    FROM dbo.ope_yyy_t
    WHERE ope_t_id = '$(ope_t_id)'

    - 対応している置換文字列について
        - $(patient_no)
            > 対象データ抽出で取得したpatient_no
        - $(ope_t_id)
            > 対象データ抽出で取得したope_t_id
        - $(datetime)
            > 現在日時（yyyyMMddHHmmss）
        - $(sequential_no)
            > 連番
