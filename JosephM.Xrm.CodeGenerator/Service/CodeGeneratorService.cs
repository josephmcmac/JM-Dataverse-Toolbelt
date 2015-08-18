﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.IService;
using JosephM.Record.Metadata;

namespace JosephM.Xrm.CodeGenerator.Service
{
    public class CodeGeneratorService<TService> :
        ServiceBase<CodeGeneratorRequest, CodeGeneratorResponse, CodeGeneratorResponseItem>
        where TService : IRecordService
    {
        private static IEnumerable<RecordFieldType> OptionSetTypes
        {
            get { return new[] {RecordFieldType.Picklist, RecordFieldType.Status}; }
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
                case CodeGeneratorType.CSharpEntitiesAndFields:
                {
                    WriteCSharpEntitiesAndFields(request, controller);
                    break;
                }
                case CodeGeneratorType.CSharpOptionSets:
                {
                    WriteCSharpOptionSets(request, controller);
                    break;
                }
                case CodeGeneratorType.JavaScriptOptionSets:
                {
                    WriteJavaScriptOptionSets(request, controller);
                    break;
                }
                //case CodeGeneratorType.SqlViews:
                //{
                //    WriteSqlViews(request, controller);
                //    break;
                //}
            }
            response.Folder = request.Folder != null ? request.Folder.FolderPath : null;
            if (request.FileName != null)
            {
                response.FileName = Path.Combine(response.Folder, request.FileName);
                if (request.Type == CodeGeneratorType.JavaScriptOptionSets)
                    response.FileName = response.FileName + ".js";
                //else if (request.Type == CodeGeneratorType.SqlViews)
                //    response.FileName = response.FileName + ".sql";
                else
                    response.FileName = response.FileName + ".cs";
            }
        }

