using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using System.Windows.Threading;
using CommonLibrary;
using Newtonsoft.Json;
using System.Xml;

namespace ExportData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // 文字エンコードタイプ
        private string encodeIdType;
        private Encoding encoding;

        public MainWindow()
        {
            InitializeComponent();
            pbFileOutPut.Visibility = System.Windows.Visibility.Collapsed;
            string dir = ConfigurationManager.AppSettings["defaultOutPutDir"];
            encodeIdType = ConfigurationManager.AppSettings["encodeIdType"];
            switch (encodeIdType)
            {
                case CommonLibrary.Constants.EncodeIdType.SJIS:
                    encoding = Encoding.GetEncoding(CommonLibrary.Constants.FileEncodeType.SJIS);
                    break;
                case CommonLibrary.Constants.EncodeIdType.JIS:
                    encoding = Encoding.GetEncoding(CommonLibrary.Constants.FileEncodeType.JIS);
                    break;
                case CommonLibrary.Constants.EncodeIdType.UTF8:
                    encoding = Encoding.GetEncoding(CommonLibrary.Constants.FileEncodeType.UTF8);
                    break;
                case CommonLibrary.Constants.EncodeIdType.UTF8N:
                    encoding = Encoding.GetEncoding(CommonLibrary.Constants.FileEncodeType.UTF8N);
                    break;
                default:
                    encoding = Encoding.GetEncoding(CommonLibrary.Constants.FileEncodeType.SJIS);
                    break;
            }
            txOutPutFolder.Text = dir;
        }

        /// <summary>
        /// フォルダ指定を行う
        /// </summary>
        private void GetFolderName_Click(object sender, RoutedEventArgs e)
        {
            string dir;
            dir = ConfigurationManager.AppSettings["defaultOutPutDir"];
            // ダイアログのインスタンスを生成
            var dialog = new CommonOpenFileDialog("フォルダーの選択")
            {
                // 選択形式をフォルダースタイルにする IsFolderPicker プロパティを設定
                IsFolderPicker = true,
                InitialDirectory = @dir
            };

            // ダイアログを表示
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txOutPutFolder.Text = dialog.FileName;
            }

        }

        private void FileOutPut_Click(object sender, RoutedEventArgs e)
        {
            string targetPatientQueryStr;
            string[] outPutDataQueryStr;
            if (!InputCheck())
            {
                return;
            }
            // クエリ文字列の取得
            if (!QueryStrGet(out targetPatientQueryStr, out outPutDataQueryStr))
            {
                return;
            }
            // データファイル抽出
            ExportDataFile(targetPatientQueryStr, outPutDataQueryStr);
        }


        /// <summary>
        /// データファイル抽出
        /// </summary>
        public async void ExportDataFile(string targetPatientQueryStr, string[] outPutDataQueryStr)
        {

            // 接続文字列の取得
            var connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            using (var command = connection.CreateCommand())
            {
                try
                {
                    string queryStr;

                    // データベースの接続開始
                    connection.Open();

                    // 対象データ抽出SQLの実行
                    var strFromDate = dpFromDate.SelectedDate.ToString();
                    var strToDate = dpToDate.SelectedDate.ToString();
                    var strOutPutFolder = txOutPutFolder.Text;
                    queryStr = @targetPatientQueryStr;
                    queryStr = queryStr.Replace("$(from_date)", !string.IsNullOrEmpty(strFromDate) ? strFromDate : "1900/1/1");
                    queryStr = queryStr.Replace("$(to_date)", !string.IsNullOrEmpty(strToDate) ? strToDate : "2199/1/1");
                    command.CommandText = queryStr;
                    var dataAdapter = new SqlDataAdapter(command);
                    var dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("対象データ抽出結果が0件でした。", "info");
                        return;
                    }

                    pbFileOutPut.Visibility = System.Windows.Visibility.Visible;
                    // ファイル出力処理
                    await Task.Run(() => OutPutFile(outPutDataQueryStr, queryStr, dataTable, command, strOutPutFolder));

                    pbFileOutPut.Visibility = System.Windows.Visibility.Collapsed;
                    MessageBox.Show("ファイル出力が完了しました。");
                }
                catch (SqlException exception)
                {
                    MessageBox.Show(string.Concat("データベースアクセス中に例外が発生しました\n\n<内容>\n", exception.Message, "\n\n<実行クエリ>\n"), "例外発生");
                    throw;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(string.Concat("ファイル出力処理中に例外が発生しました\n\n<内容>\n", exception.Message), "例外発生");
                    throw;
                }
                finally
                {
                    // データベースの接続終了
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// ファイル出力処理
        /// </summary>
        private void OutPutFile(string[] outPutDataQueryStr, string queryStr, DataTable dataTable, SqlCommand command, string strOutPutFolder)
        {
            try
            {
                // 置換文字列の取得
                List<string> replaceColumns = new List<string>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    replaceColumns.Add(column.ColumnName);
                }

                for (int i = 0; i < outPutDataQueryStr.Length; i++)
                {
                    for (int j = 0; j < dataTable.Rows.Count; j++)
                    {
                        queryStr = @outPutDataQueryStr[i];
                        foreach (string replaceColumn in replaceColumns)
                        {
                            queryStr = queryStr.Replace(String.Concat("$(", replaceColumn, ")"), dataTable.Rows[j][replaceColumn].ToString());
                        }
                        // ファイル出力SQLの実行
                        command.CommandText = queryStr;
                        var outPutDataAdapter = new SqlDataAdapter(command);
                        var outPutDataTable = new DataTable();
                        outPutDataAdapter.Fill(outPutDataTable);

                        if (outPutDataTable.Rows[0]["file_data"].Equals(DBNull.Value))
                        {
                            // ログに出力してスキップ
                            File.AppendAllText(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "ExportData.log"), String.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,"), " [INFO ] ", "ファイル出力結果がNULLのため、ファイル生成を行いませんでした。 ", outPutDataTable.Rows[0]["file_name"]) + Environment.NewLine);
                            continue;
                        }

                        // ファイル出力
                        string OutPutFilePath = Path.Combine(strOutPutFolder, outPutDataTable.Rows[0]["file_name"].ToString());
                        string OutPutFileData;
                        string str = Path.GetExtension(@OutPutFilePath);
                        switch (Path.GetExtension(@OutPutFilePath))
                        {
                            case ".json":
                                // JSONファイル
                                OutPutFileData = FormatJson(outPutDataTable.Rows[0]["file_data"].ToString());
                                break;
                            case ".xml":
                                // XMLファイル
                                OutPutFileData = FormatXML(outPutDataTable.Rows[0]["file_data"].ToString(), encodeIdType);
                                break;
                            default:
                                // デフォルト
                                OutPutFileData = outPutDataTable.Rows[0]["file_data"].ToString();
                                break;
                        }
                        using (StreamWriter sw = new StreamWriter(OutPutFilePath, false, encoding))
                        {
                            //sw.WriteLineAsync(OutPutFileData);
                            //sw.WriteLine(OutPutFileData);
                            sw.Write(OutPutFileData);
                        }

                    }

                }

            }
            catch (Exception exception)
            {
                throw;
            }
        }

        /// <summary>
        /// クエリ文字列の取得
        /// </summary>
        /// <param name="targetPatientQueryStr">対象データ抽出.sqlの内容</param>
        /// <param name="outPutDataQueryStr">ファイル出力*.sql（複数ファイル）の内容の配列</param>
        /// <returns>bool</returns>
        public bool QueryStrGet(out string targetPatientQueryStr, out string[] outPutDataQueryStr)
        {
            var ret = true;
            var msg = "";
            string dir;
            string[] files;
            string[] str;
            // App.configにディレクトリパスの指定がある場合は取得
            dir = ConfigurationManager.AppSettings["sqlFileDir"];
            if (string.IsNullOrEmpty(dir))
            {
                // 上記がない場合、実行ファイルのフォルダパスを取得
                dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();
            }

            // 対象データ抽出.sqlの内容を取得
            files = Directory.GetFiles(@dir, "対象データ抽出*", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                msg = "対象データ抽出クエリがありません。" + "ディレクトリパス：" + dir;
                ret = false;
                targetPatientQueryStr = "";
                outPutDataQueryStr = new string[0];
                MessageBox.Show(msg, "事前チェック");
                return ret;
            }
            str = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                StreamReader sr = new StreamReader(files[i], encoding);
                str[i] = sr.ReadToEnd();
                sr.Close();
            }
            targetPatientQueryStr = str[0];

            //　ファイル出力*.sqlの内容を取得（複数ファイル）
            files = Directory.GetFiles(@dir, "ファイル出力*", SearchOption.TopDirectoryOnly);
            str = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                StreamReader sr = new StreamReader(files[i], encoding);
                str[i] = sr.ReadToEnd();
                sr.Close();
            }
            if (files.Length == 0)
            {
                msg = "ファイル出力クエリがありません。" + "ディレクトリパス：" + dir;
                ret = false;
                targetPatientQueryStr = "";
                outPutDataQueryStr = new string[0];
                MessageBox.Show(msg, "事前チェック");
                return ret;
            }
            outPutDataQueryStr = str;

            return ret;
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <returns>bool</returns>
        private bool InputCheck()
        {
            var ret = true;
            var msg = "";
            if (string.IsNullOrEmpty(txOutPutFolder.Text))
            {
                msg = "出力先のフォルダ指定を行って下さい。";
                ret = false;
            }
            if(!System.IO.Directory.Exists(txOutPutFolder.Text))
            {
                msg = "指定した出力先フォルダが存在しません。" + "ディレクトリパス：" + txOutPutFolder.Text;
                ret = false;
            }
            if (string.IsNullOrEmpty(dpFromDate.SelectedDate.ToString()))
            {
                msg = "期間指定の開始日付を選択して下さい。";
                ret = false;
            }
            if (!ret)
            {
                MessageBox.Show(msg, "入力チェック");
            }

            return ret;
        }

        /// <summary>
        /// JSONファイルの整形を行う
        /// </summary>
        /// <returns>bool</returns>
        private static string FormatJson(string jsonstr)
        {
            if (string.IsNullOrEmpty(jsonstr))
            {
                return null;
            }
            dynamic parsedJson = JsonConvert.DeserializeObject(jsonstr);
            return JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// XMLファイルの整形を行う
        /// </summary>
        /// <returns>bool</returns>
        private static string FormatXML(string xmlstr, string encodeIdType)
        {
            if (string.IsNullOrEmpty(xmlstr))
            {
                return null;
            }
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlstr);
            string xmlStrB;

            switch (encodeIdType)
            {
                case CommonLibrary.Constants.EncodeIdType.SJIS:
                    StringWriterSJIS writerSJIS = new StringWriterSJIS();
                    xml.Save(writerSJIS);
                    xmlStrB = writerSJIS.ToString();
                    writerSJIS.Close();
                    break;
                case CommonLibrary.Constants.EncodeIdType.JIS:
                    StringWriterJIS writerJIS = new StringWriterJIS();
                    xml.Save(writerJIS);
                    xmlStrB = writerJIS.ToString();
                    writerJIS.Close();
                    break;
                case CommonLibrary.Constants.EncodeIdType.UTF8:
                    StringWriterUTF8 writerUTF8 = new StringWriterUTF8();
                    xml.Save(writerUTF8);
                    xmlStrB = writerUTF8.ToString();
                    writerUTF8.Close();
                    break;
                case CommonLibrary.Constants.EncodeIdType.UTF8N:
                    StringWriterUTF8N writerUTF8N = new StringWriterUTF8N();
                    xml.Save(writerUTF8N);
                    xmlStrB = writerUTF8N.ToString();
                    writerUTF8N.Close();
                    break;
                default:
                    StringWriterSJIS writerDefaultSJIS = new StringWriterSJIS();
                    xml.Save(writerDefaultSJIS);
                    xmlStrB = writerDefaultSJIS.ToString();
                    writerDefaultSJIS.Close();
                    break;
            }

            return xmlStrB;
        }


        class StringWriterSJIS : System.IO.StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.GetEncoding(CommonLibrary.Constants.FileEncodeType.SJIS); }
            }
        }
        class StringWriterJIS : System.IO.StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.GetEncoding(CommonLibrary.Constants.FileEncodeType.JIS); }
            }
        }
        class StringWriterUTF8 : System.IO.StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.GetEncoding(CommonLibrary.Constants.FileEncodeType.UTF8); }
            }
        }
        class StringWriterUTF8N : System.IO.StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.GetEncoding(CommonLibrary.Constants.FileEncodeType.UTF8N); }
            }
        }
    }
}
