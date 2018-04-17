using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PeNCrawLer_0._1._0
{
    public partial class FrmAddTag : Form
    {
        public string tagName,tagAttribute;

        public FrmAddTag()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tagName = textBox1.Text;
            tagAttribute = textBox2.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
