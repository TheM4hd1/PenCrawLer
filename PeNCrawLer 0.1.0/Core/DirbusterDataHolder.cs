using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Net.Http;

namespace PeNCrawLer_0._1._0.Core
{
    class DirbusterDataHolder
    {

        public string[] DictionaryList;
        public string CharSet;
        public string SuccessCodes = string.Empty;
        public string FailureCodes = string.Empty;
        public string NotfoundPattern = string.Empty;
        public string NotfoundHtmlSource = string.Empty;

        public bool IsPauseRequested = false;
        public bool IsDefaultSettings = true;

        public int MaxLenght;
        public int MinLenght;

        public HttpClient httpClient;
        public List<string> fileExtentions;
        public List<string> visitedLinks;
        public ManualResetEvent manualResetEvent;
        public TreeView trwDirbuster;
        public ListView lvwDirbuster;

        public DirbusterDataHolder()
        {
            manualResetEvent = new ManualResetEvent(false);
            visitedLinks = new List<string>();
        }
        public bool ReadFile(string path)
        {

            Task<bool> t = Task<bool>.Factory.StartNew(() =>
            {
                try
                {
                    DictionaryList = File.ReadAllLines(path);

                    return true;
                }
                catch { return false; }
            });

            return t.Result;

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
