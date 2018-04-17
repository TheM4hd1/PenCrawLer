using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;
namespace PeNCrawLer_0._1._0.Core
{
    class CrawlerDataHolder
    {

        public TreeView trwCrawler;
        public ListView lvwCrawler;

        public object lockRequests = new object();

        public string targetUrl;

        public bool FirstCheckStatus;
        public bool IsPauseRequested = false;

        public HttpClient httpClient;

        public ManualResetEvent manualResetEvent;

        public List<string> visitedLinks;
        public List<string> downloadedLinks;
        public List<string> blackListExtentions;
        public List<string> whiteListExtentions;

        public Dictionary<string, int> LinksPatternAndMaxValue;
        public Dictionary<string, int> RetriesAndMaxValue;

        public CrawlerDataHolder()
        {
            visitedLinks = new List<string>();
            downloadedLinks = new List<string>();
            LinksPatternAndMaxValue = new Dictionary<string, int>();
            RetriesAndMaxValue = new Dictionary<string, int>();
            manualResetEvent = new ManualResetEvent(false);
        }

        public void UpdateExtentionsLists()
        {


            if (Advanced_Settings.CrawlerSettings.CanNotProcessExtentions)
            {
                blackListExtentions = Utilities.Helper.GetExtentionList(Advanced_Settings.CrawlerSettings.BlackListExtentions);
            }

            if (Advanced_Settings.CrawlerSettings.CanDownloadExtentions)
            {
                whiteListExtentions = Utilities.Helper.GetExtentionList(Advanced_Settings.CrawlerSettings.WhiteListExtentions);
            }

            Advanced_Settings.CrawlerSettings.HasToUpdateExtentionsList = false;
            

        }

        public void Pause()
        {
            manualResetEvent.WaitOne();
        }

        public void Resume()
        {
            IsPauseRequested = false;
            manualResetEvent.Set();
            manualResetEvent.Reset();
        }

    }
}
