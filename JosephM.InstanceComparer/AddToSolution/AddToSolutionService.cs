using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.InstanceComparer.AddToSolution
{
    public class AddToSolutionService :
        ServiceBase<AddToSolutionRequest, AddToSolutionResponse, AddToSolutionResponseItem>
    {
        public AddToSolutionService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        public override void ExecuteExtention(AddToSolutionRequest request, AddToSolutionResponse response,
            LogController controller)
        {
            var typesToAdd = new List<int>();

            var propTypeMaps = new Dictionary<string, int>();
            propTypeMaps.Add(nameof(AddToSolutionRequest.Dashboards), OptionSets.SolutionComponent.ObjectTypeCode.SystemForm);
            propTypeMaps.Add(nameof(AddToSolutionRequest.EmailTemplates), OptionSets.SolutionComponent.ObjectTypeCode.EmailTemplate);
            propTypeMaps.Add(nameof(AddToSolutionRequest.Entity), OptionSets.SolutionComponent.ObjectTypeCode.Entity);
            propTypeMaps.Add(nameof(AddToSolutionRequest.PluginAssemblies), OptionSets.SolutionComponent.ObjectTypeCode.PluginAssembly);
            propTypeMaps.Add(nameof(AddToSolutionRequest.PluginTriggers), OptionSets.SolutionComponent.ObjectTypeCode.SDKMessageProcessingStep);
            propTypeMaps.Add(nameof(AddToSolutionRequest.Reports), OptionSets.SolutionComponent.ObjectTypeCode.Report);
            propTypeMaps.Add(nameof(AddToSolutionRequest.SecurityRoles), OptionSets.SolutionComponent.ObjectTypeCode.Role);
            propTypeMaps.Add(nameof(AddToSolutionRequest.SharedPicklists), OptionSets.SolutionComponent.ObjectTypeCode.OptionSet);
            propTypeMaps.Add(nameof(AddToSolutionRequest.WebResources), OptionSets.SolutionComponent.ObjectTypeCode.WebResource);
            propTypeMaps.Add(nameof(AddToSolutionRequest.Workflows), OptionSets.SolutionComponent.ObjectTypeCode.Workflow);

            foreach(var propTypeMap in propTypeMaps)
            {
                if((bool)request.GetPropertyValue(propTypeMap.Key) && request.Items.Where(i => i.ComponentType == propTypeMap.Value).Any())
                {
                    typesToAdd.Add(propTypeMap.Value);
                }
            }

            var toDo = typesToAdd.Count;
            var done = 0;
            foreach(var type in typesToAdd)
            {
                try
                {
                    controller.UpdateProgress(++done, toDo, $"Adding {XrmRecordService.GetPicklistLabel(Fields.solutioncomponent_.componenttype, Entities.solutioncomponent, type.ToString())} Components");
                    var theseItems = request.Items
                        .Where(i => i.ComponentType == type)
                        .Select(i => i.ComponentId)
                        .ToArray();
                    XrmRecordService.AddSolutionComponents(request.SolutionAddTo.Id, type, theseItems);
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new AddToSolutionResponseItem() { Exception = ex });
                }
            }
        }
    }
}