using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace مكتبة_الوقفية
{
    partial class FormWelcome : Form
    {
        public FormWelcome()
        {
            InitializeComponent();
            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Text += " " + ver.Major + "." + ver.Minor;
        }

        class LatestVersionUpdateInfo
        {
            public bool IsLatestVersion;
            public Version LatestVersion;
            public DateTime PublishDate;
            public string DownloadUrl;
        }

        private static LatestVersionUpdateInfo isLatestVersion()
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                var json = client.DownloadString("http://api.github.com/repos/Mr-DDDAlKilanny/WaqfiyahDownloader/releases/latest");
                var str = "\"published_at\":";
                int idx = json.IndexOf(str);
                idx = json.IndexOf("\"", idx + str.Length) + 1;
                var idx2 = json.IndexOf("\"", idx);
                var publishedAt = DateTime.Parse(json.Substring(idx, idx2 - idx).Substring(0, 10));

                str = "\"browser_download_url\":";
                idx = json.IndexOf(str);
                idx = json.IndexOf("\"", idx + str.Length) + 1;
                idx2 = json.IndexOf("\"", idx);
                var downloadUrl = json.Substring(idx, idx2 - idx);

                str = "\"tag_name\":";
                idx = json.IndexOf(str);
                idx = json.IndexOf("\"", idx + str.Length) + 1;
                idx2 = json.IndexOf("\"", idx);
                var tagName = json.Substring(idx, idx2 - idx);
                tagName = tagName.Substring(tagName.IndexOf("v") + 1);
                var v = tagName.Split('.');
                var latestVersion = new Version(int.Parse(v[0]), int.Parse(v[1]));
                return new LatestVersionUpdateInfo
                {
                    IsLatestVersion = latestVersion <= System.Reflection.Assembly.GetExecutingAssembly().GetName().Version,
                    PublishDate = publishedAt,
                    DownloadUrl = downloadUrl,
                    LatestVersion = latestVersion
                };
            }
        }

        private void updateToLatestVersion(string downloadUrl)
        {
            var tmp = Path.GetTempFileName();
            using (var client = new WebClient())
                client.DownloadFile(downloadUrl, tmp);
            string baseDir = Path.Combine(Path.GetTempPath(),
                "release-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, true);
            Directory.CreateDirectory(baseDir);
            ZipConstants.DefaultCodePage = 720;
            using (var zip = new ZipInputStream(File.OpenRead(tmp)))
            {
                ZipEntry theEntry;
                while ((theEntry = zip.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(Path.Combine(baseDir, directoryName));
                    if (fileName != string.Empty)
                        using (var streamWriter = File.Create(Path.Combine(baseDir, theEntry.Name)))
                        {
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = zip.Read(data, 0, data.Length);
                                if (size > 0)
                                    streamWriter.Write(data, 0, size);
                                else break;
                            }
                        }
                }
            }
            File.Delete(tmp);
            if (baseDir.EndsWith("\\"))
                baseDir = baseDir.Substring(0, baseDir.Length - 1);
            var exlFile = Path.Combine(baseDir, "excluded.txt");
            var cmdFile = Path.Combine(baseDir, "update.cmd");
            File.WriteAllText(exlFile, cmdFile + Environment.NewLine + exlFile);
            string batchFile = "@echo off" + Environment.NewLine +
                "chcp 65001" + Environment.NewLine +
                "taskkill /pid " + Process.GetCurrentProcess().Id + " /f" + Environment.NewLine +
                "xcopy \"" + baseDir + "\\*\" \"" + Application.StartupPath + "\" /E /C /I /Q /H /R /Y"
                + (exlFile.Contains(" ") ? "" : " /exclude:" + exlFile)
                + Environment.NewLine + "start \"\" \"" + Application.ExecutablePath + "\" \"--updated\"";
            File.WriteAllText(cmdFile, batchFile);
            Process.Start(cmdFile);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (Program.Updated)
            {
                MessageBox.Show(this, "تم تحديث البرنامج إلى آخر إصدار", Text, MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
            }
            else
            Task<LatestVersionUpdateInfo>.Factory.StartNew(() =>
            {
                return isLatestVersion();
            })
            .ContinueWith(t =>
            {
                if (t.IsFaulted) return;
                if (!t.Result.IsLatestVersion && MessageBox.Show(this, "يتوفر إصدار أحدث " 
                    + t.Result.LatestVersion + " بتاريخ " + t.Result.PublishDate.ToString("yyyy-MM-dd") 
                    + "\nتحديث البرنامج الآن؟", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign) == DialogResult.Yes)
                {
                    using (var f = new FormUpdate())
                        f.StartUpdate(() => updateToLatestVersion(t.Result.DownloadUrl), this);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
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
                 + "\n\nهذا البرنامج مفتوح المصدر. يمكنك المشاركة في تطويره عبر الكود المصدري الموجود على الفيديو الخاص به على يوتوب", "حول", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
        }
    }
}
