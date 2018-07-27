using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Service;
using JosephM.Record.Xrm;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Crud;
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
            AppendSlas(processContainer);
            AppendApps(processContainer);
            AppendRoutingRules(processContainer);

            AppendData(processContainer);

            var numberOfDataToProcess = processContainer.Request.Data
                && processContainer.Request.DataComparisons != null
                ? processContainer.Request.DataComparisons.Count()
                : 0;

            processContainer.NumberOfProcesses = processContainer.Comparisons.Sum(GetProcessCount) + numberOfDataToProcess;//add data again for associations

            foreach (var item in processContainer.Comparisons)
                ProcessCompare(item, processContainer);

            CompareAssociations(processContainer);
        }

        private void CompareAssociations(ProcessContainer processContainer)
        {
            //in this method for all data we compared
            //we will also compare the associations between them in the 2 instances
            //this would for example output if an microsoft portal entity permission was associated
            //to a web role in one instance but not the other
            var relationshipsDone = new List<string>();
            if (processContainer.Request.Data && processContainer.Request.DataComparisons != null)
            {
                var theTypes = processContainer.Request.DataComparisons.Select(dc => dc.Type).ToArray();
                foreach (var type in theTypes)
                {
                    try
                    {
                        if (!processContainer.MatchedRecordDictionary.ContainsKey(type))
                            continue;

                        //each data type we compared and got matches
                        processContainer.Controller.UpdateProgress(processContainer.NumberOfProcessesCompleted,
                            processContainer.NumberOfProcesses, "Comparing Data Associations - " + type);

                        //load its nn relationships where both side entities were included in data comparison
                        var nnRelationships = processContainer.ServiceOne.GetManyToManyRelationships(type)
                            .Where(
                                r =>
                                    theTypes.Contains(r.RecordType1) && theTypes.Contains(r.RecordType2));
                        foreach (var item in nnRelationships)
                        {
                            try
                            {
                                var type1 = item.RecordType1;
                                var type2 = item.RecordType2;
                                if (!relationshipsDone.Contains(item.SchemaName))
                                {
                                    //get the associations in both instances
                                    var associations1 = processContainer.ServiceOne.RetrieveAll(item.IntersectEntityName, null);
                                    var associations2 = processContainer.ServiceTwo.RetrieveAll(item.IntersectEntityName, null);
                                    foreach (var association in associations1)
                                    {
                                        //all the associations in instance 1
                                        var guid1a = association.GetIdField(item.Entity1IntersectAttribute);
                                        var guid1b = association.GetIdField(item.Entity2IntersectAttribute);
                                        if (processContainer.MatchedRecordDictionary[item.RecordType1].Any(kv => kv.Key.Id == guid1a) && processContainer.MatchedRecordDictionary[item.RecordType2].Any(kv => kv.Key.Id == guid1b))
                                        {
                                            var recordMatcha = processContainer.MatchedRecordDictionary[item.RecordType1].First(kv => kv.Key.Id == guid1a);
                                            var recordMatchb = processContainer.MatchedRecordDictionary[item.RecordType2].First(kv => kv.Key.Id == guid1b);
                                            var guid2a = recordMatcha.Value.Id;
                                            var guid2b = recordMatchb.Value.Id;
                                            var matchAssociation2 = associations2.Where(a => a.GetIdField(item.Entity1IntersectAttribute) == guid2a && a.GetIdField(item.Entity2IntersectAttribute) == guid2b);
                                            if (!matchAssociation2.Any())
                                            {
                                                //where there is not matching association in instance 2
                                                //output a difference
                                                var displayNamea = GetItemDisplayName(recordMatcha.Key, processContainer.ServiceOne, processContainer.Comparisons.First(c => c.RecordType == recordMatcha.Key.Type), false);
                                                var displayNameb = GetItemDisplayName(recordMatchb.Key, processContainer.ServiceOne, processContainer.Comparisons.First(c => c.RecordType == recordMatchb.Key.Type), false);
                                                processContainer.AddDifference("Data " + recordMatcha.Key.Type, recordMatcha.Key.Type, displayNamea, "Associated " + recordMatchb.Key.Type, displayNameb, null, recordMatcha.Key.Id, recordMatcha.Value.Id);
                                            }
                                        }
                                    }
                                    foreach (var association in associations2)
                                    {
                                        //all the associations in instance 2
                                        var guid2a = association.GetIdField(item.Entity1IntersectAttribute);
                                        var guid2b = association.GetIdField(item.Entity2IntersectAttribute);
                                        if (processContainer.MatchedRecordDictionary[item.RecordType1].Any(kv => kv.Value.Id == guid2a) && processContainer.MatchedRecordDictionary[item.RecordType2].Any(kv => kv.Value.Id == guid2b))
                                        {
                                            var recordMatcha = processContainer.MatchedRecordDictionary[item.RecordType1].First(kv => kv.Value.Id == guid2a);
                                            var recordMatchb = processContainer.MatchedRecordDictionary[item.RecordType2].First(kv => kv.Value.Id == guid2b);
                                            var guid1a = recordMatcha.Key.Id;
                                            var guid1b = recordMatchb.Key.Id;
                                            var matchAssociation1 = associations1.Where(a => a.GetIdField(item.Entity1IntersectAttribute) == guid1a && a.GetIdField(item.Entity2IntersectAttribute) == guid1b);
                                            if (!matchAssociation1.Any())
                                            {
                                                //where there is not matching association in instance 1
                                                //output a difference
                                                var displayNamea = GetItemDisplayName(recordMatcha.Value, processContainer.ServiceTwo, processContainer.Comparisons.First(c => c.RecordType == recordMatcha.Value.Type), true);
                                                var displayNameb = GetItemDisplayName(recordMatchb.Value, processContainer.ServiceTwo, processContainer.Comparisons.First(c => c.RecordType == recordMatchb.Value.Type), true);
                                                processContainer.AddDifference("Data " + recordMatcha.Value.Type, recordMatcha.Value.Type, displayNamea, "Associated " + recordMatchb.Key.Type, null, displayNameb, recordMatcha.Key.Id, recordMatcha.Value.Id);
                                            }
                                        }
                                    }
                                    relationshipsDone.Add(item.SchemaName);
                                }
                            }
                            catch (Exception ex)
                            {
                                processContainer.Response.AddResponseItem(new InstanceComparerResponseItem("Error comparing associations for " + item.SchemaName, type, ex));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        processContainer.Response.AddResponseItem(new InstanceComparerResponseItem("Error loading relationshipd to compare for " + type, type, ex));
                    }
                }
            }
        }

        private void AppendRoutingRules(ProcessContainer processContainer)
        {
            if (!processContainer.Request.RoutingRules)
                return;
            var processCompareParams = new ProcessCompareParams("Routing Rule",
                Entities.routingrule, Fields.routingrule_.name, Fields.routingrule_.name,
                null,
                new[] {Fields.routingrule_.description,
                    Fields.routingrule_.statuscode }
                )
            {
                SolutionComponentConfiguration = new ProcessCompareParams.SolutionComponentConfig(Fields.routingrule_.routingruleid, OptionSets.SolutionComponent.ObjectTypeCode.RoutingRule)
            };

            var itemCompareParams = new ProcessCompareParams("Routing Rule Item",
                Entities.routingruleitem, Fields.routingruleitem_.name, Fields.routingruleitem_.name, null,
                new[] { Fields.routingruleitem_.name, Fields.routingruleitem_.conditionxml, Fields.routingruleitem_.routedqueueid, Fields.routingruleitem_.assignobjectid },
                Fields.routingruleitem_.routingruleid, ParentLinkType.Lookup);

            processCompareParams.ChildCompares = new[] { itemCompareParams };

            if (processContainer.ServiceOne.RecordTypeExists(processCompareParams.RecordType))
                processContainer.Comparisons.Add(processCompareParams);
        }

        private void AppendApps(ProcessContainer processContainer)
        {
            if (!processContainer.Request.Apps)
                return;

            var appCompareParams = new ProcessCompareParams("Apps",
                Entities.appmodule,
                Fields.appmodule_.uniquename,
                Fields.appmodule_.uniquename,
                null,
                new[]
                {
                    Fields.appmodule_.description, Fields.appmodule_.clienttype, Fields.appmodule_.formfactor, Fields.appmodule_.isdefault, Fields.appmodule_.name
                });

            var appComponentCompareParams = new ProcessCompareParams("App Components",
                Entities.appmodulecomponent, Fields.appmodulecomponent_.objectid, Fields.appmodulecomponent_.objectid, null,
                new[] { Fields.appmodulecomponent_.objectid },
                Fields.appmodulecomponent_.appmoduleidunique, ParentLinkType.Lookup)
            {
                OverrideParentId = Fields.appmodule_.appmoduleidunique
            };

            var appRoleCompareParams = new ProcessCompareParams("App Roles",
                Relationships.appmodule_.appmoduleroles_association.EntityName, Fields.role_.roleid, Fields.role_.roleid, null,
                new[] { "roleid" },
                Fields.appmodule_.appmoduleid, ParentLinkType.Lookup);

            appCompareParams.ChildCompares = new[] { appRoleCompareParams, appComponentCompareParams };

            processContainer.Comparisons.Add(appCompareParams);
        }

        private void AppendSlas(ProcessContainer processContainer)
        {
            if (!processContainer.Request.SLAs)
                return;

            var slaCompareParams = new ProcessCompareParams("SLAs",
                Entities.sla,
                Fields.sla_.name,
                Fields.sla_.name,
                null,
                new[]
                {
                    Fields.sla_.description, Fields.sla_.applicablefrom, Fields.sla_.businesshoursid, Fields.sla_.statuscode, Fields.sla_.slatype, Fields.sla_.allowpauseresume
                });

            var slaItemCompareParams = new ProcessCompareParams("SLA Items",
                Entities.slaitem, Fields.slaitem_.name, Fields.slaitem_.name, null,
                new[] { Fields.slaitem_.description, Fields.slaitem_.warnafter, Fields.slaitem_.failureafter, Fields.slaitem_.applicablewhenxml, Fields.slaitem_.relatedfield, Fields.slaitem_.sequencenumber, Fields.slaitem_.successconditionsxml },
                Fields.slaitem_.slaid, ParentLinkType.Lookup);

            slaCompareParams.ChildCompares = new[] { slaItemCompareParams };

            processContainer.Comparisons.Add(slaCompareParams);
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
                    Fields.report_.parentreportid, Fields.report_.isscheduledreport, Fields.report_.bodytext, Fields.report_.customreportxml, Fields.report_.defaultfilter, Fields.report_.description, Fields.report_.originalbodytext, Fields.report_.queryinfo, Fields.report_.schedulexml
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

        private void AppendData(ProcessContainer processContainer)
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
                compare.AddConversionObject(Fields.annotation_.documentbody,
                    new ProcessCompareParams.ConvertFromBase64String(processContainer.ServiceOne),
                    new ProcessCompareParams.ConvertFromBase64String(processContainer.ServiceTwo));
            }
        }

        private void AppendSecurityRoles(ProcessContainer processContainer)
        {
            if (!processContainer.Request.SecurityRoles)
                return;
            var processCompareParams = new ProcessCompareParams("Security Role",
                Entities.role, Fields.role_.name, Fields.role_.name,
                new[] {
                    new Condition(Fields.role_.parentroleid, ConditionType.Null),
                    new Condition(Fields.role_.name, ConditionType.NotEqual, "System Administrator"),
                    new Condition(Fields.role_.name, ConditionType.NotEqual, "Activity Feeds"),
                    new Condition(Fields.role_.name, ConditionType.NotEqual, "Support User"),
                    new Condition(Fields.role_.name, ConditionType.NotEqual, "System Customizer") },
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
                    , Fields.sdkmessageprocessingstep_.plugintypeid, Fields.sdkmessageprocessingstep_.eventhandler
                },
                Fields.sdkmessageprocessingstep_.name,
                null,
                new[]
                {
                    Fields.sdkmessageprocessingstep_.description, Fields.sdkmessageprocessingstep_.filteringattributes, Fields.sdkmessageprocessingstep_.rank, Fields.sdkmessageprocessingstep_.statecode, Fields.sdkmessageprocessingstep_.name
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
            if (!processContainer.Request.AllTypesForEntityMetadata && processContainer.Request.EntityTypeComparisons != null)
            {
                processCompareParams.Conditions = processContainer.Request.EntityTypeComparisons.Select(c => new Condition(nameof(IRecordTypeMetadata.SchemaName), ConditionType.Equal, c.RecordType.Key)).ToArray();
            }

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
            //we always processs olution comarisons
            //in case excluding components which
            //are in a managed solution in the comparison
            //logic in method ProcessContainer.ProcessIfManagedComponentExclude
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
                                                    GetParentReference(parentCompareParams, r, processCompareParams.OverrideParentId))),
                                    null);
                            processContainer.Controller.UpdateLevel2Progress(2, 4, string.Format("Loading {0} Items", processContainer.Request.ConnectionTwo.Name));
                            var serviceTwoItems =
                                processContainer.ServiceTwo.RetrieveAllOrClauses(processCompareParams.RecordType,
                                    parents.Select(v => v.Last())
                                        .Select(
                                            r =>
                                                new Condition(processCompareParams.ParentLink, ConditionType.Equal,
                                                   GetParentReference(parentCompareParams, r, processCompareParams.OverrideParentId))),
                                    null);
                            processContainer.Controller.UpdateLevel2Progress(3, 4, "Comparing");
                            foreach (var item in serviceOneItems)
                            {
                                var referringField = item.GetField(processCompareParams.ParentLink);
                                if (referringField is Lookup)
                                    referringField = ((Lookup)referringField).Id;
                                foreach (var parent in groupThem)
                                {
                                    if (FieldsEqual(GetParentReference(parentCompareParams, parent.Keys.First(), processCompareParams.OverrideParentId),
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
                                    if (FieldsEqual(GetParentReference(parentCompareParams, parent.Keys.Last(), processCompareParams.OverrideParentId),
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
                        AddRequiredParentFields(serviceOneItems, processContainer.ServiceOne);

                        processContainer.Controller.UpdateLevel2Progress(2, 4, string.Format("Loading {0} Items", processContainer.Request.ConnectionTwo.Name));
                        var serviceTwoItems = processContainer.ServiceTwo.RetrieveAllAndClauses(
                            processCompareParams.RecordType, processCompareParams.Conditions, null);
                        AddRequiredParentFields(serviceTwoItems, processContainer.ServiceTwo);

                        processContainer.Controller.UpdateLevel2Progress(3, 4, "Comparing");
                        inBoth.AddRange(DoCompare(processCompareParams, processContainer, serviceOneItems, serviceTwoItems));

                    }
                    if (processCompareParams.Type == ProcessCompareType.Objects)
                    {
                        processContainer.Controller.UpdateLevel2Progress(1, 4, string.Format("Loading {0} Items", processContainer.Request.ConnectionOne.Name));
                        var serviceOneItems =
                            processCompareParams.GetObjects(null, null, processContainer.ServiceOne)
                                .Select(o => new ObjectRecord(o))
                                .Where(r => processCompareParams.Conditions == null || processCompareParams.Conditions.All(c => c.MeetsCondition(r)))
                                .ToArray();
                        processContainer.Controller.UpdateLevel2Progress(2, 4, string.Format("Loading {0} Items", processContainer.Request.ConnectionTwo.Name));
                        var serviceTwoItems =
                            processCompareParams.GetObjects(null, null, processContainer.ServiceTwo)
                                .Select(o => new ObjectRecord(o))
                                .Where(r => processCompareParams.Conditions == null || processCompareParams.Conditions.All(c => c.MeetsCondition(r)))
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

        private void AddRequiredParentFields(IEnumerable<IRecord> records, XrmRecordService xrmRecordService)
        {
            if (records.Any())
            {
                foreach(var record in records)
                {
                    var typeConfig = XrmTypeConfigs.GetFor(record.Type);
                    var requiredParentFields = XrmTypeConfigs.GetParentFieldsRequiredForComparison(record.Type);
                    if(typeConfig != null && requiredParentFields != null)
                    {
                        var parentId = record.GetLookupId(typeConfig.ParentLookupField);
                        var parentType = record.GetLookupType(typeConfig.ParentLookupField);
                        if(parentId != null && parentType != null)
                        {
                            var parent = xrmRecordService.Get(parentType, parentId);
                            foreach (var parentField in requiredParentFields)
                            {
                                record[typeConfig.ParentLookupField + "." + parentField] = parent.GetField(parentField);
                            }
                        }
                    }
                }
            }
        }

        private static string GetParentReference(ProcessCompareParams parentCompareParams, IRecord parentRecord, string overrideParentId)
        {
            if (parentCompareParams.Type == ProcessCompareType.Objects)
                return parentRecord.GetStringField(parentCompareParams.MatchField);
            else
            {
                if(overrideParentId != null)
                {
                    var otherId = parentRecord.GetField(overrideParentId);
                    if (otherId != null)
                    {
                        if (otherId is Lookup)
                            return ((Lookup)otherId).Id;
                        else
                            return otherId.ToString();
                    }
                }
                return parentRecord.Id;
            }
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

            var typeConfig = XrmTypeConfigs.GetFor(processCompareParams.RecordType);

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
                    if(matches.Count() > 1)
                    {
                        var displayName = GetItemDisplayName(item, processContainer.ServiceOne, processCompareParams, false);
                        foreach (var duplicate in matches.Skip(1))
                        {
                            //activity feeds use a weird pattern where they have multiple registrations
                            //and the config differentiates them so I'll just ignore them for simplicity
                            if ((displayName + "").StartsWith("ActivityFeeds"))
                                continue;
                            //surveys have a similar thing
                            //but theirs differentiates by filtering attributes
                            if ((displayName + "").StartsWith("Microsoft.Crm.Surveys"))
                                continue;
                            
                            var displayName2 = GetItemDisplayName(duplicate, processContainer.ServiceTwo, processCompareParams, true);
                            processContainer.AddDifference(processCompareParams.Context, processCompareParams.RecordType, displayName
                                , "Duplicate match. Only the first will be compared", displayName, displayName2, item.Id, duplicate.Id);
                        }
                    }
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
                    var displayName = GetItemDisplayName(item, processContainer.ServiceOne, processCompareParams, false);
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
                    var displayName = GetItemDisplayName(item, processContainer.ServiceTwo, processCompareParams, true);
                    processContainer.AddDifference(processCompareParams.Context, processCompareParams.RecordType,
                        displayName, "Only In " + processContainer.Request.ConnectionTwo.Name, null, displayName, null, item.Id, parentReference: parentReference, parentId2: parentId);
                }
            }

            //this part creates a dictionary mapping the matched records
            //it is used to compare associations for the records
            if(processContainer.Request.Data
                && processContainer.Request.DataComparisons != null
                && processContainer.Request.DataComparisons.Select(dc => dc.Type).Contains(processCompareParams.RecordType))
            {
                if(!processContainer.MatchedRecordDictionary.ContainsKey(processCompareParams.RecordType))
                {
                    processContainer.MatchedRecordDictionary.Add(processCompareParams.RecordType, new Dictionary<IRecord, IRecord>());
                    foreach(var item in thisInBoth)
                    {
                        if(!processContainer.MatchedRecordDictionary[processCompareParams.RecordType].ContainsKey(item.First()))
                        {
                            processContainer.MatchedRecordDictionary[processCompareParams.RecordType][item.First()] = item.Last();
                        }
                    }
                }
            }

            //differences
            foreach (var item in thisInBoth)
            {
                if (item.First().Type == Entities.solution && !processContainer.Request.Solutions)
                    continue;
                var displayName1 = GetItemDisplayName(item.First(), processContainer.ServiceOne, processCompareParams, false);
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
                            //okay for difference if it is a string we only really want to display part of string which is different
                            var tempDisplayValue1 = GetDifferenceDisplayPartForValue1(displayValue1, displayValue2);
                            var tempDisplayValue2 = GetDifferenceDisplayPartForValue1(displayValue2, displayValue1);
                            displayValue1 = tempDisplayValue1;
                            displayValue2 = tempDisplayValue2;
                        }
                        var parentReference = parent1 == null ? null : parent1.GetStringField(parentCompareParams.MatchField);
                        var parentId1 = parent1 == null ? null : GetParentId(parentCompareParams, parent1);
                        var parentId2 = parent2 == null ? null : GetParentId(parentCompareParams, parent2);
                        processContainer.AddDifference(processCompareParams.Context, processCompareParams.RecordType,
                            displayName1,
                            "Different " + fieldLabel, displayValue1, displayValue2, item.First().Id, item.Last().Id, parentReference: parentReference, parentId1: parentId1, parentId2: parentId2);
                    }
                }
                if(processCompareParams.RecordType == Entities.adx_webfile)
                {
                    //for adx web files lets get the latest attachment for each and comare the docuemnt body field as well
                    var query = new QueryDefinition(Entities.annotation);
                    query.Fields = new[] { Fields.annotation_.documentbody };
                    query.Sorts.Add(new SortExpression(Fields.annotation_.modifiedon, SortType.Descending));
                    query.Top = 1;
                    query.RootFilter.AddCondition(Fields.annotation_.objectid, ConditionType.Equal, item.First().Id);
                    var notes1 = processContainer.ServiceOne.RetreiveAll(query);
                    query.RootFilter.Conditions.First().Value = item.Last().Id;
                    var notes2 = processContainer.ServiceTwo.RetreiveAll(query);
                    var documentBody1 = notes1.Any() ? notes1.First().GetStringField(Fields.annotation_.documentbody) : null;
                    var documentBody2 = notes2.Any() ? notes2.First().GetStringField(Fields.annotation_.documentbody) : null;
                    if (!FieldsEqual(documentBody1, documentBody2))
                    {
                        documentBody1 = "" + processCompareParams.ConvertField1(Fields.annotation_.documentbody, documentBody1);
                        documentBody2 = "" + processCompareParams.ConvertField2(Fields.annotation_.documentbody, documentBody2);
                        var documentBodyDisplay1 = GetDifferenceDisplayPartForValue1(documentBody1, documentBody2);
                        var documentBodyDisplay2 = GetDifferenceDisplayPartForValue1(documentBody2, documentBody1);
                        processContainer.AddDifference(processCompareParams.Context, processCompareParams.RecordType,
                        displayName1, "Different File Content In Latest Modified Note",
                        documentBodyDisplay1, documentBodyDisplay2, item.First().Id, item.Last().Id);
                    }
                }
            }
            return thisInBoth;
        }

        private static string GetDifferenceDisplayPartForValue1(string value1, string value2)
        {
            if (value1 != null)
            {
                var charsToDisplay = MaxCharsToDisplayInValue;
                if (value2 == null)
                    value1 = value1.Left(charsToDisplay) + (value1.Length > charsToDisplay ? "..." : "");
                else
                {
                    //https://stackoverflow.com/questions/4585939/comparing-strings-and-get-the-first-place-where-they-vary-from-eachother
                    var indexOfDiff = value1.Zip(value2, (c1, c2) => c1 == c2).TakeWhile(b => b).Count() + 1;
                    var startIndex = indexOfDiff - 10;
                    if (startIndex < 0)
                        startIndex = 0;

                    value1 = (startIndex > 0 ? "..." : "") + value1.Substring(startIndex).Left(charsToDisplay) + ((startIndex + charsToDisplay) < value1.Length ? "..." : "");
                }
            }
            return value1;
        }

        private static int MaxCharsToDisplayInValue => 250;

        public static object GetItemDisplayName(IRecord item, XrmRecordService xrmRecordService, ProcessCompareParams processCompareParams, bool is2)
        {
            var config = XrmTypeConfigs.GetFor(item.Type);
            if(config == null)
            {
                return is2
                    ? processCompareParams.ConvertField2(processCompareParams.DisplayField, item.GetStringField(processCompareParams.DisplayField))
                    : processCompareParams.ConvertField1(processCompareParams.DisplayField, item.GetStringField(processCompareParams.DisplayField));
            }

            var displayStrings = new List<string>();
            if (config.ParentLookupField != null)
            {
                var thisOne = xrmRecordService.GetFieldAsDisplayString(item, config.ParentLookupField);
                if (!string.IsNullOrWhiteSpace(thisOne))
                    displayStrings.Add(thisOne);
            }
            if (config.UniqueChildFields != null)
            {
                foreach (var unique in (config.UniqueChildFields))
                {
                    var thisOne = xrmRecordService.GetFieldAsDisplayString(item, unique);
                    if (!string.IsNullOrWhiteSpace(thisOne))
                        displayStrings.Add(thisOne);
                }
            }
            if(!displayStrings.Any())
                displayStrings.Add(item.GetStringField(xrmRecordService.GetPrimaryField(item.Type)));
            return string.Join(".", displayStrings);
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
            else if(field1 is Lookup && field2 is Lookup)
            {
                //for lookups in comparison lets just use display names
                //in case the ids are not consistent
                return ((Lookup)field1).Name == ((Lookup)field2).Name;
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
            private object r;

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

            public ProcessCompareParams(InstanceComparerRequest.InstanceCompareDataCompare dataComparison, XrmRecordService recordService)
                : this("Data - " + dataComparison.Type,
                      dataComparison.Type,
                      XrmTypeConfigs.GetComparisonFieldsFor(dataComparison.Type, recordService),
                      recordService.GetPrimaryField(dataComparison.Type),
                      new Condition[0],
                      recordService
                            .GetFields(dataComparison.Type)
                            .Where(f => recordService.GetFieldMetadata(f, dataComparison.Type).IsCustomField)
                            .ToArray(),
                      null,
                      null
                      )
            {
            }

            public ParentLinkType? ParentLinkType { get; set; }
            public string ParentLinkProperty { get; private set; }
            public string OverrideParentId { get; internal set; }

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

            public XrmRecordService ServiceOne { get; set; }

            public XrmRecordService ServiceTwo { get; set; }
            public List<ProcessCompareParams> Comparisons { get; set; }
            public List<InstanceComparerDifference> Differences { get; set; }
            public Dictionary<string, IEnumerable<IRecord>> MissingManagedSolutionComponents { get; private set; }
            public Dictionary<string, Dictionary<IRecord, IRecord>> MatchedRecordDictionary { get; internal set; }

            public ProcessContainer(InstanceComparerRequest request, InstanceComparerResponse response,
                LogController controller)
            {
                Request = request;
                Response = response;
                Controller = controller;
                ServiceOne = new XrmRecordService(request.ConnectionOne);
                ServiceOne.SetFormService(new XrmFormService());
                ServiceTwo = new XrmRecordService(request.ConnectionTwo);
                ServiceTwo.SetFormService(new XrmFormService());
                Response.ServiceOne = ServiceOne;
                Response.ServiceTwo = ServiceTwo;
                Differences = new List<InstanceComparerDifference>();
                Comparisons = new List<ProcessCompareParams>();
                MissingManagedSolutionComponents = new Dictionary<string, IEnumerable<IRecord>>();
                MatchedRecordDictionary = new Dictionary<string, Dictionary<IRecord, IRecord>>();
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
                else if (linkRecordType == Relationships.appmodule_.appmoduleroles_association.EntityName
                    || linkRecordType == Entities.appmodulecomponent)
                {
                    linkRecordType = Entities.appmodule;
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
                
                //this part generating ids and types for solution components which may be used in add differences to solution dialog
                int? componentTypeForSolution = null;
                string idForSolution1 = null;
                string idForSolution2 = null;
                if (recordType == typeof(IRecordTypeMetadata).AssemblyQualifiedName)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.Entity;
                    idForSolution1 = id1;
                    idForSolution2 = id2;
                }
                if (recordType == typeof(IFieldMetadata).AssemblyQualifiedName)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.Entity;
                    idForSolution1 = parentId1;
                    idForSolution2 = parentId2;
                }
                if (recordType == typeof(IMany2ManyRelationshipMetadata).AssemblyQualifiedName)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.Entity;
                    idForSolution1 = parentId1;
                    idForSolution2 = parentId2;
                }
                if (recordType == Entities.savedquery)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.Entity;
                    idForSolution1 = parentId1;
                    idForSolution2 = parentId2;
                }

                if (recordType == Entities.workflow)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.Workflow;
                    idForSolution1 = id1;
                    idForSolution2 = id2;
                }
                if (recordType == Entities.webresource)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.WebResource;
                    idForSolution1 = id1;
                    idForSolution2 = id2;
                }
                if (recordType == typeof(PicklistOption).AssemblyQualifiedName)
                {
                    if (type.Contains("Shared"))
                    {
                        componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.OptionSet;
                        idForSolution1 = parentId1;
                        idForSolution2 = parentId2;
                    }
                    else
                    {
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
                        var parentRecordType = matchingFieldMetadata1?.RecordType ?? matchingFieldMetadata2?.RecordType;
                        var entityId = matchingFieldMetadata1 == null
                            ? ServiceTwo.GetRecordTypeMetadata(parentRecordType).MetadataId
                            : ServiceOne.GetRecordTypeMetadata(parentRecordType).MetadataId;
                        componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.Entity;
                        idForSolution1 = matchingFieldMetadata1 == null ? null : ServiceOne.GetRecordTypeMetadata(parentRecordType).MetadataId;
                        idForSolution2 = matchingFieldMetadata2 == null ? null : ServiceTwo.GetRecordTypeMetadata(parentRecordType).MetadataId;
                    }
                }
                if (recordType == typeof(IPicklistSet).AssemblyQualifiedName)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.OptionSet;
                    idForSolution1 = id1;
                    idForSolution2 = id2;
                }
                if (recordType == Relationships.role_.roleprivileges_association.EntityName)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.Role;
                    idForSolution1 = parentId1;
                    idForSolution2 = parentId2;
                }
                if (recordType == Entities.template)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.EmailTemplate;
                    idForSolution1 = id1;
                    idForSolution2 = id2;
                }
                if (recordType == Entities.role)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.Role;
                    idForSolution1 = id1;
                    idForSolution2 = id2;
                }
                if (recordType == Entities.systemform)
                {
                    if (parentReference != null)
                    {
                        componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.Entity;
                        idForSolution1 = parentId1;
                        idForSolution2 = parentId2;
                    }
                    else
                    {
                        componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.SystemForm;
                        idForSolution1 = id1;
                        idForSolution2 = id2;
                    }
                }
                if (recordType == Entities.report)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.Report;
                    idForSolution1 = id1;
                    idForSolution2 = id2;
                }
                if (recordType == Entities.pluginassembly)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.PluginAssembly;
                    idForSolution1 = id1;
                    idForSolution2 = id2;
                }
                if (recordType == Entities.sdkmessageprocessingstep)
                {
                    componentTypeForSolution = OptionSets.SolutionComponent.ObjectTypeCode.SDKMessageProcessingStep;
                    idForSolution1 = id1;
                    idForSolution2 = id2;
                }

                Differences.Add(new InstanceComparerDifference(type, name == null ? null : name.ToString(), difference, parentReference, value1 == null ? null : value1.ToString(), value2 == null ? null : value2.ToString(), url1, url2, id1, id2, componentTypeForSolution, idForSolution1, idForSolution2));
            }

            internal bool ProcessIfManagedComponentExclude(ProcessCompareParams processCompareParams, IRecord item, bool isInConnection1)
            {
                //okay if we have a managed solution missing in one environment
                //then lets exclude components in that solution in the list because it just creates irrelevant noise in the output

                //firstly if this is a solution we need to capture it in this record
                if(processCompareParams.RecordType == Entities.solution)
                {
                    if (item.GetBoolField(Fields.solution_.ismanaged))
                    {
                        if (!MissingManagedSolutionComponents.ContainsKey(item.Id))
                        {
                            var componentsInSolution = isInConnection1
                                ? ServiceOne.GetLinkedRecords(Entities.solutioncomponent, Entities.solution, Fields.solutioncomponent_.solutionid, item.Id)
                                : ServiceTwo.GetLinkedRecords(Entities.solutioncomponent, Entities.solution, Fields.solutioncomponent_.solutionid, item.Id);
                            MissingManagedSolutionComponents.Add(item.Id, componentsInSolution);
                        }
                    }
                    //if we aren't actually including solution differences return not to add the difference
                    if (!Request.Solutions)
                        return true;
                }
                if (!Request.IgnoreMissingManagedComponentDifferences)
                    return false;
                //okay otherwise if a solution component configured then check if this is a part of the managed solution
                else if (processCompareParams.SolutionComponentConfiguration != null)
                {
                    foreach (var solution in MissingManagedSolutionComponents)
                    {
                        foreach (var component in solution.Value)
                        {
                            if (component.GetIdField(Fields.solutioncomponent_.objectid) == item.GetIdField(processCompareParams.SolutionComponentConfiguration.MetadataIdFieldName)
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
