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
    public partial class FrmEditLink : Form
    {
        public string pattern;
        public int max;
        public FrmEditLink(string pattern,int max)
        {
            InitializeComponent();
            textBox1.Text = pattern;
            textBox2.Text = $"{max}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.pattern = textBox1.Text;
            if(!int.TryParse(textBox2.Text,out this.max))
            {
                MessageBox.Show("Please enter a valid number!", "Settings - PeNCrawLer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox2.Text = "";
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
