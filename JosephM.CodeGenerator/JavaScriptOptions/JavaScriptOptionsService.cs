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
                var fieldsToProcess = request.AllOptionSetFields
                    ? Service.GetFields(recordType).Where(f => IsValidForOptionSetCode(f, recordType))
                    : new[] { request.SpecificOptionSetField != null ? request.SpecificOptionSetField.Key : null };

                var optionDictionary = new SortedDictionary<string, SortedDictionary<string, string>>();

                foreach (var field in fieldsToProcess)
                {
                    var fieldLabel = CreateCodeLabel(Service.GetFieldLabel(field, recordType));
                    if (!optionDictionary.ContainsKey(fieldLabel))
                    {
                        optionDictionary.Add(fieldLabel, new SortedDictionary<string, string>());
                        var options = Service.GetPicklistKeyValues(field, recordType);
                        var used = new List<string>();
                        foreach (var option in options)
                        {
                            if (IsValidForCode(option))
                            {
                                var optionLabel = CreateCodeLabel(option.Value);
                                if (!optionDictionary[fieldLabel].ContainsKey(optionLabel))
                                {
                                    optionDictionary[fieldLabel].Add(optionLabel, option.Key);
                                }
                            }
                        }
                    }
                }

                stringBuilder.AppendLine("var options = {");
                var picklistsToInclude = optionDictionary.Where(i => i.Value.Any()).ToArray();
                var numberOfPicklistsRemaining = picklistsToInclude.Count();
                foreach (var optionSet in picklistsToInclude)
                {
                    stringBuilder.AppendLine("\t" + optionSet.Key + " : {");
                    var numberOfoptionsRemaining = optionSet.Value.Count();
                    foreach (var option in optionSet.Value)
                    {
                        var isLastOption = numberOfoptionsRemaining == 1;
                        stringBuilder.AppendLine("\t\t" + option.Key + " : " + option.Value + (isLastOption ? "" : ","));
                        numberOfoptionsRemaining--;
                    }
                    var isLastItem = numberOfPicklistsRemaining == 1;
                    stringBuilder.AppendLine("\t}" + (isLastItem ? "" : ","));
                    numberOfPicklistsRemaining--;
                }
                stringBuilder.AppendLine("};");
            }
            countDone++;
        }
    }
}