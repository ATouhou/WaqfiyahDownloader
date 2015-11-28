using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace مكتبة_الوقفية
{
	internal class FormSettings : Form
	{
		private IContainer components = null;

		private Button buttonOk;

		private Button buttonCancel;

		private TextBox textBoxPath;

		private Label label1;

		private Button buttonChange;

		private FolderBrowserDialog folderBrowserDialog1;

		private CheckBox checkBox1;

		private NumericUpDown numericUpDownMaxSize;

		private Label label2;
        private NumericUpDown numericUpDown1;
        private Label label4;
        private ToolTip toolTip1;
        private Label label5;

		private Label label3;

		public FormSettings()
		{
			this.InitializeComponent();
		}

		private void buttonChange_Click(object sender, EventArgs e)
		{
            again:
            if (this.folderBrowserDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                if (folderBrowserDialog1.SelectedPath.Length > 50)
                {
                    MessageBox.Show(this, "مسار الحافظة المختارة\n" + folderBrowserDialog1.SelectedPath + "\n\nطويل جدا (أكثر من 50 حرفا) فضلا اختر حافظة ذات اسم مختصر ويفضل أن تكون في مسار القرص الصلب مباشرة", "خطأ", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                    goto again;
                }
                Program.Setting.SaveFolder = this.folderBrowserDialog1.SelectedPath;
                this.textBoxPath.Text = this.folderBrowserDialog1.SelectedPath;
            }
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			if (Program.Setting.SaveFolder != null)
			{
                Program.Setting.NumThreads = (short)numericUpDown1.Value;
				Program.Setting.MaxMegaBytesDownload = (int)this.numericUpDownMaxSize.Value;
				Program.Setting.DownloadRestriction = this.checkBox1.Checked;
				base.DialogResult = System.Windows.Forms.DialogResult.OK;
			}
			else
			{
				MessageBox.Show(this, "اختر الحافظة أولا", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
			}
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			this.numericUpDownMaxSize.Enabled = this.checkBox1.Checked;
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
            this.components = new System.ComponentModel.Container();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonChange = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.numericUpDownMaxSize = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(189, 175);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(76, 23);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "موافق";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(21, 175);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(76, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "إلغاء الأمر";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxPath
            // 
            this.textBoxPath.Location = new System.Drawing.Point(68, 24);
            this.textBoxPath.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.ReadOnly = true;
            this.textBoxPath.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textBoxPath.Size = new System.Drawing.Size(220, 20);
            this.textBoxPath.TabIndex = 2;
            this.toolTip1.SetToolTip(this.textBoxPath, "المسار الذي يتم حفظ الكتب فيه");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(147, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "مسار حفظ الملفات المحملة:";
            // 
            // buttonChange
            // 
            this.buttonChange.Location = new System.Drawing.Point(12, 24);
            this.buttonChange.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.Size = new System.Drawing.Size(46, 22);
            this.buttonChange.TabIndex = 4;
            this.buttonChange.Text = "&...";
            this.buttonChange.UseVisualStyleBackColor = true;
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "اختر الحافظة التي سيقوم البرنامج بالتحميل إليها";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(68, 96);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(214, 17);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "عدم تحميل الملفات ذات الحجم الأكبر من ";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // numericUpDownMaxSize
            // 
            this.numericUpDownMaxSize.Enabled = false;
            this.numericUpDownMaxSize.Location = new System.Drawing.Point(235, 117);
            this.numericUpDownMaxSize.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericUpDownMaxSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMaxSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxSize.Name = "numericUpDownMaxSize";
            this.numericUpDownMaxSize.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMaxSize.TabIndex = 6;
            this.numericUpDownMaxSize.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(187, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "م.ب.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(99, 139);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(183, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "* قد لا يمكن معرفة حجم بعض الملفات";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(102, 62);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(54, 20);
            this.numericUpDown1.TabIndex = 9;
            this.toolTip1.SetToolTip(this.numericUpDown1, "تحديد عدد التحميلات التي تتم في نفس الوقت");
            this.numericUpDown1.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(167, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(118, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "عدد التحميلات المتوازية:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(56, 119);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "(خاص بالمزامنة فقط)";
            // 
            // FormSettings
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(294, 210);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDownMaxSize);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.buttonChange);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxPath);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "محمل المكتبة الوقفية - الإعدادات";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if ((Program.Setting.SaveFolder != null ? false : MessageBox.Show(this, "لم يتم اختيار الحافظة التي سيقوم البرنامج بالتنزيل فيها. هل أنت متأكد من الإغلاق؟", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign) == System.Windows.Forms.DialogResult.No))
			{
				e.Cancel = true;
			}
			base.OnClosing(e);
		}

		protected override void OnShown(EventArgs e)
		{
            this.numericUpDown1.Value = Program.Setting.NumThreads == 0 ? 4 : Program.Setting.NumThreads;
			this.textBoxPath.Text = (Program.Setting.SaveFolder == null ? "" : Program.Setting.SaveFolder);
			this.checkBox1.Checked = Program.Setting.DownloadRestriction;
			this.numericUpDownMaxSize.Value = (Program.Setting.MaxMegaBytesDownload > 0 ? Program.Setting.MaxMegaBytesDownload : 100);
			base.OnShown(e);
		}
	}
}