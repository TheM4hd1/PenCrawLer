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
    public partial class FrmAddLink : Form
    {
        public string pattern;
        public int max;
        public FrmAddLink()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pattern = textBox1.Text;
            if(!int.TryParse(textBox2.Text,out max))
            {
                MessageBox.Show("Please enter a valid number!","Settings - PeNCrawLer",MessageBoxButtons.OK,MessageBoxIcon.Error);
                textBox2.Text = "";
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
