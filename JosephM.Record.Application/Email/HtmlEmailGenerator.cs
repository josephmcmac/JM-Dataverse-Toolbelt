using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Extentions;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;
using JosephM.Record.Service;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JosephM.Application.ViewModel.Email
{
    public class HtmlEmailGenerator
    {
        private StringBuilder _content = new StringBuilder();
        private const string pStyle = "style='font-family: Segoe UI;font-size: 13px;color: #444444;'";
        private const string thStyle = "style='font-family: Segoe UI;font-size: 13px;color: #444444;vertical-align: top;text-align: left; margin: 5px; border-style: solid; border-width: 1px; padding: 5px;'";
        private const string tdStyle = "style='font-family: Segoe UI;font-size: 13px;color: #444444;vertical-align: top;text-align: left; margin: 5px; border-style: solid; border-width: 1px; padding: 5px;'";
        private const string aStyle = "style='font-family: Segoe UI;font-size: 13px;'";

        public void AppendParagraph(string text)
        {
            _content.AppendLine(string.Format("<p {0}>{1}</p>", pStyle, text));
        }

        public void AppendSectionHeading(string text)
        {
            _content.AppendLine(string.Format("<h2 {0}><u><b>{1}<b></u></h2>", pStyle, text));
        }

        public void AppendFieldHeading(string text)
        {
            _content.AppendLine(string.Format("<h3 {0}><b>{1}<b></h3>", pStyle, text));
        }

        public void AppendFieldValue(string text)
        {
            _content.AppendLine(string.Format("<div {0}>{1}</div>", tdStyle, text));
        }

        //don't change to negative may break queries
        public static int MaximumNumberOfEntitiesToList
        {
            get
            {
                return 100;
            }
        }

        public void WriteObject(object objectToWrite)
        {
            var objectRecord = new ObjectRecord(objectToWrite);
            var recordService = new ObjectRecordService(objectToWrite, null);
            var formService = new ObjectFormService(objectToWrite, recordService);
            var formMetadata = formService.GetFormMetadata(objectToWrite.GetType().AssemblyQualifiedName);
            foreach(var section in formMetadata.FormSections.OrderBy(s => s.Order))
            {
                if(section is FormFieldSection fieldSection)
                {
                    if (fieldSection.FormFields.Any(f => objectToWrite.IsInContext(f.FieldName)))
                    {
                        _content.AppendLine("<p>");
                        if (fieldSection.DisplayLabel)
                            AppendSectionHeading(fieldSection.SectionLabel);
                        foreach (var field in fieldSection.FormFields.OrderBy(f => f.Order))
                        {
                            if (objectToWrite.IsInContext(field.FieldName))
                            {
                                if (field.DisplayLabel)
                                    AppendFieldHeading(recordService.GetFieldLabel(field.FieldName, objectRecord.Type));
                                if (recordService.GetFieldType(field.FieldName, objectRecord.Type) == RecordFieldType.Enumerable)
                                {
                                    //okay need to generate a table
                                    var enumerableMetadata = recordService.GetFieldMetadata(field.FieldName, objectRecord.Type) as EnumerableFieldMetadata;
                                    var gridFieldMetadata = recordService.GetGridFields(enumerableMetadata.EnumeratedTypeQualifiedName, ViewType.AssociatedView);
                                    var table = new StringBuilder();
                                    table.AppendLine("<table>");
                                    table.AppendLine("<thead><tr>");

                                    var fieldJustifies = new Dictionary<string, string>();
                                    foreach (var gridField in gridFieldMetadata)
                                    {
                                        var justify = recordService.GetFieldType(gridField.FieldName, enumerableMetadata.EnumeratedTypeQualifiedName).GetHorizontalJustify(true);
                                        var htmlJustify = justify == HorizontalJustify.Left
                                            ? "left"
                                            : justify == HorizontalJustify.Middle
                                            ? "center"
                                            : "right";
                                        fieldJustifies.Add(gridField.FieldName, htmlJustify);
                                    }

                                    foreach (var gridField in gridFieldMetadata)
                                    {
                                        table.AppendLine($"<th width={gridField.WidthPart} {thStyle.Replace("left", fieldJustifies[gridField.FieldName])}>{recordService.GetFieldLabel(gridField.FieldName, enumerableMetadata.EnumeratedTypeQualifiedName)}</th>");
                                    }
                                    table.AppendLine("</tr></thead>");

                                    var linkedObjects = recordService
                                        .GetLinkedRecords(enumerableMetadata.EnumeratedTypeQualifiedName, objectRecord.Type, field.FieldName, objectRecord.Id)
                                        .Cast<ObjectRecord>()
                                        .ToArray();
                                    var objectsForTable = linkedObjects
                                        .Take(MaximumNumberOfEntitiesToList)
                                        .ToArray();

                                    foreach (var gridRecord in objectsForTable.Take(MaximumNumberOfEntitiesToList))
                                    {
                                        table.AppendLine("<tr>");
                                        foreach (var gridField in gridFieldMetadata
                                            .Where(gf => objectsForTable.Any(o => o.Instance.IsInContext(gf.FieldName))))
                                        {
                                            table.AppendLine(string.Format("<td {0}>", tdStyle.Replace("left", fieldJustifies[gridField.FieldName])));
                                            table.Append(recordService.GetFieldAsDisplayString(gridRecord, gridField.FieldName));
                                            table.AppendLine("</td>");
                                        }
                                        table.AppendLine("</tr>");
                                    }
                                    table.AppendLine("</table>");
                                    _content.AppendLine(table.ToString());

                                    if (linkedObjects.Count() > MaximumNumberOfEntitiesToList)
                                    {
                                        AppendParagraph(string.Format("Note this list is incomplete as the maximum of {0} items has been listed", MaximumNumberOfEntitiesToList));
                                    }
                                }
                                else
                                    AppendFieldValue(recordService.GetFieldAsDisplayString(objectRecord, field.FieldName));
                            }
                        }
                        _content.AppendLine("</p>");
                    }
                }
            }
        }

        public string GetContent()
        {
            return _content.ToString();
        }
    }
}