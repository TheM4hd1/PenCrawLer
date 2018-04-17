using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeNCrawLer_0._1._0.Advanced_Settings
{
    static class HttpRequestSettings
    {

        public static Dictionary<string, string> HeaderAndValue;

        public static bool CanUseProxy = false;
        public static string ProxyHost;
        public static int ProxyPort;

        public static bool CanUseAuthProxy = false;
        public static string ProxyUsername;
        public static string ProxyPassword;

        public static bool NeedAuthentication = false;
        public static string AuthenticationUsername;
        public static string AuthenticationPassword;
        public static string AuthenticationType = "Basic";

        public static string[] DefaultHeaders = {
                                 "Accept: */*",
                                 "Accept-Language: en",
                                 "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Win64; x64; Trident/5.0)",
                                 "Connection: close"
                                };

    }
}
