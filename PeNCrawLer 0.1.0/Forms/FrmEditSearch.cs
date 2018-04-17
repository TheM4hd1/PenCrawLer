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
    public partial class FrmEditSearch : Form
    {
        public string source, pattern;
        public FrmEditSearch(string source,string pattern)
        {
            InitializeComponent();
            if (source.Equals("Url"))
                radioButton1.Checked = true;
            else
                radioButton2.Checked = true;

            textBox1.Text = pattern;
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
