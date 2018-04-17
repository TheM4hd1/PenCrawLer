using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace PeNCrawLer_0._1._0.Core
{
    class DirbusterEngine
    {
        #region Vars
        string targetUrl = string.Empty, startPath = string.Empty;
        bool bruteDirs, bruteFiles, recursiveMode;
        RequestMethod requestMethod;
        AttackType attackType;
        Dictionary<string, CancellationTokenSource> taskWorkingDirectory = new Dictionary<string, CancellationTokenSource>();
        CancellationToken token = new CancellationToken();
        public DirbusterDataHolder dirbusterDataHolder;

        public enum AttackType
        {
            Bruteforce,
            Dictionary
        }

        public enum RequestMethod
        {
            GET,
            HEAD_AND_GET
        }
#endregion

        public DirbusterEngine(string targetUrl, string startPath, bool bruteDirs, bool bruteFiles, bool recursiveMode, RequestMethod requestMethod)
        {

            this.targetUrl = targetUrl;
            this.startPath = startPath;
            this.bruteDirs = bruteDirs;
            this.bruteFiles = bruteFiles;
            this.recursiveMode = recursiveMode;
            this.requestMethod = requestMethod;

        }

        public void StartAttack(AttackType attackType)
        {
            this.attackType = attackType;
            if (!Advanced_Settings.CrawlerSettings.IsCustomizeSettings) // User didnt select custom settings
            {
                dirbusterDataHolder.httpClient = new HttpClient();
            }
            else // Default Settings.
            {
                dirbusterDataHolder.httpClient = Core.HttpRequests.UpdateHttpClient();
            }

            if(recursiveMode)
            {
                Uri uri = new Uri(targetUrl);
                targetUrl = $"{uri.Scheme}://{uri.Host}{startPath}";
                var localPaths = Utilities.Helper.GetDirectoryPaths(targetUrl);

                Parallel.ForEach(localPaths, new ParallelOptions { MaxDegreeOfParallelism = Advanced_Settings.ConnectionSettings.Threads }, path =>
                {
                    if (DirectoryExists(path, string.Empty))
                    {
                        Task t = null;
                        try
                        {
                            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                            CancellationToken cancellationToken = cancellationTokenSource.Token;

                            t = new Task(() =>
                            {
                                if (bruteFiles)
                                    ScanFiles(path, cancellationToken, this.attackType);
                                if (bruteDirs)
                                    WalkOnDirectory(path, this.attackType);
                            });

                            taskWorkingDirectory.Add(path, cancellationTokenSource);
                            t.Start();
                            t.Wait(cancellationToken);
                            //t.Dispose();
                        }
                        catch { }
                        finally
                        {
                            taskWorkingDirectory.Remove(targetUrl);
                            //t.Dispose();
                        }
                    }
                });
            }

            else
            {
                if (DirectoryExists($"{targetUrl}/", null))
                {
                    if (attackType == AttackType.Dictionary)
                    {
                        Parallel.ForEach(dirbusterDataHolder.DictionaryList.ToList(), new ParallelOptions { MaxDegreeOfParallelism = Advanced_Settings.ConnectionSettings.Threads }, dictionaryItem =>
                        {
                            if (bruteDirs)
                            {
                                if (DirectoryExists(targetUrl, dictionaryItem))
                                {
                                    Task t = null;
                                    try
                                    {
                                        var cancellationTokenSource = new CancellationTokenSource();
                                        var cancellationToken = cancellationTokenSource.Token;

                                        t = new Task(() =>
                                        {
                                            if (bruteFiles)
                                                ScanFiles($"{targetUrl}/{dictionaryItem}", cancellationToken, this.attackType);
                                            WalkOnDirectory($"{targetUrl}/{dictionaryItem}", this.attackType);
                                        });

                                        taskWorkingDirectory.Add($"{targetUrl}/{dictionaryItem}", cancellationTokenSource);
                                        t.Start();
                                        t.Wait(cancellationToken);
                                    }
                                    catch { }
                                    finally
                                    {
                                        taskWorkingDirectory.Remove($"{targetUrl}/{dictionaryItem}");
                                        //t.Dispose();
                                    }
                                }
                            }

                            if (bruteFiles)
                            {
                                Parallel.ForEach(dirbusterDataHolder.fileExtentions, new ParallelOptions { MaxDegreeOfParallelism = Advanced_Settings.ConnectionSettings.Threads }, extention =>
                                {
                                    string fullName = $"{dictionaryItem}{extention}";
                                    FileExists(targetUrl, fullName);
                                });
                            }
                        });
                    }

                    else if (attackType == AttackType.Bruteforce)
                    {
                        Parallel.For(dirbusterDataHolder.MinLenght, dirbusterDataHolder.MaxLenght + 1, new ParallelOptions { MaxDegreeOfParallelism = Advanced_Settings.ConnectionSettings.Threads }, i =>
                        {
                            if (bruteDirs)
                                Bruteforce(string.Empty, 0, i, i, true, targetUrl);

                            if (bruteFiles)
                                Bruteforce(string.Empty, 0, i, i, false, targetUrl);
                        });
                    }
                }
            }

        }

        private bool DirectoryExists(string url, string dirName)
        { // Only GET Method
            string urlToCheck = string.Empty;

            if(string.IsNullOrEmpty(dirName))
                urlToCheck = url;
            else
                urlToCheck = $"{url}/{dirName}";

            //urlToCheck = urlToCheck.TrimEnd('/');
            if (dirbusterDataHolder.visitedLinks.Contains(urlToCheck))
                return false;

            Thread.Sleep(Advanced_Settings.ConnectionSettings.PauseTime); // Wait Before Create and Send Request.
            while(dirbusterDataHolder.IsPauseRequested)
            {
                Application.DoEvents();
            }          
            Utilities.RequestCalculator.IncreaseDirbuster();
            HttpClient client = dirbusterDataHolder.httpClient;
            try
            { 
                HttpResponseMessage httpResponse = client.GetAsync(urlToCheck).Result;
                if (dirbusterDataHolder.IsDefaultSettings)
                {
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        dirbusterDataHolder.visitedLinks.Add(urlToCheck);
                        if (IsDirectoryListingEnable(urlToCheck, string.Empty, httpResponse))
                        {
                            GetDirectoryItems(urlToCheck, httpResponse);
                            return false;
                        }

                        else
                        {
                            UpdateTreeView(urlToCheck, true);
                            UpdateListView(urlToCheck, "Directory", string.Empty, "In Queue...", $"{(int)httpResponse.StatusCode}");
                            return true;
                        }
                    }
                }

                else // Its custom settings
                {
                    if (dirbusterDataHolder.SuccessCodes.Contains(((int)httpResponse.StatusCode).ToString())) // Directory Exist.
                    {
                        if (dirbusterDataHolder.visitedLinks.IndexOf(urlToCheck) < 0)
                            dirbusterDataHolder.visitedLinks.Add(urlToCheck);

                        if (IsDirectoryListingEnable(url, dirName, httpResponse))
                        {
                            GetDirectoryItems($"{url}/{dirName}", httpResponse);
                            return false;
                        }

                        else if (bruteFiles)
                        {
                            UpdateTreeView(urlToCheck, true);
                            UpdateListView(urlToCheck, "Directory", string.Empty, "Bureforcing directory...", $"{(int)httpResponse.StatusCode}");
                            return true;
                        }
                    }

                    if (dirbusterDataHolder.FailureCodes.Contains(((int)httpResponse.StatusCode).ToString()))
                    {
                        return false;
                    }

                    if (dirbusterDataHolder.NotfoundHtmlSource.ToLower().Equals(httpResponse.Content.ReadAsStringAsync().Result.ToLower()))
                    {
                        return false;
                    }

                    if (System.Text.RegularExpressions.Regex.IsMatch(httpResponse.Content.ReadAsStringAsync().Result, dirbusterDataHolder.NotfoundPattern))
                    {
                        return false;
                    }
                }
            }
            catch { };

            return false;
        } 

        private void WalkOnDirectory(string url,AttackType attackType)
        {
            UpdateListViewDirectoryStatus(url, "Bruteforcing On Directory...");
            //if (url.EndsWith("/"))
                //url = url.TrimEnd('/');//.Remove(url.Length - 1);

            if(attackType == AttackType.Dictionary)
            {
                Parallel.ForEach(dirbusterDataHolder.DictionaryList, new ParallelOptions { MaxDegreeOfParallelism = Advanced_Settings.ConnectionSettings.Threads }, dir =>
                {
                    if (DirectoryExists(url, dir))
                    {
                        new Thread(new ThreadStart(() =>
                        {
                            Task t = null;
                            try
                            {
                                var cancellationTokenSource = new CancellationTokenSource();
                                var cancellationToken = cancellationTokenSource.Token;

                                t = new Task(() =>
                                {
                                    if (bruteFiles)
                                        ScanFiles($"{url}/{dir}", cancellationToken, this.attackType);
                                    WalkOnDirectory($"{url}/{dir}", this.attackType);
                                });

                                taskWorkingDirectory.Add($"{url}/{dir}", cancellationTokenSource);
                                t.Start();
                                t.Wait(cancellationToken);

                            }
                            catch (Exception) { }
                            finally
                            {
                                //t.Dispose();
                                taskWorkingDirectory.Remove($"{url}/{dir}");
                            }
                        }))
                        {
                            IsBackground = true
                        }.Start();
                    }
                });
            }

            else if(attackType == AttackType.Bruteforce)
            {
                Parallel.For(dirbusterDataHolder.MinLenght, dirbusterDataHolder.MaxLenght + 1, new ParallelOptions { MaxDegreeOfParallelism = Advanced_Settings.ConnectionSettings.Threads }, i =>
                {
                    Bruteforce("", 0, i, i, true, url);
                });
            }

            UpdateListViewDirectoryStatus(url);
        }

        private bool FileExists(string url, string fileName)
        {
            string urlToCheck = string.Empty;
            if (string.IsNullOrEmpty(fileName))
                urlToCheck = url;
            else
                urlToCheck = $"{url}/{fileName}";

            Thread.Sleep(Advanced_Settings.ConnectionSettings.PauseTime); // wait before sending request
            while (dirbusterDataHolder.IsPauseRequested)
            {
                Application.DoEvents();
            }

            if(requestMethod == RequestMethod.GET)
            {
                try
                {
                    Utilities.RequestCalculator.IncreaseDirbuster();
                    //HttpRequests._targetUrl = $"{url}/{fileName}";
                    HttpClient client = dirbusterDataHolder.httpClient;
                    HttpResponseMessage httpResponse = client.GetAsync(urlToCheck).Result;

                    if(dirbusterDataHolder.IsDefaultSettings)
                    {
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            UpdateTreeView(urlToCheck, false);
                            UpdateListView(urlToCheck, "File", System.IO.Path.GetExtension(urlToCheck), $"{httpResponse.Content.Headers.ContentLength} bytes", $"{(int)httpResponse.StatusCode}");
                            return true;
                        }
                    }

                    else // Its custom settings 
                    {
                        if(dirbusterDataHolder.SuccessCodes.Contains(((int)httpResponse.StatusCode).ToString()))
                        {
                            UpdateTreeView(urlToCheck, false);
                            UpdateListView(urlToCheck, "File", System.IO.Path.GetExtension(urlToCheck), $"{httpResponse.Content.Headers.ContentLength} bytes", $"{(int)httpResponse.StatusCode}");
                            return true;
                        }

                        if(dirbusterDataHolder.FailureCodes.Contains(((int)httpResponse.StatusCode).ToString()))
                        {
                            return false;
                        }

                        if (System.Text.RegularExpressions.Regex.IsMatch(httpResponse.Content.ReadAsStringAsync().Result, dirbusterDataHolder.NotfoundPattern))
                        {
                            return false;
                        }

                        if (dirbusterDataHolder.NotfoundHtmlSource.ToLower().Equals(httpResponse.Content.ReadAsStringAsync().Result.ToLower()))
                        {
                            return false;
                        }
                    }
                    
                }
                catch { return false; }
            }

            else //if(requestMethod == RequestMethod.HEAD_AND_GET)
            {
                try
                {

                    Utilities.RequestCalculator.IncreaseDirbuster();

                    //HttpRequests._targetUrl = $"{url}/{fileName}";
                    HttpClient httpClient = dirbusterDataHolder.httpClient;
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Head, urlToCheck);
                    HttpResponseMessage httpResponse = httpClient.SendAsync(requestMessage).Result;

                    if(dirbusterDataHolder.IsDefaultSettings)
                    {
                        if (httpResponse.IsSuccessStatusCode) 
                        {
                            UpdateTreeView(urlToCheck, false);
                            UpdateListView(urlToCheck, "File", System.IO.Path.GetExtension(urlToCheck), $"{httpResponse.Content.Headers.ContentLength} bytes", $"{(int)httpResponse.StatusCode}");
                            return true;
                        }
                    }

                    else // Its custom settings.
                    {
                        if(dirbusterDataHolder.SuccessCodes.Contains(((int)httpResponse.StatusCode).ToString()))
                        {
                            UpdateTreeView(urlToCheck, false);
                            UpdateListView(urlToCheck, "File", System.IO.Path.GetExtension(urlToCheck), $"{httpResponse.Content.Headers.ContentLength} bytes", $"{(int)httpResponse.StatusCode}");
                            return true;
                        }

                        if(dirbusterDataHolder.FailureCodes.Contains(((int)httpResponse.StatusCode).ToString()))
                        {
                            return false;
                        }

                        if (System.Text.RegularExpressions.Regex.IsMatch(httpResponse.Content.ReadAsStringAsync().Result, dirbusterDataHolder.NotfoundPattern))
                        {
                            return false;
                        }

                        if (dirbusterDataHolder.NotfoundHtmlSource.ToLower().Equals(httpResponse.Content.ReadAsStringAsync().Result.ToLower()))
                        {
                            return false;
                        }
                    }

                }
                catch (WebException) { return false; }
                catch (Exception ex) // Switch to GET method
                {
                    try
                    {
                        HttpClient client = dirbusterDataHolder.httpClient;
                        HttpResponseMessage httpResponse = client.GetAsync($"{url}/{fileName}").Result;
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            UpdateListView($"{url}/{fileName}", "File", System.IO.Path.GetExtension(fileName), $"{httpResponse.Content.Headers.ContentLength} bytes", $"{(int)httpResponse.StatusCode}");
                            return true;
                        }
                    }
                    catch { return false; }
                }
            }
            
            return false;

        } 

        private void ScanFiles(string url,CancellationToken token, AttackType attackType)
        {
            UpdateListViewDirectoryStatus(url, "Bruteforcing On Directory...");
            //if (url.EndsWith("/"))
                //url = url.TrimEnd('/');
            if(attackType == AttackType.Dictionary)
            {
                bool isCanceled = false;
                foreach (string fileExtention in dirbusterDataHolder.fileExtentions)
                {
                    if (isCanceled)
                        break;

                    foreach (string file in dirbusterDataHolder.DictionaryList)
                    {
                        if(token.IsCancellationRequested)
                        {
                            try
                            {
                                token.ThrowIfCancellationRequested();
                            }
                            catch { }
                            finally
                            {
                                isCanceled = true;
                            }
                            
                        }

                        if (isCanceled)
                            break;
                        string fullName = $"{file}{fileExtention}";
                        FileExists(url, fullName);
                    }
                }

            }

            else if(attackType == AttackType.Bruteforce)
            {

                for(int i=dirbusterDataHolder.MinLenght; i<=dirbusterDataHolder.MaxLenght;i++)
                {
                    Bruteforce(string.Empty, 0, i, i, false, url);
                }

            }

            UpdateListViewDirectoryStatus(url);

        } 

        private bool IsDirectoryListingEnable(string url, string dirName, HttpResponseMessage httpResponse)
        {
            string urlToCheck = string.Empty;
            if (string.IsNullOrEmpty(dirName))
                urlToCheck = url;
            else
                urlToCheck = $"{url}/{dirName}";

            string[] patterns = { "<title>Index of" , "Last modified</a>" ,"Parent Directory</a>"};
            foreach(string pattern in patterns)
            {
                if(System.Text.RegularExpressions.Regex.IsMatch(httpResponse.Content.ReadAsStringAsync().Result,pattern))
                {
                    UpdateTreeView(urlToCheck, true);
                    UpdateListView(urlToCheck, "Directory", string.Empty, "Directory Listing Available", $"{(int)httpResponse.StatusCode}");

                    return true;
                }
            }
            return false;
        }

        private void GetDirectoryItems(string url,HttpResponseMessage httpResponse)
        {
            var directoryItems = Utilities.Extractor.ExtractHref(httpResponse.Content.ReadAsStringAsync().Result, url);
            List<string> directories = new List<string>();

            foreach (string itemUrl in directoryItems)
            {
                if (itemUrl.EndsWith("/"))
                {
                    if (DirectoryExists(itemUrl, string.Empty))
                    {
                        directories.Add(itemUrl);
                    }
                }

                else
                {
                    if (dirbusterDataHolder.visitedLinks.IndexOf(itemUrl) < 0)
                    {
                        dirbusterDataHolder.visitedLinks.Add(itemUrl);
                        FileExists(itemUrl, string.Empty);
                    }
                }
            }

            Parallel.ForEach(directories, new ParallelOptions { MaxDegreeOfParallelism = Advanced_Settings.ConnectionSettings.Threads }, path =>
            {
                Task t = null;
                try
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    var cancellationToken = cancellationTokenSource.Token;

                    t = new Task(() =>
                    {
                        if (bruteDirs)
                            WalkOnDirectory(path, this.attackType);
                        if (bruteFiles)
                            ScanFiles(path, cancellationToken, this.attackType);
                    });

                    taskWorkingDirectory.Add(path, cancellationTokenSource);
                    t.Start();
                    t.Wait(cancellationToken);
                    //taskWorkingDirectory.Remove(path);
                }
                catch { }
                finally
                {
                    taskWorkingDirectory.Remove(path);
                    if(t.IsCanceled || t.IsFaulted || t.IsCompleted)
                        t.Dispose();
                }
            });
        }

        private  void Bruteforce(string input, int position, int minlength, int maxlength, bool isDir, string url)
        {

            if (position < maxlength)
            {

                for (int i = 0; i < dirbusterDataHolder.CharSet.Length; i++)
                {
                    if (position >= minlength) // new word created and saved into 'input' variable
                    {
                        if(isDir)
                        {
                            if(DirectoryExists(url, input))
                            {
                                new Thread(new ThreadStart(() =>
                                {
                                    Task t = null;
                                    try
                                    {
                                        var cancellationTokenSource = new CancellationTokenSource();
                                        var cancellationToken = cancellationTokenSource.Token;
                                        t = new Task(() =>
                                        {
                                            if(bruteFiles)
                                                ScanFiles($"{url}/{input}", cancellationToken, AttackType.Bruteforce);
                                            WalkOnDirectory($"{url}/{input}", AttackType.Bruteforce);
                                        });

                                        taskWorkingDirectory.Add($"{url}/{input}", cancellationTokenSource);
                                        t.Start();
                                        t.Wait(cancellationToken);
                                    }
                                    catch { }
                                    finally
                                    {
                                        UpdateListViewDirectoryStatus($"{url}/{input}");
                                        taskWorkingDirectory.Remove($"{url}/{input}");
                                        //t.Dispose();
                                    }                                   
                                }))
                                {
                                    IsBackground = true
                                }.Start();       
                            }
                        }

                        else // !isDir => Its File
                        {
                            foreach (string fileExtention in dirbusterDataHolder.fileExtentions)
                            {
                                FileExists(url, $"{input}{fileExtention}");
                            }
                        }                     
                    }

                    Bruteforce(input + dirbusterDataHolder.CharSet[i], position + 1, minlength, maxlength, isDir, url);
                }

            }

            else // new word created and saved into 'input' variable
            {
                if (isDir)
                {
                    if (DirectoryExists(url, input))
                    {
                        new Thread(new ThreadStart(() =>
                        {
                            Task t = null;
                            try
                            {
                                var cancellationTokenSource = new CancellationTokenSource();
                                var cancellationToken = cancellationTokenSource.Token;

                                t = new Task(() =>
                                {
                                    if(bruteFiles)
                                        ScanFiles($"{url}/{input}", cancellationToken, AttackType.Bruteforce);
                                    WalkOnDirectory($"{url}/{input}", AttackType.Bruteforce);
                                });

                                taskWorkingDirectory.Add($"{url}/{input}", cancellationTokenSource);
                                t.Start();
                                t.Wait(cancellationToken);
                            }
                            catch { }
                            finally
                            {
                                UpdateListViewDirectoryStatus($"{url}/{input}");
                                taskWorkingDirectory.Remove($"{url}/{input}");
                                //t.Dispose();
                            }                          
                        }))
                        {
                            IsBackground = true
                        }.Start();
                    }
                }

                else // !isDir => Its File
                {
                    foreach (string fileExtention in dirbusterDataHolder.fileExtentions)
                    {
                        FileExists(url, $"{input}{fileExtention}");
                    }
                }
            }

        } 

        public bool CancelBruteforceOnDirectory(string urlAsKey)
        {
            try
            {
                urlAsKey = $"{targetUrl}{urlAsKey}";
                taskWorkingDirectory[urlAsKey].Cancel();

                return true;
            }
            catch(Exception ex) { return false; }
        }

        private void UpdateTreeView(string url, bool isDir, bool isDirectoryIndexing = false)
        {
            Uri uri = new Uri(url);
            url = $"{uri.Host}{uri.LocalPath}";
 
            int imageIndex;
            if (isDir)
                imageIndex = 2;
            else
                imageIndex = 0;

            string path = string.Empty;
            TreeNode lastNode = null;
            dirbusterDataHolder.trwDirbuster.Invoke((MethodInvoker)delegate
            {
                foreach (string subPath in url.Split('/'))
                {
                    path += subPath + '/';
                    TreeNode[] nodes = dirbusterDataHolder.trwDirbuster.Nodes.Find(path, true);
                    if (nodes.Length == 0)
                        if (lastNode == null)
                            lastNode = dirbusterDataHolder.trwDirbuster.Nodes.Add(path, subPath, 2,3);
                        else
                        {
                            lastNode = lastNode.Nodes.Add(path, subPath, imageIndex,3);
                        }
                    else
                    {
                        lastNode = nodes[0];
                    }
                }

                lastNode = null;
            });
            
        }
        
        private void UpdateListView(string uri, string type, string extention, string info, string statusCode)
        {
            uri = new Uri(uri).LocalPath;
            if (uri.Contains("//"))
                uri = uri.Replace("//", "/");
            dirbusterDataHolder.lvwDirbuster.Invoke((MethodInvoker)delegate
            {
                ListViewItem lvi = new ListViewItem()
                {
                    Text = dirbusterDataHolder.lvwDirbuster.Items.Count.ToString()
                };

                lvi.SubItems.Add(uri);
                lvi.SubItems.Add(type);
                lvi.SubItems.Add(extention);
                lvi.SubItems.Add(info);
                lvi.SubItems.Add(statusCode);

                dirbusterDataHolder.lvwDirbuster.Items.Add(lvi);
            });
        }

        private void UpdateListViewDirectoryStatus(string url, string status = "Bruteforcing Completed.")
        {
            dirbusterDataHolder.lvwDirbuster.Invoke((MethodInvoker)delegate
            {
                try
                {
                    url = new Uri(url).LocalPath;

                    foreach(ListViewItem i in dirbusterDataHolder.lvwDirbuster.Items)
                    {
                        if(i.SubItems[1].Text == url)
                        {
                            dirbusterDataHolder.lvwDirbuster.Items[int.Parse(i.SubItems[0].Text)].SubItems[4].Text = status;
                            break;
                        }
                    }

                }
                catch { };
            });
        } 

    }
}
