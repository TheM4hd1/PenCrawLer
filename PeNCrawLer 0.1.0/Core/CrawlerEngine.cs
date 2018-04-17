using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;

namespace PeNCrawLer_0._1._0.Core
{
    class CrawlerEngine
    {
        CrawlerDataHolder crawlerDataHolder;
        public CrawlerEngine(CrawlerDataHolder crawlerDataHolder)
        {
            this.crawlerDataHolder = crawlerDataHolder;
        }


        /// <summary>
        /// analysing target for first check.
        /// if we receive a null result, the crawling will fail,
        /// otherwise it will start crawling.
        /// </summary>
        /// <param name="targetUrl"></param>
        /// <returns>true if analyse successfully passed</returns>
        public bool AnalyseTarget(string targetUrl)
        {
            Utilities.RequestCalculator.ResetData();
            Utilities.RequestCalculator.IncreaseCrawler();

            string response = SendGetRequets(targetUrl);

            if(string.IsNullOrEmpty(response))
                return crawlerDataHolder.FirstCheckStatus = false;

            return crawlerDataHolder.FirstCheckStatus = true;

        }



        /// <summary>
        /// create a get request to receive the content of webpage.
        /// if http settings needs to update again, it will update here.
        /// also if the http request recives an error we can handle "retry" here,
        /// by passing the "Http timeout" alert.
        /// </summary>
        /// <param name="targetUrl"></param>
        /// <returns></returns>
        private string SendGetRequets(string targetUrl)
        {
            if(HttpRequests.HasToUpdateCrawler)
            {
                Core.HttpRequests._targetUrl = targetUrl;
                crawlerDataHolder.httpClient = Core.HttpRequests.UpdateHttpClient();
            }


            try
            {

                HttpClient client = crawlerDataHolder.httpClient;
                HttpResponseMessage httpResponse = client.GetAsync(targetUrl).Result;
                System.Threading.Thread.Sleep(100);

                return httpResponse.Content.ReadAsStringAsync().Result;

            }

            catch(HttpRequestException)
            {

                // send retry command
                return "Http timeout";

            }

            catch(Exception)
            {

                return null;  
            }
            
        }



