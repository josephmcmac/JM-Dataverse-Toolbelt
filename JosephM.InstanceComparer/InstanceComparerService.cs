using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerService :
        ServiceBase<InstanceComparerRequest, InstanceComparerResponse, InstanceComparerResponseItem>
    {
        public override void ExecuteExtention(InstanceComparerRequest request, InstanceComparerResponse response,
            LogController controller)
        {

            var processContainer = new InstanceComparerService.ProcessContainer(request, response, controller);

            //ENSURE TO INCREASE THIS IF ADDING TO PROCESSES
            processContainer.NumberOfProcesses = 16;
            processContainer.NumberOfProcesses += request.DataComparisons.Count();

            AppendSolutions(processContainer);
            AppendWorkflows(processContainer);
            AppendResources(processContainer);
            AppendEntities(processContainer);
            AppendPlugins(processContainer);
            AppendOptions(processContainer);
            AppendSecurityRoles(processContainer);

            AppendData(processContainer);

            if (processContainer.Differences.Any())
            {
                processContainer.Controller.LogLiteral("Generating CSV");
                var fileName = "InstanceCompare_" + DateTime.Now.ToFileTime() + ".csv";
                CsvUtility.CreateCsv(request.Folder.FolderPath, fileName, processContainer.Differences);
                response.FileName = Path.Combine(request.Folder.FolderPath, fileName);
                response.Differences = true;
            }
        }

        private void AppendData(InstanceComparerService.ProcessContainer processContainer)
        {
            var compares = processContainer
                .Request.DataComparisons
                .Select(c => new ProcessCompareParams(c, processContainer.ServiceOne))
                .ToArray();
            foreach (var compare in compares)
            {
                ProcessCompare(compare, processContainer);
            }
        }

        private void AppendSecurityRoles(ProcessContainer processContainer)
        {
            var processCompareParams = new ProcessCompareParams("Security Role",
                Entities.role, Fields.role_.name, Fields.role_.name,
                new[] { new Condition(Fields.role_.parentroleid, ConditionType.Null) },
                null);

            //this is a speical many to many type containing
            //the privilegedepthmask in intersect table
            //did a bit of manipulation to get privilegedepthmask + privelegename
            var privilegeCompareParams = new ProcessCompareParams("Security Role Privilege",
                Relationships.role_.roleprivileges_association.EntityName, Fields.privilege_.privilegeid, Fields.privilege_.privilegeid, null,
                new[] { "privilegedepthmask" },
                Fields.role_.roleid, ParentLinkType.Lookup);

            privilegeCompareParams.AddConversionObject(Fields.privilege_.privilegeid,
                new ProcessCompareParams.ConvertRolePrivilegeName(processContainer.ServiceOne),
                new ProcessCompareParams.ConvertRolePrivilegeName(processContainer.ServiceTwo));

            processCompareParams.ChildCompares = new[] { privilegeCompareParams };

            ProcessCompare(processCompareParams, processContainer);
        }

        private void AppendOptions(ProcessContainer processContainer)
        {
            var processCompareParams = new ProcessCompareParams("Shared Picklist",
                s => s.GetSharedPicklists().ToArray(),
                "SchemaName",
                GetReadableProperties(typeof(IPicklistSet), new[]
                    {
                        "PicklistOptions"
                    }));

            var optionCompareParams = new ProcessCompareParams("Shared Picklist Option",
                (s, r) => r.GetSharedPicklistOptions(s).ToArray(),
                "Key",
                GetReadableProperties(typeof(PicklistOption), null));

            processCompareParams.ChildCompares = new[] { optionCompareParams };

            ProcessCompare(processCompareParams, processContainer);
        }

        private void AppendPlugins(ProcessContainer processContainer)
        {
            var processCompareParams = new ProcessCompareParams("Plugin Assembly",
                Entities.pluginassembly, Fields.pluginassembly_.pluginassemblyid, Fields.pluginassembly_.name,
                new[] { new Condition(Fields.pluginassembly_.ishidden, ConditionType.NotEqual, true) },
                new[] { Fields.pluginassembly_.content, Fields.pluginassembly_.isolationmode, Fields.pluginassembly_.description });

            var pluginTypeCompareParams = new ProcessCompareParams("Plugin Type",
                Entities.plugintype, Fields.plugintype_.typename, Fields.plugintype_.typename, null, new[]
                {
                    Fields.plugintype_.name, Fields.plugintype_.assemblyname, Fields.plugintype_.description, Fields.plugintype_.workflowactivitygroupname, Fields.plugintype_.friendlyname,
                },
                Fields.plugintype_.pluginassemblyid, ParentLinkType.Lookup);

            processCompareParams.ChildCompares = new[] { pluginTypeCompareParams };

            var pluginRegistrationParams = new ProcessCompareParams("Plugin Registration",
                Entities.sdkmessageprocessingstep,
                new[]
                {
                    Fields.sdkmessageprocessingstep_.mode, Fields.sdkmessageprocessingstep_.stage,
                    Fields.sdkmessageprocessingstep_.sdkmessageid, Fields.sdkmessageprocessingstep_.sdkmessagefilterid
                    , Fields.sdkmessageprocessingstep_.name
                },
                Fields.sdkmessageprocessingstep_.name,
                null,
                new[]
                {
                    Fields.sdkmessageprocessingstep_.description, Fields.sdkmessageprocessingstep_.filteringattributes, Fields.sdkmessageprocessingstep_.rank, Fields.sdkmessageprocessingstep_.statecode
                },
                Fields.sdkmessageprocessingstep_.plugintypeid, ParentLinkType.Lookup);
            pluginRegistrationParams.AddConversionObject(Fields.sdkmessageprocessingstep_.sdkmessagefilterid,
                new ProcessCompareParams.ConvertSdkMessageFilter(processContainer.ServiceOne),
                new ProcessCompareParams.ConvertSdkMessageFilter(processContainer.ServiceTwo));

            pluginTypeCompareParams.ChildCompares = new[] { pluginRegistrationParams };

            ProcessCompare(processCompareParams, processContainer);
        }

        private void AppendEntities(ProcessContainer processContainer)
        {
            var processCompareParams = new ProcessCompareParams("Entity",
                s => s.GetAllRecordTypes().Select(s.GetRecordTypeMetadata).ToArray(),
                "SchemaName",
                GetReadableProperties(typeof(IRecordTypeMetadata), new[]
                    {
                        "MetadataId", "RecordTypeCode", "Activities", "Notes"
                    }));

            var fieldsCompareParams = new ProcessCompareParams("Field",
                (s, r) => r.GetFieldMetadata(s).ToArray(),
                "SchemaName",
                GetReadableProperties(typeof(IFieldMetadata), new[]
                    {
                        "MetadataId"
                    }));


            var fieldsOptionParams = new ProcessCompareParams("Field Options",
                (field, recordType, service) => service.GetPicklistKeyValues(field, recordType).ToArray(),
                "Key",
                GetReadableProperties(typeof(PicklistOption), null));

            fieldsCompareParams.ChildCompares = new[] { fieldsOptionParams };

            var manyToManyCompareParams = new ProcessCompareParams("Many To Many Relationship",
                (s, r) => r.GetManyToManyRelationships(s).ToArray(),
                "SchemaName",
                GetReadableProperties(typeof(IMany2ManyRelationshipMetadata), new[]
                    {
                        "MetadataId"
                    }));

            var formCompareParams = new ProcessCompareParams("Form",
                Entities.systemform, new[] { Fields.systemform_.formid, Fields.systemform_.type }, Fields.systemform_.name,
                new[] { new Condition(Fields.systemform_.formid, ConditionType.NotNull) },
                new[] { Fields.systemform_.formpresentation, Fields.systemform_.formxml, Fields.systemform_.formactivationstate, Fields.systemform_.name, Fields.systemform_.description, Fields.systemform_.isdefault },
                 Fields.systemform_.objecttypecode, ParentLinkType.Lookup);
            formCompareParams.AddConversionObject(Fields.systemform_.formxml, new ProcessCompareParams.RemoveMiscFormXml(), new ProcessCompareParams.RemoveMiscFormXml());

            var viewCompareParams = new ProcessCompareParams("View",
                Entities.savedquery, Fields.savedquery_.savedqueryid, Fields.savedquery_.name,
                new[] { new Condition(Fields.savedquery_.savedqueryid, ConditionType.NotNull) },
                new[] { Fields.savedquery_.fetchxml, Fields.savedquery_.layoutxml, Fields.savedquery_.name, Fields.savedquery_.description, Fields.savedquery_.statecode },
                Fields.savedquery_.returnedtypecode, ParentLinkType.Lookup);
            viewCompareParams.AddConversionObject(Fields.savedquery_.layoutxml, new ProcessCompareParams.RemoveMiscViewXml(), new ProcessCompareParams.RemoveMiscViewXml());

            processCompareParams.ChildCompares = new[]
            {
                fieldsCompareParams,
                manyToManyCompareParams,
                formCompareParams,
                viewCompareParams
            };

            ProcessCompare(processCompareParams, processContainer);
        }


        private IEnumerable<string> GetReadableProperties(Type type, IEnumerable<string> exlcude)
        {
            if (exlcude == null)
                exlcude = new string[0];
            return type
                .GetReadableProperties()
                .Select(pi => pi.Name)
                .Except(exlcude)
                .ToArray();
        }

        private void AppendResources(ProcessContainer processContainer)
        {
            var processArgs = new ProcessCompareParams("Web Resource",
                Entities.webresource,
                Fields.webresource_.name,
                Fields.webresource_.name,
                new[] { new Condition(Fields.webresource_.ishidden, ConditionType.NotEqual, true) },
                new[] { Fields.webresource_.content, Fields.webresource_.description, Fields.webresource_.displayname, Fields.webresource_.webresourcetype, Fields.webresource_.languagecode });

            ProcessCompare(processArgs, processContainer);
        }

        private void AppendSolutions(ProcessContainer processContainer)
        {
            var processArgs = new ProcessCompareParams("Solution",
                Entities.solution,
                Fields.solution_.uniquename,
                Fields.solution_.friendlyname,
                new[] { new Condition(Fields.solution_.isvisible, ConditionType.Equal, true) },
                new[] { Fields.solution_.version, Fields.solution_.friendlyname, Fields.solution_.configurationpageid, Fields.solution_.description });

            ProcessCompare(processArgs, processContainer);
        }

        private void AppendWorkflows(ProcessContainer processContainer)
        {
            var processArgs = new ProcessCompareParams("Workflow",
                Entities.workflow,
                Fields.workflow_.workflowid,
                Fields.workflow_.name,
                new[] { new Condition(Fields.workflow_.type, ConditionType.Equal, OptionSets.Process.Type.Definition), new Condition(Fields.workflow_.rendererobjecttypecode, ConditionType.Null) },
                new[] { Fields.workflow_.name, Fields.workflow_.statecode, Fields.workflow_.xaml, Fields.workflow_.description, Fields.workflow_.ondemand, Fields.workflow_.rank, Fields.workflow_.triggeronupdateattributelist, Fields.workflow_.triggeroncreate, Fields.workflow_.triggerondelete, Fields.workflow_.createstage, Fields.workflow_.updatestage, Fields.workflow_.deletestage, Fields.workflow_.iscrmuiworkflow, Fields.workflow_.istransacted, Fields.workflow_.mode, Fields.workflow_.runas, Fields.workflow_.subprocess, Fields.workflow_.scope, Fields.workflow_.primaryentity, Fields.workflow_.sdkmessageid });
            processArgs.AddConversionObject(Fields.workflow_.sdkmessageid,
                new ProcessCompareParams.ConvertWorkflowMessage(processContainer.ServiceOne),
                new ProcessCompareParams.ConvertWorkflowMessage(processContainer.ServiceTwo));
            processArgs.AddConversionObject(Fields.workflow_.xaml,
                new ProcessCompareParams.RemoveMiscWorkflowXml(),
                new ProcessCompareParams.RemoveMiscWorkflowXml());
            ProcessCompare(processArgs, processContainer);
        }

        private void ProcessCompare(ProcessCompareParams processCompareParams, ProcessContainer processContainer)
        {
            ProcessCompare(processCompareParams, null, null, processContainer);
        }


        private void ProcessCompare(ProcessCompareParams processCompareParams, ProcessCompareParams parentCompareParams, List<List<IRecord>> parents,
            ProcessContainer processContainer)
        {
            try
            {
                var isChildContext = parentCompareParams != null;

                processContainer.Controller.UpdateProgress(processContainer.NumberOfProcessesCompleted,
                    processContainer.NumberOfProcesses, string.Format("Comparing {0} Components", processCompareParams.Context));

                var inBoth = new List<List<IRecord>>();

                //in one not in other
                if (isChildContext)
                {
                    List<Dictionary<IRecord, List<IRecord>>> groupThem = parents
                        .Select(l => l.ToDictionary(i => i, i => new List<IRecord>()))
                        .ToList();
                    if (processCompareParams.Type == ProcessCompareType.Records)
                    {
                        //add some comments for this mess
                        if (processCompareParams.ParentLinkType == ParentLinkType.Lookup)
                        {
                            processContainer.Controller.UpdateLevel2Progress(1, 4, "Loading Connection 1 Items");
                            var serviceOneItems =
                                processContainer.ServiceOne.RetrieveAllOrClauses(processCompareParams.RecordType,
                                    parents.Select(v => v.First())
                                        .Select(
                                            r =>
                                                new Condition(processCompareParams.ParentLink, ConditionType.Equal,
                                                    GetParentId(parentCompareParams, r))),
                                    null);
                            processContainer.Controller.UpdateLevel2Progress(2, 4, "Loading Connection 2 Items");
                            var serviceTwoItems =
                                processContainer.ServiceTwo.RetrieveAllOrClauses(processCompareParams.RecordType,
                                    parents.Select(v => v.Last())
                                        .Select(
                                            r =>
                                                new Condition(processCompareParams.ParentLink, ConditionType.Equal,
                                                   GetParentId(parentCompareParams, r))),
                                    null);
                            processContainer.Controller.UpdateLevel2Progress(3, 4, "Comparing Items");
                            foreach (var item in serviceOneItems)
                            {
                                var referringField = item.GetField(processCompareParams.ParentLink);
                                if (referringField is Lookup)
                                    referringField = ((Lookup)referringField).Id;
                                foreach (var parent in groupThem)
                                {
                                    if (FieldsEqual(GetParentId(parentCompareParams, parent.Keys.First()),
                                        referringField))
                                        parent.Values.First().Add(item);
                                }
                            }
                            foreach (var item in serviceTwoItems)
                            {
                                var referringField = item.GetField(processCompareParams.ParentLink);
                                if (referringField is Lookup)
                                    referringField = ((Lookup)referringField).Id;
                                foreach (var parent in groupThem)
                                {
                                    if (FieldsEqual(GetParentId(parentCompareParams, parent.Keys.Last()),
                                        referringField))
                                        parent.Values.Last().Add(item);
                                }
                            }
                        }
                        #region not implemented many to many compare code
                        //else if (processCompareParams.ParentLinkType == ParentLinkType.ManyToMany)
                        //{
                        //    these could filter for only related to a parent
                        //    var serviceOneItems =
                        //        processContainer.ServiceOne.RetrieveAll(processCompareParams.RecordType, null);
                        //    var serviceTwoItems =
                        //        processContainer.ServiceTwo.RetrieveAll(processCompareParams.RecordType, null);
                        //    var relationship =
                        //        processContainer.ServiceOne.GetManyRelationshipMetadata(processCompareParams.ParentLink,
                        //            processCompareParams.RecordType);
                        //    //okay need to get all the intersects linked to the parents
                        //    //then index them 

                        //    var serviceOneIntersects =
                        //        processContainer.ServiceOne.RetrieveAll(relationship.IntersectEntityName, null);
                        //    var serviceTwoIntersects =
                        //        processContainer.ServiceTwo.RetrieveAll(relationship.IntersectEntityName, null);

                        //    var childIntersectField = relationship.RecordType1 == processCompareParams.RecordType
                        //        ? relationship.Entity1IntersectAttribute
                        //        : relationship.Entity2IntersectAttribute;
                        //    var parentIntersectField = relationship.Entity1IntersectAttribute == childIntersectField
                        //        ? relationship.Entity2IntersectAttribute
                        //        : relationship.Entity1IntersectAttribute;

                        //    foreach (var item in serviceOneIntersects)
                        //    {
                        //        var parentId = item.GetStringField(parentIntersectField);
                        //        var childId = item.GetStringField(childIntersectField);
                        //        foreach (var parent in groupThem)
                        //        {
                        //            if (FieldsEqual(parent.Keys.First().Id, parentId))
                        //            {
                        //                var matchingChild =
                        //                    serviceTwoItems.Where(i => i.Id == childId);
                        //                parent.Values.First().AddRange(matchingChild);
                        //            }
                        //        }
                        //    }
                        //    foreach (var item in serviceTwoIntersects)
                        //    {
                        //        var parentId = item.GetStringField(parentIntersectField);
                        //        var childId = item.GetStringField(childIntersectField);
                        //        foreach (var parent in groupThem)
                        //        {
                        //            if (FieldsEqual(parent.Keys.Last().Id, parentId))
                        //            {
                        //                var matchingChild =
                        //                    serviceOneItems.Where(i => i.Id == childId);
                        //                parent.Values.Last().AddRange(matchingChild);
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion

                        //group by each parent and process
                        foreach (var group in groupThem)
                        {
                            var parentReference = group.First().Key.GetStringField(parentCompareParams.MatchField);
                            inBoth.AddRange(DoCompare(processCompareParams, processContainer, group.Values.First(),
                                group.Values.Last(), parentReference));
                        }
                    }
                    if (processCompareParams.Type == ProcessCompareType.Objects)
                    {
                        //differences
                        var countToDo = parents.Count();
                        var countDone = 0;
                        foreach (var group in groupThem)
                        {
                            var parentReference = group.First().Key.GetStringField(parentCompareParams.MatchField);
                            processContainer.Controller.UpdateLevel2Progress(countDone++, countToDo, "Processing " + parentReference);
                            var items1 =
                                processCompareParams.GetObjects(
                                    group.First().Key.GetStringField(parentCompareParams.MatchField),
                                    parentReference,
                                    processContainer.ServiceOne).Select(r => new ObjectRecord(r)).ToArray();
                            var items2 =
                                processCompareParams.GetObjects(
                                    group.Last().Key.GetStringField(parentCompareParams.MatchField),
                                    parentReference,
                                    processContainer.ServiceTwo).Select(r => new ObjectRecord(r)).ToArray();
                            DoCompare(processCompareParams, processContainer, items1, items2, parentReference);
                        }
                    }
                }
                else
                {
                    if (processCompareParams.Type == ProcessCompareType.Records)
                    {
                        processContainer.Controller.UpdateLevel2Progress(1, 4, "Loading Connection 1 Items");
                        var serviceOneItems = processContainer.ServiceOne.RetrieveAllAndClauses(
                            processCompareParams.RecordType, processCompareParams.Conditions, null);
                        processContainer.Controller.UpdateLevel2Progress(2, 4, "Loading Connection 2 Items");
                        var serviceTwoItems = processContainer.ServiceTwo.RetrieveAllAndClauses(
                            processCompareParams.RecordType, processCompareParams.Conditions, null);
                        processContainer.Controller.UpdateLevel2Progress(3, 4, "Comparing Items");
                        inBoth.AddRange(DoCompare(processCompareParams, processContainer, serviceOneItems, serviceTwoItems, null));

                    }
                    if (processCompareParams.Type == ProcessCompareType.Objects)
                    {
                        processContainer.Controller.UpdateLevel2Progress(1, 4, "Loading Connection 1 Items");
                        var serviceOneItems =
                            processCompareParams.GetObjects(null, null, processContainer.ServiceOne)
                                .Select(o => new ObjectRecord(o))
                                .ToArray();
                        processContainer.Controller.UpdateLevel2Progress(2, 4, "Loading Connection 2 Items");
                        var serviceTwoItems =
                            processCompareParams.GetObjects(null, null, processContainer.ServiceTwo)
                                .Select(o => new ObjectRecord(o))
                                .ToArray();
                        processContainer.Controller.UpdateLevel2Progress(3, 4, "Comparing Items");
                        inBoth.AddRange(DoCompare(processCompareParams, processContainer, serviceOneItems, serviceTwoItems, null));
                    }
                }
                processContainer.Controller.TurnOffLevel2();
                processContainer.NumberOfProcessesCompleted++;
                if (processCompareParams.ChildCompares != null)
                {
                    foreach (var item in processCompareParams.ChildCompares)
                    {
                        ProcessCompare(item, processCompareParams, inBoth, processContainer);
                    }
                }
            }
            catch (Exception ex)
            {
                processContainer.Response.AddResponseItem(new InstanceComparerResponseItem("Fatal Error Comparing", processCompareParams.Context, ex));
            }
        }

        private static object GetParentId(ProcessCompareParams parentCompareParams, IRecord parentRecord)
        {
            return parentRecord.Id ?? parentRecord.GetField(parentCompareParams.MatchField);
        }

        private List<List<IRecord>> DoCompare(ProcessCompareParams processCompareParams,
            ProcessContainer processContainer,
            IEnumerable<IRecord> serviceOneItems, IEnumerable<IRecord> serviceTwoItems, string parentReference)
        {
            var thisInBoth = new List<List<IRecord>>();
            var service2AlreadyAdded = new List<IRecord>();
            foreach (var item in serviceOneItems)
            {
                var matches = serviceTwoItems
                    .Where(
                    w =>
                        processCompareParams.MatchFields.All(f =>
                            FieldsEqual(
                                processCompareParams.ConvertField1(f, item.GetField(f)),
                                processCompareParams.ConvertField2(f, w.GetField(f)))))
                    .Where(r => !service2AlreadyAdded.Contains(r))
                    .ToArray();
                //if (matches.Count() > 1)
                //{
                //    var duplicates = true;
                //}
                //if (!matches.Any())
                //{
                //    var none = true;
                //}
                if (matches.Any())
                {
                    var match = matches.First();
                    service2AlreadyAdded.Add(match);
                    thisInBoth.Add(new List<IRecord>() { item, match });
                }
            }

            var inOneNotInTwo = serviceOneItems
                .Where(w => !thisInBoth.Select(kv => kv.First()).Contains(w))
                .ToArray();
            foreach (var item in inOneNotInTwo)
            {
                var displayName = processCompareParams.ConvertField1(processCompareParams.DisplayField, item.GetStringField(processCompareParams.DisplayField));
                processContainer.AddDifference(processCompareParams.Context,
                    displayName, "In One Not In Two", parentReference, displayName, null, item.Id, null);
            }
            var inTwoNotInOne = serviceTwoItems
                .Where(w => !thisInBoth.Select(kv => kv.Last()).Contains(w))
                .ToArray();
            foreach (var item in inTwoNotInOne)
            {
                var displayName = processCompareParams.ConvertField2(processCompareParams.DisplayField, item.GetStringField(processCompareParams.DisplayField));
                processContainer.AddDifference(processCompareParams.Context,
                    displayName, "In Two Not In One", parentReference, null, displayName, null, item.Id);
            }

            //differences
            foreach (var item in thisInBoth)
            {
                foreach (var field in processCompareParams.FieldsCheckDifference)
                {
                    var field1 = processCompareParams.ConvertField1(field, item.First().GetField(field));
                    var field2 = processCompareParams.ConvertField2(field, item.Last().GetField(field));
                    if (!FieldsEqual(field1, field2))
                    {
                        var fieldLabel = processCompareParams.Type == ProcessCompareType.Objects
                                ? field
                                : processContainer.ServiceOne.GetFieldLabel(field, processCompareParams.RecordType);
                        if (string.IsNullOrWhiteSpace(fieldLabel))
                            fieldLabel = field;
                        var displayValue1 = field1 == null ? string.Empty : field1.ToString();
                        var displayValue2 = field2 == null ? string.Empty : field2.ToString();
                        if (processCompareParams.Type == ProcessCompareType.Records)
                        {
                            displayValue1 = processContainer.ServiceOne.GetFieldAsDisplayString(item.First(), field);
                            displayValue2 = processContainer.ServiceTwo.GetFieldAsDisplayString(item.Last(), field);
                            //okay for difference if it is a string we only really want to display s part of string which is different
                            if (field1 is string || field2 is string)
                            {
                                var charsToDisplay = 40;
                                if (field1 == null)
                                    displayValue2 = displayValue2.Left(charsToDisplay) + (displayValue2.Length > 40 ? "..." : "");
                                else if (field2 == null)
                                    displayValue1 = displayValue1.Left(charsToDisplay) + (displayValue1.Length > 40 ? "..." : "");
                                else
                                {
                                    //https://stackoverflow.com/questions/4585939/comparing-strings-and-get-the-first-place-where-they-vary-from-eachother
                                    var indexOfDiff = displayValue1.Zip(displayValue2, (c1, c2) => c1 == c2).TakeWhile(b => b).Count() + 1;
                                    var startIndex = indexOfDiff - 10;
                                    if (startIndex < 0)
                                        startIndex = 0;

                                    displayValue1 = displayValue1.Substring(startIndex).Left(charsToDisplay);
                                    displayValue2 = displayValue2.Substring(startIndex).Left(charsToDisplay);
                                }
                            }
                        }
                        processContainer.AddDifference(processCompareParams.Context,
                            processCompareParams.ConvertField1(processCompareParams.DisplayField, item.First().GetStringField(processCompareParams.DisplayField)),
                            "Field Different - " + fieldLabel, parentReference, displayValue1, displayValue2, item.First().Id, item.Last().Id);
                    }
                }
            }
            return thisInBoth;
        }

        private static bool FieldsEqual(object field1, object field2)
        {
            if ((field1 == null && field2 != null)
                || (field1 != null && field2 == null)
                || (field1 != null && !field1.Equals(field2)))
                return false;
            return true;
        }

        public class ProcessCompareParams
        {
            public string Context { get; set; }
            public Func<string, string, IRecordService, IEnumerable<object>> GetObjects { get; set; }
            public IEnumerable<Condition> Conditions { get; set; }
            public IEnumerable<string> MatchFields { get; set; }

            public string MatchField
            {
                get
                {
                    if (MatchFields.Count() != 1)
                        throw new Exception("Error More Than One Match Field");
                    return MatchFields.First();
                }
            }
            public string DisplayField { get; set; }
            public IEnumerable<string> FieldsCheckDifference { get; set; }
            public string RecordType { get; set; }
            public string ParentLink { get; set; }

            public IEnumerable<ProcessCompareParams> ChildCompares { get; set; }

            public void AddConversionObject(string fieldName, ConvertField conversionObject1,
                ConvertField conversionObject2)
            {
                _conversionsObjects.Add(fieldName, new List<ConvertField>() { conversionObject1, conversionObject2 });
            }

            public Dictionary<string, List<ConvertField>> _conversionsObjects =
                new Dictionary<string, List<ConvertField>>();
            private InstanceComparerRequest.InstanceCompareDataCompare c;

            public object ConvertField1(string field, object value)
            {
                if (!_conversionsObjects.ContainsKey(field))
                    return value;
                return _conversionsObjects[field].First().Convert(value);
            }

            public object ConvertField2(string field, object value)
            {
                if (!_conversionsObjects.ContainsKey(field))
                    return value;
                return _conversionsObjects[field].Last().Convert(value);
            }

            public ProcessCompareParams(string context, Func<IRecordService, IEnumerable<object>> getObjects, string keyProperty, IEnumerable<string> fieldsCheckDifference)
                : this(context, (s, r) => getObjects(r), keyProperty, fieldsCheckDifference)
            {
            }

            public ProcessCompareParams(string context, Func<string, IRecordService, IEnumerable<object>> getObjects, string keyProperty, IEnumerable<string> fieldsCheckDifference)
                : this(context, (s1, s2, s3) => getObjects(s1, s3), keyProperty, fieldsCheckDifference)
            {
            }

            public ProcessCompareParams(string context, Func<string, string, IRecordService, IEnumerable<object>> getObjects, string keyProperty, IEnumerable<string> fieldsCheckDifference)
            {
                Context = context;
                GetObjects = getObjects;
                MatchFields = new[] { keyProperty };
                DisplayField = keyProperty;
                Type = ProcessCompareType.Objects;
                Conditions = new Condition[0];
                FieldsCheckDifference = fieldsCheckDifference ?? new string[0]; ;
                ChildCompares = new ProcessCompareParams[0];
            }

            public ProcessCompareType Type { get; set; }

            public ProcessCompareParams(string context, string recordType, string matchField, string displayField,
                IEnumerable<Condition> conditions, IEnumerable<string> fieldsCheckDifference)
                : this(context, recordType, matchField, displayField, conditions, fieldsCheckDifference, null, null)
            {
            }

            public ProcessCompareParams(string context, string recordType, string matchField, string displayField,
                IEnumerable<Condition> conditions, IEnumerable<string> fieldsCheckDifference,
                string parentlink, ParentLinkType? parentLinkType)
                : this(
                    context, recordType, new[] { matchField }, displayField, conditions, fieldsCheckDifference,
                    parentlink, parentLinkType)
            {
            }

            public ProcessCompareParams(string context, string recordType, IEnumerable<string> matchFields,
                string displayField, IEnumerable<Condition> conditions, IEnumerable<string> fieldsCheckDifference,
                string parentlink, ParentLinkType? parentLinkType)
            {
                Context = context;
                Conditions = conditions ?? new Condition[0];
                MatchFields = matchFields;
                DisplayField = displayField;
                FieldsCheckDifference = fieldsCheckDifference ?? new string[0];
                RecordType = recordType;
                ParentLink = parentlink;
                ParentLinkType = parentLinkType;
                Type = ProcessCompareType.Records;
            }

            public ProcessCompareParams(InstanceComparerRequest.InstanceCompareDataCompare dataComparison, IRecordService recordService)
                : this("Data - " + dataComparison.Type,
                      dataComparison.Type,
                      recordService.GetPrimaryField(dataComparison.Type),
                      recordService.GetPrimaryField(dataComparison.Type),
                      new Condition[0],
                      recordService
                            .GetFields(dataComparison.Type)
                            .Where(f => recordService.GetFieldMetadata(f, dataComparison.Type).IsCustomField)
                            .ToArray()
                      )
            {
            }

            public ParentLinkType? ParentLinkType { get; set; }

            public abstract class ConvertField
            {
                public abstract object Convert(object sourceValue);

                public static string RemoveStrings(string[] removeStrings, string theString)
                {
                    foreach (var removeString in removeStrings)
                    {
                        theString = theString.Replace(removeString, "");
                    }
                    return theString;
                }

                public static string StripCharactersAfter(string theString, string matchString, int stripCharactersAfter)
                {
                    var minIndex = 0;
                    while (true)
                    {
                        var index = theString.IndexOf(matchString, minIndex, StringComparison.OrdinalIgnoreCase);
                        if (index == -1)
                            break;
                        minIndex = index + 1;
                        index = index + matchString.Length;
                        var end = index + stripCharactersAfter;
                        if (theString.Length < end)
                            break;
                        theString = theString.Substring(0, index) + theString.Substring(end);
                    }
                    return theString;
                }

                public static string StripStartToEnd(string theString, string matchString, string endMatch)
                {
                    var minIndex = 0;
                    while (true)
                    {
                        var index = theString.IndexOf(matchString, minIndex, StringComparison.OrdinalIgnoreCase);
                        if (index == -1)
                            break;
                        minIndex = index + 1;
                        var end = theString.IndexOf(endMatch, index + matchString.Length, StringComparison.OrdinalIgnoreCase);
                        if (end == -1)
                            break;
                        end = end + endMatch.Length;
                        theString = theString.Substring(0, index) + theString.Substring(end);
                    }
                    return theString;
                }
            }

            public class RemoveMiscWorkflowXml : ConvertField
            {
                public override object Convert(object sourceValue)
                {
                    if (sourceValue == null)
                        return null;
                    var theString = (string)sourceValue;
                    theString = theString.Replace("\"True\"", "\"true\"");
                    //remove xml encoding + whitespace which were different in a camparison when actually equivalent
                    var removeStrings = new[] { "<?xml version=\"1.0\" encoding=\"utf-16\"?>", " ", "\n", "\r", "\t", "<Persist/>" };
                    theString = RemoveStrings(removeStrings, theString);
                    //strips out id in e.g. XrmWorkflowf736af5a4cba497084b54eb7d2cc0354
                    theString = StripCharactersAfter(theString, "XrmWorkflow", 32);
                    //had different declarations for some reason though equivalent
                    theString = StripStartToEnd(theString, "<Activityx:Class=", "xaml\">");
                    //strips out id
                    theString = StripStartToEnd(theString, "PropertyType.Guid,", "UniqueIdentifier");
                    //strips out id
                    theString = StripStartToEnd(theString, "Default=", "Name=\"stepLabelLabelId");
                    //strips target parts missing in some for unknown reason
                    theString = StripStartToEnd(theString, "<x:PropertyName=\"Target\"",
                        "</x:Property.Attributes></x:Property>");
                    var additionalRemoveStrings = new[]
                    {
                        RemoveStrings(removeStrings,
                        "<this:XrmWorkflow.Target><InArgument x:TypeArguments=\"mxs:EntityReference\" /></this:XrmWorkflow.Target>")
                    };
                    theString = RemoveStrings(additionalRemoveStrings, theString);
                    return theString;
                }
            }
            //object="10010"
            public class RemoveMiscViewXml : ConvertField
            {
                public override object Convert(object sourceValue)
                {
                    if (sourceValue == null)
                        return null;
                    var theString = (string)sourceValue;
                    //strips out id in e.g. object="10010"
                    theString = StripStartToEnd(theString, "object=\"", "\"");
                    return theString;
                }
            }
            public class RemoveMiscFormXml : ConvertField
            {
                public override object Convert(object sourceValue)
                {
                    if (sourceValue == null)
                        return null;
                    var theString = (string)sourceValue;
                    var removeStrings = new[] { "{}", " ", "\n", "\r", "\t" };
                    theString = RemoveStrings(removeStrings, theString);
                    //strips out id in e.g. id="{0ae8f26b-f7b6-4ad8-933e-7c972b297624}"
                    theString = StripStartToEnd(theString, "id=\"{", "}");
                    return theString;
                }
            }

            public class ConvertRolePrivilegeName : ConvertField
            {
                public IRecordService Service { get; set; }

                public ConvertRolePrivilegeName(IRecordService service)
                {
                    Service = service;
                }

                public override object Convert(object sourceValue)
                {
                    if (sourceValue == null)
                        return null;
                    if (_privelegeIdToName == null)
                    {
                        var allMapped = Service.RetrieveAll(Entities.privilege,
                            new[] { Fields.privilege_.name });
                        _privelegeIdToName = new Dictionary<string, string>();
                        foreach (var item in allMapped)
                        {
                            var name = item.GetStringField(Fields.privilege_.name);
                            _privelegeIdToName.Add(item.Id, name);
                        }
                    }
                    var id = (string)sourceValue;

                    if (_privelegeIdToName.ContainsKey(id))
                        return _privelegeIdToName[id];
                    throw new Exception("Could Not Match Id Of " + id);
                }

                private IDictionary<string, string> _privelegeIdToName = null;
            }

            public class ConvertSdkMessageFilter : ConvertField
            {
                public IRecordService Service { get; set; }

                public ConvertSdkMessageFilter(IRecordService service)
                {
                    Service = service;
                }

                public override object Convert(object sourceValue)
                {
                    if (sourceValue == null)
                        return null;
                    if (_idToRecordTypeMap == null)
                    {
                        var allFilters = Service.RetrieveAll(Entities.sdkmessagefilter,
                            new[] { Fields.sdkmessagefilter_.primaryobjecttypecode });
                        _idToRecordTypeMap = new Dictionary<string, string>();
                        foreach (var item in allFilters)
                        {
                            var objectType = item.GetStringField(Fields.sdkmessagefilter_.primaryobjecttypecode);
                            _idToRecordTypeMap.Add(item.Id, objectType);
                        }
                    }
                    var lookup = (Lookup)sourceValue;

                    if (_idToRecordTypeMap.ContainsKey(lookup.Id))
                        return _idToRecordTypeMap[lookup.Id];
                    throw new Exception("Could Not Match Id Of " + lookup.Id);
                }

                private IDictionary<string, string> _idToRecordTypeMap = null;
            }

            public class ConvertWorkflowMessage : ConvertField
            {
                public IRecordService Service { get; set; }

                public ConvertWorkflowMessage(IRecordService service)
                {
                    Service = service;
                }

                public override object Convert(object sourceValue)
                {
                    if (sourceValue == null)
                        return null;
                    if (_mapDictionary == null)
                    {
                        var allMapped = Service.RetrieveAll(Entities.sdkmessage,
                            new[] { Fields.sdkmessage_.name });
                        _mapDictionary = new Dictionary<string, string>();
                        foreach (var item in allMapped)
                        {
                            var objectType = item.GetStringField(Fields.sdkmessage_.name);
                            _mapDictionary.Add(item.Id, objectType);
                        }
                    }
                    var lookup = (Lookup)sourceValue;

                    if (_mapDictionary.ContainsKey(lookup.Id))
                        return _mapDictionary[lookup.Id];
                    throw new Exception("Could Not Match Id Of " + lookup.Id);
                }

                private IDictionary<string, string> _mapDictionary = null;
            }
        }

        public class ProcessContainer
        {
            public int NumberOfProcesses { get; set; }

            public int NumberOfProcessesCompleted { get; set; }
            public LogController Controller { get; set; }
            public InstanceComparerRequest Request { get; set; }
            public InstanceComparerResponse Response { get; set; }

            public IRecordService ServiceOne { get; set; }

            public IRecordService ServiceTwo { get; set; }

            public List<InstanceComparerDifference> Differences { get; set; }

            //public IEnumerable<InstanceComparerRequest.InstanceCompareDataCompare> TypesToCompareData
            //{
            //    get
            //    {
            //        return Request. ?? new InstanceComparerRequest.InstanceCompareDataCompare[0];
            //    }
            //}

            public ProcessContainer(InstanceComparerRequest request, InstanceComparerResponse response,
                LogController controller)
            {
                Request = request;
                Response = response;
                Controller = controller;
                ServiceOne = new XrmRecordService(request.ConnectionOne);
                ServiceTwo = new XrmRecordService(request.ConnectionTwo);
                Differences = new List<InstanceComparerDifference>();
            }

            internal void AddDifference(string type, object name, string difference, string parentReference, object value1, object value2, string id1, string id2)
            {
                Differences.Add(new InstanceComparerDifference(type, name == null ? null : name.ToString(), difference, parentReference, value1 == null ? null : value1.ToString(), value2 == null ? null : value2.ToString(), id1, id2));
            }
        }
    }

    public enum ParentLinkType
    {
        Lookup
    }

    public enum ProcessCompareType
    {
        Records,
        Objects
    }
}
