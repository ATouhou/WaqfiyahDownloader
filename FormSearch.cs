using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace مكتبة_الوقفية
{
    public partial class FormSearch : Form
    {
        private class DropdownItem
        {
            public string value { get; set; }
            public string name { get; set; }
            public override string ToString()
            {
                return name;
            }
        }
        private class Book
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string Author { get; set; }
            public string Print { get; set; }
            public string Category { get; set; }
            public List<string> Urls { get; private set; }
            public Book()
            {
                Urls = new List<string>();
            }
        }
        private bool checkProgramatically = false;
        private List<Book> temp;
        private List<Book> all;
        private int bookCounter;
        private readonly Shared<bool> stop = new Shared<bool>();

        public FormSearch()
        {
            InitializeComponent();
            comboBox1.Items.Add(new DropdownItem() { value = "btags", name = "بحث عام" });
            comboBox1.Items.Add(new DropdownItem() { value = "btitle", name = "عنوان" });
            comboBox1.Items.Add(new DropdownItem() { value = "athid", name = "مؤلف" });
            comboBox1.Items.Add(new DropdownItem() { value = "verid", name = "محقق" });
            comboBox1.Items.Add(new DropdownItem() { value = "verid", name = "محقق" });
            comboBox1.Items.Add(new DropdownItem() { value = "binfo", name = "بطاقة" });
            comboBox1.Items.Add(new DropdownItem() { value = "btoc", name = "فهرس" });
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "بحث")
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show(this, "فضلا أدخل قيمة في مربع البحث", "خطأ", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                    return;
                }
                stop.Data = false;
                button1.Text = "إيقاف";
                treeView1.Nodes.Clear();
                toolStripProgressBar1.Visible = true;
                toolStripStatusLabel1.Text = "يتم البحث...";
                var qStr = "getword=" + (textBox1.Text)
                        + "&field=" + (comboBox1.SelectedItem as DropdownItem).value;
                Task.Factory.StartNew(() =>
                {
                    var encoding = Encoding.GetEncoding("windows-1256");
                    all = new List<Book>();
                    bookCounter = 1;
                    int st = 0;
                    while (!stop.Data)
                    {
                        var request = (HttpWebRequest)WebRequest.Create("http://waqfeya.com/search.php");
                        var data = encoding.GetBytes(qStr + "&st=" + st);
                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.ContentLength = data.Length;
                        using (var stream = request.GetRequestStream())
                            stream.Write(data, 0, data.Length);
                        string html;
                        using (var response = (HttpWebResponse)request.GetResponse())
                        using (var str = new StreamReader(response.GetResponseStream(), encoding))
                            html = str.ReadToEnd();
                        if (html.Contains("لم يُعثر على أية نتائج، جرب كلمات أخرى للبحث!") || html.Contains("لا توجد نتائج"))
                            break;
                        parseHtml(html);
                        st += 15;
                    }
                    //var reg = new Regex("<a href[=]\'search.php[?]getword=[^\\s]+[&]st[=]\\d+[&]field=\\w+\'><u>\\d+</u></a>", RegexOptions.IgnoreCase);
                    //var pages = reg.Matches(html);
                    //foreach (Match item in pages)
                    //{
                    //    var tmp = item.Value.IndexOf("=") + 2;
                    //    var url = item.Value.Substring(tmp, item.Value.IndexOf(">") - tmp - 2);
                    //    request = (HttpWebRequest)WebRequest.Create("http://waqfeya.com/" + url);
                    //    request.ContentType = "application/x-www-form-urlencoded";
                    //    using (var response = (HttpWebResponse)request.GetResponse())
                    //    using (var str = new StreamReader(response.GetResponseStream(), encoding))
                    //        html = str.ReadToEnd();
                    //    parseHtml(html);
                    //}
                }).ContinueWith(t =>
                {
                    button1.Enabled = true;
                    button1.Text = "بحث";
                    toolStripProgressBar1.Visible = false;
                    if (t.IsFaulted)
                        MessageBox.Show(this, "فشلت العملية. تأكد من اتصالك بالإنترنت");
                    else if (all.Count == 0)
                        MessageBox.Show(this, "لا يوجد نتائج");
                    //else Populate();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                stop.Data = true;
                button1.Enabled = false;
                button1.Text = "الإيقاف...";
            }
        }

        private string getString(string str)
        {
            return Uri.UnescapeDataString(str).Replace("&nbsp;", "");
        }

        private void parseHtml(string html)
        {
            int idx = 0;
            var search = "عنوان الكتاب:";
            while ((idx = html.IndexOf(search, idx)) >= 0)
            {
                idx += search.Length;
                Book b = new Book();
                int tmp;
                b.Name = (b.id = bookCounter++) + "- " +
                    getString(html.Substring(idx, (tmp = html.IndexOf("</li>", idx)) - idx)).Trim();
                var str = "القسم:";
                tmp = html.IndexOf(str, tmp);
                tmp = html.IndexOf(">", tmp) + 1;
                b.Category = getString(html.Substring(tmp, html.IndexOf("</a>", tmp) - tmp));
                tmp = html.IndexOf("<li>", tmp) + 4;
                b.Author = getString(html.Substring(tmp, html.IndexOf("</li>", tmp) - tmp));
                idx = html.IndexOf("</ul>", tmp);
                while ((tmp = html.IndexOf("لحين الاتصال", tmp)) >= 0)
                {
                    int temp = html.LastIndexOf("href", tmp);
                    temp += 6;
                    b.Urls.Add(html.Substring(temp, html.IndexOf("'", temp) - temp));
                    tmp += 5;
                }
                all.Add(b);
                Invoke(new Action(() =>
                {
                    toolStripStatusLabel1.Text = "تم العثور على " + all.Count + " نتيجة";
                    var yes = false;
                    Action<TreeNode, Book> add = (i, j) =>
                        i.Nodes.Add(j.Name, j.Name + " - " + j.Author + " - " + j.Print);
                    foreach (TreeNode item in treeView1.Nodes)
                    {
                        if (item.Text == b.Category)
                        {
                            add(item, b);
                            yes = true;
                            break;
                        }
                    }
                    if (!yes)
                    {
                        var parent = new TreeNode(b.Category);
                        add(parent, b);
                        treeView1.Nodes.Add(parent);
                    }
                }));
            }
        }

        private List<Book> SelectedBooks
        {
            get
            {
                temp = new List<Book>();
                for (int i = 0; i < treeView1.Nodes.Count; i++)
                {
                    getSelectedBooks(treeView1.Nodes[i]);
                }
                return temp;
            }
        }

        private void getSelectedBooks(TreeNode node)
        {
            if (string.IsNullOrEmpty(node.Name))
            {
                for (int i = 0; i < node.Nodes.Count; i++)
                {
                    getSelectedBooks(node.Nodes[i]);
                }
            }
            else if (node.Checked)
            {
                temp.Add(all.First(k => k.Name == node.Name));
            }
        }

        private void Populate()
        {
            treeView1.Nodes.Clear();
            foreach (var cat in all.GroupBy(k => k.Category))
            {
                var parent = new TreeNode(cat.Key);
                foreach (var item in cat)
                    parent.Nodes.Add(item.Name, 
                        item.Name + " - " + item.Author + " - " + item.Print);
                treeView1.Nodes.Add(parent);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var sel = SelectedBooks;
            if (sel.Count == 0)
            {
                MessageBox.Show(this, "لا يوجد عناصر مختارة لتحميلها", "خطأ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                return;
            }
            sel.ForEach(k =>
            {
                k.Name = k.Name.Substring(k.Name.IndexOf("-") + 2);
                k.Author = k.Author.Substring(k.Author.IndexOf(":") + 2);
            });
            //any books already added will be ignored when their files exist
            sel.ForEach(k =>
            {
                var id = Program.Setting.IdentityId++;
                Program.Setting.DownloadList.Add(new Settings.DownloadListItem()
                {
                    Book = k.Name,
                    Author = k.Author,
                    Category = k.Category,
                    AddDate = DateTime.Now,
                    Urls = k.Urls.ToArray(),
                    Id = id,
                    Order = id
                });
            });
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void checkAll(TreeNode node, bool val)
        {
            node.Checked = val;
            for (int i = 0; i < node.Nodes.Count; i++)
            {
                checkAll(node.Nodes[i], val);
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (checkProgramatically)
                return;
            checkProgramatically = true;
            checkAll(e.Node, e.Node.Checked);
            checkProgramatically = false;
        }
    }


    class Shared<T>
    {
        private T _t;
        private readonly object ox = new object();
        public T Data
        {
            get
            {
                try
                {
                    Monitor.Enter(ox);
                    return _t;
                }
                finally
                {
                    Monitor.Exit(ox);
                }
            }
            set
            {
                try
                {
                    Monitor.Enter(ox);
                    _t = value;
                }
                finally
                {
                    Monitor.Exit(ox);
                }
            }
        }
    }
}