        /// <summary>
        /// checking conditions for crawling. first of all we check
        /// for settings, if settings changes, we need to update them.
        /// then we check for blacklist extentions and whitelist extentions(for download)
        /// if the link limitter is enable, we have to check links with given patterns.
        /// and finally if the conditions for crawling are true, the given url will be crawled.
        /// </summary>
        /// <param name="targetUrl"></param>
        public void Crawl(string targetUrl)
        {


            lock (crawlerDataHolder.lockRequests)
            {
                Utilities.RequestCalculator.IncreaseCrawler();
            }

            bool allowCrawl_FirstTest = true;
            bool allowCrawl_SecondTest = true;
            bool downloadFlag = false;

            if (crawlerDataHolder.IsPauseRequested)
            {

                crawlerDataHolder.Pause();

            }

            if (Advanced_Settings.CrawlerSettings.HasToUpdateExtentionsList)
            {

                crawlerDataHolder.UpdateExtentionsLists();

            }

            if (Advanced_Settings.CrawlerSettings.CanNotProcessExtentions)
            {


                foreach (string extention in crawlerDataHolder.blackListExtentions)
                {
                    if (targetUrl.EndsWith(extention))
                    {
                        allowCrawl_FirstTest = false;
                    }
                }


            }

            if (Advanced_Settings.CrawlerSettings.CanDownloadExtentions)
            {

                foreach (string extention in crawlerDataHolder.whiteListExtentions)
                {
                    if (targetUrl.EndsWith(extention) && crawlerDataHolder.downloadedLinks.IndexOf(targetUrl) < 0)
                    {
                        downloadFlag = true;
                        crawlerDataHolder.downloadedLinks.Add(targetUrl);
                        break;
                    }
                }

            }

            if (allowCrawl_FirstTest && Advanced_Settings.CrawlerSettings.CanLimitCrawling)
            {

                allowCrawl_SecondTest = false;
                foreach (KeyValuePair<string, int> limitedLink in Advanced_Settings.CrawlerSettings.LinksPatternAndMaxValue)
                {
                    if (Regex.IsMatch(targetUrl, limitedLink.Key))
                    {
                        if (crawlerDataHolder.LinksPatternAndMaxValue.ContainsKey(targetUrl))
                        {
                            if (crawlerDataHolder.LinksPatternAndMaxValue[targetUrl] < limitedLink.Value)
                            {
                                crawlerDataHolder.LinksPatternAndMaxValue[targetUrl]++;
                                allowCrawl_SecondTest = true;
                                break;
                            }
                        }

                        else
                        {
                            crawlerDataHolder.LinksPatternAndMaxValue.Add(targetUrl, 1);
                            allowCrawl_SecondTest = true;
                            break;
                        }
                    }
                    // agar ba harkodom match bod , indexesh ro check kone , age kochiktar bod be index ezafe kone allowcrawl=true & brek |

                }

            }

            if (downloadFlag)
            {
                try
                {
                    HttpClient client = crawlerDataHolder.httpClient;
                    HttpResponseMessage response = client.GetAsync(targetUrl).Result;
                    using (Stream stream = response.Content.ReadAsStreamAsync().Result)
                    {
                        using (var fileStream = new FileStream($"{Advanced_Settings.CrawlerSettings.PathToSave}\\{Path.GetFileName(targetUrl)}", FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
                catch { };

            }

            //else
            //{


            if (allowCrawl_FirstTest && allowCrawl_SecondTest && crawlerDataHolder.visitedLinks.IndexOf(targetUrl) < 0)
            {

                crawlerDataHolder.visitedLinks.Add(targetUrl);

            }

            else
            {

                allowCrawl_SecondTest = false;

            }

            if (allowCrawl_FirstTest && allowCrawl_SecondTest)
            {
                bool isFile;
                try
                {
                    isFile = Utilities.Helper.GetFileSize(new Uri(targetUrl));
                }
                catch { isFile = false; };
                PopulateCrawlerTree(targetUrl, isFile);

                if (Advanced_Settings.CrawlerSettings.CanRenderJavascript)
                {

                    RenderJavaScript(targetUrl);

                }

                else
                {

                    string response = SendGetRequets(targetUrl);
                    if (!string.IsNullOrEmpty(response))
                    {

                        if (response.Equals("Http timeout"))
                        {

                            if (crawlerDataHolder.RetriesAndMaxValue.ContainsKey(targetUrl))
                            {
                                if (crawlerDataHolder.RetriesAndMaxValue[targetUrl] < Advanced_Settings.ConnectionSettings.Retries)
                                {
                                    crawlerDataHolder.RetriesAndMaxValue[targetUrl]++;
                                    crawlerDataHolder.visitedLinks.Remove(targetUrl);
                                    Crawl(targetUrl);
                                }
                            }

                            else
                            {
                                crawlerDataHolder.RetriesAndMaxValue.Add(targetUrl, 1);
                                crawlerDataHolder.visitedLinks.Remove(targetUrl);
                                Crawl(targetUrl);
                            }

                        }

                        ExtractLinksAndProcessing(response, targetUrl);
                        System.Threading.Thread.Sleep((int)TimeSpan.FromSeconds(Advanced_Settings.ConnectionSettings.PauseTime).TotalMilliseconds);

                    }


                }


            }


            //}


        }



        /// <summary>
        /// rendering webpage by webbrowser.
        /// we need to set the SetApartmentState of thread to ApartmentState.STA
        /// otherwise we can not run webbrowser in a thread.
        /// </summary>
        /// <param name="targetUrl"></param>
        private void RenderJavaScript(string targetUrl)
        {

            var th = new System.Threading.Thread(() =>
            {
                var wb = new WebBrowser();
                wb.DocumentCompleted += Wb_DocumentCompleted;
                wb.ScriptErrorsSuppressed = true;
                wb.Navigate(targetUrl);
                Application.Run();
            });

            th.SetApartmentState(System.Threading.ApartmentState.STA);
            th.Start();

        }
        private void Wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser w = (WebBrowser)sender;
            ExtractLinksAndProcessing(w.DocumentText, w.Url.AbsolutePath);
            Application.ExitThread();
        }

        

        /// <summary>
        /// extracting links from http content by given elements and pattern.
        /// also we will check here for formsubmissiona and given data to search.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="targetUrl"></param>
        private void ExtractLinksAndProcessing(string response,string targetUrl)
        {

            if (Advanced_Settings.FormSubmissionSettings.CanSubmitForm)
            {
                using (CrawlerFormSubmission formSubmission = new CrawlerFormSubmission(response,targetUrl,crawlerDataHolder.httpClient))
                {
                    string res = formSubmission.Submit();
                    if(!string.IsNullOrEmpty(res))
                    {
                        response = string.Concat(response, res);
                    }
                }
            }

            if (Advanced_Settings.CrawlerSettings.CanSearch)
            {
                SearchForData(response, targetUrl);
            }

            if (Advanced_Settings.CrawlerSettings.CanExtractLinkByHtmlElement)
            {

                List<Task> crawlerTasks = new List<Task>();

                foreach (string link in Utilities.Extractor.ExtractAttributesByTagName(response,targetUrl))
                {

                    Task t = new Task(() =>
                    {
                        Crawl(link);
                    });

                    crawlerTasks.Add(t);

                }

                for(int i=0;i<crawlerTasks.Count;i++)
                {
                    crawlerTasks[i].Start();
                }
                Task.WaitAll(crawlerTasks.ToArray());

            }

            if (Advanced_Settings.CrawlerSettings.CanExtractLinkByRegex)
            {

                List<Task> crawlerTasks = new List<Task>();

                foreach (string link in Utilities.Extractor.ExtractLinksByRegex(response,targetUrl))
                {

                    Task t = new Task(() =>
                    {
                        Crawl(link);
                    });

                    crawlerTasks.Add(t);

                }

                for (int i = 0; i < crawlerTasks.Count; i++)
                {
                    crawlerTasks[i].Start();
                }
                Task.WaitAll(crawlerTasks.ToArray());
            }

        }
        private void SearchForData(string response, string targetUrl)
        {
            foreach (KeyValuePair<char, string> itemToSearch in Advanced_Settings.CrawlerSettings.SearchInputAndValue)
            {
                if (itemToSearch.Key == 'U') // search in url
                {
                    if (Regex.IsMatch(targetUrl, itemToSearch.Value))
                    {
                        MatchCollection matches = Regex.Matches(targetUrl, itemToSearch.Value);
                        UpdateSearchedItems(targetUrl, matches, "url");
                    }
                }

                if (itemToSearch.Key == 'R') // search in response
                {
                    if (Regex.IsMatch(response, itemToSearch.Value))
                    {
                        MatchCollection matches = Regex.Matches(response, itemToSearch.Value);
                        UpdateSearchedItems(targetUrl, matches, "response");
                    }
                }
            }
        }



        /// <summary>
        /// updating UI elements
        /// </summary>
        /// <param name="s"></param>
        /// <param name="isFile"></param>
        private void PopulateCrawlerTree(string s,bool isFile)
        {
            int imageIndex = 1;
            char pathSeparator = '/';
            
            crawlerDataHolder.trwCrawler.Invoke((MethodInvoker)delegate
            {
                TreeNode lastNode = null;
                string subPathAgg;

                string path = s;
                if (s.StartsWith("http://"))
                {
                    path = s.Replace("http://", "");
                }
                if (s.StartsWith("https://"))
                {
                    path = s.Replace("https://", "");
                }
                //path = path.Replace("?", "/");
                //path = path.Replace("&", "/");
                subPathAgg = string.Empty;

                var lastItem = path.Split(pathSeparator).Last();
                foreach (string subPath in path.Split(pathSeparator))
                {
                    imageIndex = 1;
                    
                    if (subPath.Contains("?") || subPath.Contains("&") )
                    {
                        imageIndex = 0;
                    }
                    else
                    {
                        imageIndex = 1;
                    }

                    if (subPath.Equals(lastItem) && isFile)
                    {
                        imageIndex = 0;
                    }

                    subPathAgg += subPath + pathSeparator;
                    TreeNode[] nodes = crawlerDataHolder.trwCrawler.Nodes.Find(subPathAgg, true);
                    if (nodes.Length == 0)
                        if (lastNode == null)
                            lastNode = crawlerDataHolder.trwCrawler.Nodes.Add(subPathAgg, subPath, 2);
                        else
                        {
                            //imageIndex = 1;
                            lastNode = lastNode.Nodes.Add(subPathAgg, subPath, imageIndex);
                        }
                    else
                    {
                        //imageIndex = 0;
                        lastNode = nodes[0];
                    }
                }

                lastNode = null; 
            });
        }
        private void UpdateSearchedItems(string url,MatchCollection matches,string resource)
        {
            crawlerDataHolder.lvwCrawler.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                foreach(Match m in matches)
                {
                    System.Windows.Forms.ListViewItem lvi = new System.Windows.Forms.ListViewItem()
                    {
                        Text = $"{crawlerDataHolder.lvwCrawler.Items.Count}"
                    };

                    lvi.SubItems.Add(resource);
                    lvi.SubItems.Add(m.Value);
                    lvi.SubItems.Add(url);

                    crawlerDataHolder.lvwCrawler.Items.Add(lvi);
                }
            });
        }
    }
}
