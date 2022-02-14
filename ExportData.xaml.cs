using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Text;

namespace ExportData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// フォルダ指定を行う
        /// </summary>
        private void GetFolderName_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new CommonOpenFileDialog("フォルダーの選択")
            {
                // 選択形式をフォルダースタイルにする IsFolderPicker プロパティを設定
                IsFolderPicker = true,
                InitialDirectory = @"C:\tmp"
            };

            // ダイアログを表示
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txOutPutFolder.Text = dialog.FileName;
            }

        }

        private void FileOutPut_Click(object sender, RoutedEventArgs e)
        {
            this.dbConnect();



        }
        public void dbConnect()
        {

            // 接続文字列の取得
            var connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            using (var command = connection.CreateCommand())
            {
                try
                {
                    // データベースの接続開始
                    connection.Open();

                    // 対象データ抽出SQLの実行
                    command.CommandText = @"SELECT subject_id, hadm_id FROM ADMISSIONS WHERE language = 'RUSS'";

                    // SqlDataAdapterで全行のデータをDataTableに読み込みます。
                    var dataAdapter = new SqlDataAdapter(command);
                    var dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        var queryStr = "SELECT '$(subject_id)_1.txt' AS file_name, curr_service AS file_data FROM dbo.SERVICES WHERE hadm_id = '$(hadm_id)'";
                        queryStr = queryStr.Replace("$(subject_id)", dataTable.Rows[i]["subject_id"].ToString());
                        queryStr = queryStr.Replace("$(hadm_id)", dataTable.Rows[i]["hadm_id"].ToString());

                        // ファイル出力SQLの実行
                        command.CommandText = queryStr;
                        var outPutDataAdapter = new SqlDataAdapter(command);
                        var outPutDataTable = new DataTable();
                        outPutDataAdapter.Fill(outPutDataTable);

                        // ファイル出力
                        string OutPutFilePath = Path.Combine(txOutPutFolder.Text, outPutDataTable.Rows[0]["file_name"].ToString());
                        using (StreamWriter sw = new StreamWriter(OutPutFilePath, false, Encoding.UTF8))
                        {
                            sw.WriteLine(outPutDataTable.Rows[0]["file_data"]);
                        }
                    }


                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    throw;
                }
                finally
                {
                    // データベースの接続終了
                    connection.Close();
                }
            }
        }
    }
}
