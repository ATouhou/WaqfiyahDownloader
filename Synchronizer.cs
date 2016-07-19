using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Text;

namespace مكتبة_الوقفية
{
	internal class Synchronizer : IDisposable
	{
        private int threadCount
        {
            get { return Program.Setting.NumThreads; }
        }

		private Synchronizer.ReportProgressCallback callback;

		private Point index;

		private Point progress;

		private WebBrowser browser;

		private List<Category> categories;

		private List<Book> toBeVerified;

		private WebClient client;

		public Synchronizer.Status CurrentStatus
		{
			get;
			private set;
		}

		public Synchronizer(WebBrowser br, Synchronizer.ReportProgressCallback c)
		{
			this.callback = new Synchronizer.ReportProgressCallback(c.Invoke);
			this.browser = br;
			this.browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.browser_DocumentCompleted);
			this.client = new WebClient();
			this.client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.client_DownloadProgressChanged);
			this.client.DownloadFileCompleted += new AsyncCompletedEventHandler(this.Client_DownloadFileCompleted);
		}

		private void addBooksInCurrentCategory(Category cat, MatchCollection links, List<int> titles, string all)
		{
			int num = 0;
			titles.Add(all.Length);
			for (int i = 0; i < titles.Count - 1; i++)
			{
				int item = titles[i] + "عنوان الكتاب: ".Length;
				int num1 = all.IndexOf('<', item + 1);
				string str = all.Substring(item, num1 - item).Trim();
                item = all.IndexOf("المؤلف:", num1) + "المؤلف:".Length + 1;
                num1 = all.IndexOf("<", item + 1);
                var auth = all.Substring(item, num1 - item);
				List<string> strs = new List<string>();
				bool flag = false;
				while (true)
				{
					if ((num >= links.Count ? true : links[num].Index >= titles[i + 1]))
					{
						break;
					}
					string value = links[num].Value;
					item = value.IndexOf("href='") + "href='".Length;
					string str1 = value.Substring(item, Math.Min(value.IndexOf('>', item) - 1, value.IndexOf("'", item)) - item);
					if (!this.isFileLink(str1))
					{
						flag = true;
					}
					strs.Add(str1);
					num++;
				}
                Book book = new Book(str, strs) { Author = auth };
				cat.Books.Add(book);
				if (flag)
				{
					this.toBeVerified.Add(book);
				}
			}
		}

		private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			switch (this.CurrentStatus)
			{
				case Synchronizer.Status.Connecting:
				{
					this.CurrentStatus = Synchronizer.Status.AddingCategories;
					this.runWork();
					break;
				}
				case Synchronizer.Status.AddingCategories:
				{
					this.CurrentStatus = Synchronizer.Status.AddingPages;
					this.runWork();
					break;
				}
				case Synchronizer.Status.AddingPages:
				{
					if (this.index.X >= this.categories.Count)
					{
						this.CurrentStatus = Synchronizer.Status.AddingBooks;
						this.index = new Point();
					}
					this.runWork();
					break;
				}
				case Synchronizer.Status.AddingBooks:
				{
					if (this.index.X >= this.categories.Count)
					{
						this.CurrentStatus = Synchronizer.Status.VerifyingBookLinks;
						this.index = new Point();
					}
					this.runWork();
					break;
				}
				case Synchronizer.Status.VerifyingBookLinks:
				{
					this.runWork();
					break;
				}
			}
		}

		private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				this.callback(0, "فشل تحميل الملف");
				this.callback(0, e.Error.Message);
			}
		}

		private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			int bytesReceived = (int)((double)e.BytesReceived * 1 / (double)e.TotalBytesToReceive * 100);
			this.callback(4096, (bytesReceived < 0 || bytesReceived > 100 ? 0 : bytesReceived));
		}

		public void Dispose()
		{
			this.client.Dispose();
		}

		private void addFilesToDownloadList()
		{
			//this.progress = new Point();
			//this.callback(0, "يتم الآن مراجعة الملفات...");
            //foreach (Category category in this.categories)
            //{
            //    if (!Directory.Exists(category.FolderPath))
            //    {
            //        Directory.CreateDirectory(category.FolderPath);
            //    }
            //    for (int i = category.Books.Count - 1; i >= 0; i--)
            //    {
            //        if (Directory.Exists(category.Books[i].FolderPath))
            //        {
            //            category.Books.RemoveAt(i);
            //        }
            //        else if (Program.Setting.DownloadRestriction)
            //        {
            //            for (int j = category.Books[i].URLs.Count - 1; j >= 0; j--)
            //            {
            //                if (this.getFileSize(category.Books[i].URLs[j]) > (long)(Program.Setting.MaxMegaBytesDownload * 1024 * 1024))
            //                {
            //                    category.Books[i].URLs.RemoveAt(j);
            //                }
            //            }
            //        }
            //    }
            //    this.progress.Y = this.progress.Y + category.Books.Count;
            //}
            this.callback(Form1.ChooseBooksMsg, this.categories);
            Application.DoEvents();
            while (Form1.UserHasChoosedCategories == 1)
            {
            }
            while (Form1.UserHasChoosedCategories == 0)
            {
            }
            if (Form1.UserHasChoosedCategories == -1)
                return;
            var books = Form1.SelectedBooks;
            books.ForEach(k =>
            {
                var id = Program.Setting.IdentityId++;
                Program.Setting.DownloadList.Add(new Settings.DownloadListItem()
                {
                    Book = k.Title,
                    Author = k.Author,
                    Category = categories.First(j => j.Books.Contains(k)).Name,
                    Id = id,
                    Order = id,
                    Urls = k.URLs.ToArray(),
                    AddDate = DateTime.Now
                });
            });
            //int cnt = 0;
            //foreach (Book book in books)
            //{
            //    cnt++;
            //    Directory.CreateDirectory(book.FolderPath);
            //    foreach (string uRL in book.URLs)
            //    {
            //        File.WriteAllText(Path.Combine(book.FolderPath, "معلومات.txt"),
            //            "تم التحميل بواسطة محمل المكتبة الوقفية" + Environment.NewLine
            //            + "روابط الملفات:" + Environment.NewLine
            //            + string.Join(Environment.NewLine, book.URLs.ToArray()));
            //        callback(cnt * 100 / books.Count, string.Concat("يتم الآن تحميل الملف ", Path.GetFileName(uRL), " لكتاب ", book.Title));
            //        this.downloadCompleted = false;
            //        this.client.DownloadFileAsync(new Uri(uRL), Path.Combine(book.FolderPath, Path.GetFileName(uRL)));
            //        while (!this.downloadCompleted)
            //        {
            //        }
            //    }
            //}
		}

		private long getFileSize(string url)
		{
			long num;
			try
			{
				Stream stream = this.client.OpenRead(url);
				try
				{
					num = long.Parse(this.client.ResponseHeaders["Content-Length"]);
				}
				finally
				{
					if (stream != null)
					{
						((IDisposable)stream).Dispose();
					}
				}
			}
			catch
			{
				num = (long)-1;
			}
			return num;
		}

		private bool isFileLink(string url)
		{
			bool flag;
			int length = url.Length - 1;
			while (true)
			{
				if (length < 0)
				{
					flag = false;
					break;
				}
				else if (url[length] == '.')
				{
					flag = true;
					break;
				}
				else if (url[length] != '?')
				{
					length--;
				}
				else
				{
					flag = false;
					break;
				}
			}
			return flag;
		}

		private Category parseCategory(string val)
		{
			int num = val.IndexOf("cid=") + "cid=".Length;
			int num1 = num + 1;
			while (char.IsDigit(val[num1]))
			{
				num1++;
			}
			string str = val.Substring(num, num1 - num);
			num = val.IndexOf('>') + 1;
			num1 = val.IndexOf('<', num);
			string str1 = val.Substring(num, num1 - num);
			return new Category(str1, str);
		}

        private void addPages()
        {
            var threads = new Task[threadCount];
            var size = categories.Count / threadCount;
            for (int th = 0; th < threadCount; th++)
            {
                threads[th] = Task.Factory.StartNew((st) =>
                {
                    var ss = (int)st;
                    int myStart = ss * size;
                    int myEnd = ss == threadCount - 1 ? categories.Count : (ss + 1) * size;
                    for (int j = myStart; j < myEnd; j++)
                    {
                        var client = new WebClient();
                        try
                        {
                            client.Encoding = Encoding.GetEncoding("windows-1256");
                            string s = null;
                            using (var data = client.OpenRead(categories[j].Pages[0]))
                            using (var reader = new StreamReader(data, Encoding.GetEncoding("windows-1256"), true))
                                s = reader.ReadToEnd();
                            var regex2 = new Regex(string.Concat("<A href[=]\'category[.]php[?]cid[=]",
                                this.categories[j].Id, "[&]st=[0-9]+\'><U>[0-9]+</U></A>"), 
                                RegexOptions.IgnoreCase);
                            var matchCollections2 = regex2.Matches(s);
                            for (int i = 0; i < matchCollections2.Count; i++)
                            {
                                var num = matchCollections2[i].Value.IndexOf("href='") + "href='".Length;
                                int num2 = matchCollections2[i].Value.IndexOf('\'', num + 1);
                                string str = matchCollections2[i].Value.Substring(num, num2 - num);
                                //str = str.Remove(str.IndexOf("amp;"), 4);
                                categories[j].Pages.Add(string.Concat("http://www.waqfeya.com/", str));
                            }
                            this.callback(0, string.Concat("تم استكشاف الصفحات لقسم ", this.categories[j].Name));
                        }
                        catch
                        {
                            // ignore for now
                        }
                        client.Dispose();
                    }
                }, th);
            }
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Wait();
            }
        }

        private void addBooks()
        {
            var threads = new Task[threadCount];
            var size = categories.Count / threadCount;
            for (int th = 0; th < threadCount; th++)
            {
                threads[th] = Task.Factory.StartNew((st) =>
                {
                    var ss = (int)st;
                    int myStart = ss * size;
                    int myEnd = ss == threadCount - 1 ? categories.Count : (ss + 1) * size;
                    for (int j = myStart; j < myEnd; j++)
                    {
                        var cat = categories[j];
                        for (int k = 0; k < cat.Pages.Count; k++)
                        {
                            var client = new WebClient();
                            try
                            {
                                client.Encoding = Encoding.GetEncoding("windows-1256");
                                string s = null;
                                using (var data = client.OpenRead(cat.Pages[k]))
                                using (var reader = new StreamReader(data, Encoding.GetEncoding("windows-1256"), true))
                                    s = reader.ReadToEnd();
                                var regex = new Regex("(<A title[=]\'اضغط على الرابط وانتظر قليلاً لحين الاتصال بالسيرفر\' href[=]\'[^\']+\'>[^\']+</A>)"
                                + "|(<A href[=]\'[^\']+\' title[=]\'اضغط على الرابط وانتظر قليلاً لحين الاتصال بالسيرفر\'>[^\']+</A>)",
                                RegexOptions.IgnoreCase);
                                var matchCollections = regex.Matches(s);
                                List<int> nums = new List<int>();
                                var i = 0;
                                while (true)
                                {
                                    int num3 = s.IndexOf("عنوان الكتاب", i);
                                    i = num3;
                                    if (num3 < 0)
                                    {
                                        break;
                                    }
                                    int num4 = i;
                                    i = num4 + 1;
                                    nums.Add(num4);
                                }
                                this.addBooksInCurrentCategory(cat, matchCollections, nums, s);
                                callback(0, string.Concat(new object[] { "تم استكشاف الكتب في الصفحة ",
                                    k + 1, " من قسم ", cat.Name }));
                            }
                            catch
                            {
                                // ignore
                            }
                            client.Dispose();
                        }
                    }
                }, th);
            }
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Wait();
            }
        }

        private void verifyBooks()
        {
            this.callback(0, "يتم الآن مراجعة الملفات...");
            var threads = new Task[threadCount];
            var size = toBeVerified.Count / threadCount;
            for (int th = 0; th < threadCount; th++)
            {
                threads[th] = Task.Factory.StartNew((st) =>
                {
                    var ss = (int)st;
                    int myStart = ss * size;
                    int myEnd = ss == threadCount - 1 ? toBeVerified.Count : (ss + 1) * size;
                    for (int idx = myStart; idx < myEnd; idx++)
                    {
                        var book = toBeVerified[idx];
                        for (int k = 0; k < book.URLs.Count; k++)
                        {
                            if (!isFileLink(book.URLs[k]))
                            {
                                string s = null;
                                try
                                {
                                    using (var client = new WebClient())
                                    {
                                        client.Encoding = Encoding.GetEncoding("windows-1256");
                                        using (var data = client.OpenRead(book.URLs[k]))
                                        using (var reader = new StreamReader(data, Encoding.GetEncoding("windows-1256"), true))
                                            s = reader.ReadToEnd();
                                    }
                                    var regex = new Regex("(<A title[=]\'اضغط على الرابط وانتظر قليلاً لحين الاتصال بالسيرفر\' href[=]\'[^\']+\'>[^\']+</A>)"
                                        + "|(<A href[=]\'[^\']+\' title[=]\'اضغط على الرابط وانتظر قليلاً لحين الاتصال بالسيرفر\'>[^\']+</A>)",
                                        RegexOptions.IgnoreCase);
                                    var matchCollections = regex.Matches(s);
                                    book.URLs.RemoveAt(k--);
                                    for (int i = 0; i < matchCollections.Count; i++)
                                    {
                                        string value = matchCollections[i].Value;
                                        var num = value.IndexOf("href='") + "href='".Length;
                                        var url = value.Substring(num, Math.Min(value.IndexOf('>', num) - 1, value.IndexOf('\'', num)) - num);
                                        if (url.StartsWith("/"))
                                            url = "http://www.waqfeya.com" + url;
                                        book.URLs.Add(url);
                                    }
                                }
                                catch
                                {
                                    // ignore
                                }
                            }
                        }
                    }
                }, th);
            }
            Task.WaitAll(threads);

            if (Program.Setting.DownloadRestriction)
            {
                foreach (Category category in this.categories)
                {
                    for (int i = category.Books.Count - 1; i >= 0; i--)
                    {
                        for (int j = category.Books[i].URLs.Count - 1; j >= 0; j--)
                        {
                            if (this.getFileSize(category.Books[i].URLs[j]) > (long)(Program.Setting.MaxMegaBytesDownload * 1024 * 1024))
                            {
                                category.Books[i].URLs.RemoveAt(j);
                            }
                        }
                    }
                }
            }
        }

		private void runWork()
		{
			switch (this.CurrentStatus)
			{
				case Synchronizer.Status.Connecting:
				{
					this.browser.Navigate("http://www.waqfeya.com/index.php");
					break;
				}
				case Synchronizer.Status.AddingCategories:
				{
					Regex regex1 = new Regex("<A title=\"[^\"]+\" href=\"category[.]php[?]cid[=][0-9]+\">[^\"]+</A>");
					MatchCollection matchCollections1 = regex1.Matches(this.browser.Document.Body.OuterHtml);
					this.callback(0, string.Concat("تم العثور على ", matchCollections1.Count, " قسم"));
					for (var i = 0; i < matchCollections1.Count; i++)
					{
						this.categories.Add(this.parseCategory(matchCollections1[i].Value));
					}
					if (matchCollections1.Count <= 0)
					{
						this.CurrentStatus = Synchronizer.Status.End;
					}
					else
					{
                        this.callback(Form1.ChoocingCategoriesMsg, this.categories);
                        Application.DoEvents();
                        while (Form1.UserHasChoosedCategories == 0)
                        {
                        }
                        this.CurrentStatus = Status.AddingPages;
					}
					break;
				}
			}
		}

        private void runWork_old()
        {
            int i;
            int num;
            Regex regex;
            MatchCollection matchCollections;
            bool flag;
            int num1;
            bool count;
            switch (this.CurrentStatus)
            {
                case Synchronizer.Status.Connecting:
                    {
                        this.browser.Navigate("http://www.waqfeya.com/index.php");
                        break;
                    }
                case Synchronizer.Status.AddingCategories:
                    {
                        Regex regex1 = new Regex("<A title=\"[^\"]+\" href=\"category[.]php[?]cid[=][0-9]+\">[^\"]+</A>");
                        MatchCollection matchCollections1 = regex1.Matches(this.browser.Document.Body.OuterHtml);
                        this.callback(0, string.Concat("تم العثور على ", matchCollections1.Count, " قسم"));
                        for (i = 0; i < matchCollections1.Count; i++)
                        {
                            this.categories.Add(this.parseCategory(matchCollections1[i].Value));
                        }
                        if (matchCollections1.Count <= 0)
                        {
                            this.CurrentStatus = Synchronizer.Status.End;
                        }
                        else
                        {
                            this.callback(Form1.ChoocingCategoriesMsg, this.categories);
                            Application.DoEvents();
                            while (Form1.UserHasChoosedCategories == 0)
                            {
                            }
                            this.index.X = 0;
                            this.browser.Navigate(this.categories[0].Pages[0]);
                        }
                        break;
                    }
                case Synchronizer.Status.AddingPages:
                    {
                        if (this.index.X < this.categories.Count)
                        {
                            Regex regex2 = new Regex(string.Concat("<A href[=]\"category[.]php[?]cid[=]", this.categories[this.index.X].Id, "[&]amp;st=[0-9]+\"><U>[0-9]+</U></A>"));
                            MatchCollection matchCollections2 = regex2.Matches(this.browser.Document.Body.OuterHtml);
                            for (i = 0; i < matchCollections2.Count; i++)
                            {
                                num = matchCollections2[i].Value.IndexOf("href=\"") + "href=\"".Length;
                                int num2 = matchCollections2[i].Value.IndexOf('\"', num + 1);
                                string str = matchCollections2[i].Value.Substring(num, num2 - num);
                                str = str.Remove(str.IndexOf("amp;"), 4);
                                this.categories[this.index.X].Pages.Add(string.Concat("http://www.waqfeya.com/", str));
                            }
                            this.callback((this.index.X + 1) * 100 / this.categories.Count, string.Concat("تم استكشاف الصفحات لقسم ", this.categories[this.index.X].Name));
                            this.progress.Y = this.progress.Y + this.categories[this.index.X].Pages.Count;
                        }
                        int x = this.index.X + 1;
                        num1 = x;
                        this.index.X = x;
                        if (num1 >= this.categories.Count)
                        {
                            this.toBeVerified = new List<Book>();
                            this.browser.Navigate(this.categories[0].Pages[0]);
                        }
                        else
                        {
                            this.browser.Navigate(this.categories[this.index.X].Pages[0]);
                        }
                        break;
                    }
                case Synchronizer.Status.AddingBooks:
                    {
                        if (this.index.X < this.categories.Count)
                        {
                            regex = new Regex("<A title[=]\"اضغط على الرابط وانتظر قليلاً لحين الاتصال بالسيرفر\" href=\"[^\"]+\">[^\"]+</A>");
                            matchCollections = regex.Matches(this.browser.Document.Body.OuterHtml);
                            List<int> nums = new List<int>();
                            i = 0;
                            while (true)
                            {
                                int num3 = this.browser.Document.Body.OuterHtml.IndexOf("عنوان الكتاب", i);
                                i = num3;
                                if (num3 < 0)
                                {
                                    break;
                                }
                                int num4 = i;
                                i = num4 + 1;
                                nums.Add(num4);
                            }
                            this.addBooksInCurrentCategory(categories[this.progress.X], matchCollections, nums, this.browser.Document.Body.OuterHtml);
                            Synchronizer.ReportProgressCallback reportProgressCallback = this.callback;
                            int x1 = this.progress.X;
                            num1 = x1;
                            this.progress.X = x1 + 1;
                            int y = num1 * 100 / this.progress.Y;
                            object[] objArray = new object[] { "تم استكشاف الكتب في الصفحة ", this.index.Y + 1, " من قسم ", this.categories[this.index.X].Name };
                            reportProgressCallback(y, string.Concat(objArray));
                        }
                        if (this.index.X >= this.categories.Count)
                        {
                            count = true;
                        }
                        else
                        {
                            int y1 = this.index.Y + 1;
                            num1 = y1;
                            this.index.Y = y1;
                            count = num1 < this.categories[this.index.X].Pages.Count;
                        }
                        if (!count)
                        {
                            this.index = new Point(this.index.X + 1, 0);
                        }
                        if (this.index.X < this.categories.Count)
                        {
                            this.browser.Navigate(this.categories[this.index.X].Pages[this.index.Y]);
                        }
                        else if (this.toBeVerified.Count <= 0)
                        {
                            this.CurrentStatus = Synchronizer.Status.End;
                        }
                        else
                        {
                            this.callback(0, "يتم الآن إصلاح الروابط غير المباشرة للملفات...");
                            flag = false;
                            i = 0;
                            while (i < this.toBeVerified.Count)
                            {
                                int num5 = 0;
                                while (num5 < this.toBeVerified[i].URLs.Count)
                                {
                                    if (this.isFileLink(this.toBeVerified[i].URLs[num5]))
                                    {
                                        num5++;
                                    }
                                    else
                                    {
                                        this.browser.Navigate(this.toBeVerified[0].URLs[0]);
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    i++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case Synchronizer.Status.VerifyingBookLinks:
                    {
                        if (this.index.X < this.toBeVerified.Count)
                        {
                            regex = new Regex("<A title[=]\"اضغط على الرابط وانتظر قليلاً لحين الاتصال بالسيرفر\" href=\"[^\"]+\">[^\"]+</A>");
                            matchCollections = regex.Matches(this.browser.Document.Body.OuterHtml);
                            this.toBeVerified[this.index.X].URLs.RemoveAt(this.index.Y);
                            for (i = 0; i < matchCollections.Count; i++)
                            {
                                string value = matchCollections[i].Value;
                                num = value.IndexOf("href=\"") + "href=\"".Length;
                                this.toBeVerified[this.index.X].URLs.Add(value.Substring(num, value.IndexOf('>', num) - 1 - num));
                            }
                            this.index.Y = this.index.Y + 1;
                            flag = false;
                            while (true)
                            {
                                if (this.index.Y >= this.toBeVerified[this.index.X].URLs.Count)
                                {
                                    this.index = new Point(this.index.X + 1, 0);
                                }
                                if (this.index.X < this.toBeVerified.Count)
                                {
                                    while (this.index.Y < this.toBeVerified[this.index.X].URLs.Count)
                                    {
                                        if (this.isFileLink(this.toBeVerified[this.index.X].URLs[this.index.Y]))
                                        {
                                            this.index.Y = this.index.Y + 1;
                                        }
                                        else
                                        {
                                            this.browser.Navigate(this.toBeVerified[this.index.X].URLs[this.index.Y]);
                                            flag = true;
                                            break;
                                        }
                                    }
                                    if (flag)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        if (this.index.X >= this.toBeVerified.Count)
                        {
                            this.CurrentStatus = Synchronizer.Status.End;
                        }
                        else
                        {
                            this.browser.Navigate(this.toBeVerified[this.index.X].URLs[this.index.Y]);
                        }
                        break;
                    }
            }
        }

		public void Sync()
		{
			this.CurrentStatus = Synchronizer.Status.Connecting;
			this.categories = new List<Category>();
			this.callback(0, "يتم الآن إحصاء أقسام الكتب...");
			this.runWork();
			while (this.CurrentStatus != Synchronizer.Status.AddingPages)
			{
            }
            addPages();
            this.toBeVerified = new List<Book>();
            addBooks();
            verifyBooks();
            this.CurrentStatus = Status.End;
			//this.callback(0, "يتم الآن تحميل الملفات...");
			this.addFilesToDownloadList();
		}

		public delegate void ReportProgressCallback(int progress, object userState);

		public enum Status
		{
			Connecting = 1,
			AddingCategories = 2,
			AddingPages = 3,
			AddingBooks = 4,
			VerifyingBookLinks = 5,
			End = 6
		}
	}
}