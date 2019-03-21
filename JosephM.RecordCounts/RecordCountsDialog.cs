using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.RecordCounts
{
    [RequiresConnection]
    public class RecordCountsDialog :
        ServiceRequestDialog
            <RecordCountsService, RecordCountsRequest, RecordCountsResponse,
                RecordCountsResponseItem>
    {
        public RecordCountsDialog(RecordCountsService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            addProperty("All Types", Request.AllRecordTypes.ToString());
            addProperty("Group By Owner", Request.GroupCountsByOwner.ToString());
            addProperty("Only Specific Owner", Request.OnlyIncludeSelectedOwner.ToString());
            if (Response.RecordCounts != null)
            {
                var max = Response.RecordCounts.OrderByDescending(rc => rc.Count).First();
                addProperty("Max Count", max.Count.ToString());
                addProperty("Max Count Type", max.RecordType.ToString());
            }
            return dictionary;
        }
    }
}