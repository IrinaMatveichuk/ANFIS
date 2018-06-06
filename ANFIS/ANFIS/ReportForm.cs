using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Diagnostics;

namespace ANFIS
{
    public partial class ReportForm : Form
    {
        string report_start;
        string report_end;
        string connStr;
        int k;
        string[] kRg, kTh, kDy, kEr, kWd, group, UID, date, mark;
        string[] user_id, coll_id;
        int count;
        string path;
        int columns = 9;
        
        private Excel.Application m_objExcel = null;
        private Excel.Workbooks m_objBooks = null;
        private Excel._Workbook m_objBook = null;
        private Excel.Sheets m_objSheets = null;
        private Excel._Worksheet m_objSheet = null;
        private Excel.Range m_objRange = null;
        private Excel.Font m_objFont = null;
        private object m_objOpt = System.Reflection.Missing.Value;
        private object m_strSampleFolder = "D:\\ExcelData\\";
        Excel.Application excel = new Excel.Application();
        DateTime now;
        string filename;

        public ReportForm(string conn)
        {
            InitializeComponent();
            connStr = conn;
            path = "report.xlsx";
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            dateTimePicker1.CustomFormat = "dd.MM.yyyy";
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker2.CustomFormat = "dd.MM.yyyy";
            dateTimePicker2.Format = DateTimePickerFormat.Custom;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ReportCreate();
            if (count == 0) MessageBox.Show("Не найдено данных за выбранный период времени.");
            else WriteReportToFile();
        }

        public void CreateObj(int k)
        {

            kRg = new string[k];
            kTh = new string[k];
            kDy = new string[k];
            kEr = new string[k];
            kWd = new string[k];
            group = new string[k];
            UID = new string[k];
            date = new string[k];
            mark = new string[k];
            coll_id = new string[k]; ;
            user_id = new string[k];
        }

        public void ReportCreate()
        {
            report_start = dateTimePicker1.Value.ToShortDateString() + " 00:00:00";
            report_end = dateTimePicker2.Value.ToShortDateString() + " 00:00:00";

            using (var conn = new MySqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "use QualityInfo;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "select count(1) from collection_file WHERE collection_start BETWEEN " +
                    "STR_TO_DATE('" + report_start + "', '%d.%m.%Y %H:%i:%s') AND " +
                    "STR_TO_DATE('" + report_end + "', '%d.%m.%Y %H:%i:%s');";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    count = Convert.ToInt32(reader[0].ToString());
                reader.Close();

                CreateObj(count);

                cmd.CommandText = "select collection_id, collection_end, quality_mark, user_id from collection_file WHERE collection_start BETWEEN " +
                    "STR_TO_DATE('" + report_start + "', '%d.%m.%Y %H:%i:%s') AND " +
                    "STR_TO_DATE('" + report_end + "', '%d.%m.%Y %H:%i:%s');";
                reader = cmd.ExecuteReader();
                k = 0;
                while (reader.Read())
                {
                    coll_id[k] = reader[0].ToString();
                    date[k] = reader[1].ToString();
                    mark[k] = reader[2].ToString();
                    user_id[k] = reader[3].ToString();
                    k++;
                }
                reader.Close();

                for (int i = 0; i < count; i++)
                {
                    cmd.CommandText = "select kRg, kTh, kDy, kEr, kWd from parameter WHERE collection_id = '" + coll_id[i] + "';";
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        kRg[i] = reader[0].ToString();
                        kTh[i] = reader[1].ToString();
                        kDy[i] = reader[2].ToString();
                        kEr[i] = reader[3].ToString();
                        kWd[i] = reader[4].ToString();
                    }
                    reader.Close();

                    cmd.CommandText = "select user_uid, user_group from users WHERE user_id = '" + user_id[i] + "';";
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UID[i] = reader[0].ToString();
                        group[i] = reader[1].ToString();
                    }
                    reader.Close();
                }


                conn.Close();
            }
        }

        public void WriteReportToFile()
        {
            // Start a new workbook in Excel.
            m_objExcel = new Excel.Application();
            m_objBooks = (Excel.Workbooks)m_objExcel.Workbooks;
            m_objBook = (Excel._Workbook)(m_objBooks.Add(m_objOpt));
            m_objSheets = (Excel.Sheets)m_objBook.Worksheets;
            m_objSheet = (Excel._Worksheet)(m_objSheets.get_Item(1));

            // Create an array for the headers and add it to cells A1:C1.
            object[] objHeaders = { "UID", "Группа", "Дата", "Уровень использования", "Скорость", "Задержка", "Ошибки", "Временное окно", "Оценка" };
            m_objRange = m_objSheet.get_Range("A1", "I1");
            m_objRange.Value = objHeaders;
            m_objFont = m_objRange.Font;
            m_objFont.Bold = true;

            // Create an array and add it to the worksheet starting at cell A2.
            object[,] objData = new Object[count, columns];
            for (int r = 0; r < count; r++)
            {
                objData[r, 0] = UID[r];
                objData[r, 1] = group[r];
                objData[r, 2] = date[r];
                objData[r, 3] = kRg[r];
                objData[r, 4] = kTh[r];
                objData[r, 5] = kDy[r];
                objData[r, 6] = kEr[r];
                objData[r, 7] = kWd[r];
                objData[r, 8] = mark[r];
            }
            m_objRange = m_objSheet.get_Range("A2", m_objOpt);
            m_objRange = m_objRange.get_Resize(count, columns);
            m_objRange.Value = objData;

            m_objExcel.DisplayAlerts = false;
            now = DateTime.Now;
            filename = m_strSampleFolder + "report_" + now.ToString("dd/MM/yyyy_hh-mm-ss") + ".xlsx";
            File.Create(filename).Close();
            m_objBook.SaveAs(filename, m_objOpt, m_objOpt,
                m_objOpt, m_objOpt, m_objOpt, Excel.XlSaveAsAccessMode.xlNoChange,
                m_objOpt, m_objOpt, m_objOpt, m_objOpt);
            m_objExcel.DisplayAlerts = true;
            m_objBook.Close(false, m_objOpt, m_objOpt);
            m_objExcel.Quit();
            Process.Start(filename);
        }
    }
}
