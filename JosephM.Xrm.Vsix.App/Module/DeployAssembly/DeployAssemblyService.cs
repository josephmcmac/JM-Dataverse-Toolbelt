using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    public class DeployAssemblyService :
        ServiceBase<DeployAssemblyRequest, DeployAssemblyResponse, DeployAssemblyResponseItem>
    {
        public DeployAssemblyService(XrmRecordService service, XrmPackageSettings packageSettings)
        {
            Service = service;
            PackageSettings = packageSettings;
        }

        public XrmPackageSettings PackageSettings { get; set; }
        public XrmRecordService Service { get; set; }

        public override void ExecuteExtention(DeployAssemblyRequest request, DeployAssemblyResponse response,
            ServiceRequestController controller)
        {
            var service = Service;

            controller.UpdateProgress(0, 4, "Processing Deletions");

            var removedPlugins = request.GetPreTypeRecords().Where(ptr => request.PluginTypes.All(pt => pt.Id != ptr.Id)).ToArray();

            var deletions = service.DeleteInCrm(removedPlugins);
            response.AddResponseItems(deletions.Errors.Select(e => new DeployAssemblyResponseItem("Plugin Type Delete", e.Key.GetStringField(Fields.plugintype_.typename), e.Value)));
            response.AddResponseItems(deletions.Deleted.Select(d => new DeployAssemblyResponseItem("Plugin Type Delete", d.GetStringField(Fields.plugintype_.typename))));

            controller.UpdateProgress(1, 4, "Deploying Assembly");
            //okay first create/update the plugin assembly
            var assemblyRecord = service.NewRecord(Entities.pluginassembly);
            assemblyRecord.Id = request.Id;
            if (request.Id != null)
                assemblyRecord.SetField(Fields.pluginassembly_.pluginassemblyid, request.Id, service);
            assemblyRecord.SetField(Fields.pluginassembly_.name, request.AssemblyName, service);
            assemblyRecord.SetField(Fields.pluginassembly_.content, request.Content, service);
            assemblyRecord.SetField(Fields.pluginassembly_.isolationmode, (int)request.IsolationMode, service);
            var matchField = Fields.pluginassembly_.pluginassemblyid;

            var assemblyLoadResponse = service.LoadIntoCrm(new[] { assemblyRecord }, matchField);
            foreach (var e in assemblyLoadResponse.Errors)
            {
                response.AddResponseItem(new DeployAssemblyResponseItem(e.Key.Id != null ? "Assembly Update" : "Assembly Create", e.Key.GetStringField(Fields.pluginassembly_.name), e.Value));
            }
            response.AddResponseItems(assemblyLoadResponse.Created.Select(d => new DeployAssemblyResponseItem("Assembly Create", d.GetStringField(Fields.pluginassembly_.name))));
            response.AddResponseItems(assemblyLoadResponse.Updated.Select(d => new DeployAssemblyResponseItem("Assembly Update", d.GetStringField(Fields.pluginassembly_.name))));
            if (assemblyLoadResponse.Errors.Any())
            {
                response.Message = $"The Process Did Not Complete Due To An Error Deploying The {Service.GetDisplayName(Entities.pluginassembly)} Record";
            }
            else
            {
                controller.UpdateProgress(2, 4, "Updating Plugin Types");

                //okay create/update the plugin types
                var pluginTypes = new List<IRecord>();
                foreach (var pluginType in request.PluginTypes)
                {
                    var pluginTypeRecord = service.NewRecord(Entities.plugintype);
                    pluginTypeRecord.Id = pluginType.Id;
                    if (pluginType.Id != null)
                        pluginTypeRecord.SetField(Fields.plugintype_.plugintypeid, pluginType.Id, service);
                    pluginTypeRecord.SetField(Fields.plugintype_.typename, pluginType.TypeName, service);
                    pluginTypeRecord.SetField(Fields.plugintype_.name, pluginType.Name, service);
                    pluginTypeRecord.SetField(Fields.plugintype_.friendlyname, pluginType.FriendlyName, service);
                    pluginTypeRecord.SetField(Fields.plugintype_.assemblyname, request.AssemblyName, service);
                    pluginTypeRecord.SetLookup(Fields.plugintype_.pluginassemblyid, assemblyRecord.Id, assemblyRecord.Type);
                    pluginTypeRecord.SetField(Fields.plugintype_.isworkflowactivity, pluginType.IsWorkflowActivity, service);
                    pluginTypeRecord.SetField(Fields.plugintype_.workflowactivitygroupname, pluginType.GroupName, service);
                    pluginTypes.Add(pluginTypeRecord);
                }

                var pluginTypeLoadResponse = service.LoadIntoCrm(pluginTypes, Fields.plugintype_.plugintypeid, setWorkflowRefreshField: request.TriggerWorkflowActivityRefreshes);
                foreach (var e in pluginTypeLoadResponse.Errors)
                {
                    response.AddResponseItem(new DeployAssemblyResponseItem(e.Key.Id != null ? "Plugin Type Update" : "Plugin Type Create", e.Key.GetStringField(Fields.plugintype_.typename), e.Value));
                }
                response.AddResponseItems(pluginTypeLoadResponse.Created.Select(d => new DeployAssemblyResponseItem("Plugin Type Create", d.GetStringField(Fields.plugintype_.typename))));
                response.AddResponseItems(pluginTypeLoadResponse.Updated.Select(d => new DeployAssemblyResponseItem("Plugin Type Update", d.GetStringField(Fields.plugintype_.typename))));

                //add plugin assembly to the solution
                var componentType = OptionSets.SolutionComponent.ObjectTypeCode.PluginAssembly;
                var itemsToAdd = assemblyLoadResponse.Created.Union(assemblyLoadResponse.Updated);
                if (PackageSettings.AddToSolution)
                {
                    controller.UpdateProgress(3, 4, "Adding Components To Solution");
                    service.AddSolutionComponents(PackageSettings.Solution.Id, componentType, itemsToAdd.Select(i => i.Id));
                }

                controller.UpdateProgress(4, 4, "Completing");
                if (response.HasResponseItemError)
                    response.Message = "There Were Errors Thrown Updating The Plugins";
                else if (response.HasResponseItems)
                    response.Message = "Plugins Updated";
                else
                    response.Message = "No Updates Were Identified";
            }
        }
    }
}