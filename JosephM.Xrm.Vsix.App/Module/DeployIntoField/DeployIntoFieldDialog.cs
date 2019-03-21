using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployIntoField
{
    [RequiresConnection]
    public class DeployIntoFieldDialog
        : ServiceRequestDialog<DeployIntoFieldService, DeployIntoFieldRequest, DeployIntoFieldResponse, DeployIntoFieldResponseItem>
    {
        public DeployIntoFieldDialog(DeployIntoFieldService service, IDialogController dialogController)
            : base(service, dialogController)
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
            if (Response.ResponseItems != null)
            {
                addProperty("Import Count", Response.ResponseItems.Count().ToString());
            }
            if (Response.ResponseItems != null)
            {
                foreach (var typeGroup in Response.ResponseItems.Where(ri => ri.RecordType != null).GroupBy(it => it.RecordType))
                {
                    addProperty($"Import {typeGroup.Key} Count", typeGroup.Count().ToString());
                }
            }
            return dictionary;
        }
    }
}
