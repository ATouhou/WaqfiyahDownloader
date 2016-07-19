using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace مكتبة_الوقفية
{
    partial class FormDownload : Form
    {
        private readonly Shared<bool> started = new Shared<bool>();
        private object[] oxLocks;
        
        public FormDownload()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            toolStripComboBox1.SelectedIndex = 0;
        }

        private void toolStripButtonSetting_Click(object sender, EventArgs e)
        {
            using (var f = new FormSettings())
            {
                f.ShowDialog(this);
            }
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            var list = listView1.SelectedIndices.Cast<int>().ToList();
            list.Sort((i, j) => j - i);
            list.ForEach(k => listView1.Items.RemoveAt(k));
            updateDownloadList();
        }

        private void toolStripButtonUp_Click(object sender, EventArgs e)
        {
            var list = listView1.SelectedIndices.Cast<int>().ToList();
            list.Sort((i, j) => i - j);
            list.ForEach(k =>
            {
                if (k > 0)
                {
                    listView1.Items[k].Tag = (long)listView1.Items[k].Tag - 1;
                    listView1.Items[k - 1].Tag = (long)listView1.Items[k - 1].Tag + 1;
                    var item = listView1.Items[k];
                    listView1.Items.RemoveAt(k);
                    listView1.Items.Insert(k - 1, item);
                }
            });
            updateDownloadList();
        }

        private void toolStripButtonDown_Click(object sender, EventArgs e)
        {
            var list = listView1.SelectedIndices.Cast<int>().ToList();
            list.Sort((i, j) => j - i);
            list.ForEach(k =>
            {
                if (k < listView1.Items.Count - 1)
                {
                    listView1.Items[k].Tag = (long)listView1.Items[k].Tag + 1;
                    listView1.Items[k + 1].Tag = (long)listView1.Items[k + 1].Tag - 1;
                    var item = listView1.Items[k];
                    listView1.Items.RemoveAt(k);
                    listView1.Items.Insert(k + 1, item);
                }
            });
            updateDownloadList();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (started.Data)
            {
                if (MessageBox.Show(this, "هل أنت متأكد من إيقاف التحميل؟", "محمل المكتبة الوقفية",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign)
                == System.Windows.Forms.DialogResult.No)
                    e.Cancel = true;
                else
                    stopDownload();
            }
            base.OnClosing(e);
        }

        private void updateDownloadList()
        {
            var original = new List<Settings.DownloadListItem>(getList(toolStripComboBox1.SelectedIndex));
            var updated = listView1.Items.Cast<ListViewItem>().ToList();
            Program.Setting.DownloadList.RemoveAll(k => original.Exists(j => j == k) &&
                !updated.Exists(j => j.Name == k.Id.ToString()));
            (from k in original
             join j in updated on k.Id.ToString() equals j.Name
             where k.Order != (long)j.Tag
             select new { k, j })
             .ToList().ForEach(k => k.k.Order = (long)k.j.Tag);
        }

        private void wait(object ox)
        {
            Monitor.Enter(ox);
            Monitor.Wait(ox);
            Monitor.Exit(ox);
        }

        private void pulse(object ox)
        {
            Monitor.Enter(ox);
            Monitor.Pulse(ox);
            Monitor.Exit(ox);
        }

        private void downloadAsync(IEnumerable<Settings.DownloadListItem> list)
        {
            Action<long, string> progress = (id, val) =>
            {
                try
                {
                    Invoke(new Action(() =>
                        listView1.Items.Cast<ListViewItem>().First(k => k.Name == id.ToString()).SubItems[4].Text = val));
                }
                catch { }
            };
            Task.Factory.StartNew(() =>
            {
                var all = new ConcurrentQueue<Settings.DownloadListItem>(list); 
                int count = all.Count / Program.Setting.NumThreads;
                var tasks = new Task[Program.Setting.NumThreads];
                oxLocks = new object[Program.Setting.NumThreads];
                for (int i = 0; i < oxLocks.Length; i++)
                    oxLocks[i] = new object();
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = Task.Factory.StartNew(arg =>
                    {
                        Settings.DownloadListItem item = null;
                        var ox = this.oxLocks[(int)arg];
                        int idxUrl = 0;
                        int completed = 0;
                        string lastProgress = null;
                        string file = null;
                        using (var client = new WebClient())
                        {
                            client.DownloadFileCompleted += (k, j) =>
                            {
                                if (j.Cancelled)
                                {
                                    if (File.Exists(file))
                                        File.Delete(file);
                                }
                                else
                                {
                                    if (j.Error == null)
                                        ++completed;
                                    else if (File.Exists(file))
                                        File.Delete(file);
                                    pulse(ox);
                                }
                            };
                            client.DownloadProgressChanged += (k, j) =>
                            {
                                var tmp = string.Format("تحميل {0} ({1}%) من {2} ({3}%)", idxUrl + 1,
                                    j.ProgressPercentage, item.Urls.Length, 
                                    completed * 100 / item.Urls.Length + j.ProgressPercentage / item.Urls.Length);
                                if (tmp != lastProgress)
                                {
                                    progress(item.Id, tmp);
                                    lastProgress = tmp;
                                }
                            };
                            while (started.Data && all.TryDequeue(out item))
                            {
                                completed = 0;
                                var dir = getDir(item.Category, item.Book);
                                if (!Directory.Exists(dir))
                                    Directory.CreateDirectory(dir);
                                File.WriteAllText(Path.Combine(dir, "معلومات.txt"),
                                    "تم التحميل بواسطة محمل المكتبة الوقفية" + Environment.NewLine
                                    + "روابط الملفات:" + Environment.NewLine
                                    + string.Join(Environment.NewLine, item.Urls));
                                for (idxUrl = 0; idxUrl < item.Urls.Length && started.Data; ++idxUrl)
                                {
                                    file = getFile(dir, item.Urls[idxUrl]);
                                    if (!File.Exists(file))// if user stops before all Urls complete
                                    {
                                        try
                                        {
                                            client.DownloadFileAsync(new Uri(item.Urls[idxUrl]), file);
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                        wait(ox);
                                        if (!File.Exists(file)) // some error happened ??
                                            continue;
                                        else if (started.Data) // download completed
                                            item.Status = string.Format("اكتمل {0} من {1}", completed, item.Urls.Length);
                                        else client.CancelAsync(); // user has cancelled
                                    }
                                    else ++completed;
                                }
                                progress(item.Id, item.Status = completed == item.Urls.Length ? "تم التحميل عند " + DateTime.Now
                                    : string.Format("اكتمل {0} من {1}", completed, item.Urls.Length));
                            }
                        }
                    }, i);
                }
                Task.WaitAll(tasks);
            }).ContinueWith(t => {
                started.Data = false;
                startedUiChange();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private Func<string, int, string> limit = (i, j) => i.Length > j ? i.Substring(0, j) : i;

        private string getDir(string cat, string book)
        {
            return Path.Combine(Program.Setting.SaveFolder, limit(Program.PreparePath(cat), 25), limit(Program.PreparePath(book), 75));
        }

        private string getFile(string dir, string file)
        {
            return Path.Combine(dir, limit(Path.GetFileNameWithoutExtension(file), 50) + Path.GetExtension(file));
        }

        private void startedUiChange()
        {
            if (started.Data)
            {
                toolStrip1.Items.Cast<ToolStripItem>().ToList().ForEach(k => k.Enabled = false);
                contextMenuStrip1.Items.OfType<ToolStripMenuItem>().ToList().ForEach(k => k.Enabled = false);
                toolStripButtonStart.Enabled = true;
                فتحالمجلدالمحتويToolStripMenuItem.Enabled = true;
                toolStripButtonStart.Image = Properties.Resources.RecordRound_16xLG;
            }
            else
            {
                toolStrip1.Items.Cast<ToolStripItem>().ToList().ForEach(k => k.Enabled = true);
                contextMenuStrip1.Items.OfType<ToolStripMenuItem>().ToList().ForEach(k => k.Enabled = true);
                toolStripButtonStart.Image = Properties.Resources.arrow_run_16xLG;
            }
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
            if (!started.Data)
            {
                // always get non completed list, regardlress of user selection
                var list = getList(1);
                if (toolStripComboBox1.SelectedIndex == 2 || list.Count == 0)
                {
                    MessageBox.Show(this, "لا يوجد ملفات تنتظر التحميل في هذه القائمة", "معلومات", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                    return;
                }
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadString("http://www.waqfeya.com/index.php");
                    }
                }
                catch
                {
                    MessageBox.Show(this, "لا يمكن الاتصال بالمكتبة الوقفية. إما أن اتصال الإنترنت لديك غير متوفر أو أن خادم المكتبة يواجه بعض الصعوبات", "خطأ", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                    return;
                }
                started.Data = true;
                startedUiChange();
                downloadAsync(list);
            }
            else
            {
                stopDownload();
            }
        }

        private void stopDownload()
        {
            toolStripButtonStart.Enabled = false;
            started.Data = false;
            if (oxLocks != null)
                Parallel.ForEach(oxLocks, item => pulse(item));
        }

        private void فتحالمجلدالمحتويToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                var k = listView1.Items[listView1.SelectedIndices[0]];
                var item = Program.Setting.DownloadList.First(j => j.Id.ToString() == k.Name);
                var dir = getDir(item.Category, item.Book);
                if (Directory.Exists(dir))
                    Process.Start(dir);
                else
                    MessageBox.Show(this, "المجلد المطلوب غير موجود");
            }
        }

        private void إزالةToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButtonDelete_Click(sender, e);
        }

        private void تحريكلأعلىToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButtonUp_Click(sender, e);
        }

        private void تحريكلأسفلToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButtonDown_Click(sender, e);
        }

        private List<Settings.DownloadListItem> getList(int type)
        {
            switch (type)
            {
                case 0:
                    return Program.Setting.DownloadList.OrderBy(k => k.Order).ToList();
                case 1:
                    return Program.Setting.DownloadList
                        .Where(k => k.Status == null || !k.Status.StartsWith("تم التحميل")).OrderBy(k => k.Order).ToList();
                case 2:
                    return Program.Setting.DownloadList
                        .Where(k => k.Status != null && k.Status.StartsWith("تم التحميل")).OrderBy(k => k.Order).ToList();
                default:
                    throw new NotImplementedException();
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            getList(toolStripComboBox1.SelectedIndex).ForEach(k => listView1.Items.Add(new ListViewItem(new string[] {
                k.Book, k.Author, k.Category, k.AddDate.ToString(), k.Status
            }) { Tag = k.Order, Name = k.Id.ToString() }));
        }
    }
}
