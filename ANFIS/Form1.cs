using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace ANFIS
{
    public partial class Form1 : Form
    {
        NeuralNetwork nn1, nn2, nn3;
        string connStr;
        string[] id;
        public int k, kolvo;
        double kRg, kTh, kDy, kEr, kWd;
        int group;

        private void buttonChange_Click(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "ANFIS_install.exe";
            proc.Start();
            proc.WaitForExit();//ожидания выполнения
            nn1 = new NeuralNetwork(5, 0.005, settingspath1);
            nn2 = new NeuralNetwork(5, 0.005, settingspath2);
            nn3 = new NeuralNetwork(5, 0.005, settingspath3);
        }

        private void buttonReport_Click(object sender, EventArgs e)
        {
            Form3 f3 = new Form3(connStr);
            f3.ShowDialog();
        }

        string settingspath1, settingspath2, settingspath3;
        int mark;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            settingspath1 = "ANFIS_settings_1.txt";
            settingspath2 = "ANFIS_settings_2.txt";
            settingspath3 = "ANFIS_settings_3.txt";
            buttonWork.Enabled = false;
            label5.Visible = false;
            buttonReport.Enabled = false;
            nn1 = new NeuralNetwork(5, 0.005, settingspath1);
            nn2 = new NeuralNetwork(5, 0.005, settingspath2);
            nn3 = new NeuralNetwork(5, 0.005, settingspath3);
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                connStr = "server=" + textBoxHost.Text + ";user=" + textBoxLogin.Text + ";port=" + textBoxPort.Text + ";password=" + textBoxPassword.Text + ";";

                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    conn.Close();
                }
                buttonWork.Enabled = true;
                buttonConnect.Enabled = false;
                label5.Visible = true;
                buttonReport.Enabled = true;
            }
            catch
            {
                MessageBox.Show("Проверьте правильность ввода параметров");
            }
        }

        private void buttonWork_Click(object sender, EventArgs e)
        {
            using (var conn = new MySqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "use QualityInfo;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "select count(1) from collection_file where quality_mark is null;";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    id = new string[Convert.ToInt32(reader[0])];
                reader.Close();

                cmd.CommandText = "select collection_id from collection_file where quality_mark is null;";
                reader = cmd.ExecuteReader();
                k = 0;
                while (reader.Read())
                {
                    id[k] = reader[0].ToString();
                    k++;
                }
                reader.Close();
                conn.Close();
            }
            if (k == 0) MessageBox.Show("Не найдено файлов без оценки");
            else
            {
                Form2 f2 = new Form2(k);
                f2.ShowDialog();
            }
            for(int i=0;i<Data.Value;i++)
            {
                MessageBox.Show("data.value   "+Data.Value.ToString());
                mark = GiveMark(connStr, Convert.ToInt32(id[i]));
                putMarkToDB(connStr, Convert.ToInt32(id[i]), mark);
                MessageBox.Show("Файлу поставлена оценка "+mark.ToString());
            }
        }

        public int GiveMark(string connStr, int id)
        {
            int paramid = 0, userid=0;
            using (var conn = new MySqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "use QualityInfo;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "select user_id from collection_file where collection_id = "+id+";";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    userid = Convert.ToInt32(reader[0]);
                reader.Close();
                cmd.CommandText = "select user_group from users where user_id = "+userid+";";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                    group = Convert.ToInt32(reader[0]);
                reader.Close();
                cmd.CommandText = "select parameter_id from parameter where collection_id = "+id+";";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                    paramid = Convert.ToInt32(reader[0]);
                reader.Close();
                cmd.CommandText = "select kRg from parameter where collection_id = "+paramid+";";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                    kRg = Convert.ToDouble(reader[0]);
                reader.Close();
                cmd.CommandText = "select kTh from parameter where collection_id = " + paramid + ";";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                    kTh = Convert.ToDouble(reader[0]);
                reader.Close();
                cmd.CommandText = "select kDy from parameter where collection_id = " + paramid + ";";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                    kDy = Convert.ToDouble(reader[0]);
                reader.Close();
                cmd.CommandText = "select kEr from parameter where collection_id = " + paramid + ";";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                    kEr = Convert.ToDouble(reader[0]);
                reader.Close();
                reader.Close();
                cmd.CommandText = "select kWd from parameter where collection_id = " + paramid + ";";
                reader = cmd.ExecuteReader();
                while (reader.Read())
                    kWd = Convert.ToDouble(reader[0]);
                reader.Close();
                conn.Close();
            }
            if (group == 1) return nn1.NetworkOutput(kRg, kTh, kDy, kEr, kWd);
            else if (group == 2) return nn2.NetworkOutput(kRg, kTh, kDy, kEr, kWd);
            else return nn3.NetworkOutput(kRg, kTh, kDy, kEr, kWd);
        }

        public void putMarkToDB(string connStr, int id, int mark)
        {
            using (var conn = new MySqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "use QualityInfo;";
                cmd.ExecuteNonQuery();
                MessageBox.Show("id  " + id.ToString());
                cmd.CommandText = "update collection_file set quality_mark = "+mark+" where collection_id = "+id+";";
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
