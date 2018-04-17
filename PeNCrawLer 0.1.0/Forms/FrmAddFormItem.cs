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
    public partial class FrmAddFormItem : Form
    {
        public string name, value;
        public FrmAddFormItem()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            name = textBox1.Text;
            value = textBox2.Text;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
