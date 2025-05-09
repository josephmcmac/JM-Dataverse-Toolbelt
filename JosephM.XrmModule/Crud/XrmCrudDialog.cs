using JosephM.Application.Desktop.Module.Crud;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Crud.AddRoles;
using JosephM.XrmModule.Crud.BulkWorkflow;
using JosephM.XrmModule.Crud.RemoveRoles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace JosephM.XrmModule.Crud
{
    [RequiresConnection(escapeConnectionCheckProperty: nameof(LoadedFromConnection))]
    public class XrmCrudDialog : CrudDialog
    {
        public XrmCrudDialog(XrmRecordService xrmrecordService, IDialogController dialogController)
            : base(dialogController, xrmrecordService)
        {
            XrmRecordService = xrmrecordService;
        }

        public bool LoadedFromConnection { get; set; }

        public override IEnumerable<string> AdditionalExplicitTypes => new[] { Entities.webresource, Entities.savedqueryvisualization, Entities.userqueryvisualization, Entities.appmodule, Entities.appmodulecomponent, Entities.solutioncomponent, Entities.solution, Entities.timezonedefinition, Entities.activityparty, Entities.userquery, Entities.sitemap, Entities.subject, Entities.systemform, Entities.uom, Entities.savedquery, Entities.calendar, Entities.role, Entities.sdkmessageprocessingstep, Entities.activitymimeattachment, Entities.organization, Entities.usersettings, Entities.productpricelevel, Entities.fieldpermission, Entities.incidentresolution, Entities.principalobjectaccess, Entities.audit, Entities.attributemap, Entities.entitymap, Entities.relationshiprolemap, Entities.privilege, Entities.processstage, Entities.ribboncommand, Entities.ribbonrule, Entities.ribboncustomization, Entities.ribbondiff, Entities.exchangesyncidmapping };

        public XrmRecordService XrmRecordService { get; }

        protected override void AppendExtendedGridFunctions(List<CustomGridFunction> gridFunctionList)
        {
            var otherActionMatches = gridFunctionList.Where(g => g.Id == "OTHERACTIONS");
            if (otherActionMatches.Any())
            {
                var otherActionsMenu = otherActionMatches.First();
                var newChildFunctionList = new List<CustomGridFunction>(otherActionsMenu.ChildGridFunctions);
                newChildFunctionList.AddRange(new[]
                {
                    new CustomGridFunction("COPYQUERY", "Copy FetchXML", new[]
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
                            }, (g) => !QueryViewModel.IncludeNotIn),
                        new CustomGridFunction("PRIMARYFETCHFILTER", "Primary Filter", (g) =>
                            {
                                DoOnAsynchThread(() =>
                                {
                                    var fetchXmlString = GetQueryFetchXml();
                                    var fetchXml = new XmlDocument();
                                    fetchXml.LoadXml(fetchXmlString);

                                    var attributeNodes = fetchXml.SelectNodes("//filter");
                                    if (attributeNodes == null && attributeNodes.Count == 0)
                                    {
                                        ApplicationController.UserMessage("No Filter Conditions Found");
                                    }
                                    else
                                    {
                                    DoOnMainThread(() =>
                                    {
                                        Clipboard.SetText(FormatXml(attributeNodes[0].OuterXml, "filter"));
                                        ApplicationController.UserMessage("Primary Filter Copied To Clipboard!");
                                    });
                                    }
                                });
                            }, (g) => !QueryViewModel.IncludeNotIn)
                    }),
                    new CustomGridFunction("ADDROLES", "Add Security Role", new[]
                    {
                        new CustomGridFunction("ADDROLESALL", "All Results", (g) =>
                            {
                                TriggerAddRoles(false);
                            }, (g) => g.RecordType == Entities.systemuser),
                        new CustomGridFunction("ADDROLESSELECTED", "Selected Only", (g) =>
                            {
                                TriggerAddRoles(true);
                            }, (g) => g.RecordType == Entities.systemuser),
                    }),
                    new CustomGridFunction("REMOVEROLES", "Remove Security Role", new[]
                    {
                        new CustomGridFunction("REMOVEROLESALL", "All Results", (g) =>
                            {
                                TriggerRemoveRoles(false);
                            }, (g) => g.RecordType == Entities.systemuser),
                        new CustomGridFunction("REMOVEROLESSELECTED", "Selected Only", (g) =>
                            {
                                TriggerRemoveRoles(true);
                            }, (g) => g.RecordType == Entities.systemuser),
                    }),
                    new CustomGridFunction("BULKWORKFLOW", "Run Workflow", new[]
                    {
                        new CustomGridFunction("BULKWORKFLOWALL", "All Results", (g) =>
                            {
                                TriggerBulkWorkflow(false);
                            }, (g) => g.GridRecords != null && g.GridRecords.Any()),
                        new CustomGridFunction("BULKWORKFLOWSELECTED", "Selected Only", (g) =>
                            {
                                TriggerBulkWorkflow(true);
                            },  (g) => g.SelectedRows.Any()),
                    }),
                });
                otherActionsMenu.ChildGridFunctions = newChildFunctionList;
            }
        }

        private void TriggerAddRoles(bool selectedOnly)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                var recordsToUpdate = GetRecordsToProcess(selectedOnly);

                var request = new AddRolesRequest(recordsToUpdate);
                var addRolesDialog = new AddRolesDialog(XrmRecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request, CompleteChildDialogAndReload);
                LoadChildForm(addRolesDialog);
            });
        }

        private void TriggerRemoveRoles(bool selectedOnly)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                var recordsToUpdate = GetRecordsToProcess(selectedOnly);

                var request = new RemoveRolesRequest(recordsToUpdate);
                var removeRolesDialog = new RemoveRolesDialog(XrmRecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request, CompleteChildDialogAndReload);
                LoadChildForm(removeRolesDialog);
            });
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
            var lastSortExpression = QueryViewModel.DynamicGridViewModel.GetLastSortExpression();
            if(lastSortExpression != null)
            {
                queryDefinition.Sorts = new List<Record.Query.SortExpression> { lastSortExpression };
            }
            var fetchXml = FormatXml(XrmRecordService.ToFetchXml(queryDefinition), "fetch");
            return fetchXml;
        }


        /// <summary>
        /// Formats the provided XML so it's indented and humanly-readable.
        /// https://yetanotherchris.dev/csharp/formatting-xml-in-csharp/
        /// </summary>
        /// <param name="inputXml">The input XML to format.</param>
        /// <returns></returns>
        public static string FormatXml(string inputXml, string firstNodeName)
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
            return toString.Substring(toString.IndexOf($"<{firstNodeName}"));
        }

        private static string WriteFetchToJavascript(string fetchXml)
        {
            var stringCharacter = "\'";
            var variableName = "fetchXml";

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

        private void TriggerBulkWorkflow(bool selectedOnly)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                var recordsToUpdate = GetRecordsToProcess(selectedOnly);
                var request = new BulkWorkflowRequest(new RecordType(QueryViewModel.RecordType, RecordService.GetDisplayName(QueryViewModel.RecordType)), recordsToUpdate);
                request.AllowExecuteMultiples = RecordService.SupportsExecuteMultiple;
                var bulkDialog = new BulkWorkflowDialog(XrmRecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request, CompleteChildDialogAndReload);
                LoadChildForm(bulkDialog);
            });
        }
    }
}