using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.CustomisationExporter.Type;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Spreadsheet;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JosephM.CustomisationExporter
{
    public class CustomisationExporterService :
        ServiceBase<CustomisationExporterRequest, CustomisationExporterResponse, CustomisationExporterResponseItem>
    {
        public CustomisationExporterService(XrmRecordService service)
        {
            Service = service;
        }

        private XrmRecordService Service { get; set; }

        public override void ExecuteExtention(CustomisationExporterRequest request,
            CustomisationExporterResponse response,
            ServiceRequestController controller)
        {
            response.Folder = request.SaveToFolder.FolderPath;
            controller.LogLiteral("Loading Metadata");

            ProcessForEntities(request, response, controller.Controller);

            if ((request.Fields || request.FieldOptionSets))
            {
                Service.LoadFieldsForAllEntities(controller.Controller);
            }
            if ((request.Fields || request.Relationships))
            {
                Service.LoadRelationshipsForAllEntities(controller.Controller);
            }

            //add progress counts

            ProcessForFields(request, response, controller.Controller);
            ProcessForRelationships(request, response, controller.Controller);
            ProcessForOptionSets(request, response, controller.Controller);

            var remainingToDo = new Dictionary<string, Action<CustomisationExporterRequest, CustomisationExporterResponse, LogController>>
            {
                { nameof(CustomisationExporterRequest.Solutions), ProcessForSolutions },
                { nameof(CustomisationExporterRequest.Workflows), ProcessForWorkflows },
                { nameof(CustomisationExporterRequest.PluginAssemblies), ProcessForPluginAssemblies },
                { nameof(CustomisationExporterRequest.PluginTriggers), ProcessForPluginTriggers },
                { nameof(CustomisationExporterRequest.SecurityRoles), ProcessForSecurityRoles },
                { nameof(CustomisationExporterRequest.SecurityRolesPrivileges), ProcessForRolePrivileges },
                { nameof(CustomisationExporterRequest.FieldSecurityProfiles), ProcessForFieldSecurityProfiles },
                { nameof(CustomisationExporterRequest.Users), ProcessForUsers },
                { nameof(CustomisationExporterRequest.Teams), ProcessForTeams },
                { nameof(CustomisationExporterRequest.Reports), ProcessForReports },
                { nameof(CustomisationExporterRequest.WebResources), ProcessForWebResources },
                { nameof(CustomisationExporterRequest.FormsAndDashboards), ProcessForFormsAndDashboards },
            }
            .Where(kv => (bool)request.GetPropertyValue(kv.Key));
            var i = 1;
            foreach (var todo in remainingToDo)
            {
                controller.Controller.UpdateProgress(i++, remainingToDo.Count(), "Exporting " + typeof(CustomisationExporterRequest).GetPropertyDisplayName(todo.Key));
                todo.Value(request, response, controller.Controller);
            }

            response.Folder = request.SaveToFolder.FolderPath;

            var excelFileName = "Customisation Export " + DateTime.Now.ToFileTime() + ".xlsx";
            ExcelUtility.CreateXlsx(request.SaveToFolder.FolderPath, excelFileName, response.GetListsToOutput());
            response.ExcelFileName = excelFileName;

            response.Message = "The Export is Complete";
        }

        private void ProcessForFormsAndDashboards(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.systemform);
                var fields = new List<string>(new[]
                {
                    Fields.systemform_.type,
                    Fields.systemform_.objecttypecode,
                    Fields.systemform_.name,
                    Fields.systemform_.description,
                });
                if(Service.FieldExists(Fields.systemform_.formactivationstate, Entities.systemform))
                {
                    fields.Add(Fields.systemform_.formactivationstate);
                }
                query.Fields = fields;
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.systemform_.type, SortType.Ascending),
                        new SortExpression(Fields.systemform_.objecttypecode, SortType.Ascending),
                        new SortExpression(Fields.systemform_.name, SortType.Ascending),
                    };

                var records = Service.RetreiveAll(query);

                var objects = records.Select(r => new FormOrDashboardExport(
                        Service.GetFieldAsDisplayString(r, Fields.systemform_.type),
                        Service.GetFieldAsDisplayString(r, Fields.systemform_.objecttypecode),
                        Service.GetFieldAsDisplayString(r, Fields.systemform_.name),
                        Service.GetFieldAsDisplayString(r, Fields.systemform_.description),
                        Service.GetFieldAsDisplayString(r, Fields.systemform_.formactivationstate))).ToList();

                response.AddListToOutput("Forms and Dashboards", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Forms and Dashboards", null, ex));
            }
        }

        private void ProcessForPluginTriggers(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.sdkmessageprocessingstep);
                query.RootFilter = new Filter()
                {
                    Conditions = new List<Condition>
                         {
                             new Condition(Fields.sdkmessageprocessingstep_.ishidden, ConditionType.NotEqual, true)
                         }
                };
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.sdkmessageprocessingstep_.name, SortType.Ascending),
                    };
                var pluginTypeJoin = new Join(Fields.sdkmessageprocessingstep_.plugintypeid, Entities.plugintype, Fields.plugintype_.plugintypeid);
                query.Joins.Add(pluginTypeJoin);
                pluginTypeJoin.JoinType = JoinType.LeftOuter;
                pluginTypeJoin.Alias = "PT";
                pluginTypeJoin.Fields = new[] { Fields.plugintype_.assemblyname };
                var pluginFilterJoin = new Join(Fields.sdkmessageprocessingstep_.sdkmessagefilterid, Entities.sdkmessagefilter, Fields.sdkmessagefilter_.sdkmessagefilterid);
                query.Joins.Add(pluginFilterJoin);
                pluginFilterJoin.JoinType = JoinType.LeftOuter;
                pluginFilterJoin.Alias = "PF";
                pluginFilterJoin.Fields = new[] { Fields.sdkmessagefilter_.primaryobjecttypecode };

                var records = Service.RetreiveAll(query);

                var objects = records.Select(r => new PluginTriggerExport(
                        Service.GetFieldAsDisplayString(r, Fields.sdkmessageprocessingstep_.name),
                        Service.GetFieldAsDisplayString(Entities.plugintype, Fields.plugintype_.assemblyname, r.GetField("PT." + Fields.plugintype_.assemblyname)),
                        Service.GetFieldAsDisplayString(Entities.sdkmessagefilter, Fields.sdkmessagefilter_.primaryobjecttypecode, r.GetField("PF." + Fields.sdkmessagefilter_.primaryobjecttypecode)),
                        Service.GetFieldAsDisplayString(r, Fields.sdkmessageprocessingstep_.stage),
                        Service.GetFieldAsDisplayString(r, Fields.sdkmessageprocessingstep_.mode),
                        Service.GetFieldAsDisplayString(r, Fields.sdkmessageprocessingstep_.filteringattributes),
                        Service.GetFieldAsDisplayString(r, Fields.sdkmessageprocessingstep_.impersonatinguserid),
                        Service.GetFieldAsDisplayString(r, Fields.sdkmessageprocessingstep_.description),
                        Service.GetFieldAsDisplayString(r, Fields.sdkmessageprocessingstep_.supporteddeployment),
                        Service.GetFieldAsDisplayString(r, Fields.sdkmessageprocessingstep_.statecode),
                        Service.GetFieldAsDisplayString(r, Fields.sdkmessageprocessingstep_.modifiedby),
                        r.GetDateTime(Fields.sdkmessageprocessingstep_.modifiedon) ?? DateTime.Now)).ToList();

                response.AddListToOutput("Plugin Triggers", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Plugin Triggers", null, ex));
            }
        }

        private void ProcessForWebResources(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.webresource);
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.webresource_.name, SortType.Ascending),
                    };
                query.Fields = new[]
                {
                        Fields.webresource_.name,
                        Fields.webresource_.displayname,
                        Fields.webresource_.webresourcetype,
                        Fields.webresource_.description,
                        Fields.webresource_.modifiedby,
                        Fields.webresource_.modifiedon,
                    };

                var records = Service.RetreiveAll(query);

                var objects = records.Select(r => new WebResourcesExport(
                        Service.GetFieldAsDisplayString(r, Fields.webresource_.name),
                        Service.GetFieldAsDisplayString(r, Fields.webresource_.displayname),
                        Service.GetFieldAsDisplayString(r, Fields.webresource_.webresourcetype),
                        Service.GetFieldAsDisplayString(r, Fields.webresource_.description),
                        Service.GetFieldAsDisplayString(r, Fields.webresource_.modifiedby),
                        r.GetDateTime(Fields.webresource_.modifiedon) ?? DateTime.Now)).ToList();

                response.AddListToOutput("Web Resources", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Web Resources", null, ex));
            }
        }

        private void ProcessForReports(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.report);
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.report_.name, SortType.Ascending),
                    };

                var records = Service.RetreiveAll(query);

                var objects = records.Select(r => new ReportExport(
                        Service.GetFieldAsDisplayString(r, Fields.report_.name),
                        Service.GetFieldAsDisplayString(r, Fields.report_.description),
                        Service.GetFieldAsDisplayString(r, Fields.report_.iscustomreport),
                        Service.GetFieldAsDisplayString(r, Fields.report_.ispersonal),
                        Service.GetFieldAsDisplayString(r, Fields.report_.reporttypecode),
                        Service.GetFieldAsDisplayString(r, Fields.report_.modifiedby),
                        r.GetDateTime(Fields.report_.modifiedon) ?? DateTime.Now)).ToList();

                response.AddListToOutput("Reports", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Reports", null, ex));
            }
        }

        private void ProcessForTeams(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            try
            {
                var conditions = new List<Condition>();
                if(Service.FieldExists(Fields.team_.teamtype, Entities.team))
                {
                    conditions.Add(new Condition(Fields.team_.teamtype, ConditionType.NotEqual, OptionSets.Team.TeamType.Access));
                }

                var query = new QueryDefinition(Entities.team);
                query.RootFilter = new Filter()
                {
                    Conditions = conditions
                };
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.team_.name, SortType.Ascending),
                    };

                var records = Service.RetreiveAll(query);

                var roles = Service.IndexAssociatedRecords(Relationships.team_.teamroles_association.EntityName, Fields.team_.teamid, Fields.role_.roleid, Entities.role);
                var users = Service.IndexAssociatedRecords(Relationships.team_.teammembership_association.EntityName, Fields.team_.teamid, Fields.systemuser_.systemuserid, Entities.systemuser);
                var fieldSecurities = Service.IndexAssociatedRecords(Relationships.team_.teamprofiles_association.EntityName, Fields.team_.teamid, Fields.fieldsecurityprofile_.fieldsecurityprofileid, Entities.fieldsecurityprofile);

                var objects = records.Select(t => new TeamExport(
                        Service.GetFieldAsDisplayString(t, Fields.team_.name),
                        Service.GetFieldAsDisplayString(t, Fields.team_.teamtype),
                        !users.ContainsKey(t.Id) ? 0 : users[t.Id].Count(),
                        !users.ContainsKey(t.Id) ? null : string.Join(",", users[t.Id].Select(u => u.GetStringField(Fields.systemuser_.fullname)).OrderBy(s => s)),
                        !roles.ContainsKey(t.Id) ? 0 : roles[t.Id].Count(),
                        !roles.ContainsKey(t.Id) ? null : string.Join(",", roles[t.Id].Select(r => r.GetStringField(Fields.role_.name)).OrderBy(s => s)),
                        !fieldSecurities.ContainsKey(t.Id) ? 0 : fieldSecurities[t.Id].Count(),
                        !fieldSecurities.ContainsKey(t.Id) ? null : string.Join(",", fieldSecurities[t.Id].Select(fs => fs.GetStringField(Fields.fieldsecurityprofile_.name)).OrderBy(s => s)))).ToList();

                response.AddListToOutput("Teams", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Teams", null, ex));
            }
        }

        private void ProcessForUsers(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.systemuser);
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.systemuser_.fullname, SortType.Ascending),
                    };

                var records = Service.RetreiveAll(query);

                var roles = Service.IndexAssociatedRecords(Relationships.systemuser_.systemuserroles_association.EntityName, Fields.systemuser_.systemuserid, Fields.role_.roleid, Entities.role);
                var teams = Service.IndexAssociatedRecords(Relationships.systemuser_.teammembership_association.EntityName, Fields.systemuser_.systemuserid, Fields.team_.teamid, Entities.team);
                var fieldSecurities = Service.IndexAssociatedRecords(Relationships.systemuser_.systemuserprofiles_association.EntityName, Fields.systemuser_.systemuserid, Fields.fieldsecurityprofile_.fieldsecurityprofileid, Entities.fieldsecurityprofile);

                var objects = records.Select(u => new UserExport(
                        Service.GetFieldAsDisplayString(u, Fields.systemuser_.fullname),
                        Service.GetFieldAsDisplayString(u, Fields.systemuser_.isdisabled),
                        !roles.ContainsKey(u.Id) ? 0 : roles[u.Id].Count(),
                        !roles.ContainsKey(u.Id) ? null : string.Join(",", roles[u.Id].Select(r => r.GetStringField(Fields.role_.name)).OrderBy(s => s)),
                        !teams.ContainsKey(u.Id) ? 0 : teams[u.Id].Count(),
                        !teams.ContainsKey(u.Id) ? null : string.Join(",", teams[u.Id].Select(t => t.GetStringField(Fields.team_.name)).OrderBy(s => s)),
                        !fieldSecurities.ContainsKey(u.Id) ? 0 : fieldSecurities[u.Id].Count(),
                        !fieldSecurities.ContainsKey(u.Id) ? null : string.Join(",", fieldSecurities[u.Id].Select(fs => fs.GetStringField(Fields.fieldsecurityprofile_.name)).OrderBy(s => s)))).ToList();

                response.AddListToOutput("Users", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Users", null, ex));
            }
        }

        private void ProcessForFieldSecurityProfiles(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.fieldsecurityprofile);
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.fieldsecurityprofile_.name, SortType.Ascending)
                    };

                var records = Service.RetreiveAll(query);

                var users = Service.IndexAssociatedRecords(Relationships.fieldsecurityprofile_.systemuserprofiles_association.EntityName, Fields.fieldsecurityprofile_.fieldsecurityprofileid, Fields.systemuser_.systemuserid, Entities.systemuser);
                var teams = Service.IndexAssociatedRecords(Relationships.fieldsecurityprofile_.teamprofiles_association.EntityName, Fields.fieldsecurityprofile_.fieldsecurityprofileid, Fields.team_.teamid, Entities.team);

                var objects = records.Select(r => new FieldSecurityProfileExport(
                        Service.GetFieldAsDisplayString(r, Fields.fieldsecurityprofile_.name),
                        !teams.ContainsKey(r.Id) ? 0 : teams[r.Id].Count(),
                        !teams.ContainsKey(r.Id) ? null : string.Join(",", teams[r.Id].Select(t => t.GetStringField(Fields.team_.name)).OrderBy(s => s)),
                        !users.ContainsKey(r.Id) ? 0 : users[r.Id].Count(),
                        !users.ContainsKey(r.Id) ? null : string.Join(",", users[r.Id].Select(u => u.GetStringField(Fields.systemuser_.fullname)).OrderBy(s => s)),
                        Service.GetFieldAsDisplayString(r, Fields.fieldsecurityprofile_.modifiedby),
                        r.GetDateTime(Fields.fieldsecurityprofile_.modifiedon) ?? DateTime.Now)).ToList();

                response.AddListToOutput("Field Security Profiles", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Field Security Profiles", null, ex));
            }
        }

        private void ProcessForSecurityRoles(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.role);
                query.RootFilter = new Filter()
                {
                    Conditions = new List<Condition>
                         {
                             new Condition(Fields.role_.parentroleid, ConditionType.Null)
                         }
                };
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.role_.name, SortType.Ascending)
                    };

                var records = Service.RetreiveAll(query);

                var users = Service.IndexAssociatedRecords(Relationships.role_.systemuserroles_association.EntityName, Fields.role_.roleid, Fields.systemuser_.systemuserid, Entities.systemuser);
                var teams = Service.IndexAssociatedRecords(Relationships.role_.teamroles_association.EntityName, Fields.role_.roleid, Fields.team_.teamid, Entities.team);

                var objects = records.Select(r => new SecurityRoleExport(
                        Service.GetFieldAsDisplayString(r, Fields.role_.name),
                        Service.GetFieldAsDisplayString(r, Fields.role_.businessunitid),
                        !users.ContainsKey(r.Id) ? 0 : users[r.Id].Count(),
                        !users.ContainsKey(r.Id) ? null : string.Join(",", users[r.Id].Select(u => u.GetStringField(Fields.systemuser_.fullname)).OrderBy(s => s)),
                        !teams.ContainsKey(r.Id) ? 0 : teams[r.Id].Count(),
                        !teams.ContainsKey(r.Id) ? null : string.Join(",", teams[r.Id].Select(t => t.GetStringField(Fields.team_.name)).OrderBy(s => s)),
                        Service.GetFieldAsDisplayString(r, Fields.role_.modifiedby),
                        r.GetDateTime(Fields.role_.modifiedon) ?? DateTime.Now)).ToList();

                response.AddListToOutput("Security Roles", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Security Roles", null, ex));
            }
        }

        private void ProcessForRolePrivileges(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.role);
                var joinAssocation = new Join(Fields.role_.roleid, Relationships.role_.roleprivileges_association.EntityName, Fields.role_.roleid);
                joinAssocation.Alias = "A";
                joinAssocation.Fields = new[] { "privilegedepthmask" };
                query.Joins = new List<Join> { joinAssocation };
                var joinPrivilege = new Join(Fields.privilege_.privilegeid, Entities.privilege, Fields.privilege_.privilegeid);
                joinAssocation.Joins = new List<Join> { joinPrivilege };
                joinPrivilege.Alias = "P";
                joinPrivilege.Fields = new[] { Fields.privilege_.name, Fields.privilege_.accessright };
                var joinPrivilegeEntity = new Join(Fields.privilege_.privilegeid, Entities.privilegeobjecttypecodes, Fields.privilegeobjecttypecodes_.privilegeid);
                joinPrivilegeEntity.JoinType = JoinType.LeftOuter;
                joinPrivilege.Joins = new List<Join> { joinPrivilegeEntity };
                joinPrivilegeEntity.Alias = "PE";
                joinPrivilegeEntity.Fields = new[] { Fields.privilegeobjecttypecodes_.objecttypecode };

                var results = Service.RetreiveAll(query);

                var objects = results
                    .Select(r => new PrivilegeExport(

                        r.GetStringField(Fields.role_.name),
                        r.GetIntegerField("A.privilegedepthmask"),
                        r.GetStringField("P." + Fields.privilege_.name),
                        r.GetIntegerField("P." + Fields.privilege_.accessright),
                        r.GetStringField("PE." + Fields.privilegeobjecttypecodes_.objecttypecode)))
                        .ToList();

                response.AddListToOutput("Role Privileges", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Role Privilegess", null, ex));
            }
        }

        private void ProcessForPluginAssemblies(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.pluginassembly);
                query.Fields = new[]
                {
                    Fields.pluginassembly_.pluginassemblyid,
                    Fields.pluginassembly_.ishidden,
                    Fields.pluginassembly_.name,
                    Fields.pluginassembly_.isolationmode,
                    Fields.pluginassembly_.sourcetype,
                    Fields.pluginassembly_.version,
                    Fields.pluginassembly_.modifiedby,
                    Fields.pluginassembly_.modifiedon
                };
                query.RootFilter = new Filter()
                {
                    Conditions = new List<Condition>
                         {
                             new Condition(Fields.pluginassembly_.ishidden, ConditionType.NotEqual, true)
                         }
                };
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.pluginassembly_.modifiedon, SortType.Descending)
                    };

                var records = Service.RetreiveAll(query);

                var objects = records.Select(r => new PluginAssemblyExport(
                        Service.GetFieldAsDisplayString(r, Fields.pluginassembly_.name),
                        Service.GetFieldAsDisplayString(r, Fields.pluginassembly_.isolationmode),
                        Service.GetFieldAsDisplayString(r, Fields.pluginassembly_.sourcetype),
                        Service.GetFieldAsDisplayString(r, Fields.pluginassembly_.version),
                        Service.GetFieldAsDisplayString(r, Fields.pluginassembly_.modifiedby),
                        r.GetDateTime(Fields.pluginassembly_.modifiedon) ?? DateTime.Now)).ToList();

                response.AddListToOutput("Plugin Assemblies", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Plugin Assemblies", null, ex));
            }
        }

        private void ProcessForSolutions(CustomisationExporterRequest request,
            CustomisationExporterResponse response,
            LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.solution);
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.solution_.modifiedon, SortType.Descending)
                    };

                var records = Service.RetreiveAll(query);

                var objects = records.Select(r => new SolutionExport(
                        Service.GetFieldAsDisplayString(r, Fields.solution_.ismanaged),
                        Service.GetFieldAsDisplayString(r, Fields.solution_.friendlyname),
                        Service.GetFieldAsDisplayString(r, Fields.solution_.version),
                        Service.GetFieldAsDisplayString(r, Fields.solution_.publisherid),
                        Service.GetFieldAsDisplayString(r, Fields.solution_.description),
                        Service.GetFieldAsDisplayString(r, Fields.solution_.modifiedby),
                        r.GetDateTime(Fields.solution_.modifiedon) ?? DateTime.Now)).ToList();

                response.AddListToOutput("Solutions", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Solutions", null, ex));
            }
        }

        private void ProcessForWorkflows(CustomisationExporterRequest request,
            CustomisationExporterResponse response,
            LogController controller)
        {
            try
            {
                var query = new QueryDefinition(Entities.workflow);
                var fields = new List<string>();
                foreach(var field in new []
                    {
                        Fields.workflow_.statecode,
                        Fields.workflow_.category,
                        Fields.workflow_.name,
                        Fields.workflow_.primaryentity,
                        Fields.workflow_.ownerid,
                        Fields.workflow_.mode,
                        Fields.workflow_.scope,
                        Fields.workflow_.runas,
                        Fields.workflow_.ondemand,
                        Fields.workflow_.subprocess,
                        Fields.workflow_.triggeroncreate,
                        Fields.workflow_.triggeronupdateattributelist,
                        Fields.workflow_.updatestage,
                        Fields.workflow_.triggerondelete,
                        Fields.workflow_.deletestage,
                        Fields.workflow_.istransacted,
                        Fields.workflow_.syncworkflowlogonfailure,
                        Fields.workflow_.asyncautodelete,
                        Fields.workflow_.businessprocesstype
                    })
                {
                    if (Service.FieldExists(field, Entities.workflow))
                    {
                        fields.Add(field);
                    }
                }

                query.Fields = fields;
                query.RootFilter = new Filter()
                {
                    Conditions = new List<Condition>
                         {
                             new Condition(Fields.workflow_.type, ConditionType.Equal, OptionSets.Process.Type.Definition)
                         }
                };
                query.Sorts = new List<SortExpression>
                    {
                        new SortExpression(Fields.workflow_.name, SortType.Ascending)
                    };

                var records = Service.RetreiveAll(query);

                var objects = records.Select(r => new WorkflowExport(
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.statecode),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.category),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.name),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.primaryentity),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.ownerid),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.mode),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.scope),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.runas),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.ondemand),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.subprocess),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.triggeroncreate),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.triggeronupdateattributelist),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.updatestage),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.triggerondelete),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.deletestage),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.istransacted),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.syncworkflowlogonfailure),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.asyncautodelete),
                        Service.GetFieldAsDisplayString(r, Fields.workflow_.businessprocesstype))).ToList();

                response.AddListToOutput("Workflows", objects);
            }
            catch (Exception ex)
            {
                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Workflows", null, ex));
            }
        }

        private void ProcessForRelationships(CustomisationExporterRequest request,
            CustomisationExporterResponse response,
            LogController controller)
        {
            if (request.Relationships)
            {
                var allRelationship = new List<RelationshipExport>();
                var types = GetRecordTypesToExport(request).OrderBy(t => Service.GetDisplayName(t)).ToArray();
                var count = types.Count();
                var manyToManyDone = new List<string>();
                for (var i = 0; i < count; i++)
                {
                    var thisType = types.ElementAt(i);
                    var thisTypeLabel = Service.GetDisplayName(thisType);
                    controller.UpdateProgress(i, count, "Exporting Relationships");
                    try
                    {
                        var relationships = Service.GetManyToManyRelationships(thisType);
                        for (var j = 0; j < relationships.Count(); j++)
                        {
                            var relationship = relationships.ElementAt(j);
                            try
                            {
                                if (relationship.RecordType1 == thisType
                                    ||
                                    (!request.DuplicateManyToManyRelationshipSides &&
                                     !manyToManyDone.Contains(relationship.SchemaName))
                                    )
                                {
                                    allRelationship.Add(new RelationshipExport(relationship.SchemaName,
                                        relationship.RecordType1, relationship.RecordType2,
                                        relationship.IsCustomRelationship, relationship.RecordType1DisplayRelated,
                                        relationship.RecordType2DisplayRelated
                                        , relationship.Entity1IntersectAttribute, relationship.Entity2IntersectAttribute,
                                        RelationshipExport.RelationshipType.ManyToMany,
                                        relationship.RecordType1UseCustomLabel, relationship.RecordType2UseCustomLabel,
                                        relationship.RecordType1CustomLabel, relationship.RecordType2CustomLabel
                                        , relationship.RecordType1DisplayOrder, relationship.RecordType2DisplayOrder, relationship.MetadataId
                                        , null));
                                    manyToManyDone.Add(relationship.SchemaName);
                                }
                                if (relationship.RecordType2 == thisType
                                    && (request.DuplicateManyToManyRelationshipSides
                                        || (!manyToManyDone.Contains(relationship.SchemaName))))
                                {
                                    allRelationship.Add(new RelationshipExport(relationship.SchemaName,
                                        relationship.RecordType2, relationship.RecordType1,
                                        relationship.IsCustomRelationship, relationship.RecordType2DisplayRelated,
                                        relationship.RecordType1DisplayRelated
                                        , relationship.Entity2IntersectAttribute, relationship.Entity1IntersectAttribute,
                                        RelationshipExport.RelationshipType.ManyToMany,
                                        relationship.RecordType2UseCustomLabel, relationship.RecordType1UseCustomLabel,
                                        relationship.RecordType2CustomLabel, relationship.RecordType1CustomLabel
                                        , relationship.RecordType2DisplayOrder, relationship.RecordType1DisplayOrder
                                        , relationship.MetadataId, null));
                                    manyToManyDone.Add(relationship.SchemaName);
                                }
                            }
                            catch (Exception ex)
                            {
                                response.AddResponseItem(
                                    new CustomisationExporterResponseItem("Error Exporting Relationship",
                                        relationship.SchemaName, ex));
                            }
                        }
                        if (request.IncludeOneToManyRelationships)
                        {
                            var oneTorelationships = Service.GetOneToManyRelationships(thisType);
                            for (var j = 0; j < oneTorelationships.Count(); j++)
                            {
                                var relationship = oneTorelationships.ElementAt(j);
                                try
                                {
                                    var isCustomRelationship = Service.FieldExists(relationship.ReferencingAttribute, relationship.ReferencingEntity)
                                        && Service.GetFieldMetadata(relationship.ReferencingAttribute, relationship.ReferencingEntity).IsCustomField;
                                    allRelationship.Add(new RelationshipExport(relationship.SchemaName,
                                        relationship.ReferencedEntity, relationship.ReferencingEntity,
                                        isCustomRelationship, false,
                                        relationship.DisplayRelated
                                        , null, relationship.ReferencingAttribute,
                                        RelationshipExport.RelationshipType.OneToMany, false, relationship.IsCustomLabel,
                                        null, relationship.GetRelationshipLabel
                                        , 0, relationship.DisplayOrder, relationship.MetadataId, relationship.DeleteCascadeConfiguration));
                                }
                                catch (Exception ex)
                                {
                                    response.AddResponseItem(
                                        new CustomisationExporterResponseItem("Error Exporting Relationship",
                                            relationship.SchemaName, ex));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Relationships",
                            thisType, ex));
                    }
                }
                response.AddListToOutput("Relationships", allRelationship);
            }
        }

        private static IEnumerable<RecordFieldType> OptionSetTypes
        {
            get { return new[] {RecordFieldType.Picklist, RecordFieldType.Status}; }
        }

        private void ProcessForOptionSets(CustomisationExporterRequest request, CustomisationExporterResponse response,
            LogController controller)
        {
            if (!request.FieldOptionSets && !request.SharedOptionSets)
                return;

            var allOptions = new List<OptionExport>();

            if (request.FieldOptionSets)
            {
                var types = GetRecordTypesToExport(request).OrderBy(t => Service.GetDisplayName(t)).ToArray();
                var count = types.Count();
                for (var i = 0; i < count; i++)
                {
                    var thisType = types.ElementAt(i);
                    var thisTypeLabel = Service.GetDisplayName(thisType);
                    controller.UpdateProgress(i, count, "Exporting Field Option Sets");
                    try
                    {
                        var fields =
                            Service.GetFields(thisType)
                                .Where(f => !Service.GetFieldLabel(f, thisType).IsNullOrWhiteSpace())
                                .Where(f => OptionSetTypes.Contains(Service.GetFieldType(f, thisType)))
                                .ToArray();
                        var numberOfFields = fields.Count();
                        for (var j = 0; j < numberOfFields; j++)
                        {
                            var field = fields.ElementAt(j);
                            var fieldLabel = Service.GetFieldLabel(field, thisType);
                            try
                            {
                                var keyValues = Service.GetPicklistKeyValues(field, thisType);
                                foreach (var keyValue in keyValues)
                                {
                                    allOptions.Add(new OptionExport(thisTypeLabel, thisType,
                                        fieldLabel, field, keyValue.Key, keyValue.Value, false, null,
                                        JoinTypeAndFieldName(field, thisType)));
                                }
                            }
                            catch (Exception ex)
                            {
                                response.AddResponseItem(
                                    new CustomisationExporterResponseItem("Error Exporting Options For Field",
                                        field, ex));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(
                            new CustomisationExporterResponseItem("Error Exporting Record Type Options",
                                thisType, ex));
                    }
                }
            }
            if (request.SharedOptionSets)
            {
                var sets = Service.GetSharedPicklists();
                var countSets = sets.Count();
                for (var i = 0; i < countSets; i++)
                {
                    var thisSet = sets.ElementAt(i);
                    controller.UpdateProgress(i, countSets, "Exporting Share Option Sets");
                    try
                    {
                        var options = thisSet.PicklistOptions;
                        var label = thisSet.DisplayName;
                        foreach (var option in options)
                        {
                            allOptions.Add(new OptionExport(null, null,
                                null, null, option.Key, option.Value, true, thisSet.SchemaName, label));
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(
                            new CustomisationExporterResponseItem("Error Exporting Shared Option Set",
                                thisSet.SchemaName, ex));
                    }
                }
            }
            response.AddListToOutput("Option Sets", allOptions);
        }

        private void ProcessForFields(CustomisationExporterRequest request, CustomisationExporterResponse response,
            LogController controller)
        {
            if (request.Fields)
            {
                var allFields = new List<FieldExport>();
                var types = GetRecordTypesToExport(request).OrderBy(t => Service.GetDisplayName(t)).ToArray();
                var count = types.Count();
                for (var i = 0; i < count; i++)
                {
                    var thisType = types.ElementAt(i);
                    var thisTypeLabel = Service.GetDisplayName(thisType);
                    var primaryField = Service.GetPrimaryField(thisType);
                    controller.UpdateProgress(i, count, "Exporting Fields");
                    try
                    {
                        var fields =
                            Service.GetFields(thisType)
                                .Where(f => !Service.GetFieldLabel(f, thisType).IsNullOrWhiteSpace())
                                .ToArray();
                        var numberOfFields = fields.Count();
                        for (var j = 0; j < numberOfFields; j++)
                        {
                            var field = fields.ElementAt(j);
                            var fieldLabel = Service.GetFieldLabel(field, thisType);
                            try
                            {
                                var thisFieldType = Service.GetFieldType(field, thisType);
                                var displayRelated = Service.IsLookup(field, thisType) &&
                                                     Service.GetFieldMetadata(field, thisType).IsDisplayRelated;
                                var picklist = thisFieldType == RecordFieldType.Picklist
                                    ? CreatePicklistName(field, thisType)
                                    : "N/A";
                                string referencedType = "N/A";
                                if (Service.IsLookup(field, thisType))
                                {
                                    referencedType = Service.GetLookupTargetType(field, thisType);
                                }
                                var isString = Service.IsString(field, thisType);
                                int maxLength = isString ? Service.GetMaxLength(field, thisType) : -1;
                                var textFormat = thisFieldType == RecordFieldType.String
                                    ? Service.GetFieldMetadata(field, thisType).TextFormat.ToString()
                                    : null;
                                var includeTime = false;
                                var dateBehaviour = "N/A";
                                var integerFormat = thisFieldType == RecordFieldType.Integer
                                    ? Service.GetFieldMetadata(field, thisType).IntegerFormat.ToString()
                                    : null;
                                var minValue = "-1";
                                var maxValue = "-1";
                                var precision = "-1";
                                if (thisFieldType == RecordFieldType.Date)
                                {
                                    includeTime = Service.GetFieldMetadata(field, thisType).IncludeTime;
                                    dateBehaviour = Service.GetFieldMetadata(field, thisType).DateBehaviour;
                                }
                                if (thisFieldType == RecordFieldType.Decimal)
                                {
                                    minValue =
                                        Service.GetFieldMetadata(field, thisType)
                                            .MinValue.ToString(CultureInfo.InvariantCulture);
                                    maxValue =
                                        Service.GetFieldMetadata(field, thisType)
                                            .MaxValue.ToString(CultureInfo.InvariantCulture);
                                    precision =
                                        Service.GetFieldMetadata(field, thisType)
                                            .DecimalPrecision.ToString(CultureInfo.InvariantCulture);
                                }
                                if (thisFieldType == RecordFieldType.Double)
                                {
                                    minValue =
                                        Service.GetFieldMetadata(field, thisType)
                                            .MinValue.ToString(CultureInfo.InvariantCulture);
                                    maxValue =
                                        Service.GetFieldMetadata(field, thisType)
                                            .MaxValue.ToString(CultureInfo.InvariantCulture);
                                    precision =
                                        Service.GetFieldMetadata(field, thisType)
                                            .DecimalPrecision.ToString(CultureInfo.InvariantCulture);
                                }
                                if (thisFieldType == RecordFieldType.Integer)
                                {
                                    minValue =
                                        Service.GetFieldMetadata(field, thisType)
                                            .MinValue.ToString(CultureInfo.InvariantCulture);
                                    maxValue =
                                        Service.GetFieldMetadata(field, thisType)
                                            .MaxValue.ToString(CultureInfo.InvariantCulture);
                                }
                                if (thisFieldType == RecordFieldType.Money)
                                {
                                    minValue =
                                        Service.GetFieldMetadata(field, thisType)
                                            .MinValue.ToString(CultureInfo.InvariantCulture);
                                    maxValue =
                                        Service.GetFieldMetadata(field, thisType)
                                            .MaxValue.ToString(CultureInfo.InvariantCulture);
                                }
                                var fieldMetadata = Service.GetFieldMetadata(field, thisType);
                                var fieldExport = new FieldExport(thisTypeLabel, thisType,
                                    fieldLabel, field, Service.GetFieldType(field, thisType),
                                    fieldMetadata.IsCustomField,
                                    fieldMetadata.IsMandatory,
                                    fieldMetadata.Description, primaryField == field,
                                    fieldMetadata.Audit,
                                    fieldMetadata.Searchable
                                    , displayRelated, referencedType, maxLength, textFormat, integerFormat, dateBehaviour, includeTime, minValue,
                                    maxValue, precision, picklist, fieldMetadata.MetadataId, fieldMetadata.IsMultiSelect
                                    , fieldMetadata.HasFieldSecurity, fieldMetadata.NavigationProperty);
                                if (Service.IsString(field, thisType))
                                    fieldExport.MaxLength = Service.GetMaxLength(field, thisType);
                                allFields.Add(fieldExport);
                            }
                            catch (Exception ex)
                            {
                                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Field",
                                    field, ex));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(
                            new CustomisationExporterResponseItem("Error Exporting Record Type Fields",
                                thisType, ex));
                    }
                }
                response.AddListToOutput("Fields", allFields);
            }
        }

        private string CreatePicklistName(string field, string thisType)
        {
            return Service.GetFieldMetadata(field, thisType).IsSharedPicklist
                ? Service.GetFieldMetadata(field, thisType).PicklistName
                : JoinTypeAndFieldName(field, thisType);
        }

        private static string JoinTypeAndFieldName(string field, string thisType)
        {
            return string.Format("{0}.{1}", thisType, field);
        }

        private void ProcessForEntities(CustomisationExporterRequest request, CustomisationExporterResponse response,
            LogController controller)
        {
            if (request.Entities)
            {
                var allEntities = new List<EntityExport>();
                var types = GetRecordTypesToExport(request).OrderBy(t => Service.GetDisplayName(t)).ToArray();
                var count = types.Count();
                for (var i = 0; i < count; i++)
                {
                    var thisType = types.ElementAt(i);
                    var thisTypeLabel = Service.GetDisplayName(thisType);
                    controller.UpdateProgress(i, count, "Exporting Record Types");
                    try
                    {
                        var mt = Service.GetRecordTypeMetadata(thisType);
                        allEntities.Add(new EntityExport(
                            mt.DisplayName,
                            thisType,
                            mt.IsCustomType,
                            mt.RecordTypeCode,
                            mt.CollectionName,
                            mt.Description,
                            mt.Audit,
                            mt.IsActivityType,
                            mt.Notes,
                            mt.Activities,
                            mt.Connections,
                            mt.MailMerge,
                            mt.Queues,
                            mt.MetadataId,
                            mt.ChangeTracking,
                            mt.EntitySetName,
                            mt.DocumentManagement,
                            mt.QuickCreate,
                            mt.AutoAddToQueue
                            ));
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Record Type",
                            thisType, ex));
                    }
                }

                response.AddListToOutput("Record Types", allEntities);
            }
        }

        private IEnumerable<string> GetRecordTypesToExport(CustomisationExporterRequest request)
        {
            return Service.GetAllRecordTypes()
                .Where(r => !Service.GetDisplayName(r).IsNullOrWhiteSpace())
                .ToArray();
        }
    }
}