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
            extraFunctions.Add(new CustomGridFunction("COPYFETCH", "Copy FetchXML", (g) =>
                                {
                                    DoOnAsynchThread(() =>
                                    {
                                        var queryDefinition = this.QueryViewModel.GenerateQuery();
                                        queryDefinition.Top = 0;
                                        var fields = new List<string>();

                                        if (QueryViewModel.DynamicGridViewModel.FieldMetadata != null)
                                            fields.AddRange(QueryViewModel.DynamicGridViewModel.FieldMetadata
                                                .Select(fm => fm.FieldName)
                                                .Where(s => !s.Contains(".")));
                                        if (!fields.Contains(XrmRecordService.GetPrimaryKey(queryDefinition.RecordType)))
                                            fields.Insert(0, XrmRecordService.GetPrimaryKey(queryDefinition.RecordType));
                                        queryDefinition.Fields = fields;
                                        var fetchXml = XrmRecordService.ToFetchXml(queryDefinition);
                                        DoOnMainThread(() =>
                                        {
                                            Clipboard.SetText(FormatXml(fetchXml));
                                            ApplicationController.UserMessage("Fetch Copied To Clipboard!");
                                        });
                                    });
                                }));
            return extraFunctions;
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

            return builder.ToString();
        }
    }
}
