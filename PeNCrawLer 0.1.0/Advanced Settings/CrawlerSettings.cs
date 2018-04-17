using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeNCrawLer_0._1._0.Advanced_Settings
{
    /// <summary>
    /// This class holds the state of CrawlerSettings.
    /// If any state or values change, it will affect in runtime.
    /// </summary>
    static class CrawlerSettings
    {
        public static bool HasToUpdateExtentionsList = false;
        public static bool IsCustomizeSettings = false;

        public static bool CanFollowRedirect = true;
        public static bool CanRenderJavascript = false;

        public static bool CanExtractLinkByHtmlElement = true;
        public static Dictionary<string, string> HtmlTagAndAttribute; // <tagName,tagAttribute> -> <a,href>

        public static bool CanExtractLinkByRegex;
        public static string PatternExtractLink = @"[(http|https|ftp|)]+[\:\/\/]+[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?\/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]";

        public static bool CanNotProcessExtentions;
        public static string BlackListExtentions = "pdf,doc,docx,js,css,gif,jpg,png,swf,zip,rar,exe,pdf,mp3,wma,mp4,avi";

        public static bool CanDownloadExtentions;
        public static string WhiteListExtentions;
        public static string PathToSave;

        public static bool CanLimitCrawling;
        public static Dictionary<string, int> LinksPatternAndMaxValue; // <RegexPattern,Number> -> <news,10>

        public static bool CanSearch;
        public static Dictionary<char, string> SearchInputAndValue; // <Input,Value> -> 'u' character for url and 'r' character for response
    }
}
