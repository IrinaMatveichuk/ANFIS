using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ANFIS
{
    public partial class Form2 : Form
    {
        int kolvo;

        public Form2(int text)
        {
            InitializeComponent();
            kolvo = text;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            if (kolvo%10==1) label1.Text = "Найден " + kolvo.ToString() + " файл без оценки.";
            else if (kolvo%10==2 || kolvo % 10 == 3 || kolvo % 10 == 4)
                label1.Text = "Найдено " + kolvo.ToString() + " файла без оценки.";
            else
                label1.Text = "Найдено " + kolvo.ToString() + " файлов без оценки.";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Data.Value = 1;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Data.Value = kolvo;
            this.Close();
        }
    }
}
