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
            ServiceRequestController controller)
        {
            var toDo = request.GetItemsToInclude().Count();
            var done = 0;
            foreach(var type in request.GetItemsToInclude())
            {
                try
                {
                    controller.UpdateProgress(++done, toDo, $"Adding {XrmRecordService.GetPicklistLabel(Fields.solutioncomponent_.componenttype, Entities.solutioncomponent, type.ComponentTypeKey.ToString())} Components");
                    var theseItems = type.AddAllItems
                        ? type.AllItems.Select(i => i.Id).ToArray()
                        : type.ItemsSelection.Where(i => i.Selected).Select(i => i.Id).ToArray();
                    XrmRecordService.AddSolutionComponents(request.SolutionAddTo.Id, type.ComponentTypeKey, theseItems);
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new AddToSolutionResponseItem() { Exception = ex });
                }
            }
            response.Message = "Add To Solution Completed";
        }
    }
}