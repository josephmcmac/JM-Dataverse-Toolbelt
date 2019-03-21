using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using System.Collections.Generic;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerDialog :
        ServiceRequestDialog<InstanceComparerService, InstanceComparerRequest, InstanceComparerResponse, InstanceComparerResponseItem>
    {
        public InstanceComparerDialog(IDialogController dialogController)
            : base(new InstanceComparerService(), dialogController)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();

            if (Response.AreDifferences)
                CompletionMessage = "Differences Were Found Between The Environments";
            else
                CompletionMessage = "No Difference Were Found";
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            if(Response.Summary != null)
            {
                foreach (var summaryItem in Response.Summary)
                {
                    addProperty($"{summaryItem.Type} Difference Count", summaryItem.Total.ToString());
                }
            }
            return dictionary;
        }
    }
}