﻿using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace JosephM.CodeGenerator.Service
{
    public class CodeGeneratorService<TService> :
        ServiceBase<CodeGeneratorRequest, CodeGeneratorResponse, CodeGeneratorResponseItem>
        where TService : IRecordService
    {
        private static IEnumerable<RecordFieldType> OptionSetTypes
        {
            get { return new[] { RecordFieldType.Picklist, RecordFieldType.Status, RecordFieldType.State, }; }
        }

        public CodeGeneratorService(TService service)
        {
            Service = service;
        }

        private IRecordService Service { get; set; }

        public override void ExecuteExtention(CodeGeneratorRequest request, CodeGeneratorResponse response,
            LogController controller)
        {
            switch (request.Type)
            {
                case CodeGeneratorType.CSharpMetadata:
                    {
                        WriteCSharpMetadata(request, controller);
                        break;
                    }
                case CodeGeneratorType.JavaScriptOptionSets:
                    {
                        WriteJavaScriptOptionSets(request, controller);
                        break;
                    }
                case CodeGeneratorType.FetchToJavascript:
                    {
                        response.Javascript = WriteFetchToJavascript(request, controller);
                        break;
                    }
            }
            response.Folder = request.Folder != null ? request.Folder.FolderPath : null;
            if (request.FileName != null)
            {
                response.FileName = Path.Combine(response.Folder, request.FileName);
                if (request.Type == CodeGeneratorType.JavaScriptOptionSets)
                    response.FileName = response.FileName + ".js";
                else
                    response.FileName = response.FileName + ".cs";
            }
        }

        private string WriteFetchToJavascript(CodeGeneratorRequest request, LogController controller)
        {
            var fetch = request.Fetch;
            var splitLines = fetch
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            var stringCharacter = "'";
            var variableName = "fetchXml";

            var conversionList = new List<string>();
            for (var i = 0; i < splitLines.Length; i++)
            {

                if (i == 0)
                    conversionList.Add(string.Format("var {0} = {1}{2}{1};", variableName, stringCharacter, splitLines[i]));
                else
                    conversionList.Add(string.Format("{0} = {0} + {1}{2}{1};", variableName, stringCharacter, splitLines[i]));
            }
            return string.Join(Environment.NewLine, conversionList);
        }

        private void AppendGenerationComments(StringBuilder sb)
        {
            sb.AppendLine("/*");
            sb.AppendLine(
                "This code was generated by the JosephM Xrm Developer Tool https://github.com/josephmcmac/XRM-Developer-Tool");
            sb.AppendLine("*/");
        }

        private void WriteCSharpMetadata(CodeGeneratorRequest request, LogController controller)
        {
            var stringBuilder = new StringBuilder();
            AppendGenerationComments(stringBuilder);
            stringBuilder.AppendLine("namespace " + request.Namespace);
            stringBuilder.AppendLine("{");
            controller.LogLiteral("Loading Types.....");
            AppendEntities(controller, request, stringBuilder);
            AppendRelationships(controller, request, stringBuilder);
            AppendFields(controller, stringBuilder, request);
            AppendOptionSets(stringBuilder, request, controller);
            AppendActions(stringBuilder, request, controller);
            stringBuilder.AppendLine("}");
            FileUtility.WriteToFile(request.Folder.FolderPath,
                request.FileName.EndsWith(".cs") ? request.FileName : request.FileName + ".cs",
                stringBuilder.ToString());
        }

        private void AppendEntities(LogController controller, CodeGeneratorRequest request, StringBuilder stringBuilder)
        {
            if (!request.IncludeEntities)
                return;
            var types = GetRecordTypesToImport(request);
            var countToDo = types.Count();
            var countDone = 0;
            stringBuilder.AppendLine(string.Format("\tpublic static class {0}", "Entities"));
            stringBuilder.AppendLine("\t{");
            foreach (var recordType in types)
            {
                controller.UpdateProgress(countDone, countToDo,
                    string.Format("Processing Entities ({0})", Service.GetDisplayName(recordType)));

                if (!recordType.IsNullOrWhiteSpace())
                    stringBuilder.AppendLine(string.Format("\t\tpublic const string {0} = \"{1}\";", recordType,
                        recordType));
                countDone++;
            }
            stringBuilder.AppendLine("\t}");
        }

        private void AppendRelationships(LogController controller, CodeGeneratorRequest request, StringBuilder stringBuilder)
        {
            if (!request.IncludeRelationships)
                return;
            var types = GetRecordTypesToImport(request);
            var countToDo = types.Count();
            var countDone = 0;
            var relationshipsAdded = false;
            countDone = 0;
            foreach (var recordType in types)
            {
                controller.UpdateProgress(countDone, countToDo,
                    string.Format("Processing Relationships ({0})", Service.GetDisplayName(recordType)));
                if (!recordType.IsNullOrWhiteSpace())
                {
                    var relationships = Service.GetManyToManyRelationships(recordType);
                    if (relationships.Any())
                    {
                        if (!relationshipsAdded)
                        {
                            stringBuilder.AppendLine(string.Format("\tpublic static class {0}", "Relationships"));
                            stringBuilder.AppendLine("\t{");
                            relationshipsAdded = true;
                        }
                        stringBuilder.AppendLine(string.Format("\t\tpublic static class {0}_", recordType));
                        stringBuilder.AppendLine("\t\t{");
                        foreach (var relationship in relationships)
                        {
                            stringBuilder.AppendLine(string.Format("\t\t\tpublic static class {0}", relationship.SchemaName));
                            stringBuilder.AppendLine("\t\t\t{");
                            stringBuilder.AppendLine(string.Format("\t\t\t\tpublic const string Name = \"{0}\";",
                                relationship.SchemaName));
                            stringBuilder.AppendLine(string.Format("\t\t\t\tpublic const string EntityName = \"{0}\";",
                                relationship.IntersectEntityName));
                            stringBuilder.AppendLine("\t\t\t}");
                        }
                        stringBuilder.AppendLine("\t\t}");
                    }
                }
                countDone++;
            }
            if (relationshipsAdded)
                stringBuilder.AppendLine("\t}");
        }

        private void AppendFields(LogController controller, StringBuilder stringBuilder, CodeGeneratorRequest request)
        {
            if (!request.IncludeFields)
                return;
            var types = GetRecordTypesToImport(request);
            var countToDo = types.Count();
            stringBuilder.AppendLine("\tpublic static class Fields");
            stringBuilder.AppendLine("\t{");
            var countDone = 0;
            foreach (var recordType in types)
            {
                controller.UpdateProgress(countDone, countToDo,
                    string.Format("Processing Fields ({0})", Service.GetDisplayName(recordType)));
                if (!recordType.IsNullOrWhiteSpace())
                {
                    stringBuilder.AppendLine(string.Format("\t\tpublic static class {0}_", recordType));
                    stringBuilder.AppendLine("\t\t{");
                    var fieldLabels = new Dictionary<string, string>();
                    foreach (var field in Service.GetFields(recordType))
                    {
                        if (!field.IsNullOrWhiteSpace())
                            stringBuilder.AppendLine(string.Format("\t\t\tpublic const string {0} = \"{1}\";",
                                CreateCodeLabel(field), field));
                    }
                    stringBuilder.AppendLine("\t\t}");
                }
                countDone++;
            }
            stringBuilder.AppendLine("\t}");
        }

        private IEnumerable<string> GetRecordTypesToImport(CodeGeneratorRequest request)
        {
            return request.AllRecordTypes
                ? Service.GetAllRecordTypes()
                : request.RecordTypes.Select(r => r.RecordType.Key);
        }

        private void WriteJavaScriptOptionSets(CodeGeneratorRequest request, LogController controller)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("{0} = new Object()", request.Namespace));
            AppendJavaScriptOptionSets(stringBuilder, request, controller);
            FileUtility.WriteToFile(request.Folder.FolderPath,
                request.FileName.EndsWith(".js") ? request.FileName : request.FileName + ".js",
                stringBuilder.ToString());
        }

        private void AppendJavaScriptOptionSets(StringBuilder stringBuilder, CodeGeneratorRequest request,
            LogController controller)
        {
            var countToDo = 2;
            var countDone = 1;

            if(request.RecordType == null)
                throw new NullReferenceException("Error record type is null");
            var recordType = request.RecordType.Key;

            controller.UpdateProgress(countDone, countToDo,
                string.Format("Processing Options ({0})", Service.GetDisplayName(recordType)));
            if (IsValidForCode(recordType))
            {
                stringBuilder.AppendLine(string.Format("{0}.Options = new Object();", request.Namespace));
                var fieldsToProcess = request.AllFields
                    ? Service.GetFields(recordType).Where(f => IsValidForOptionSetCode(f, recordType))
                    : new[] {request.OptionField != null ? request.OptionField.Key : null};

                foreach (var field in fieldsToProcess)
                {
                        var fieldLabel = CreateCodeLabel(Service.GetFieldLabel(field, recordType));
                        stringBuilder.AppendLine(string.Format("{0}.{1}.{2} = new Object();", request.Namespace,
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
                                    request.Namespace, "Options", fieldLabel, label, option.Key));
                                used.Add(label);
                            }
                        }
                    }
                }
            }
            countDone++;
        }

        private void AppendActions(StringBuilder stringBuilder, CodeGeneratorRequest request,
            LogController controller)
        {
            if (!request.IncludeActions)
                return;

            controller.UpdateProgress(1, 2,
                        string.Format("Processing Actions"));

            var actions = Service.RetrieveAllAndClauses("workflow", new[]
            {
                new Condition("category", ConditionType.Equal, XrmPicklists.WorkflowCategory.Action),
                new Condition("type", ConditionType.Equal, XrmPicklists.WorkflowType.Definition)
            }, null);

            if (actions.Any())
            {
                var requests = Service.RetrieveAllOrClauses(Entities.sdkmessage,
                    actions.Select(a => a.GetLookupId(Fields.workflow_.sdkmessageid))
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => new Condition(Fields.sdkmessage_.sdkmessageid, ConditionType.Equal, s))
                    , null);

                stringBuilder.AppendLine("\tpublic static class Actions");
                stringBuilder.AppendLine("\t{");
                foreach (var action in actions)
                {
                    var noTarget = action.GetStringField("primaryentity") == "none";

                    var sdkMessageId = action.GetLookupId(Fields.workflow_.sdkmessageid);
                    var matchingRequestNames = requests
                        .Where(r => r.Id == sdkMessageId)
                        .Select(r => r.GetStringField(Fields.workflow_.name))
                        .ToArray();
                    if (matchingRequestNames.Any())
                    {
                        var actionName = matchingRequestNames.First();
                        var inArguments = new List<string>();
                        var outArguments = new List<string>();

                        var document = new XmlDocument();
                        document.LoadXml(action.GetStringField("xaml"));


                        foreach (XmlNode childNode in document.ChildNodes)
                        {
                            if (childNode.Name == "Activity")
                            {
                                foreach (XmlNode childNode2 in childNode.ChildNodes)
                                {
                                    if (childNode2.Name == "x:Members")
                                    {
                                        foreach (XmlNode property in childNode2.ChildNodes)
                                        {
                                            foreach (XmlNode propertyChild in property.ChildNodes)
                                            {
                                                if (propertyChild.Name == "x:Property.Attributes")
                                                {
                                                    var argumentName = property.Attributes != null
                                                                       && property.Attributes["Name"] != null
                                                        ? property.Attributes["Name"].InnerText
                                                        : null;
                                                    foreach (XmlNode attribute in propertyChild.ChildNodes)
                                                    {
                                                        var direction = attribute.Attributes != null
                                                                        && attribute.Attributes["Value"] != null
                                                            ? attribute.Attributes["Value"].InnerText
                                                            : null;
                                                        if (direction == "Input")
                                                        {
                                                            //don't add a=target of global action type
                                                            if (!noTarget || argumentName != "Target")
                                                                inArguments.Add(argumentName);
                                                        }
                                                        if (direction == "Output")
                                                        {
                                                            outArguments.Add(argumentName);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        stringBuilder.AppendLine(string.Format("\t\tpublic static class {0}", actionName));
                        stringBuilder.AppendLine("\t\t{");
                        stringBuilder.AppendLine(string.Format("\t\t\tpublic const string Name = \"{0}\";", actionName));
                        if (inArguments.Any())
                        {
                            stringBuilder.AppendLine("\t\t\tpublic static class In");
                            stringBuilder.AppendLine("\t\t\t{");
                            foreach (var item in inArguments)
                            {
                                stringBuilder.AppendLine(string.Format("\t\t\t\tpublic const string {0} = \"{0}\";", item));
                            }
                            stringBuilder.AppendLine("\t\t\t}");
                        }
                        if (outArguments.Any())
                        {
                            stringBuilder.AppendLine("\t\t\tpublic static class Out");
                            stringBuilder.AppendLine("\t\t\t{");
                            foreach (var item in outArguments)
                            {
                                stringBuilder.AppendLine(string.Format("\t\t\t\tpublic const string {0} = \"{0}\";", item));
                            }
                            stringBuilder.AppendLine("\t\t\t}");

                        }
                        stringBuilder.AppendLine("\t\t}");
                    }
                }
                stringBuilder.AppendLine("\t}");
            }
        }

        private void AppendOptionSets(StringBuilder stringBuilder, CodeGeneratorRequest request,
            LogController controller)
        {
            if (!request.IncludeOptions)
                return;
            stringBuilder.AppendLine("\tpublic static class OptionSets");
            stringBuilder.AppendLine("\t{");

            if (request.IncludeAllSharedOptions)
            {
                var picklists = Service.GetSharedPicklists();
                var countOptionsToDo = picklists.Count();
                var countOptionsDone = 0;

                var duplicateSharedLabels = picklists
                    .GroupBy(t => CreateCodeLabel(t.DisplayName), t => t)
                    .Where(t => t.Count() > 1)
                    .Select(t => t.Key)
                    .ToArray();

                stringBuilder.AppendLine("\t\tpublic static class Shared");
                stringBuilder.AppendLine("\t\t{");
                foreach (var item in picklists)
                {
                    controller.UpdateProgress(countOptionsDone, countOptionsToDo,
                        string.Format("Processing Shared Options ({0})", item.DisplayName));
                    var optionSetLabel = CreateCodeLabel(item.DisplayName);
                    if (duplicateSharedLabels.Contains(optionSetLabel))
                        optionSetLabel = optionSetLabel + "_" + item.SchemaName;
                    stringBuilder.AppendLine("\t\t\tpublic static class " + optionSetLabel);
                    stringBuilder.AppendLine("\t\t\t{");
                    foreach (var option in item.PicklistOptions)
                    {
                        var optionLabel = CreateCodeLabel(option.Value);
                        if (optionLabel == optionSetLabel)
                            optionLabel = optionLabel + "_";
                        stringBuilder.AppendLine(
                            string.Format("\t\t\t\tpublic const int {0} = {1};", optionLabel, option.Key));
                    }
                    stringBuilder.AppendLine("\t\t\t}");
                }
                stringBuilder.AppendLine("\t\t}");
            }
            var types = GetRecordTypesToImport(request);
            var countToDo = types.Count();
            var countDone = 0;
            var duplicateTypesLabels = types
                .GroupBy(t => CreateCodeLabel(Service.GetDisplayName(t)), t => t)
                .Where(t => t.Count() > 1)
                .Select(t => t.Key)
                .ToArray();
            foreach (var recordType in types)
            {
                controller.UpdateProgress(countDone, countToDo,
                    string.Format("Processing Options ({0})", Service.GetDisplayName(recordType)));

                if (IsValidForCode(recordType))
                {
                    var recordTypeCodeLabel = CreateCodeLabel(Service.GetDisplayName(recordType));
                    if (duplicateTypesLabels.Contains(recordTypeCodeLabel))
                        recordTypeCodeLabel = recordTypeCodeLabel + "_" + recordType;
                    stringBuilder.AppendLine("\t\tpublic static class " + recordTypeCodeLabel);
                    stringBuilder.AppendLine("\t\t{");
                    var optionFields = Service.GetFields(recordType).Where(f => IsValidForOptionSetCode(f, recordType));
                    var optionFieldLabels = optionFields.ToDictionary(f => f,
                        f => CreateCodeLabel(Service.GetFieldLabel(f, recordType)));
                    foreach (var field in optionFieldLabels)
                    {
                        var fieldLabel = field.Value;
                        if (optionFieldLabels.Any(kv => kv.Key != field.Key && kv.Value == field.Value))
                            fieldLabel = string.Format("{0}_{1}", fieldLabel, field.Key);
                        if (fieldLabel == recordTypeCodeLabel)
                            fieldLabel = string.Format("{0}_", fieldLabel);
                        stringBuilder.AppendLine("\t\t\tpublic static class " + fieldLabel);
                        stringBuilder.AppendLine("\t\t\t{");
                        var options = Service.GetPicklistKeyValues(field.Key, recordType);
                        var used = new List<string>();
                        foreach (var option in options)
                        {
                            if (IsValidForCode(option))
                            {
                                var optionLabel = CreateCodeLabel(option.Value);
                                if (optionLabel == fieldLabel)
                                    optionLabel = string.Format("{0}_", optionLabel);
                                if (!used.Contains(optionLabel))
                                {
                                    stringBuilder.AppendLine(
                                        string.Format("\t\t\t\tpublic const int {0} = {1};", optionLabel, option.Key));
                                    used.Add(optionLabel);
                                }
                            }
                        }
                        stringBuilder.AppendLine("\t\t\t}");

                    }
                    stringBuilder.AppendLine("\t\t}");
                }
                countDone++;
            }
            stringBuilder.AppendLine("\t}");
        }

        private static IEnumerable<string> KeyWords
        {
            get { return new[] { "abstract", "event", "namespace" }; }
        }

        private static string CreateCodeLabel(string rawLabel)
        {
            var stringBuilder = new StringBuilder();
            if (!rawLabel.IsNullOrWhiteSpace())
            {
                for (var i = 0; i < rawLabel.Length; i++)
                {
                    var c = rawLabel.ElementAt(i);
                    if (c != '_' && (char.IsWhiteSpace(c) || !char.IsLetterOrDigit(c)))
                    {
                        continue;
                    }
                    if (stringBuilder.Length == 0 && char.IsDigit(c))
                    {
                        stringBuilder.Append("_");
                        stringBuilder.Append(c);
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }
                }
            }
            var result = stringBuilder.ToString();
            if (KeyWords.Contains(result))
                result = result + "_";
            return result;
        }

        private bool IsValidForCode(string recordType)
        {
            return
                !CreateCodeLabel(Service.GetDisplayName(recordType)).IsNullOrWhiteSpace()
                && Service.GetFields(recordType).Any(f => IsValidForOptionSetCode(f, recordType));
        }

        private bool IsValidForOptionSetCode(string field, string recordType)
        {
            return
                OptionSetTypes.Contains(Service.GetFieldType(field, recordType))
                && !CreateCodeLabel(Service.GetFieldLabel(field, recordType)).IsNullOrWhiteSpace()
                && Service.GetPicklistKeyValues(field, recordType).Any(IsValidForCode);
        }

        private bool IsValidForCode(PicklistOption option)
        {
            return
                !CreateCodeLabel(option.Value).IsNullOrWhiteSpace();
        }
    }
}