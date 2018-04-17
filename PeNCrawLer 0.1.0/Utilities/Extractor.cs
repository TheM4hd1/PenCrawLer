using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace PeNCrawLer_0._1._0.Utilities
{
    static class Extractor
    {

        public static List<string> ExtractAttributesByTagName(string html_response, string baseUrl)
        {
            if (string.IsNullOrEmpty(html_response))
                return null;

            List<string> extractedLinks = new List<string>();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html_response);

            foreach(KeyValuePair<string,string> element in Advanced_Settings.CrawlerSettings.HtmlTagAndAttribute)
            {

                var links = from link in document.DocumentNode.Descendants()
                            where link.Name == element.Key &&
                            link.Attributes[element.Value] != null
                            select new
                            {
                                linkValue = link.Attributes[element.Value].Value
                            };

                foreach (var item in links)
                {

                    string finalLink = Helper.FixLink(item.linkValue, baseUrl);
                    if(!string.IsNullOrEmpty(finalLink) && extractedLinks.IndexOf(finalLink) <0)
                    {
                        extractedLinks.Add(finalLink);
                    }
                        
                }

            }

            return extractedLinks;

        }
        
        public static List<string> ExtractLinksByRegex(string html_response, string baseUrl)
        {
         /*
         * Crawler can crawl other hosts too, to prevent this,
         * user must customize the pattern for specific url-address
         * but we can add that limit here to be checked automatically.
         * if(Helper.Gethostname(link.value).Equals(Helper.hostname) && ...
         */
            List<string> extractedLinks = new List<string>();
            MatchCollection matchedLinks = Regex.Matches(html_response,Advanced_Settings.CrawlerSettings.PatternExtractLink);

            foreach(Match link in matchedLinks)
            {

                if(extractedLinks.IndexOf(link.Value)<0) 
                {
                    extractedLinks.Add(link.Value);
                }

            }

            return extractedLinks;

        }

        public static List<string> ExtractHref(string html_response, string baseUrl)
        {
            if (string.IsNullOrEmpty(html_response))
                return null;

            baseUrl = baseUrl.TrimEnd('/');
            List<string> extractedLinks = new List<string>();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html_response);

            var links = from link in document.DocumentNode.Descendants() where link.Name == "a" && link.Attributes["href"] != null && !link.Attributes["href"].Value.StartsWith("?") select new { linkValue = link.Attributes["href"].Value };
            foreach(var item in links)
            {
                string finalLink = Helper.FixLink(item.linkValue, baseUrl);
                if (!string.IsNullOrEmpty(finalLink) && extractedLinks.IndexOf(finalLink) < 0)
                {
                    extractedLinks.Add(finalLink);
                }
            }

            return extractedLinks;
        }

    }
}