        private void WriteCSharpEntitiesAndFields(CodeGeneratorRequest request, LogController controller)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("namespace " + request.Namespace);
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(string.Format("\tpublic class {0}", "Entities"));
            stringBuilder.AppendLine("\t{");
            controller.LogLiteral("Loading Types.....");
            var types = GetRecordTypesToImport(request);
            var countToDo = types.Count();
            var countDone = 0;
            foreach (var recordType in types)
            {
                controller.UpdateProgress(countDone, countToDo, string.Format("Processing Entities ({0})", Service.GetDisplayName(recordType)));
                
                if (!recordType.IsNullOrWhiteSpace())
                    stringBuilder.AppendLine(string.Format("\t\tpublic const string {0} = \"{1}\";", recordType,
                        recordType));
                countDone++;
            }
            stringBuilder.AppendLine("\t}");
            var relationshipsAdded = false;
            countDone = 0;
            foreach (var recordType in types)
            {
                controller.UpdateProgress(countDone, countToDo, string.Format("Processing Relationships ({0})", Service.GetDisplayName(recordType)));
                if (!recordType.IsNullOrWhiteSpace())
                {
                    var relationships = Service.GetManyToManyRelationships(recordType);
                    if (relationships.Any())
                    {
                        if (!relationshipsAdded)
                        {
                            stringBuilder.AppendLine(string.Format("\tpublic class {0}", "Relationships"));
                            stringBuilder.AppendLine("\t{");
                            relationshipsAdded = true;
                        }
                        stringBuilder.AppendLine(string.Format("\t\tpublic class {0}_", recordType));
                        stringBuilder.AppendLine("\t\t{");
                        foreach (var relationship in relationships)
                        {
                            stringBuilder.AppendLine(string.Format("\t\t\tpublic class {0}", relationship.SchemaName));
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
            stringBuilder.AppendLine(string.Format("\tpublic class {0}", "Fields"));
            stringBuilder.AppendLine("\t{");
            countDone = 0;
            foreach (var recordType in types)
            {
                controller.UpdateProgress(countDone, countToDo, string.Format("Processing Fields ({0})", Service.GetDisplayName(recordType)));
                if (!recordType.IsNullOrWhiteSpace())
                {
                    stringBuilder.AppendLine(string.Format("\t\tpublic class {0}_", recordType));
                    stringBuilder.AppendLine("\t\t{");
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
            stringBuilder.AppendLine("}");
            FileUtility.WriteToFile(request.Folder.FolderPath,
                request.FileName.EndsWith(".cs") ? request.FileName : request.FileName + ".cs",
                stringBuilder.ToString());
        }

        private void WriteSqlViews(CodeGeneratorRequest request, LogController controller)
        {
            foreach (var recordType in GetRecordTypesToImport(request))
            {
                var thisRecordType = recordType;
                var stringBuilder = new StringBuilder();
                var viewName = CreateCodeLabel(Service.GetCollectionName(thisRecordType));
                stringBuilder.AppendLine(
                    string.Format("if exists (select * from sys.all_objects where type='V' and name='{0}')", viewName));
                stringBuilder.AppendLine("begin");
                stringBuilder.AppendLine(string.Format("drop view {0}", viewName));
                stringBuilder.AppendLine("end");
                stringBuilder.AppendLine("GO");
                stringBuilder.AppendLine(string.Format("create view {0} as", viewName));
                stringBuilder.AppendLine("select");
                var fieldsOrdered =
                    Service.GetFields(thisRecordType)
                        .OrderBy(f => CreateCodeLabel(Service.GetFieldLabel(f, thisRecordType)));
                foreach (var field in fieldsOrdered)
                {
                    if (new[] {"isautocreate", "subscriptionid"}.Contains(field)) continue;
                    var fieldLabel = CreateCodeLabel(Service.GetFieldLabel(field, thisRecordType));
                    if (fieldLabel.IsNullOrWhiteSpace()) continue;
                    switch (Service.GetFieldType(field, thisRecordType))
                    {
                        case RecordFieldType.Uniqueidentifier:
                        {
                            stringBuilder.AppendLine(string.Format("[{0}] as [{1}Guid],", field, fieldLabel));
                            break;
                        }
                        case RecordFieldType.Date:
                        {
                            stringBuilder.AppendLine(string.Format("[{0}] as [{1}Local],", field, fieldLabel));
                            stringBuilder.AppendLine(string.Format("[{0}utc] as [{1}UTC],", field, fieldLabel));
                            break;
                        }
                        case RecordFieldType.Picklist:
                        {
                            stringBuilder.AppendLine(string.Format("[{0}] as [{1}Code],", field, fieldLabel));
                            stringBuilder.AppendLine(string.Format("[{0}name] as [{1}Label],", field, fieldLabel));
                            break;
                        }
                        case RecordFieldType.Boolean:
                        {
                            stringBuilder.AppendLine(string.Format("[{0}] as [{1}],", field, fieldLabel));
                            if (field == "donotsendmm")
                                stringBuilder.AppendLine(
                                    string.Format("[donotsendmarketingmaterialname] as [{0}Label],", fieldLabel));
                            else
                                stringBuilder.AppendLine(string.Format("[{0}name] as [{1}Label],", field,
                                    fieldLabel));
                            break;
                        }
                        case RecordFieldType.Lookup:
                        {
                            stringBuilder.AppendLine(string.Format("[{0}] as [{1}Id],", field, fieldLabel));
                            if (
                                !new[] {"owningbusinessunit", "owninguser", "owningteam", "masterid", "salesorderid"}
                                    .Contains(
                                        field))
                                stringBuilder.AppendLine(string.Format("[{0}name] as [{1}IdName],", field,
                                    fieldLabel));
                            break;
                        }
                        default:
                        {
                            stringBuilder.AppendLine(string.Format("[{0}] as [{1}],", field, fieldLabel));
                            break;
                        }
                    }
                }
                var indexOfLastComma = stringBuilder.ToString().LastIndexOf(",");
                stringBuilder.Remove(indexOfLastComma, stringBuilder.Length - indexOfLastComma);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("from");
                stringBuilder.AppendLine(string.Format("[{0}].dbo.[{1}]", Service.GetDatabaseName(),
                    Service.GetSqlViewName(thisRecordType)));
                stringBuilder.AppendLine("GO");
                stringBuilder.AppendLine(string.Format("GRANT select on {0} to [public]", viewName));
                stringBuilder.AppendLine("GO");
                FileUtility.WriteToFile(request.Folder.FolderPath,
                    string.Format("{0}.sql", viewName),
                    stringBuilder.ToString());
            }
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
            stringBuilder.AppendLine(string.Format("{0} = new Object()", request.ClassName));
            AppendJavaScriptOptionSets(stringBuilder, request, controller);
            FileUtility.WriteToFile(request.Folder.FolderPath,
                request.FileName.EndsWith(".js") ? request.FileName : request.FileName + ".js",
                stringBuilder.ToString());
        }

        private void AppendJavaScriptOptionSets(StringBuilder stringBuilder, CodeGeneratorRequest request,
            LogController controller)
        {
            var types = GetRecordTypesToImport(request);
            var countToDo = types.Count();
            var countDone = 0;
            foreach (var recordType in GetRecordTypesToImport(request))
            {
                controller.UpdateProgress(countDone, countToDo, string.Format("Processing Options ({0})", Service.GetDisplayName(recordType)));
                if (IsValidForCode(recordType))
                {
                    var recordTypeCodeLabel = CreateCodeLabel(Service.GetDisplayName(recordType));
                    stringBuilder.AppendLine(string.Format("{0}.{1} = new Object();", request.ClassName,
                        recordTypeCodeLabel));
                    foreach (var field in Service.GetFields(recordType))
                    {
                        if (IsValidForCode(field, recordType))
                        {
                            var fieldLabel = CreateCodeLabel(Service.GetFieldLabel(field, recordType));
                            stringBuilder.AppendLine(string.Format("{0}.{1}.{2} = new Object();", request.ClassName,
                                recordTypeCodeLabel,
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
                                            request.ClassName, recordTypeCodeLabel, fieldLabel, label, option.Key));
                                        used.Add(label);
                                    }
                                }
                            }
                        }
                    }
                }
                countDone++;
            }
        }


        private void WriteCSharpOptionSets(CodeGeneratorRequest request, LogController controller)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("namespace " + request.Namespace);
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(string.Format("\tpublic static class {0}", request.ClassName));
            stringBuilder.AppendLine("\t{");
            AppendCSharpOptionSets(stringBuilder, request, controller);
            stringBuilder.AppendLine("\t}");
            stringBuilder.AppendLine("}");
            FileUtility.WriteToFile(request.Folder.FolderPath,
                request.FileName.EndsWith(".cs") ? request.FileName : request.FileName + ".cs",
                stringBuilder.ToString());
        }

        private void AppendCSharpOptionSets(StringBuilder stringBuilder, CodeGeneratorRequest request,
            LogController controller)
        {
            var types = GetRecordTypesToImport(request);
            var countToDo = types.Count();
            var countDone = 0;
            foreach (var recordType in types)
            {
                controller.UpdateProgress(countDone, countToDo, string.Format("Processing Options ({0})", Service.GetDisplayName(recordType)));
                if (IsValidForCode(recordType))
                {
                    var recordTypeCodeLabel = CreateCodeLabel(Service.GetDisplayName(recordType));
                    stringBuilder.AppendLine("\t\tpublic static class " + recordTypeCodeLabel);
                    stringBuilder.AppendLine("\t\t{");
                    foreach (var field in Service.GetFields(recordType))
                    {
                        if (IsValidForCode(field, recordType))
                        {
                            stringBuilder.AppendLine("\t\t\tpublic static class " +
                                                     CreateCodeLabel(Service.GetFieldLabel(field, recordType)));
                            stringBuilder.AppendLine("\t\t\t{");
                            var options = Service.GetPicklistKeyValues(field, recordType);
                            var used = new List<string>();
                            foreach (var option in options)
                            {
                                if (IsValidForCode(option))
                                {
                                    var label = CreateCodeLabel(option.Value);
                                    if (!used.Contains(label))
                                    {
                                        stringBuilder.AppendLine(
                                            string.Format("\t\t\t\tpublic const int {0} = {1};", label, option.Key));
                                        used.Add(label);
                                    }
                                }
                            }
                            stringBuilder.AppendLine("\t\t\t}");
                        }
                    }
                    stringBuilder.AppendLine("\t\t}");
                }
                countDone++;
            }
        }

        private static IEnumerable<string> KeyWords
        {
            get { return new[] {"abstract", "event", "namespace"}; }
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

        private static IEnumerable<string> NumberWords
        {
            get
            {
                return new[]
                {
                    "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten"
                };
            }
        }

        public static string DigitToString(char character)
        {
            return NumberWords.ElementAt(Convert.ToInt32(char.GetNumericValue(character)));
        }

        private bool IsValidForCode(string recordType)
        {
            return
                !CreateCodeLabel(Service.GetDisplayName(recordType)).IsNullOrWhiteSpace()
                && Service.GetFields(recordType).Any(f => IsValidForCode(f, recordType));
        }

        private bool IsValidForCode(string field, string recordType)
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