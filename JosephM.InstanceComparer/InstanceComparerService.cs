using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerService :
        ServiceBase<InstanceComparerRequest, InstanceComparerResponse, InstanceComparerResponseItem>
    {
        public override void ExecuteExtention(InstanceComparerRequest request, InstanceComparerResponse response,
            LogController controller)
        {

            var processContainer = new ProcessContainer(request, response, controller);
            response.AllDifferences = processContainer.Differences;
            //ENSURE TO INCREASE THIS IF ADDING TO PROCESSES

            AppendSolutions(processContainer);
            AppendWorkflows(processContainer);
            AppendResources(processContainer);
            AppendEntities(processContainer);
            AppendPlugins(processContainer);
            AppendOptions(processContainer);
            AppendSecurityRoles(processContainer);
            AppendDashboards(processContainer);
            AppendEmailTemplates(processContainer);
            AppendReports(processContainer);
            AppendCaseCreationRules(processContainer);

            AppendData(processContainer);

            processContainer.NumberOfProcesses = processContainer.Comparisons.Sum(GetProcessCount);

            foreach (var item in processContainer.Comparisons)
                ProcessCompare(item, processContainer);
        }

        private void AppendEmailTemplates(ProcessContainer processContainer)
        {
            if (!processContainer.Request.EmailTemplates)
                return;

            var dashboardCompareParams = new ProcessCompareParams("Email Templates",
                Entities.template,
                Fields.template_.title,
                Fields.template_.title,
                new[]
                {
                    new Condition(Fields.template_.ispersonal, ConditionType.NotEqual, true)
                },
                new[]
                {
                    Fields.template_.subject, Fields.template_.body, Fields.template_.description
                });
            dashboardCompareParams.AddConversionObject(Fields.template_.body, new ProcessCompareParams.RemoveMiscEmailTemplateXml(), new ProcessCompareParams.RemoveMiscEmailTemplateXml());
            processContainer.Comparisons.Add(dashboardCompareParams);
        }

        private void AppendReports(ProcessContainer processContainer)
        {
            if (!processContainer.Request.Reports)
                return;

            var dashboardCompareParams = new ProcessCompareParams("Report",
                Entities.report,
                Fields.report_.name,
                Fields.report_.name,
                new[]
                {
                    new Condition(Fields.report_.ispersonal, ConditionType.NotEqual, true)
                },
                new[]
                {
                    Fields.report_.isscheduledreport, Fields.report_.bodytext, Fields.report_.customreportxml, Fields.report_.defaultfilter, Fields.report_.description, Fields.report_.originalbodytext, Fields.report_.queryinfo, Fields.report_.schedulexml
                });

            processContainer.Comparisons.Add(dashboardCompareParams);
        }

        private void AppendDashboards(ProcessContainer processContainer)
        {
            if (!processContainer.Request.Dashboards)
                return;

            var dashboardCompareParams = new ProcessCompareParams("Dashboard",
                Entities.systemform,
                Fields.systemform_.name,
                Fields.systemform_.name,
                new[]
                {
                    new Condition(Fields.systemform_.type, ConditionType.Equal, OptionSets.SystemForm.FormType.Dashboard)
                },
                new[]
                {
                    Fields.systemform_.formxml, Fields.systemform_.formpresentation, Fields.systemform_.formactivationstate, Fields.systemform_.description
                });

            processContainer.Comparisons.Add(dashboardCompareParams);
        }

        public int GetProcessCount(ProcessCompareParams compare)
        {
            return 1 + (compare.ChildCompares == null ? 0 : compare.ChildCompares.Sum(GetProcessCount));
        }

        private void AppendCaseCreationRules(ProcessContainer processContainer)
        {
            if (!processContainer.Request.CaseCreationRules)
                return;
            var processCompareParams = new ProcessCompareParams("Case Creation Rule",
                Entities.convertrule, Fields.convertrule_.name, Fields.convertrule_.name,
                null,
                new[] {Fields.convertrule_.statecode,
                    Fields.convertrule_.statuscode,
                    Fields.convertrule_.sourcetypecode,
                    Fields.convertrule_.allowunknownsender,
                    Fields.convertrule_.checkactiveentitlement,
                    Fields.convertrule_.checkifresolved,
                    Fields.convertrule_.resolvedsince,
                    Fields.convertrule_.sendautomaticresponse,
                    Fields.convertrule_.responsetemplateid,
                    Fields.convertrule_.checkblockedsocialprofile,
                    Fields.convertrule_.checkdirectmessages}
                )
            {
                SolutionComponentConfiguration = new ProcessCompareParams.SolutionComponentConfig(Fields.convertrule_.convertruleid, OptionSets.SolutionComponent.ObjectTypeCode.ConvertRule)
            };

            if (processContainer.ServiceOne.RecordTypeExists(processCompareParams.RecordType))
                processContainer.Comparisons.Add(processCompareParams);
        }

        private void AppendData(InstanceComparerService.ProcessContainer processContainer)
        {
            if (!processContainer.Request.Data)
                return;
            var compares = processContainer
                .Request.DataComparisons
                .Select(c => new ProcessCompareParams(c, processContainer.ServiceOne))
                .ToArray();
            foreach (var compare in compares)
            {
                processContainer.Comparisons.Add(compare);
            }
        }

        private void AppendSecurityRoles(ProcessContainer processContainer)
        {
            if (!processContainer.Request.SecurityRoles)
                return;
            var processCompareParams = new ProcessCompareParams("Security Role",
                Entities.role, Fields.role_.name, Fields.role_.name,
                new[] { new Condition(Fields.role_.parentroleid, ConditionType.Null), new Condition(Fields.role_.name, ConditionType.NotEqual, "System Administrator"), new Condition(Fields.role_.name, ConditionType.NotEqual, "System Customizer") },
                null)
            {
                SolutionComponentConfiguration = new ProcessCompareParams.SolutionComponentConfig(Fields.role_.roleid, OptionSets.SolutionComponent.ObjectTypeCode.Role)
            };

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

            processContainer.Comparisons.Add(processCompareParams);
        }

        private void AppendOptions(ProcessContainer processContainer)
        {
            if (!processContainer.Request.SharedOptions)
                return;
            var processCompareParams = new ProcessCompareParams("Shared Picklist",
                typeof(IPicklistSet),
                s => s.GetSharedPicklists().ToArray(),
                nameof(IPicklistSet.SchemaName),
                GetReadableProperties(typeof(IPicklistSet), new[]
                    {
                        nameof(IPicklistSet.PicklistOptions)
                    }))
            {
                SolutionComponentConfiguration = new ProcessCompareParams.SolutionComponentConfig(nameof(IPicklistSet.MetadataId), OptionSets.SolutionComponent.ObjectTypeCode.OptionSet)
            };

            //note the Shared word is used for the url to point to a shared picklist
            var optionCompareParams = new ProcessCompareParams("Shared Picklist Option", typeof(PicklistOption),
                (s, r) => r.GetSharedPicklistOptions(s).ToArray(),
                nameof(PicklistOption.Key),
                GetReadableProperties(typeof(PicklistOption), null));

            processCompareParams.ChildCompares = new[] { optionCompareParams };

            processContainer.Comparisons.Add(processCompareParams);
        }

        private void AppendPlugins(ProcessContainer processContainer)
        {

            if (!processContainer.Request.Plugins)
                return;
            var processCompareParams = new ProcessCompareParams("Plugin Assembly",
                Entities.pluginassembly, Fields.pluginassembly_.pluginassemblyid, Fields.pluginassembly_.name,
                new[] { new Condition(Fields.pluginassembly_.ishidden, ConditionType.NotEqual, true) },
                new[] { Fields.pluginassembly_.content, Fields.pluginassembly_.isolationmode, Fields.pluginassembly_.description })
            {
                SolutionComponentConfiguration = new ProcessCompareParams.SolutionComponentConfig(Fields.pluginassembly_.pluginassemblyid, OptionSets.SolutionComponent.ObjectTypeCode.PluginAssembly)
            };

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

            processContainer.Comparisons.Add(processCompareParams);
        }

        private void AppendEntities(ProcessContainer processContainer)
        {
            if (!processContainer.Request.Entities)
                return;
            var processCompareParams = new ProcessCompareParams("Entity", typeof(IRecordTypeMetadata),
                s => s.GetAllRecordTypes().Select(s.GetRecordTypeMetadata).ToArray(),
                nameof(IRecordTypeMetadata.SchemaName),
                GetReadableProperties(typeof(IRecordTypeMetadata), new[]
                    {
                        nameof(IRecordTypeMetadata.MetadataId), nameof(IRecordTypeMetadata.RecordTypeCode), nameof(IRecordTypeMetadata.Activities), nameof(IRecordTypeMetadata.Notes)
                    }))
            {
                SolutionComponentConfiguration = new ProcessCompareParams.SolutionComponentConfig(nameof(IRecordTypeMetadata.MetadataId), OptionSets.SolutionComponent.ObjectTypeCode.Entity)
            };

            var fieldsCompareParams = new ProcessCompareParams("Field", typeof(IFieldMetadata),
                (s, r) => r.GetFieldMetadata(s).ToArray(),
                nameof(IFieldMetadata.SchemaName),
                GetReadableProperties(typeof(IFieldMetadata), new[]
                    {
                        nameof(IFieldMetadata.MetadataId)
                    }), parentLinkProperty: nameof(IFieldMetadata.RecordType));

            var ignoreOptionFieldNames = new[]
            {
                //these ones are system sets reference object type codes so lets ignore them
                //may be a better way to identify them but this will do for now
                "objecttypecode", "targetentity", "baseentitytypecode", "matchingentitytypecode", "baseentitytypecode", "matchingentitytypecode"
            };
            var fieldsOptionParams = new ProcessCompareParams("Field Options", typeof(PicklistOption),
                (field, recordType, service) =>
                {
                    var picklist = service.GetPicklistKeyValues(field, recordType);
                    return picklist == null || ignoreOptionFieldNames.Contains(field) ? new PicklistOption[0] : picklist.ToArray();
                },
                nameof(PicklistOption.Key),
                GetReadableProperties(typeof(PicklistOption), null));

            fieldsCompareParams.ChildCompares = new[] { fieldsOptionParams };

            var manyToManyCompareParams = new ProcessCompareParams("Many To Many Relationship", typeof(IMany2ManyRelationshipMetadata),
                (s, r) => r.GetManyToManyRelationships(s).ToArray(),
                nameof(IMany2ManyRelationshipMetadata.SchemaName),
                GetReadableProperties(typeof(IMany2ManyRelationshipMetadata), new[]
                    {
                        nameof(IMany2ManyRelationshipMetadata.MetadataId)
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

            processContainer.Comparisons.Add(processCompareParams);
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
            if (!processContainer.Request.WebResources)
                return;
            var processArgs = new ProcessCompareParams("Web Resource",
                Entities.webresource,
                Fields.webresource_.name,
                Fields.webresource_.name,
                new[] { new Condition(Fields.webresource_.ishidden, ConditionType.NotEqual, true) },
                new[] { Fields.webresource_.content, Fields.webresource_.description, Fields.webresource_.displayname, Fields.webresource_.webresourcetype, Fields.webresource_.languagecode })
            {
                SolutionComponentConfiguration = new ProcessCompareParams.SolutionComponentConfig(Fields.webresource_.webresourceid, OptionSets.SolutionComponent.ObjectTypeCode.WebResource)
            };

            processArgs.AddConversionObject(Fields.webresource_.content,
                new ProcessCompareParams.ConvertFromBase64String(processContainer.ServiceOne),
                new ProcessCompareParams.ConvertFromBase64String(processContainer.ServiceTwo));

            processContainer.Comparisons.Add(processArgs);
        }

        private void AppendSolutions(ProcessContainer processContainer)
        {
            if (!processContainer.Request.Solutions)
                return;
            var processArgs = new ProcessCompareParams("Solution",
                Entities.solution,
                Fields.solution_.uniquename,
                Fields.solution_.friendlyname,
                new[] { new Condition(Fields.solution_.isvisible, ConditionType.Equal, true) },
                new[] { Fields.solution_.version, Fields.solution_.friendlyname, Fields.solution_.configurationpageid, Fields.solution_.description });

            processContainer.Comparisons.Add(processArgs);
        }

        private void AppendWorkflows(ProcessContainer processContainer)
        {
            if (!processContainer.Request.Workflows)
                return;
            var processArgs = new ProcessCompareParams("Workflow",
                Entities.workflow,
                Fields.workflow_.workflowid,
                Fields.workflow_.name,
                new[] { new Condition(Fields.workflow_.type, ConditionType.Equal, OptionSets.Process.Type.Definition), new Condition(Fields.workflow_.rendererobjecttypecode, ConditionType.Null) },
                new[] { Fields.workflow_.name, Fields.workflow_.statecode, Fields.workflow_.xaml, Fields.workflow_.description, Fields.workflow_.ondemand, Fields.workflow_.rank, Fields.workflow_.triggeronupdateattributelist, Fields.workflow_.triggeroncreate, Fields.workflow_.triggerondelete, Fields.workflow_.createstage, Fields.workflow_.updatestage, Fields.workflow_.deletestage, Fields.workflow_.iscrmuiworkflow, Fields.workflow_.istransacted, Fields.workflow_.mode, Fields.workflow_.runas, Fields.workflow_.subprocess, Fields.workflow_.scope, Fields.workflow_.primaryentity, Fields.workflow_.sdkmessageid })
            {
                SolutionComponentConfiguration = new ProcessCompareParams.SolutionComponentConfig(Fields.workflow_.workflowid, OptionSets.SolutionComponent.ObjectTypeCode.Workflow)
            };

            processArgs.AddConversionObject(Fields.workflow_.sdkmessageid,
                new ProcessCompareParams.ConvertWorkflowMessage(processContainer.ServiceOne),
                new ProcessCompareParams.ConvertWorkflowMessage(processContainer.ServiceTwo));
            processArgs.AddConversionObject(Fields.workflow_.xaml,
                new ProcessCompareParams.RemoveMiscWorkflowXml(),
                new ProcessCompareParams.RemoveMiscWorkflowXml());

            processContainer.Comparisons.Add(processArgs);
        }

        private void ProcessCompare(ProcessCompareParams processCompareParams, ProcessContainer processContainer, ProcessCompareParams parentCompareParams = null, List<List<IRecord>> parents = null)
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
                    var groupThem = parents
                        .Select(l => l.ToDictionary(i => i, i => new List<IRecord>()))
                        .ToList();
                    if (processCompareParams.Type == ProcessCompareType.Records)
                    {
                        //add some comments for this mess
                        if (processCompareParams.ParentLinkType == ParentLinkType.Lookup)
                        {
                            processContainer.Controller.UpdateLevel2Progress(1, 4, string.Format("Loading {0} Items", processContainer.Request.ConnectionOne.Name));
                            var serviceOneItems =
                                processContainer.ServiceOne.RetrieveAllOrClauses(processCompareParams.RecordType,
                                    parents.Select(v => v.First())
                                        .Select(
                                            r =>
                                                new Condition(processCompareParams.ParentLink, ConditionType.Equal,
                                                    GetParentReference(parentCompareParams, r))),
                                    null);
                            processContainer.Controller.UpdateLevel2Progress(2, 4, string.Format("Loading {0} Items", processContainer.Request.ConnectionTwo.Name));
                            var serviceTwoItems =
                                processContainer.ServiceTwo.RetrieveAllOrClauses(processCompareParams.RecordType,
                                    parents.Select(v => v.Last())
                                        .Select(
                                            r =>
                                                new Condition(processCompareParams.ParentLink, ConditionType.Equal,
                                                   GetParentReference(parentCompareParams, r))),
                                    null);
                            processContainer.Controller.UpdateLevel2Progress(3, 4, "Comparing");
                            foreach (var item in serviceOneItems)
                            {
                                var referringField = item.GetField(processCompareParams.ParentLink);
                                if (referringField is Lookup)
                                    referringField = ((Lookup)referringField).Id;
                                foreach (var parent in groupThem)
                                {
                                    if (FieldsEqual(GetParentReference(parentCompareParams, parent.Keys.First()),
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
                                    if (FieldsEqual(GetParentReference(parentCompareParams, parent.Keys.Last()),
                                        referringField))
                                        parent.Values.Last().Add(item);
                                }
                            }
                        }

                        //group by each parent and process
                        foreach (var group in groupThem)
                        {
                            inBoth.AddRange(DoCompare(processCompareParams, processContainer, group.Values.First(),
                                group.Values.Last(), parentCompareParams: parentCompareParams, parent1: group.Keys.First(), parent2: group.Keys.Last()));
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
                                    parentReference,
                                    parentCompareParams.ParentLinkProperty == null ? null : group.First().Key.GetStringField(parentCompareParams.ParentLinkProperty),
                                    processContainer.ServiceOne).Select(r => new ObjectRecord(r)).ToArray();
                            var items2 =
                                processCompareParams.GetObjects(
                                    parentReference,
                                    parentCompareParams.ParentLinkProperty == null ? null : group.Last().Key.GetStringField(parentCompareParams.ParentLinkProperty),
                                    processContainer.ServiceTwo).Select(r => new ObjectRecord(r)).ToArray();
                            inBoth.AddRange(DoCompare(processCompareParams, processContainer, items1, items2, parentCompareParams: parentCompareParams, parent1: group.First().Key, parent2: group.Last().Key));
                        }
                    }
                }
                else
                {
                    if (processCompareParams.Type == ProcessCompareType.Records)
                    {
                        processContainer.Controller.UpdateLevel2Progress(1, 4, string.Format("Loading {0} Items", processContainer.Request.ConnectionOne.Name));
                        var serviceOneItems = processContainer.ServiceOne.RetrieveAllAndClauses(
                            processCompareParams.RecordType, processCompareParams.Conditions, null);
                        processContainer.Controller.UpdateLevel2Progress(2, 4, string.Format("Loading {0} Items", processContainer.Request.ConnectionTwo.Name));
                        var serviceTwoItems = processContainer.ServiceTwo.RetrieveAllAndClauses(
                            processCompareParams.RecordType, processCompareParams.Conditions, null);
                        processContainer.Controller.UpdateLevel2Progress(3, 4, "Comparing");
                        inBoth.AddRange(DoCompare(processCompareParams, processContainer, serviceOneItems, serviceTwoItems));

                    }
                    if (processCompareParams.Type == ProcessCompareType.Objects)
                    {
                        processContainer.Controller.UpdateLevel2Progress(1, 4, string.Format("Loading {0} Items", processContainer.Request.ConnectionOne.Name));
                        var serviceOneItems =
                            processCompareParams.GetObjects(null, null, processContainer.ServiceOne)
                                .Select(o => new ObjectRecord(o))
                                .ToArray();
                        processContainer.Controller.UpdateLevel2Progress(2, 4, string.Format("Loading {0} Items", processContainer.Request.ConnectionTwo.Name));
                        var serviceTwoItems =
                            processCompareParams.GetObjects(null, null, processContainer.ServiceTwo)
                                .Select(o => new ObjectRecord(o))
                                .ToArray();
                        processContainer.Controller.UpdateLevel2Progress(3, 4, "Comparing");
                        inBoth.AddRange(DoCompare(processCompareParams, processContainer, serviceOneItems, serviceTwoItems));
                    }
                }
                processContainer.Controller.TurnOffLevel2();
                processContainer.NumberOfProcessesCompleted++;
                if (processCompareParams.ChildCompares != null)
                {
                    foreach (var item in processCompareParams.ChildCompares)
                    {
                        ProcessCompare(item, processContainer, processCompareParams, inBoth);
                    }
                }
            }
            catch (Exception ex)
            {
                processContainer.Response.AddResponseItem(new InstanceComparerResponseItem("Fatal Error Comparing", processCompareParams.Context, ex));
            }
        }

        private static string GetParentReference(ProcessCompareParams parentCompareParams, IRecord parentRecord)
        {
            return parentCompareParams.Type == ProcessCompareType.Objects ? parentRecord.GetStringField(parentCompareParams.MatchField) : parentRecord.Id;
        }

        private static string GetParentId(ProcessCompareParams parentCompareParams, IRecord parentRecord)
        {
            return parentRecord.Id == null ? parentRecord.GetStringField(parentCompareParams.MatchField) : parentRecord.Id;
        }

        private List<List<IRecord>> DoCompare(ProcessCompareParams processCompareParams,
            ProcessContainer processContainer,
            IEnumerable<IRecord> serviceOneItems, IEnumerable<IRecord> serviceTwoItems, ProcessCompareParams parentCompareParams = null, IRecord parent1 = null, IRecord parent2 = null)
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
                if (!processContainer.ProcessIfManagedComponentExclude(processCompareParams, item, true))
                {
                    var parentReference = parent1 == null ? null : parent1.GetStringField(parentCompareParams.MatchField);
                    var parentId = parent1 == null ? null : GetParentId(parentCompareParams, parent1);
                    var displayName = processCompareParams.ConvertField1(processCompareParams.DisplayField, item.GetStringField(processCompareParams.DisplayField));
                    processContainer.AddDifference(processCompareParams.Context, processCompareParams.RecordType,
                        displayName, "Only In " + processContainer.Request.ConnectionOne.Name, displayName, null, item.Id, null, parentReference: parentReference, parentId1: parentId);
                }
            }
            var inTwoNotInOne = serviceTwoItems
                .Where(w => !thisInBoth.Select(kv => kv.Last()).Contains(w))
                .ToArray();
            foreach (var item in inTwoNotInOne)
            {
                if (!processContainer.ProcessIfManagedComponentExclude(processCompareParams, item, false))
                {
                    var parentReference = parent2 == null ? null : parent2.GetStringField(parentCompareParams.MatchField);
                    var parentId = parent2 == null ? null : GetParentId(parentCompareParams, parent2);
                    var displayName = processCompareParams.ConvertField2(processCompareParams.DisplayField, item.GetStringField(processCompareParams.DisplayField));
                    processContainer.AddDifference(processCompareParams.Context, processCompareParams.RecordType,
                        displayName, "Only In " + processContainer.Request.ConnectionTwo.Name, null, displayName, null, item.Id, parentReference: parentReference, parentId2: parentId);
                }
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
                                displayValue1 = (string)field1;
                                displayValue2 = (string)field2;

                                var charsToDisplay = 250;
                                if (field1 == null)
                                    displayValue2 = displayValue2.Left(charsToDisplay) + (displayValue2.Length > charsToDisplay ? "..." : "");
                                else if (field2 == null)
                                    field1 = displayValue1.Left(charsToDisplay) + (displayValue1.Length > charsToDisplay ? "..." : "");
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
                        var parentReference = parent1 == null ? null : parent1.GetStringField(parentCompareParams.MatchField);
                        var parentId1 = parent1 == null ? null : GetParentId(parentCompareParams, parent1);
                        var parentId2 = parent2 == null ? null : GetParentId(parentCompareParams, parent2);
                        processContainer.AddDifference(processCompareParams.Context, processCompareParams.RecordType,
                            processCompareParams.ConvertField1(processCompareParams.DisplayField, item.First().GetStringField(processCompareParams.DisplayField)),
                            "Different " + fieldLabel, displayValue1, displayValue2, item.First().Id, item.Last().Id, parentReference: parentReference, parentId1: parentId1, parentId2: parentId2);
                    }
                }
            }
            return thisInBoth;
        }

        private static bool FieldsEqual(object field1, object field2)
        {
            if (field1 == null && field2 == null)
                return true;
            else if (field1 == null || field2 == null)
            {
                if (field1 is string || field2 is string)
                    return String.IsNullOrEmpty((string)field1) && String.IsNullOrEmpty((string)field2);
                else
                    return false;
            }
            else
                return field1.Equals(field2);
        }

        public class ProcessCompareParams
        {
            public class SolutionComponentConfig
            {
                public string MetadataIdFieldName { get; set; }
                public int ComponentType { get; set; }

                public SolutionComponentConfig(string metadataIdFieldName, int componentType)
                {
                    MetadataIdFieldName = metadataIdFieldName;
                    ComponentType = componentType;
                }
            }

            public SolutionComponentConfig SolutionComponentConfiguration { get; set; }
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

            public ProcessCompareParams(string context, Type type, Func<IRecordService, IEnumerable<object>> getObjects, string keyProperty, IEnumerable<string> fieldsCheckDifference)
                : this(context, type, (s, r) => getObjects(r), keyProperty, fieldsCheckDifference)
            {
            }

            public ProcessCompareParams(string context, Type type, Func<string, IRecordService, IEnumerable<object>> getObjects, string keyProperty, IEnumerable<string> fieldsCheckDifference, string parentLinkProperty = null)
                : this(context, type, (s1, s2, s3) => getObjects(s1, s3), keyProperty, fieldsCheckDifference)
            {
                ParentLinkProperty = parentLinkProperty;
            }

            public ProcessCompareParams(string context, Type type, Func<string, string, IRecordService, IEnumerable<object>> getObjects, string keyProperty, IEnumerable<string> fieldsCheckDifference)
            {
                Context = context;
                RecordType = type.AssemblyQualifiedName;
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
            public string ParentLinkProperty { get; private set; }

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
                    theString = StripStartToEnd(theString, "TypeArguments=\"x:String\"Default=", "Name=\"stepLabelLabelId");
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
                    theString = theString.Replace(" />", "/>");
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
                    theString = theString.Replace("<row></row>", "<row />");
                    return theString;
                }
            }

            public class RemoveMiscEmailTemplateXml : ConvertField
            {
                public override object Convert(object sourceValue)
                {
                    if (sourceValue == null)
                        return null;

                    var theString = (string)sourceValue;
                    theString = RemoveUrlPart(theString, "import/edit.aspx");
                    theString = RemoveUrlPart(theString, "tools/asyncoperation/");
                    theString = RemoveUrlPart(theString, "import/edit.aspx");
                    theString = RemoveUrlPart(theString, "tools/asyncoperation/");
                    theString = RemoveUrlPart(theString, "import/edit.aspx");
                    theString = RemoveUrlPart(theString, "tools/asyncoperation/");

                    return theString;
                }

                private static string RemoveUrlPart(object sourceValue, string urlPart)
                {
                    var theString = (string)sourceValue;
                    var linkIndex = theString.IndexOf(urlPart);
                    if (linkIndex != -1)
                    {
                        var httpIndex = ((string)sourceValue).Substring(0, linkIndex).LastIndexOf("http");
                        if (httpIndex != -1 && httpIndex < linkIndex)
                            theString = theString.Substring(0, httpIndex) + ((string)sourceValue).Substring(linkIndex + (urlPart.Length - 1));
                    }

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

            public class ConvertFromBase64String : ConvertField
            {
                public IRecordService Service { get; set; }

                public ConvertFromBase64String(IRecordService service)
                {
                    Service = service;
                }

                public override object Convert(object sourceValue)
                {
                    if (sourceValue == null)
                        return null;

                    var data = System.Convert.FromBase64String(sourceValue.ToString());
                    return Encoding.UTF8.GetString(data);
                }
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
            public List<ProcessCompareParams> Comparisons { get; set; }
            public List<InstanceComparerDifference> Differences { get; set; }
            public Dictionary<string, IEnumerable<IRecord>> MissingManagedSolutionComponents { get; private set; }

            public ProcessContainer(InstanceComparerRequest request, InstanceComparerResponse response,
                LogController controller)
            {
                Request = request;
                Response = response;
                Controller = controller;
                ServiceOne = new XrmRecordService(request.ConnectionOne);
                ServiceTwo = new XrmRecordService(request.ConnectionTwo);
                Differences = new List<InstanceComparerDifference>();
                Comparisons = new List<ProcessCompareParams>();
                MissingManagedSolutionComponents = new Dictionary<string, IEnumerable<IRecord>>();
            }

            internal void AddDifference(string type, string recordType, object name, string difference, object value1, object value2, string id1, string id2, string parentReference = null, string parentId1 = null, string parentId2 = null)
            {
                //this part generating url logic then adds the difference
                var linkRecordType = recordType;
                var linkId1 = id1;
                var linkId2 = id2;
                string additionalParams = null;
                if (linkRecordType == Relationships.role_.roleprivileges_association.EntityName)
                {
                    linkRecordType = Entities.role;
                    linkId1 = parentId1;
                    linkId2 = parentId2;
                }
                else if (linkRecordType == typeof(IFieldMetadata).AssemblyQualifiedName)
                {
                    linkRecordType = "field";
                    linkId1 = id1 == null ? null : ServiceOne.GetFieldMetadata(name?.ToString(), parentReference).MetadataId;
                    linkId2 = id2 == null ? null : ServiceTwo.GetFieldMetadata(name?.ToString(), parentReference).MetadataId;
                    additionalParams = "entityId=" + ServiceOne.GetRecordTypeMetadata(parentReference).MetadataId;
                }
                else if (linkRecordType == typeof(IMany2ManyRelationshipMetadata).AssemblyQualifiedName)
                {
                    linkRecordType = "manytomanyrelationship";
                    linkId1 = id1 == null ? null : ServiceOne.GetManyRelationshipMetadata(name?.ToString(), parentReference).MetadataId;
                    linkId2 = id2 == null ? null : ServiceTwo.GetManyRelationshipMetadata(name?.ToString(), parentReference).MetadataId;
                    additionalParams = "entityId=" + ServiceOne.GetRecordTypeMetadata(parentReference).MetadataId;
                }
                else if (linkRecordType == typeof(IPicklistSet).AssemblyQualifiedName)
                {
                    linkRecordType = "sharedoptionset";
                    linkId1 = id1 == null ? null : ServiceOne.GetSharedPicklist(name?.ToString()).MetadataId;
                    linkId2 = id2 == null ? null : ServiceTwo.GetSharedPicklist(name?.ToString()).MetadataId;
                }
                else if (linkRecordType == typeof(IRecordTypeMetadata).AssemblyQualifiedName)
                {
                    linkRecordType = "entity";
                }
                else if (linkRecordType == typeof(PicklistOption).AssemblyQualifiedName)
                {
                    if(type.Contains("Shared"))
                    {
                        linkRecordType = "sharedoptionset";
                        linkId1 = id1 == null ? null : ServiceOne.GetSharedPicklist(parentReference).MetadataId;
                        linkId2 = id2 == null ? null : ServiceTwo.GetSharedPicklist(parentReference).MetadataId;
                    }
                    else
                    {
                        linkRecordType = "field";
                        var matchingFieldMetadata1 = parentId1 == null ? null :
                            ServiceOne
                            .GetAllRecordTypes()
                            .Select(r => ServiceOne.GetFieldMetadata(r))
                            .SelectMany(f => f.ToArray())
                            .First(f => f.MetadataId == parentId1);
                        var matchingFieldMetadata2 = parentId2 == null ? null : 
                            ServiceTwo
                            .GetAllRecordTypes()
                            .Select(r => ServiceTwo.GetFieldMetadata(r))
                            .SelectMany(f => f.ToArray())
                            .First(f => f.MetadataId == parentId2);
                        linkId1 = matchingFieldMetadata1 == null ? null : matchingFieldMetadata1.MetadataId;
                        linkId2 = matchingFieldMetadata2 == null ? null : matchingFieldMetadata2.MetadataId;
                        var parentRecordType = matchingFieldMetadata1?.RecordType ?? matchingFieldMetadata2?.RecordType;
                        var entityId = matchingFieldMetadata1 == null
                            ? ServiceTwo.GetRecordTypeMetadata(parentRecordType).MetadataId
                            : ServiceOne.GetRecordTypeMetadata(parentRecordType).MetadataId;
                        additionalParams = "entityId=" + entityId;
                    }
                }
                var url1String = id1 == null ? null : ServiceOne.GetWebUrl(linkRecordType, linkId1, additionalParams);
                var url2String = id2 == null ? null : ServiceTwo.GetWebUrl(linkRecordType, linkId2, additionalParams);

                var url1 = string.IsNullOrWhiteSpace(url1String)
                    ? null
                    : new Url(url1String, string.Format("Open ({0})", Request.ConnectionOne.Name));
                var url2 = string.IsNullOrWhiteSpace(url2String)
                    ? null
                    : new Url(url2String, string.Format("Open ({0})", Request.ConnectionTwo.Name));

                Differences.Add(new InstanceComparerDifference(type, name == null ? null : name.ToString(), difference, parentReference, value1 == null ? null : value1.ToString(), value2 == null ? null : value2.ToString(), url1, url2, id1, id2));
            }

            internal bool ProcessIfManagedComponentExclude(ProcessCompareParams processCompareParams, IRecord item, bool isInConnection1)
            {
                //okay if we have a managed solution missing in one environment
                //then lets exclude components in that solution in the list because it just creates irrelevant noise in the output

                //firstly if this is a solution we need to capture it in this record
                if(processCompareParams.RecordType == Entities.solution && item.GetBoolField(Fields.solution_.ismanaged))
                {
                    if(!MissingManagedSolutionComponents.ContainsKey(item.Id))
                    {
                        var componentsInSolution = isInConnection1
                            ? ServiceOne.GetLinkedRecords(Entities.solutioncomponent, Entities.solution, Fields.solutioncomponent_.solutionid, item.Id)
                            : ServiceTwo.GetLinkedRecords(Entities.solutioncomponent, Entities.solution, Fields.solutioncomponent_.solutionid, item.Id);
                        MissingManagedSolutionComponents.Add(item.Id, componentsInSolution);
                    }
                }
                //okay otherwise if a solution component configured then check if this is a part of the managed solution
                else if (processCompareParams.SolutionComponentConfiguration != null)
                {
                    foreach(var solution in MissingManagedSolutionComponents)
                    {
                        foreach(var component in solution.Value)
                        {
                            if(component.GetIdField(Fields.solutioncomponent_.objectid) == item.GetIdField(processCompareParams.SolutionComponentConfiguration.MetadataIdFieldName)
                                && component.GetOptionKey(Fields.solutioncomponent_.componenttype) == processCompareParams.SolutionComponentConfiguration.ComponentType.ToString())
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
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
