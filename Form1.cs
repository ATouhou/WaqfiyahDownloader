using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace مكتبة_الوقفية
{
	public class Form1 : Form
	{
		public const int ChoocingCategoriesMsg = 1992;

		public const int DownloadProgressMsg = 4096;

        public const int ChooseBooksMsg = 1436;

		private IContainer components = null;

		private ProgressBar progressBar1;

		private BackgroundWorker backgroundWorker1;

		private Button buttonAsync;

		private StatusStrip statusStrip1;

		private ToolStripStatusLabel toolStripStatusLabel1;

        private Button buttonHelp;

		private WebBrowser webBrowser1;

		private ListBox listBoxStatus;

		private ToolStripProgressBar toolStripProgressBar1;

		private Synchronizer sync;

		private string lastError;

		public static int UserHasChoosedCategories
		{
			get;
			private set;
		}

        internal static List<Book> SelectedBooks { get; private set; }

		public Form1()
		{
			this.InitializeComponent();
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				this.lastError = "";
				this.sync.Sync();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.lastError = string.Concat("فشلت العملية. خطأ: ", exception.Message, "\n");
			}
		}

		private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (this.backgroundWorker1.IsBusy)
			{
                if (e.ProgressPercentage == ChoocingCategoriesMsg)
                {
                    List<Category> userState = e.UserState as List<Category>;
                    List<Category> categories = new List<Category>();
                    FormChoocingCategories formChoocingCategory = new FormChoocingCategories(userState, categories);
                    try
                    {
                        formChoocingCategory.ShowDialog(this);
                    }
                    finally
                    {
                        if (formChoocingCategory != null)
                        {
                            ((IDisposable)formChoocingCategory).Dispose();
                        }
                    }
                    userState.Clear();
                    userState.AddRange(categories);
                    UserHasChoosedCategories = 1;
                }
				else if (e.ProgressPercentage == ChooseBooksMsg)
				{
                    Form1.UserHasChoosedCategories = 0;
					List<Category> userState = e.UserState as List<Category>;
                    using (var f = new FormChooseBooks())
                    {
                        f.Populate(userState);
                        if (f.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                        {
                            Form1.UserHasChoosedCategories = -1;
                        }
                        else
                        {
                            SelectedBooks = f.SelectedBooks;
                            Form1.UserHasChoosedCategories = 1;
                        }
                    }
				}
				else if (e.ProgressPercentage != 4096)
				{
					int progressPercentage = e.ProgressPercentage;
					if ((e.ProgressPercentage < 0 ? true : e.ProgressPercentage > 100))
					{
						progressPercentage = 0;
					}
					if (this.sync.CurrentStatus == Synchronizer.Status.End)
					{
						this.toolStripProgressBar1.Value = progressPercentage;
					}
					else
					{
						this.progressBar1.Value = progressPercentage;
					}
					this.listBoxStatus.Items.Add(e.UserState);
					this.listBoxStatus.SelectedIndex = this.listBoxStatus.Items.Count - 1;
					if (this.sync.CurrentStatus == Synchronizer.Status.Connecting)
					{
						this.toolStripStatusLabel1.Text = "(1 من 5) يتم الآن الاتصال بالموقع...";
					}
					else if (this.sync.CurrentStatus == Synchronizer.Status.AddingPages)
					{
						this.toolStripStatusLabel1.Text = "(3 من 5) يتم الآن استكشاف صفحات أقسام الكتب...";
					}
					else if (this.sync.CurrentStatus == Synchronizer.Status.AddingCategories)
					{
						this.toolStripStatusLabel1.Text = "(2 من 5) يتم الآن استكشاف أقسام الكتب...";
					}
					else if (this.sync.CurrentStatus == Synchronizer.Status.End)
					{
						this.toolStripProgressBar1.Visible = true;
						this.toolStripStatusLabel1.Text = "(5 من 5) يتم الآن تحميل الكتب...";
					}
                    else if (this.sync.CurrentStatus == Synchronizer.Status.AddingBooks)
					{
						this.toolStripStatusLabel1.Text = "(4 من 5) يتم الآن استكشاف الكتب...";
					}
				}
				else
				{
					int num = Convert.ToInt32(e.UserState);
					this.progressBar1.Value = (num < 0 || num > 100 ? 0 : num);
				}
			}
		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.buttonAsync.Enabled = true;
			this.toolStripProgressBar1.Visible = false;
			progressBar1.Visible = false;
			this.listBoxStatus.Items.Add(this.lastError);
			this.toolStripStatusLabel1.Text = (this.lastError == "" ? "تم الانتهاء من المزامنة بنجاح" : "جاهز");
		}

		private void buttonAsync_Click(object sender, EventArgs e)
		{
			if (Program.Setting.SaveFolder == null)
			{
				MessageBox.Show(this, "لا يمكن بدء المزامنة. فضلا قم بتحديد الحافظة التي سيتم التحميل فيها من الإعدادات. إذا كانت هذه الحافظة موجودة مسبقا، قم بتحديدها ولن يعيد البرنامج تحميل ما سبق تحميله", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                return;
			}
			this.listBoxStatus.Items.Clear();
			Form1.UserHasChoosedCategories = 0;
			this.buttonAsync.Enabled = false;
			this.progressBar1.Value = 0;
			this.progressBar1.Visible = true;
			this.toolStripStatusLabel1.Text = "يتم الآن التزامن...";
			this.backgroundWorker1.RunWorkerAsync();
		}

		private void buttonHelp_Click(object sender, EventArgs e)
		{
			MessageBox.Show(this, "بسم الله الرحمن الرحيم\nيقوم البرنامج بالاتصال بموقع مكتبة الوقفية وتحميل الكتب الجديدة أو ذات الطبعة الأعلى. ماعليك سوى تحديد الحافظة التي سيتم التحميل فيها ثم الضغط على زر بدء التزامن.\n\nمهم: يجب عدم تغيير اسماء الحافظات التي ينشأها البرنامج أو نقلها من مكانها وإلا سيعيد البرنامج تحميل تلك الحافظات من جديد.\n\nهناك 5 مراحل للتزامن: 1)الاتصال بالموقع 2)جلب أقسام الكتب 3)استكشاف كافة صفحات أقسام الكتب 4)استكشاف كافة الكتب 5)تحميل الكتب غير المحملة سابقا",
                "مساعدة", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
		}

		protected override void Dispose(bool disposing)
		{
			if ((!disposing ? false : this.components != null))
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.buttonAsync = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.listBoxStatus = new System.Windows.Forms.ListBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(3, 282);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(377, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 0;
            this.progressBar1.Visible = false;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // buttonAsync
            // 
            this.buttonAsync.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAsync.Image = ((System.Drawing.Image)(resources.GetObject("buttonAsync.Image")));
            this.buttonAsync.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonAsync.Location = new System.Drawing.Point(266, 10);
            this.buttonAsync.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonAsync.Name = "buttonAsync";
            this.buttonAsync.Size = new System.Drawing.Size(102, 28);
            this.buttonAsync.TabIndex = 2;
            this.buttonAsync.Text = "       بدء التزامن";
            this.buttonAsync.UseVisualStyleBackColor = true;
            this.buttonAsync.Click += new System.EventHandler(this.buttonAsync_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 311);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(12, 0, 1, 0);
            this.statusStrip1.Size = new System.Drawing.Size(380, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(30, 17);
            this.toolStripStatusLabel1.Text = "جاهز";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(86, 16);
            this.toolStripProgressBar1.Visible = false;
            // 
            // buttonHelp
            // 
            this.buttonHelp.Image = ((System.Drawing.Image)(resources.GetObject("buttonHelp.Image")));
            this.buttonHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonHelp.Location = new System.Drawing.Point(3, 10);
            this.buttonHelp.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(102, 28);
            this.buttonHelp.TabIndex = 4;
            this.buttonHelp.Text = "     مساعدة";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Location = new System.Drawing.Point(3, 43);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(17, 16);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(377, 127);
            this.webBrowser1.TabIndex = 6;
            // 
            // listBoxStatus
            // 
            this.listBoxStatus.FormattingEnabled = true;
            this.listBoxStatus.Location = new System.Drawing.Point(0, 175);
            this.listBoxStatus.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listBoxStatus.Name = "listBoxStatus";
            this.listBoxStatus.Size = new System.Drawing.Size(380, 108);
            this.listBoxStatus.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 333);
            this.Controls.Add(this.listBoxStatus);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.buttonHelp);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.buttonAsync);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.Text = "مزامن المكتبة الوقفية";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if ((!this.backgroundWorker1.IsBusy ? true : MessageBox.Show(this, "أنت على وشك إغلاق البرنامج. لم يتم إنهاء عملية التزامن بعد، ولا بد عندئذ من بدء التزامن من جديد. هل أنت متأكد أنك تريد الخروج؟", "تحذير", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign) == System.Windows.Forms.DialogResult.Yes))
			{
				if (sync !=null) this.sync.Dispose();
				base.OnClosing(e);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			if (Program.Setting.SaveFolder != null)
			{
				this.backgroundWorker1.ReportProgress(0);
				this.sync = new Synchronizer(this.webBrowser1, new Synchronizer.ReportProgressCallback(this.backgroundWorker1.ReportProgress));
			}
			base.OnLoad(e);
		}
	}
}