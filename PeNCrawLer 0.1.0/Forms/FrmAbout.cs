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
    public partial class FrmAbout : Form
    {
        public FrmAbout()
        {
            InitializeComponent();
        }

        private void FrmAbout_Load(object sender, EventArgs e)
        {
            System.Threading.Thread updateThread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                label3.Invoke((MethodInvoker)delegate
                {
                    label3.Text = "Checking for updates...";
                });

                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    try
                    {
                        System.Net.Http.HttpResponseMessage res = client.GetAsync("http://0app.me/Projects/PeNCrawLer/Update").Result;
                        if (!res.IsSuccessStatusCode)
                            throw new Exception();

                        string newVersion = res.Content.ReadAsStringAsync().Result;
                        if (newVersion.Equals(Application.ProductVersion))
                        {
                            label3.Invoke((MethodInvoker)delegate
                            {
                                label3.Text = "PeNCrawLer is up to date";
                            });
                        }

                        else
                        {
                            label3.Invoke((MethodInvoker)delegate
                            {
                                label3.Text = $"Update is available ({newVersion})";
                            });
                        }
                    }
                    catch(Exception)
                    {
                        label3.Invoke((MethodInvoker)delegate
                        {
                            label3.ForeColor = Color.FromArgb(255, 128, 0);
                            label3.Text = "Update error\nThe connection to the server was unsuccessful";
                        });
                    }
                }
            }))
            {
                IsBackground = true
            };

            updateThread.Start();
        }
        
    }
}
