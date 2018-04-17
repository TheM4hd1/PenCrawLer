using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace PeNCrawLer_0._1._0.Utilities
{
    static class Report
    {
        static int counter = 0;
        static string dataa = string.Empty;
        public enum ReportType
        {
            Crawler,
            SearchedData,
            DirBuster
        }
        public static void MakeReport(ReportType reportType, TreeView trw = null, ListView lvw = null)
        {
            string
                date = string.Empty,
                title = string.Empty,
                url = string.Empty,
                ID = string.Empty,
                subitem1 = string.Empty,
                subitem2 = string.Empty,
                subitem3 = string.Empty,
                data = string.Empty,
                reportName = string.Empty,
                content = string.Empty;

            content = Properties.Resources.indexhtml;
            if (trw != null)
            {
                if(reportType == ReportType.Crawler)
                {
                    int i = 0;
                    title = "Crawler results ";
                    subitem1 = "Url Address";
                    
                    foreach (TreeNode node in trw.Nodes)
                    {
                        PrintNodesRecursive(node);
                    }

                    data = dataa;
                }
            }

            else
            {
                if(reportType == ReportType.SearchedData)
                {
                    title = "searched results by Crawler ";
                    subitem1 = "Match type";
                    subitem2 = "Value";
                    subitem3 = "Url Address";

                    foreach (ListViewItem item in lvw.Items)
                    {
                        ID = item.Text;
                        data += $"<tr>" +
                                $"<td data-title=\"ID\">{ID}</td>" +
                                $"<td data-title=\"Match type\">Regex</td>" +
                                $"<td data-title=\"Value\">{item.SubItems[2].Text}</td>" +
                                $"<td data-title=\"Url Address\">{item.SubItems[3].Text}</td>" +
                                $"</tr>\n";
                    }
                }
                if(reportType == ReportType.DirBuster)
                {
                    title = "DirBuster results ";
                    subitem1 = "Local path";
                    subitem2 = "Type";
                    subitem3 = "Status code";

                    foreach (ListViewItem item in lvw.Items)
                    {
                        ID = item.Text;
                        data += $"<tr>" +
                                $"<td data-title=\"ID\">{ID}</td>" +
                                $"<td data-title=\"Local path\">{item.SubItems[1].Text}</td>" +
                                $"<td data-title=\"Type\">{item.SubItems[2].Text}</td>" +
                                $"<td data-title=\"Status code\">{item.SubItems[5].Text}</td>" +
                                $"</tr>\n";
                    }
                }
            }


            try
            {
                date = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss tt");
                url = Helper.GetHostname(Helper.hostName);

                if (!Directory.Exists("PeNCrawler.Reports"))
                    Directory.CreateDirectory("PeNCrawler.Reports");

                if (!Directory.Exists("PeNCrawler.Reports\\css"))
                {
                    Directory.CreateDirectory("PeNCrawler.Reports\\css");
                    File.WriteAllText("PeNCrawler.Reports\\css\\style.css", Properties.Resources.stylecss);
                }

                if (!Directory.Exists("PeNCrawler.Reports\\js"))
                {
                    Directory.CreateDirectory("PeNCrawler.Reports\\js");
                    File.WriteAllText("PeNCrawler.Reports\\js\\index.js", Properties.Resources.indexjs);
                }

                if (!Directory.Exists("PeNCrawler.Reports\\less"))
                {
                    Directory.CreateDirectory("PeNCrawler.Reports\\less");
                    File.WriteAllText("PeNCrawler.Reports\\less\\style.less", Properties.Resources.styleless);
                }

                content = content.Replace("txtTitle", title);
                content = content.Replace("txtTarget", url);
                content = content.Replace("txtDate", date);
                content = content.Replace("txtData", data);
                content = content.Replace("subItem[1]", subitem1);
                content = content.Replace("subItem[2]", subitem2);
                content = content.Replace("subItem[3]", subitem3);

                reportName = $"Report@{url}  {date}.html";
                File.WriteAllText($"PeNCrawler.Reports\\{reportName}", content, Encoding.UTF8);

                MessageBox.Show($"Report file created successfully.\nFilename: {reportName}", "PeNCrawLer - Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            catch(Exception ex) { MessageBox.Show(ex.Message); }

        }

        public static void PrintNodesRecursive(TreeNode oParentNode)
        {
            counter++;
            dataa += $"<tr>" +
                    $"<td data-title=\"ID\">{counter}</td>" +
                    $"<td data-title=\"Url Address\">{oParentNode.FullPath}</td>" +
                    $"</tr>\n";

            // Start recursion on all subnodes.
            foreach (TreeNode oSubNode in oParentNode.Nodes)
            {
                PrintNodesRecursive(oSubNode);
            }
        }
    }
}

/*
 * $"<tr>" +
                                $"<td data-title=\"ID\">{ID}</td>" +
                                $"<td data-title=\"Url Address\">{node.FullPath}</td>" +
                                $"<td data-title=\"\"></td>" +
                                $"<td data-title=\"\"></td>" +
                                $"</tr>\n"; 
 *      <tr>
          <td data-title="ID">subItem[0].Text</td>
          <td data-title="Name">subItem[1].Text</td>
          <td data-title="Link">subItem[2].Text</td>
          <td data-title="Status">subItem[3].Text</td>
        </tr>
 */
