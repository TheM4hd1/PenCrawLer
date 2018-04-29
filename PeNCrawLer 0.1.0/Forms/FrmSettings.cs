using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PeNCrawLer_0._1._0.Advanced_Settings;
namespace PeNCrawLer_0._1._0
{
    public partial class FrmSettings : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
	    static extern bool AnimateWindow(IntPtr hWnd, int time, AnimateWindowFlags flags);
        [Flags]
        enum AnimateWindowFlags
        {
            AW_HOR_POSITIVE = 0x00000001,
            AW_HOR_NEGATIVE = 0x00000002,
            AW_VER_POSITIVE = 0x00000004,
            AW_VER_NEGATIVE = 0x00000008,
            AW_CENTER = 0x00000010,
            AW_HIDE = 0x00010000,
            AW_ACTIVATE = 0x00020000,
            AW_SLIDE = 0x00040000,
            AW_BLEND = 0x00080000
        }

        bool applySettings = true;
        public FrmSettings()
        {
            InitializeComponent();
        }

        private void FrmSettings_Load(object sender, EventArgs e)
        {
            AnimateWindow(this.Handle, 200, AnimateWindowFlags.AW_BLEND);
        }

        private void FrmSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                AnimateWindow(this.Handle, 200, AnimateWindowFlags.AW_BLEND | AnimateWindowFlags.AW_HIDE);
                Hide();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if(CrawlerSettings.CanExtractLinkByHtmlElement)
            {
                CrawlerSettings.HtmlTagAndAttribute = new Dictionary<string, string>();

                foreach(ListViewItem item in lvwHtmlElements.Items)
                {
                    if (CrawlerSettings.HtmlTagAndAttribute.ContainsKey(item.Text))
                        continue;
                    CrawlerSettings.HtmlTagAndAttribute.Add(item.Text, item.SubItems[1].Text);
                }
            }

            if(CrawlerSettings.CanLimitCrawling)
            {
                CrawlerSettings.LinksPatternAndMaxValue = new Dictionary<string, int>();

                foreach (ListViewItem item in lvwLimitCrawling.Items)
                {
                    if (CrawlerSettings.LinksPatternAndMaxValue.ContainsKey(item.Text))
                        continue;
                    CrawlerSettings.LinksPatternAndMaxValue.Add(item.Text, int.Parse(item.SubItems[1].Text));
                }
            }

            if(CrawlerSettings.CanSearch)
            {
                CrawlerSettings.SearchInputAndValue = new Dictionary<char, string>();

                foreach(ListViewItem item in lvwSearch.Items)
                {
                    if (CrawlerSettings.SearchInputAndValue.ContainsKey(item.Text[0]))
                        continue;
                    CrawlerSettings.SearchInputAndValue.Add(item.Text[0], item.SubItems[1].Text);
                }
            }

            if(FormSubmissionSettings.CanSubmitForm)
            {
                FormSubmissionSettings.FieldnameAndFieldvalue = new Dictionary<string, string>();

                foreach(ListViewItem item in lvwFormSubmission.Items)
                {
                    if (FormSubmissionSettings.FieldnameAndFieldvalue.ContainsKey(item.SubItems[1].Text))
                        continue;
                    FormSubmissionSettings.FieldnameAndFieldvalue.Add(item.SubItems[1].Text, item.SubItems[2].Text);
                }
            }

            {
                HttpRequestSettings.HeaderAndValue = new Dictionary<string, string>();

                foreach(string header in lbxHeaders.Items)
                {
                    string[] values = header.Split(':');
                    if (HttpRequestSettings.HeaderAndValue.ContainsKey(values[0]))
                        continue;
                    HttpRequestSettings.HeaderAndValue.Add(values[0], values[1]);
                }
            }

