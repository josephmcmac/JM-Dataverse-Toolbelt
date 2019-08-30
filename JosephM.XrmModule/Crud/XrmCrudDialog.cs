using JosephM.Application.Desktop.Module.Crud;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace JosephM.XrmModule.Crud
{
    [RequiresConnection]
    public class XrmCrudDialog : CrudDialog
    {
        public XrmCrudDialog(XrmRecordService xrmrecordService, IDialogController dialogController)
            : base(dialogController, xrmrecordService)
        {
            XrmRecordService = xrmrecordService;
        }

        public override IEnumerable<string> AdditionalExplicitTypes => new[] { Entities.savedqueryvisualization, Entities.userqueryvisualization, Entities.appmodule, Entities.appmodulecomponent, Entities.solutioncomponent, Entities.solution, Entities.timezonedefinition, Entities.activityparty, Entities.userquery, Entities.sitemap, Entities.subject, Entities.systemform, Entities.uom, Entities.savedquery, Entities.calendar, Entities.role, Entities.sdkmessageprocessingstep, Entities.activitymimeattachment, Entities.organization, Entities.usersettings, Entities.productpricelevel };

        public XrmRecordService XrmRecordService { get; }

        public override IEnumerable<CustomGridFunction> GetExtendedGridFunctions()
        {
            var extraFunctions = new List<CustomGridFunction>();
            extraFunctions.Add(new CustomGridFunction("COPYQUERY", "Copy FetchXML", new[]
            {
                new CustomGridFunction("RAWFETCHXML", "Raw FetchXML", (g) =>
                                {
                                    DoOnAsynchThread(() =>
                                    {
                                        var fetchXml = GetQueryFetchXml();
                                        DoOnMainThread(() =>
                                        {
                                            Clipboard.SetText(fetchXml);
                                            ApplicationController.UserMessage("Fetch Copied To Clipboard!");
                                        });
                                    });
                                }, (g) => !QueryViewModel.IncludeNotIn),
                new CustomGridFunction("JSFETCHXML", "JavaScript FetchXML", (g) =>
                                {
                                    DoOnAsynchThread(() =>
                                    {
                                        var fetchXml = GetQueryFetchXml();
                                        DoOnMainThread(() =>
                                        {
                                            Clipboard.SetText(WriteFetchToJavascript(fetchXml));
                                            ApplicationController.UserMessage("JavaScript Copied To Clipboard!");
                                        });
                                    });
                                }, (g) => !QueryViewModel.IncludeNotIn)
            }));
            return extraFunctions;
        }

        private string GetQueryFetchXml()
        {
            var queryDefinition = this.QueryViewModel.GenerateQuery();
            queryDefinition.Top = 0;
            var fields = new List<string>();

            if (QueryViewModel.DynamicGridViewModel.FieldMetadata != null)
                fields.AddRange(QueryViewModel.DynamicGridViewModel.FieldMetadata
                    .Where(fm => fm.AltRecordType == null)
                    .Select(fm => fm.FieldName)
                    .Where(s => !s.Contains(".")));
            if (!fields.Contains(XrmRecordService.GetPrimaryKey(queryDefinition.RecordType)))
                fields.Insert(0, XrmRecordService.GetPrimaryKey(queryDefinition.RecordType));
            queryDefinition.Fields = fields;
            var fetchXml = FormatXml(XrmRecordService.ToFetchXml(queryDefinition));
            return fetchXml;
        }


        /// <summary>
        /// Formats the provided XML so it's indented and humanly-readable.
        /// https://yetanotherchris.dev/csharp/formatting-xml-in-csharp/
        /// </summary>
        /// <param name="inputXml">The input XML to format.</param>
        /// <returns></returns>
        public static string FormatXml(string inputXml)
        {
            var document = new XmlDocument();
            document.Load(new StringReader(inputXml));

            var builder = new StringBuilder();
            using (var writer = new XmlTextWriter(new StringWriter(builder)))
            {
                writer.Formatting = Formatting.Indented;
                document.Save(writer);
            }
            var toString = builder.ToString();
            return toString.Substring(toString.IndexOf("<fetch"));
        }

        private string WriteFetchToJavascript(string fetchXml)
        {
            var stringCharacter = "\'";
            var variableName = "xml";

            var fetch = fetchXml;
            if (fetch != null)
                fetch = fetch.Replace("'", "\\'");
            var splitLines = fetch
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            var conversionList = new List<string>();
            for (var i = 0; i < splitLines.Length; i++)
            {

                if (i == 0)
                    conversionList.Add(string.Format("var {0} = {1}{2}{1};", variableName, stringCharacter, splitLines[i]));
                else
                    conversionList.Add(string.Format("{0} += {1}{2}{1};", variableName, stringCharacter, splitLines[i]));
            }
            return string.Join(Environment.NewLine, conversionList);
        }
    }
}
