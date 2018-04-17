using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PeNCrawLer_0._1._0.Utilities;
using System.Threading;
using System.Net.Http;

namespace PeNCrawLer_0._1._0
{
    public partial class FrmMain : Form
    {

        #region Definition
        public static DateTime StartTime;
        FrmSettings frmSettings;
        Task taskCrawler;

        Core.CrawlerEngine crawlerEngine;
        Core.CrawlerDataHolder crawlerDataHolder;
        bool isCrawlerStarted = false;

        Core.DirbusterEngine dirbusterEngine;
        Core.DirbusterEngine.RequestMethod requestMethod = Core.DirbusterEngine.RequestMethod.HEAD_AND_GET;
        Core.DirbusterDataHolder dirbusterDataHolder;
        bool isDirbusterStarted = false;
        string charset = string.Empty;
        #endregion

        public FrmMain()
        {
            InitializeComponent();
            frmSettings = new FrmSettings();
        }


        #region Crawler
        private void timerCrawler_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsedTime = DateTime.Now - StartTime;
            tslCrawlerTime.Text = $"{elapsedTime.Days} Days, {elapsedTime.Hours} Hours, {elapsedTime.Minutes} Min, {elapsedTime.Seconds} Sec";
            tslCrawlerRequests.Text = $"{RequestCalculator.CalculateCrawlerSpeed()} requests/sec";

            //tslCrawler.Text = taskCrawler.Status.ToString();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSettings.Show();
        }

        private void tsbStartCrawler_Click(object sender, EventArgs e)
        {
            tslCrawler.ForeColor = Color.Black;

            if(!isCrawlerStarted)
            {

                string target = Helper.FixStartUrl(txtUrl.Text);
                txtUrl.Text = target;
                if (!Uri.IsWellFormedUriString(target, UriKind.Absolute))
                {
                    MessageBox.Show("Invalid Url Address.", "PeNCrawLer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                crawlerDataHolder = new Core.CrawlerDataHolder()
                {
                    targetUrl = txtUrl.Text,
                    trwCrawler = this.trwCrawler,
                    lvwCrawler = this.lvwCrawler
                };

                if(!Advanced_Settings.CrawlerSettings.IsCustomizeSettings)
                {
                    crawlerDataHolder.httpClient = new HttpClient();
                    Advanced_Settings.CrawlerSettings.HtmlTagAndAttribute = new Dictionary<string, string>();
                    Advanced_Settings.CrawlerSettings.HtmlTagAndAttribute.Add("a", "href");
                    Advanced_Settings.CrawlerSettings.HtmlTagAndAttribute.Add("link", "href");

                }

                crawlerEngine = new Core.CrawlerEngine(crawlerDataHolder);
                isCrawlerStarted = true;

                tslCrawler.Text = "Processing ...";
                StartTime = DateTime.Now;
                timerCrawler.Start();

                new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                {

                    taskCrawler = new Task(() =>
                    {


                        if (crawlerEngine.AnalyseTarget(crawlerDataHolder.targetUrl))
                        {
                            crawlerEngine.Crawl(crawlerDataHolder.targetUrl);
                        }

                        else
                        {
                            crawlerDataHolder.FirstCheckStatus = false;
                        }


                    });

                    taskCrawler.Start();
                    taskCrawler.Wait();

                    if (crawlerDataHolder.FirstCheckStatus)
                    {

                        toolStrip1.Invoke((MethodInvoker)delegate
                        {
                            tslCrawler.ForeColor = Color.Green;
                        });

                        tslCrawler.Text = "Operation Successfully Completed.";
                        tslCrawlerRequests.Text = $"Total number of processed requests: {RequestCalculator.crawlerRequests}";

                    }

                    else
                    {

                        toolStrip1.Invoke((MethodInvoker)delegate
                        {
                            tslCrawler.ForeColor = Color.Red;
                        });

                        tslCrawler.Text = "The Operation Failed!";

                    }

                    timerCrawler.Stop();

                }))

                {
                    IsBackground = true
                }.Start();
            }
            
            else
            {

                if (crawlerDataHolder.IsPauseRequested)
                {
                    tslCrawler.Text = "Processing ...";
                    timerCrawler.Start();
                    crawlerDataHolder.Resume();
                }

                else
                {

                    DialogResult result = MessageBox.Show("Crawler is currently running\nAre you sure you want to start new one?", "PeNCrawLer 0.1.0 - Crawler", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (result == DialogResult.Yes)
                    {

                        // cancel the task
                        isCrawlerStarted = false;

                    }

                    else if (result == DialogResult.No)
                    {

                        return;

                    }

                }

            }
            

        }

        private void tsbPauseCrawler_Click(object sender, EventArgs e)
        {
            try
            {
                crawlerDataHolder.IsPauseRequested = true;
                timerCrawler.Stop();
                tslCrawler.Text = "Paused.";
            }
            catch { return; };
        }

        private void trwCrawler_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
        }

