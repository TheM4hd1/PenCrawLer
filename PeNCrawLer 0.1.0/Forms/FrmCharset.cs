using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PeNCrawLer_0._1._0.Forms
{
    public partial class FrmCharset : Form
    {
        public string charset = string.Empty;
        public string textToShow = string.Empty;
        public FrmCharset()
        {
            InitializeComponent();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = radioButton2.Checked;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            checkedListBox1.Enabled = radioButton1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(radioButton1.Checked) // Default charset
            {

                foreach(var item in checkedListBox1.CheckedItems)
                {
                    string i = item.ToString().Split('\t')[0];
                    textToShow = textToShow + $"[ {i} ]";

                    switch(i)
                    {
                        case "a-z":
                            charset += "qwertyuiopasdfghjklzxcvbnm";
                            break;
                        case "A-Z":
                            charset += "QWERTYUIOPASDFGHJKLZXCVBNM";
                            break;
                        case "0-9":
                            charset += "0123456789";
                            break;
                        case "!":
                            charset += "!";
                            break;
                        case "-":
                            charset += "-";
                            break;
                        case "_":
                            charset += "_";
                            break;
                        case "%20":
                            charset += "%20"; //%20 equals to '+' or ' '
                            break;
                    }
                }

            }

            else //if(radioButton2.Checked) // Custom charset
            {

                charset = textBox1.Text;
                textToShow = charset;
                

            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
