using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace مكتبة_الوقفية
{
    partial class FormWelcome : Form
    {
        public FormWelcome()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var f = new Form1())
            {
                f.ShowDialog(this);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var f = new FormSearch())
            {
                f.ShowDialog(this);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Program.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var f = new FormDownload())
            {
                f.ShowDialog(this);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.waqfeya.com/index.php");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "بسم الله الرحمن الرحيم\n الحمد لله الذي بنعمته تتم الصالحات، وصلى الله على نبينا محمد.\n\n" +
                "في عام 1433هـ قام الدكتور بقسم علوم الحاسب/ وائل الكيلاني بتكليف الطالب/ إبراهيم الكيلاني بعمل أداة تحميل ومزامنة للمكتبة الوقفية ليتمكن الدكتور من (تحميل جميع الكتب الجديدة من المكتبة بضغطة واحدة) كما قال. وتم عمل والحمد لله نسخة مبدئية منه في رمضان 1433هـ ثم وفَّق الله بنشره للمسلمين لينتفعوا به عام 1436هـ و تم أيضا إضافة بعض التحسينات"
                 + "\n\nشكر خاص لـ م/عمر أيمن على مساعداته التقنية"
                 + "\n\nهذا البرنامج مفتوح المصدر. يمكنك تحميل الكود المصدري من خلال الفيديو الخاص به على يوتوب", "حول", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
        }
    }
}
