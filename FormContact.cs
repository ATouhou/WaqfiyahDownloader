using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;

namespace مكتبة_الوقفية
{
    public partial class FormContact : Form
    {
        public FormContact()
        {
            InitializeComponent();
        }

        private static void sendEmail(string subject, string body)
        {
            var fromAddress = new MailAddress(Encoding.UTF8.GetString(Convert.FromBase64String("eWVhdGVtcEBnbWFpbC5jb20=")), 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().FullName);
            var toAddress = new MailAddress(Encoding.UTF8.GetString(Convert.FromBase64String("aWJyYWhpbWFsa2lsYW5ueUBnbWFpbC5jb20=")),
                "Ibrahim Alkilanny");
            string fromPassword = Encoding.UTF8.GetString(Convert.FromBase64String("eWVhQHRlbXA="));
            
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text.Trim().Length < 3)
            {
                MessageBox.Show(this, "فضلا أدخل سبب المراسلة", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign);
                return;
            }
            if (richTextBox1.Text.Trim().Length < 5)
            {
                MessageBox.Show(this, "فضلا أدخل نص الرسالة بأحرف كافية", Text, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign);
                return;
            }
            string message = "من: " + textBox1.Text
                + "<br />الوظيفة: " + textBox2.Text
                + "<br />البريد الإلكتروني: " + textBox3.Text
                + "<br />السبب: " + comboBox1.Text
                + "نص الرسالة: <hr />" + richTextBox1.Text;
            try
            {
                sendEmail(comboBox1.Text, message);
                MessageBox.Show(this, "تم إرسال رسالتك بنجاح", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign);
                Close();
            }
            catch
            {
                MessageBox.Show(this, "فشل إرسال الرسالة. ربما توجد مشكلة في اتصالك بالإنترنت\nيرجى المحاولة لاحقا", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign);
                return;
            }
        }
    }
}
