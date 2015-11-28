using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace مكتبة_الوقفية
{
    internal partial class FormChooseBooks : Form
    {
        private bool notAcceptButton = true;
        private bool checkProgramatically = false;
        private List<Book> temp;

        public FormChooseBooks()
        {
            InitializeComponent();
        }

        public List<Book> SelectedBooks
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
                temp.Add(Book.GetById(int.Parse(node.Name)));
            }
        }

        public void Populate(List<Category> cats)
        {
            treeView1.Nodes.Clear();
            foreach (var item in cats)
            {
                var node = new TreeNode(item.Name) { Checked = true };
                for (int i = 0; i < item.Books.Count; i++)
                {
                    node.Nodes.Add(item.Books[i].ID.ToString(), item.Books[i].Title).Checked = true;
                }
                treeView1.Nodes.Add(node);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (notAcceptButton && MessageBox.Show(this, "هل أنت متأكد من إلغاء التحميل؟", "محمل المكتبة الوقفية",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign)
                == System.Windows.Forms.DialogResult.No)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < treeView1.Nodes.Count; i++)
            {
                checkAll(treeView1.Nodes[i], true);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < treeView1.Nodes.Count; i++)
            {
                checkAll(treeView1.Nodes[i], false);
            }
        }

        private void checkAll(TreeNode node, bool val)
        {
            node.Checked = val;
            for (int i = 0; i < node.Nodes.Count; i++)
            {
                checkAll(node.Nodes[i], val);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            notAcceptButton = false;
            DialogResult = System.Windows.Forms.DialogResult.OK;
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
}
