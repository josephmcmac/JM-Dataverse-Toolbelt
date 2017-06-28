using $safeprojectname$.Xrm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using $safeprojectname$.Extentions;
using Microsoft.Xrm.Sdk;

namespace $safeprojectname$.HtmlEmails
{
    public class HtmlEmailGenerator
    {
        public XrmService XrmService { get; set; }
        public string WebUrl { get; set; }
        private bool IncludeHyperlinks
        {
            get { return !string.IsNullOrWhiteSpace(WebUrl); }
        }
        private StringBuilder _content = new StringBuilder();
        private const string pStyle = "style='font-family: Segoe UI;font-size: 13px;color: #444444;'";
        private const string thStyle = "style='font-family: Segoe UI;font-size: 13px;color: #444444;vertical-align: top;text-align: left; margin: 5px; border-style: solid; border-width: 1px; padding: 5px;'";
        private const string tdStyle = "style='font-family: Segoe UI;font-size: 13px;color: #444444;vertical-align: top;text-align: left; margin: 5px; border-style: solid; border-width: 1px; padding: 5px;'";
        private const string aStyle = "style='font-family: Segoe UI;font-size: 13px;'";

        public HtmlEmailGenerator(XrmService xrmService, string webUrl)
        {
            XrmService = xrmService;
            WebUrl = webUrl != null && webUrl.EndsWith("/")
                ? webUrl.Substring(0, webUrl.Length - 1)
                : webUrl;
        }

        public void AppendParagraph(string text)
        {
            _content.AppendLine(string.Format("<p {0}>{1}</p>", pStyle, text));
        }

        public void AppendTable(IEnumerable<Entity> take, IEnumerable<string> fields = null, bool noHyperLinks = false, string appId = null)
        {


            if (!take.Any())
                return;

            var table = new StringBuilder();
            table.AppendLine("<table>");
            table.AppendLine("<thead><tr>");
            if (IncludeHyperlinks && !noHyperLinks)
            {
                table.AppendLine(string.Format("<th {0}></th>", thStyle));
            }
            var firstItem = take.First();
            if (fields == null)
                fields = new[] { XrmService.GetPrimaryNameField(firstItem.LogicalName) };
            foreach (var field in fields)
                AppendThForField(table, firstItem, field);
            table.AppendLine("</tr></thead>");
            foreach (var item in take)
            {
                table.AppendLine("<tr>");
                if (IncludeHyperlinks && !noHyperLinks)
                {
                    table.AppendLine(string.Format("<td {0}>", tdStyle));
                    var url = string.Format("{0}/main.aspx?{1}pagetype=entityrecord&etn={2}&id={3}", WebUrl,
                        appId == null ? null : ("appid=" + appId + "&"),
                        item.LogicalName, item.Id);
                    AppendUrl(url, "View", table);
                    table.AppendLine("</td>");
                }

                foreach (var field in fields)
                    AppendTdForField(table, item, field);
                table.AppendLine("</tr>");
            }
            table.AppendLine("</table>");
            _content.AppendLine(table.ToString());
        }

        public void AppendTable(IEnumerable<ProcessEntityResponseBase> take, IEnumerable<string> fields = null, bool noHyperLinks = false)
        {
            AppendTable(take.Select(p => p.Entity).ToArray(), fields, noHyperLinks);
        }

        private void AppendThForField(StringBuilder table, Microsoft.Xrm.Sdk.Entity firstItem, string field)
        {
            var path = XrmService.GetTypeFieldPath(field, firstItem.LogicalName);
            table.AppendLine(string.Format("<th {0}>{1}</th>", thStyle, XrmService.GetFieldLabel(path.Last().Value, path.Last().Key)));
        }

        private void AppendTdForField(StringBuilder table, Entity item, string field)
        {
            var path = XrmService.GetTypeFieldPath(field, item.LogicalName);
            table.AppendLine(string.Format("<td {0}>", tdStyle));
            table.Append(XrmService.GetFieldAsDisplayString(path.Last().Key, path.Last().Value, item.GetFieldValue(field)));
            table.AppendLine("</td>");
        }

        private void AppendUrl(string url, string label, StringBuilder sb)
        {
            sb.Append(string.Format("<a {0} href='{1}' >{2}</a>", aStyle, url, label));
        }

        public string GetContent()
        {
            return _content.ToString();
        }
    }
}