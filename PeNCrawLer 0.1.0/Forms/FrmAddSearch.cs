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
    public partial class FrmAddSearch : Form
    {
        public string source, pattern;
        public FrmAddSearch()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                source = "Url";
            else
                source = "Response";

            pattern = textBox1.Text;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
