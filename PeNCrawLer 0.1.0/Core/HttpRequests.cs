using System.Net;
using System.Net.Http;
using PeNCrawLer_0._1._0.Advanced_Settings;
using System.Text;
using System;

namespace PeNCrawLer_0._1._0.Core
{
    static class HttpRequests
    {
        public static bool HasToUpdateCrawler = false;
        public static bool HasToUpdateDirbuster = false;

        public static string _targetUrl;

        static HttpClient httpClient;
        static HttpClientHandler clientHandler;
        public static HttpClient UpdateHttpClient()
        {
            clientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = CrawlerSettings.CanFollowRedirect
            };

            httpClient = new HttpClient(clientHandler);

            if(Advanced_Settings.HttpRequestSettings.CanUseProxy)
            {

                clientHandler.UseProxy = true;
                clientHandler.Proxy = new WebProxy(HttpRequestSettings.ProxyHost,HttpRequestSettings.ProxyPort);

                if(HttpRequestSettings.CanUseAuthProxy)
                {
                    ICredentials credentials = new NetworkCredential(HttpRequestSettings.ProxyUsername, HttpRequestSettings.ProxyPassword);
                    clientHandler.Proxy = new WebProxy($"{HttpRequestSettings.ProxyHost}:{HttpRequestSettings.ProxyPort}",true,null, credentials);
                }

                httpClient = new HttpClient(clientHandler);

            }

            if(Advanced_Settings.HttpRequestSettings.NeedAuthentication)
            {

                if(HttpRequestSettings.AuthenticationType.Equals("Basic"))
                {

                    byte[] creds = Encoding.ASCII.GetBytes($"{HttpRequestSettings.AuthenticationUsername}:{HttpRequestSettings.AuthenticationPassword}");
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(HttpRequestSettings.AuthenticationType, Convert.ToBase64String(creds));

                }

                else if(HttpRequestSettings.AuthenticationType.Equals("Digest"))
                {
                    Uri targetUrl = new Uri(_targetUrl);

                    var credentialCache = new CredentialCache
                        {
                            {
                                new Uri(targetUrl.GetLeftPart(UriPartial.Authority)),
                                HttpRequestSettings.AuthenticationType,
                                new NetworkCredential(HttpRequestSettings.AuthenticationUsername, HttpRequestSettings.AuthenticationPassword)
                            }
                        };

                    clientHandler.Credentials = credentialCache;
                    httpClient = new HttpClient(clientHandler);
                }

                else
                {
                    // Something wrong happens, lets turn NeedAuthentication to false
                    HttpRequestSettings.NeedAuthentication = false;
                }

            }

            try
            {
                foreach (var item in Advanced_Settings.HttpRequestSettings.HeaderAndValue)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            } // Set header values
            catch(Exception)
            {
                System.Windows.Forms.MessageBox.Show("Error in adding http headers.", "PeNCrawLer", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                //return httpClient;
            }

            HasToUpdateCrawler = false;
            return httpClient;
        }

    }
}