        private void valueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvwCrawler.SelectedItems.Count > 0)
            {
                Clipboard.SetText(lvwCrawler.SelectedItems[0].SubItems[2].Text);
            }
        }

        private void urlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvwCrawler.SelectedItems.Count > 0)
            {
                Clipboard.SetText(lvwCrawler.SelectedItems[0].SubItems[3].Text);
            }
        }

        private void copyFullPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = trwCrawler.SelectedNode;
            if(node != null)
                Clipboard.SetText(node.FullPath);
        }
        #endregion

        #region DirBuster
        private void rbnDicAttack_CheckedChanged(object sender, EventArgs e)
        {
            txtPath.Enabled = btnOpenPath.Enabled = rbnDicAttack.Checked;
        }

        private void rbnBruteAttack_CheckedChanged(object sender, EventArgs e)
        {
            cmbCharset.Enabled = txtMin.Enabled = txtMax.Enabled = rbnBruteAttack.Checked;
        }

        private void ckbBruteDirs_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void ckbBruteFiles_CheckedChanged(object sender, EventArgs e)
        {
            txtFileExtentions.Enabled = ckbBruteFiles.Checked;
        }

        private void ckbDetection_CheckedChanged(object sender, EventArgs e)
        {
            txtSuccessCodes.Enabled = txtFailureCodes.Enabled = txtDetectionPattern.Enabled = txtDetectionHtml.Enabled = btnCheckDetectionPattern.Enabled = !ckbDetection.Checked;
        }

        private void btnOpenPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "Select Dictionary File",
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = ofd.FileName;
            }

        }

        private void tsbStartDirbuster_Click(object sender, EventArgs e)
        {

            tslDirbuster.ForeColor = Color.Black;

            if (!isDirbusterStarted)
            {

                if (!ckbBruteDirs.Checked && !ckbBruteFiles.Checked)
                {
                    MessageBox.Show("You cant not choose to bruteforce neither files and dirs.", "PeNCrawLer - DirBuster", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                txtUrl.Text = Helper.FixStartUrl(txtUrl.Text);
                dirbusterDataHolder = new Core.DirbusterDataHolder
                {

                    trwDirbuster = trwDirbuster,
                    lvwDirbuster = lvwDirbuster,
                    fileExtentions = Helper.GetExtentionList(txtFileExtentions.Text),
                    IsDefaultSettings = ckbDetection.Checked,
                    SuccessCodes = txtSuccessCodes.Text,
                    FailureCodes = txtFailureCodes.Text,
                    NotfoundHtmlSource = txtDetectionHtml.Text

                };
                dirbusterEngine = new Core.DirbusterEngine(txtUrl.Text, txtStartFrom.Text, ckbBruteDirs.Checked, ckbBruteFiles.Checked, ckbRecursive.Checked, requestMethod)
                {

                    dirbusterDataHolder = dirbusterDataHolder

                };

                if (rbnBruteAttack.Checked)
                {

                    try
                    {
                        dirbusterEngine.dirbusterDataHolder.MinLenght = int.Parse(txtMin.Text);
                        dirbusterEngine.dirbusterDataHolder.MaxLenght = int.Parse(txtMax.Text);
                        dirbusterEngine.dirbusterDataHolder.CharSet = charset;
                    }
                    catch(FormatException) { MessageBox.Show("Please enter a correct number", "PeNCrawLer - DirBuster", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                    catch(ArgumentNullException) { MessageBox.Show("Lenght can not be nulled.", "PeNCrawLer - DirBuster", MessageBoxButtons.OK, MessageBoxIcon.Error);return; }
                    catch(OverflowException ofx) { MessageBox.Show(ofx.Message, "PeNCrawLer - DirBuster", MessageBoxButtons.OK, MessageBoxIcon.Error); return; };

                    StartTime = DateTime.Now;
                    timerDirbuster.Start();
                    new Thread(new ThreadStart(() =>
                    {

                        Task t = new Task(() =>
                        {
                            dirbusterEngine.StartAttack(Core.DirbusterEngine.AttackType.Bruteforce);
                        });
                        
                        t.Start();
                        t.Wait();
                        timerDirbuster.Stop();

                        toolStrip2.Invoke((MethodInvoker)delegate
                        {
                            tslDirbuster.ForeColor = Color.Green;
                            tslDirbuster.Text = "Operation Successfully Completed.";
                            tslDirbusterRequests.Text = $"Total number of processed requests: {RequestCalculator.dirbusterRequests}";
                        });
                    }))
                    {
                        IsBackground = true
                    }.Start();
                    

                }
                else //if(rbnDicAttack.Checked)
                {
                    tslDirbuster.Text = "Reading file ...";
                    if (dirbusterDataHolder.ReadFile(txtPath.Text))
                    {
                        tslDirbuster.Text = $"Dictionary Lenght: {dirbusterDataHolder.DictionaryList.Length}";
                    }

                    else
                    {
                        tslDirbuster.ForeColor = Color.Red;
                        tslDirbuster.Text = "Error in reading file.";
                        return;
                    }

                    timerDirbuster.Start();
                    StartTime = DateTime.Now;
                    new Thread(new ThreadStart(() =>
                    {

                        Task t = new Task(() =>
                        {
                            dirbusterEngine.StartAttack(Core.DirbusterEngine.AttackType.Dictionary);
                        });

                        t.Start();
                        t.Wait();
                        timerDirbuster.Stop();

                        toolStrip2.Invoke((MethodInvoker)delegate
                        {
                            tslDirbuster.ForeColor = Color.Green;
                            tslDirbuster.Text = "Operation Successfully Completed.";
                            tslDirbusterRequests.Text = $"Total number of processed requests: {RequestCalculator.dirbusterRequests}";
                        });

                    }))
                    {
                        IsBackground = true
                    }.Start();
                    

                }

                isDirbusterStarted = true;

            }

            else // Resume or Abort current task
            {

                if(dirbusterDataHolder.IsPauseRequested)
                {

                    dirbusterDataHolder.IsPauseRequested = false;
                    dirbusterDataHolder.Resume();
                    timerDirbuster.Start();
                }

                else // Create new task
                {

                    DialogResult result = MessageBox.Show("DirBuster is currently running\nAre you sure you want to start new one?", "PeNCrawLer 0.1.0 - DirBuster", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if(result == DialogResult.Yes)
                    {

                        tslDirbuster.Text = "DirBuster is free now, You can create new task";
                        isDirbusterStarted = false;

                    }

                    else if(result == DialogResult.No)
                    {

                        return;

                    }

                }

            }
            
        }

        private void tsbPauseDirbuster_Click(object sender, EventArgs e)
        {
            try
            {
                dirbusterEngine.dirbusterDataHolder.IsPauseRequested = true;
                timerDirbuster.Stop();
                tslDirbuster.Text = "Paused";
            }
            catch { return; }
        }

        private void timerDirbuster_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsedTime = DateTime.Now - StartTime;
            tslDirbuster.Text = $"{elapsedTime.Days} Days, {elapsedTime.Hours} Hours, {elapsedTime.Minutes} Min, {elapsedTime.Seconds} Sec";
            tslDirbusterRequests.Text = $"{RequestCalculator.CalculateDirbusterSpeed()} requests/sec";
        }

        private void cmbCharset_Click(object sender, EventArgs e)
        {
            using (var f = new Forms.FrmCharset())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    cmbCharset.Text = f.textToShow;
                    charset = f.charset;
                }
            }
        }

        private void ckbRecursive_CheckedChanged(object sender, EventArgs e)
        {
            txtStartFrom.Enabled = ckbRecursive.Checked;
        }

        private void rbnGet_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnGet.Checked)
            {
                requestMethod = Core.DirbusterEngine.RequestMethod.GET;
            }
        }

        private void rbnAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnAuto.Checked)
            {
                requestMethod = Core.DirbusterEngine.RequestMethod.HEAD_AND_GET;
            }
        }

        private void btnCheckDetectionPattern_Click(object sender, EventArgs e)
        {
            if (Helper.IsValidPattern(txtDetectionPattern.Text))
            {
                MessageBox.Show("Pattern is valid.", "PeNCrawLer - DirBuster", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Invalid input pattern.", "PeNCrawLer - DirBuster", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void removeFromQueueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvwDirbuster.SelectedItems.Count > 0)
            {
                string urlAsKey = lvwDirbuster.SelectedItems[0].SubItems[1].Text;

                if (dirbusterEngine.CancelBruteforceOnDirectory(urlAsKey))
                    lvwDirbuster.SelectedItems[0].SubItems[4].Text = "Canceling ...";
                else
                    MessageBox.Show("Its not possible to cancel this task.", "PeNCrawLer - DirBuster", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void copyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvwDirbuster.SelectedItems.Count > 0)
            {
                Clipboard.SetText(lvwDirbuster.SelectedItems[0].SubItems[1].Text);
            }
        }




        #endregion

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Forms.FrmAbout().Show();
        }

        private void saveReportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Utilities.Report.MakeReport(Report.ReportType.DirBuster, null, lvwDirbuster);
        }

        private void saveReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utilities.Report.MakeReport(Report.ReportType.SearchedData, null, lvwCrawler);
        }

        private void saveReportToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Utilities.Report.MakeReport(Report.ReportType.Crawler, trwCrawler, null);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvwCrawler.Items.Clear();
        }

        private void removeDuplicatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tags = new HashSet<string>();
            var duplicates = new List<ListViewItem>();

            foreach (ListViewItem item in lvwDirbuster.Items)
            {
                // HashSet.Add() returns false if it already contains the key.
                if (!tags.Add(item.SubItems[1].Text))
                    duplicates.Add(item);
            }

            foreach (ListViewItem item in duplicates)
            {
                lvwDirbuster.Items.Remove(item);
            }
        }
    }
}
