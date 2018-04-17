using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeNCrawLer_0._1._0.Utilities
{
    static class Helper
    {

        public static string hostName = string.Empty;

        public static bool IsValidPattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            try
            {
                System.Text.RegularExpressions.Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        public static string FixStartUrl(string url,string startPath="")
        {
            if (!(url.StartsWith("http://") || url.StartsWith("https://")))
            {
                url = $"http://{url}";
            }

            if (url.Contains("://www."))
            {
                url = url.Replace("www.", "");
            }

            if (url.EndsWith("/"))
            {
                url = url.TrimEnd('/');
            }

            hostName = $"{url}{startPath}";

            return hostName;

        }

        public static string FixLink(string targetUrl, string baseUrl)
        {
            if (string.IsNullOrEmpty(targetUrl))
                return null;

            if (baseUrl.EndsWith("/"))
                baseUrl = baseUrl.TrimEnd('/');
            if (Uri.IsWellFormedUriString(targetUrl, UriKind.Relative))
            {

                if (targetUrl.StartsWith("/")) // root relative url
                {
                    targetUrl = $"{hostName}{targetUrl}";
                }

                else if (targetUrl.StartsWith("./"))
                {
                    targetUrl = targetUrl.Substring(2);
                    string localPath = new Uri(baseUrl).LocalPath;
                    string[] paths = localPath.Split('/');

                    string newPath = string.Empty;
                    //get current dir
                    for (int i = 0; i < paths.Length - 1; i++)
                    {
                        if (!string.IsNullOrEmpty(paths[i]))
                            newPath = $"{newPath}/{paths[i]}";

                    }

                    baseUrl = baseUrl.Replace(localPath, newPath);

                    if (baseUrl.EndsWith("/"))
                    {
                        targetUrl = $"{baseUrl}{targetUrl}";
                    }

                    else
                    {
                        targetUrl = $"{baseUrl}/{targetUrl}";
                    }

                    return targetUrl;

                }

                else if (targetUrl.StartsWith("../"))
                {
                    targetUrl = targetUrl.Substring(3);
                    string localPath = new Uri(baseUrl).LocalPath;
                    string[] paths = localPath.Split('/');

                    string newPath = string.Empty;
                    //get current dir
                    for (int i = 0; i < paths.Length - 2; i++)
                    {
                        if (!string.IsNullOrEmpty(paths[i]))
                            newPath = $"{newPath}/{paths[i]}";

                    }

                    baseUrl = baseUrl.Replace(localPath, newPath);

                    if (baseUrl.EndsWith("/"))
                    {
                        targetUrl = $"{baseUrl}{targetUrl}";
                    }

                    else
                    {
                        targetUrl = $"{baseUrl}/{targetUrl}";
                    }

                    return targetUrl;
                }

                else
                {
                    targetUrl = $"{baseUrl}/{targetUrl}";
                    return targetUrl;
                }
            }

            
            if (Uri.IsWellFormedUriString(targetUrl, UriKind.Absolute))
            {
                
                try
                {

                    string host = GetHostname(targetUrl);
                    if (!string.IsNullOrEmpty(host))
                    {
                        if (host.Equals(GetHostname(hostName)))
                        {
                            return targetUrl;
                        }
                    }
                }

                catch { };
            }


            return null;

        }

        public static string GetHostname(string url)
        {
            try
            {
                return new Uri(url).Host.ToLower();
            }

            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetExtentionList(string extentions)
        {
            List<string> extentionsList = new List<string>();

            foreach(string extention in extentions.Split(','))
            {
                extentionsList.Add($".{extention}");
            }

            //Advanced_Settings.CrawlerSettings.UpdateSettings = false;
            return extentionsList;

        }

        public static bool GetFileSize(Uri uriPath)
        {

            var webRequest = System.Net.HttpWebRequest.Create(uriPath);
            webRequest.Method = "HEAD";
            try
            {
                RequestCalculator.IncreaseCrawler();
                using (var webResponse = webRequest.GetResponse())
                {
                    var fileSize = webResponse.Headers.Get("Content-Length");
                    var fileSizeInMegaByte = Math.Round(Convert.ToDouble(fileSize) / 1024.0 / 1024.0, 2);
                    return fileSizeInMegaByte > 0 ? true : false;
                }
            }
            catch { return false; };

        }

        public static List<string> GetDirectoryPaths(string url)
        {
            List<string> links = new List<string>();
            Uri uri = new Uri(url);
            string localPath = uri.LocalPath;
            string[] paths = localPath.Split('/');
            string host = $"{uri.Scheme}://{uri.Host}";
            string addHostLast = host;
            foreach (string path in paths)
            {
                if(!string.IsNullOrEmpty(path))
                {
                    host = $"{host}/{path}";
                    links.Add(host);
                }
                
            }

            links.Add(addHostLast);
            return links;
        }

    }
}
