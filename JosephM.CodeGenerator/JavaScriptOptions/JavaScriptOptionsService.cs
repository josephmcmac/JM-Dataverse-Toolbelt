using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JosephM.CodeGenerator.JavaScriptOptions
{
    public class JavaScriptOptionsService : CodeGeneratorServiceBase<JavaScriptOptionsRequest, JavaScriptOptionsResponse, ServiceResponseItem>
    {
        public JavaScriptOptionsService(XrmRecordService service)
            : base(service)
        {
        }

        public override void ExecuteExtention(JavaScriptOptionsRequest request, JavaScriptOptionsResponse response,
            ServiceRequestController controller)
        {
            response.Javascript = WriteJavaScriptOptionSets(request, controller.Controller);
        }

        private string WriteJavaScriptOptionSets(JavaScriptOptionsRequest request, LogController controller)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("{0} = new Object()", request.NamespaceOfTheJavaScriptObject));
            AppendJavaScriptOptionSets(stringBuilder, request, controller);
            return stringBuilder.ToString();
        }

        private void AppendJavaScriptOptionSets(StringBuilder stringBuilder, JavaScriptOptionsRequest request,
            LogController controller)
        {
            var countToDo = 2;
            var countDone = 1;

            if (request.RecordType == null)
                throw new NullReferenceException("Error record type is null");
            var recordType = request.RecordType.Key;

            controller.UpdateProgress(countDone, countToDo,
                string.Format("Processing Options ({0})", Service.GetDisplayName(recordType)));
            if (IsValidForCode(recordType))
            {
                stringBuilder.AppendLine(string.Format("{0}.Options = new Object();", request.NamespaceOfTheJavaScriptObject));
                var fieldsToProcess = request.AllOptionSetFields
                    ? Service.GetFields(recordType).Where(f => IsValidForOptionSetCode(f, recordType))
                    : new[] { request.SpecificOptionSetField != null ? request.SpecificOptionSetField.Key : null };

                foreach (var field in fieldsToProcess)
                {
                    var fieldLabel = CreateCodeLabel(Service.GetFieldLabel(field, recordType));
                    stringBuilder.AppendLine(string.Format("{0}.{1}.{2} = new Object();", request.NamespaceOfTheJavaScriptObject,
                        "Options",
                        fieldLabel));
                    var options = Service.GetPicklistKeyValues(field, recordType);
                    var used = new List<string>();
                    foreach (var option in options)
                    {
                        if (IsValidForCode(option))
                        {
                            var label = CreateCodeLabel(option.Value);
                            if (!used.Contains(label))
                            {
                                stringBuilder.AppendLine(string.Format("{0}.{1}.{2}.{3} = {4};",
                                    request.NamespaceOfTheJavaScriptObject, "Options", fieldLabel, label, option.Key));
                                used.Add(label);
                            }
                        }
                    }
                }
            }
            countDone++;
        }
    }
}