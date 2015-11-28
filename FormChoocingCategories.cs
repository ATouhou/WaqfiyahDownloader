using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace مكتبة_الوقفية
{
	internal class FormChoocingCategories : Form
	{
		private List<Category> categories;

		private List<Category> selected;

		private IContainer components = null;

		private CheckedListBox checkedListBox1;

		private Label label1;

		private Button button1;

		private Label labelCatCnt;

		private Button button2;

		private Button button3;

		public FormChoocingCategories(List<Category> categ, List<Category> selected)
		{
			this.InitializeComponent();
			this.labelCatCnt.Text = string.Format("تم العثور على {0} قسم بالمكتبة", categ.Count);
			this.categories = categ;
			for (int i = 0; i < categ.Count; i++)
			{
				this.checkedListBox1.Items.Add(categ[i].Name, true);
			}
			this.selected = selected;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < this.checkedListBox1.Items.Count; i++)
			{
				if (this.checkedListBox1.GetItemChecked(i))
				{
					this.selected.Add(this.categories[i]);
				}
			}
			if (this.selected.Count != 0)
			{
				base.DialogResult = System.Windows.Forms.DialogResult.OK;
			}
			else
			{
				MessageBox.Show(this, "فضلا اختر قسما واحدا على الأقل للمزامنة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < this.checkedListBox1.Items.Count; i++)
			{
				this.checkedListBox1.SetItemChecked(i, true);
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < this.checkedListBox1.Items.Count; i++)
			{
				this.checkedListBox1.SetItemChecked(i, false);
			}
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
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.labelCatCnt = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(10, 48);
            this.checkedListBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(321, 184);
            this.checkedListBox1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(204, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "مزامنة أقسام الكتب التالية:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(267, 236);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(64, 27);
            this.button1.TabIndex = 2;
            this.button1.Text = "موافق";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // labelCatCnt
            // 
            this.labelCatCnt.AutoSize = true;
            this.labelCatCnt.Location = new System.Drawing.Point(178, 9);
            this.labelCatCnt.Name = "labelCatCnt";
            this.labelCatCnt.Size = new System.Drawing.Size(153, 13);
            this.labelCatCnt.TabIndex = 3;
            this.labelCatCnt.Text = "تم العثور على 90 قسم بالمكتبة";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(105, 236);
            this.button2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(93, 27);
            this.button2.TabIndex = 4;
            this.button2.Text = "تحديد الكل";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(10, 236);
            this.button3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(93, 27);
            this.button3.TabIndex = 5;
            this.button3.Text = "إلغاء تحديد الكل";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // FormChoocingCategories
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(343, 273);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.labelCatCnt);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkedListBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormChoocingCategories";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "اختيار الأقسام للمزامنة - محمل المكتبة الوقفية";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
		}
	}
}