            if(!applySettings)
            {
                MessageBox.Show("Please fix your regex pattern for extracting links from response!", "Error - PeNCrawLer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CrawlerSettings.IsCustomizeSettings = true;
            Core.HttpRequests.HasToUpdateCrawler = true; // force crawler to update http requests on runtime
            Core.HttpRequests.HasToUpdateDirbuster = true;
            CrawlerSettings.HasToUpdateExtentionsList = true; // force crawler to update settings on runtime.
            Close();
        }

        private void ckbFollowRedirect_CheckedChanged(object sender, EventArgs e)
        {
            CrawlerSettings.CanFollowRedirect = ckbFollowRedirect.Checked;
        }

        private void ckbRenderJavascript_CheckedChanged(object sender, EventArgs e)
        {
            CrawlerSettings.CanRenderJavascript = ckbRenderJavascript.Checked;
        }

        private void ckbExtractByHtml_CheckedChanged(object sender, EventArgs e)
        {
            CrawlerSettings.CanExtractLinkByHtmlElement = btnAddHtmlElement.Enabled = btnEditHtmlElement.Enabled = btnRemoveHtmlElement.Enabled = ckbExtractByHtml.Checked;
        }

        private void btnAddHtmlElement_Click(object sender, EventArgs e)
        {
            using (FrmAddTag f = new FrmAddTag())
            {
                DialogResult dialogResult = f.ShowDialog();
                if(dialogResult == DialogResult.OK)
                {
                    ListViewItem lvi = new ListViewItem()
                    {
                        Text = f.tagName
                    };

                    lvi.SubItems.Add(f.tagAttribute);
                    lvwHtmlElements.Items.Add(lvi);
                }
            }
        }

        private void btnEditHtmlElement_Click(object sender, EventArgs e)
        {
            if(lvwHtmlElements.SelectedItems.Count>0)
            {
                string tag = lvwHtmlElements.SelectedItems[0].Text;
                string attribute = lvwHtmlElements.SelectedItems[0].SubItems[1].Text;
                using (FrmEditTag f = new FrmEditTag(tag, attribute))
                {
                    DialogResult dialogResult = f.ShowDialog();
                    if (dialogResult == DialogResult.OK)
                    {
                        lvwHtmlElements.SelectedItems[0].Text = f.tagName;
                        lvwHtmlElements.SelectedItems[0].SubItems[1].Text = f.tagAttribute;
                    }
                }
            }
            
        }

        private void btnRemoveHtmlElement_Click(object sender, EventArgs e)
        {
            if(lvwHtmlElements.SelectedItems.Count>0)
            {
                lvwHtmlElements.SelectedItems[0].Remove();
            }
        }

        private void ckbExtractByRegex_CheckedChanged(object sender, EventArgs e)
        {
            CrawlerSettings.CanExtractLinkByRegex = btnCheckLinkPattern.Enabled = ckbExtractByRegex.Checked;
        }

        private void txtLinkPattern_TextChanged(object sender, EventArgs e)
        {
            CrawlerSettings.PatternExtractLink = txtLinkPattern.Text;
        }

        private void btnCheckLinkPattern_Click(object sender, EventArgs e)
        {
            if(Utilities.Helper.IsValidPattern(CrawlerSettings.PatternExtractLink))
            {
                lblLinkRegexPattern.ForeColor = Color.Green;
                lblLinkRegexPattern.Text = "Valid Pattern!";
                applySettings = true;
            }
            else
            {
                lblLinkRegexPattern.ForeColor = Color.Red;
                lblLinkRegexPattern.Text = "Invalid Pattern!";
                applySettings = false;
            }
        }

        private void ckbNotProcess_CheckedChanged(object sender, EventArgs e)
        {
            CrawlerSettings.CanNotProcessExtentions = ckbNotProcess.Checked;
        }

        private void txtBlackListExtentions_TextChanged(object sender, EventArgs e)
        {
            CrawlerSettings.BlackListExtentions = txtBlackListExtentions.Text;
            toolTip1.Show("use comma (,) to seprate extentions", txtBlackListExtentions);
        }

        private void ckbDownload_CheckedChanged(object sender, EventArgs e)
        {
            CrawlerSettings.CanDownloadExtentions = btnOpenPath.Enabled = ckbDownload.Checked;
        }

        private void txtWhiteListExtentions_TextChanged(object sender, EventArgs e)
        {
            CrawlerSettings.WhiteListExtentions = txtWhiteListExtentions.Text;
            toolTip1.Show("use comma (,) to seprate extentions", txtWhiteListExtentions);
        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {
            CrawlerSettings.PathToSave = txtPath.Text;
        }

        private void btnOpenPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = fbd.SelectedPath;
                }
            }
  
        }

        private void ckbLimitCrawling_CheckedChanged(object sender, EventArgs e)
        {
            CrawlerSettings.CanLimitCrawling = btnAddLinkPattern.Enabled = btnEditLinkPattern.Enabled = btnRemoveLinkPattern.Enabled = ckbLimitCrawling.Checked;
        }

        private void btnAddLinkPattern_Click(object sender, EventArgs e)
        {
            using (FrmAddLink f = new FrmAddLink())
            {
                if(f.ShowDialog() == DialogResult.OK)
                {
                    ListViewItem lvi = new ListViewItem()
                    {
                        Text = f.pattern
                    };

                    lvi.SubItems.Add($"{f.max}");
                    lvwLimitCrawling.Items.Add(lvi);
                }
            }
        }

        private void btnEditLinkPattern_Click(object sender, EventArgs e)
        {
            if(lvwLimitCrawling.SelectedItems.Count>0)
            {
                string pattern = lvwLimitCrawling.SelectedItems[0].Text;
                int max = int.Parse(lvwLimitCrawling.SelectedItems[0].SubItems[1].Text);
                using (FrmEditLink f = new FrmEditLink(pattern,max))
                {
                    if(f.ShowDialog() == DialogResult.OK)
                    {
                        lvwLimitCrawling.SelectedItems[0].Text = f.pattern;
                        lvwLimitCrawling.SelectedItems[0].SubItems[1].Text = $"{f.max}";
                    } 
                }
            }
        }

        private void btnRemoveLinkPattern_Click(object sender, EventArgs e)
        {
            if(lvwLimitCrawling.SelectedItems.Count>0)
            {
                lvwLimitCrawling.SelectedItems[0].Remove();
            }
        }

        private void ckbSearch_CheckedChanged(object sender, EventArgs e)
        {
            CrawlerSettings.CanSearch = btnAddSearchItem.Enabled = btnEditSearchItem.Enabled = btnRemoveSearchItem.Enabled = ckbSearch.Checked;
        }

        private void btnAddSearchItem_Click(object sender, EventArgs e)
        {
            using (FrmAddSearch f = new FrmAddSearch())
            {
                if(f.ShowDialog() == DialogResult.OK)
                {
                    ListViewItem lvi = new ListViewItem()
                    {
                        Text = f.source
                    };

                    lvi.SubItems.Add(f.pattern);

                    lvwSearch.Items.Add(lvi);
                }
            }
        }

        private void btnEditSearchItem_Click(object sender, EventArgs e)
        {
            if(lvwSearch.SelectedItems.Count>0)
            {
                string source = lvwSearch.SelectedItems[0].Text;
                string pattern = lvwSearch.SelectedItems[0].SubItems[1].Text;
                using (FrmEditSearch f = new FrmEditSearch(source, pattern))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        lvwSearch.SelectedItems[0].Text = f.source;
                        lvwSearch.SelectedItems[0].SubItems[1].Text = f.pattern;
                    }
                }
            }
        }

        private void btnRemoveSearchItem_Click(object sender, EventArgs e)
        {
            if(lvwSearch.SelectedItems.Count>0)
            {
                lvwSearch.SelectedItems[0].Remove();
            }
        }

        private void ckbFormSubmission_CheckedChanged(object sender, EventArgs e)
        {
            FormSubmissionSettings.CanSubmitForm = btnAddRule.Enabled = btnEditRule.Enabled = btnRemoveRule.Enabled = ckbFormSubmission.Checked;
        }

        private void txtUnMatchedValue_TextChanged(object sender, EventArgs e)
        {
            FormSubmissionSettings.UnMatchedField = txtUnMatchedValue.Text;
        }

        private void btnAddRule_Click(object sender, EventArgs e)
        {
            using (FrmAddFormItem f = new FrmAddFormItem())
            {
                if(f.ShowDialog() == DialogResult.OK)
                {
                    ListViewItem lvi = new ListViewItem()
                    {
                        Text = "Regex"
                    };

                    lvi.SubItems.Add(f.name);
                    lvi.SubItems.Add(f.value);

                    lvwFormSubmission.Items.Add(lvi);
                }
            }
        }

        private void btnEditRule_Click(object sender, EventArgs e)
        {
            if(lvwFormSubmission.SelectedItems.Count>0)
            {
                string name = lvwFormSubmission.SelectedItems[0].SubItems[1].Text;
                string value = lvwFormSubmission.SelectedItems[0].SubItems[2].Text;

                using (FrmEditFormItem f = new FrmEditFormItem(name, value))
                {
                    if(f.ShowDialog() == DialogResult.OK)
                    {
                        lvwFormSubmission.SelectedItems[0].SubItems[1].Text = f.name;
                        lvwFormSubmission.SelectedItems[0].SubItems[2].Text = f.value;
                    }
                }

            }
        }

        private void btnRemoveRule_Click(object sender, EventArgs e)
        {
            if(lvwFormSubmission.SelectedItems.Count>0)
            {
                lvwFormSubmission.SelectedItems[0].Remove();
            }
        }

        private void btnAddHeader_Click(object sender, EventArgs e)
        {
            using (FrmAddHeader f = new FrmAddHeader())
            {
                if(f.ShowDialog() == DialogResult.OK)
                {
                    lbxHeaders.Items.Add(f.header);
                }
            }
        }

        private void btnEditHeader_Click(object sender, EventArgs e)
        {
            if(lbxHeaders.SelectedItems.Count>0)
            {
                string header = lbxHeaders.SelectedItem.ToString();
                using (FrmEditHeader f = new FrmEditHeader(header))
                {
                    if(f.ShowDialog() == DialogResult.OK)
                    {
                        lbxHeaders.Items[lbxHeaders.SelectedIndex] = f.header;
                    }
                }
            }
        }

        private void btnRemoveHeader_Click(object sender, EventArgs e)
        {
            if(lbxHeaders.SelectedItems.Count>0)
            {
                lbxHeaders.Items.Remove(lbxHeaders.SelectedItem);
            }
        }

        public void MoveItem(int direction)
        {
            if (lbxHeaders.SelectedItem == null || lbxHeaders.SelectedIndex < 0)
                return;

            int newIndex = lbxHeaders.SelectedIndex + direction;
            if (newIndex < 0 || newIndex >= lbxHeaders.Items.Count)
                return;

            object selected = lbxHeaders.SelectedItem;
            lbxHeaders.Items.Remove(selected);
            lbxHeaders.Items.Insert(newIndex, selected);
            lbxHeaders.SetSelected(newIndex, true);
        }

        private void btnUpHeader_Click(object sender, EventArgs e)
        {
            MoveItem(-1);
        }

        private void btnDownHeader_Click(object sender, EventArgs e)
        {
            MoveItem(1);
        }

        private void btnRestoreHeaders_Click(object sender, EventArgs e)
        {
            lbxHeaders.Items.Clear();
            foreach(string header in HttpRequestSettings.DefaultHeaders)
            {
                lbxHeaders.Items.Add(header);
            }
        }

        private void ckbProxy_CheckedChanged(object sender, EventArgs e)
        {
            HttpRequestSettings.CanUseProxy = ckbProxy.Checked;
        }

        private void txtHost_TextChanged(object sender, EventArgs e)
        {
            HttpRequestSettings.ProxyHost = txtHost.Text;
        }

        private void txtPort_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(txtPort.Text, out int port);
            HttpRequestSettings.ProxyPort = port;
        }

        private void chbProxyAuth_CheckedChanged(object sender, EventArgs e)
        {
            HttpRequestSettings.CanUseAuthProxy = chbProxyAuth.Checked;
        }

        private void txtProxyUser_TextChanged(object sender, EventArgs e)
        {
            HttpRequestSettings.ProxyUsername = txtProxyUser.Text;
        }

        private void txtProxyPass_TextChanged(object sender, EventArgs e)
        {
            HttpRequestSettings.ProxyPassword = txtProxyPass.Text;
        }

        private void ckbAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            HttpRequestSettings.NeedAuthentication = ckbAuthentication.Checked;
        }

        private void txtAuthUser_TextChanged(object sender, EventArgs e)
        {
            HttpRequestSettings.AuthenticationUsername = txtAuthUser.Text;
        }

        private void txtAuthPass_TextChanged(object sender, EventArgs e)
        {
            HttpRequestSettings.AuthenticationPassword = txtAuthPass.Text;
        }

        private void cmbAuth_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpRequestSettings.AuthenticationType = cmbAuth.SelectedItem.ToString();
        }

        private void nudThreads_ValueChanged(object sender, EventArgs e)
        {
            ConnectionSettings.Threads = (short)nudThreads.Value;
        }

        private void nudRetries_ValueChanged(object sender, EventArgs e)
        {
            ConnectionSettings.Retries = (short)nudRetries.Value;
        }

        private void nudPauseTime_ValueChanged(object sender, EventArgs e)
        {
            ConnectionSettings.PauseTime = (short)nudPauseTime.Value;
        }
    }
}
