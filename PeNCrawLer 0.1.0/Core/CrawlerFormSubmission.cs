using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
namespace PeNCrawLer_0._1._0.Core
{
    class CrawlerFormSubmission : IDisposable
    {
        public string response = string.Empty;
        public string targetUrl = string.Empty;
        public HttpClient httpClient;
        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public CrawlerFormSubmission(string response,string targetUrl,HttpClient httpClient)
        {
            this.response = response;
            this.targetUrl = targetUrl;
            this.httpClient = httpClient;
        }

        public string Submit()
        {
            try
            {

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response);
                HtmlNodeCollection formNodes = doc.DocumentNode.SelectNodes("//form");

                if (formNodes.Count() > 0)
                {
                    
                    foreach (var node in formNodes)
                    {
                        Dictionary<string, string> param = new Dictionary<string, string>();
                        doc.LoadHtml(node.OuterHtml);

                        string actionUrl = string.Empty;
                        var forms = from form in doc.DocumentNode.Descendants()
                                    where form.Name == "form" &&
                                    form.Attributes["action"] != null
                                    select new
                                    {
                                        action = form.Attributes["action"].Value,
                                    };

                        if (forms.Count() > 0)
                        {
                            foreach(var a in forms)
                            {
                                actionUrl = a.action;
                                break;
                            }
                            
                            actionUrl = Utilities.Helper.FixLink(actionUrl, targetUrl);
                            
                            string method = string.Empty;
                            if (Regex.IsMatch(node.OuterHtml, "method=[\"](.+?)[\"]"))
                            {
                                method = Regex.Match(response, "method=[\"](.+?)[\"]").Groups[1].Value.ToLower();
                            }
                            else
                            {
                                method = "get";
                            }

                            HtmlNodeCollection inputNodes = doc.DocumentNode.SelectNodes("//input");

                            foreach (var inputNode in inputNodes)
                            {
                                var names = Regex.Match(inputNode.OuterHtml, "name=[\"](.+?)[\"]");
                                var values = Regex.Match(inputNode.OuterHtml, "value=[\"](.+?)[\"]");
                                var type = Regex.Match(inputNode.OuterHtml, "type=[\"](.+?)[\"]").Groups[1].Value.ToLower();
                                
                                if (!type.Equals("button") && !type.Equals("reset") && !string.IsNullOrEmpty(names.Value)) // prevent for adding unpostable parameters
                                {
                                    param.Add(names.Groups[1].Value, values.Groups[1].Value);
                                }
                            }

                            Dictionary<string, string> temp = new Dictionary<string, string>(param);
                            foreach (var key in temp.Keys)
                            {

                                if (string.IsNullOrEmpty(temp[key]))
                                {
                                    param[key] = GetFieldValue(key);
                                }
                            }

                            var content = new FormUrlEncodedContent(param);
                            if (method.Equals("post"))
                            {
                                var res = httpClient.PostAsync(actionUrl, content).Result;
                                if (res.IsSuccessStatusCode)
                                {
                                    return res.Content.ReadAsStringAsync().Result;
                                }
                            }

                            else // get request
                            {
                                string uri = AttachParameters(actionUrl, param);
                                var result = httpClient.GetAsync(uri).Result;
                                return result.Content.ReadAsStringAsync().Result;
                            }
                        }

                        else
                        {
                            return null;
                        }

                    }

                }

                else
                {
                    return null;
                }
            }

            catch { return null; };

            return null;
        }

        private string GetFieldValue(string fieldName)
        {
            foreach (KeyValuePair<string,string> keyAndValue in Advanced_Settings.FormSubmissionSettings.FieldnameAndFieldvalue)
            {
                if (Regex.IsMatch(fieldName, keyAndValue.Key))
                {
                    return keyAndValue.Value;
                }
            }
            return Advanced_Settings.FormSubmissionSettings.UnMatchedField;
        }

        public string AttachParameters(string uri, Dictionary<string, string> parameters)
        {
            var stringBuilder = new StringBuilder();
            string str = "?";

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                stringBuilder.Append(str + parameter.Key + "=" + parameter.Value);
                str = "&";
            }
            return $"{uri}/{stringBuilder}";
        }
    }
}
