using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Collections.Generic;

namespace JosephM.Application.Desktop.Module.Crud.ConfigureAutonumber
{
    public class ConfigureAutonumberDialog :
        ServiceRequestDialog<ConfigureAutonumberService, ConfigureAutonumberRequest, ConfigureAutonumberResponse, ConfigureAutonumberResponseItem>
    {
        public ConfigureAutonumberDialog(XrmRecordService recordService, IDialogController dialogController, ConfigureAutonumberRequest request, Action onClose)
            : base(new ConfigureAutonumberService(recordService), dialogController, recordService, request, onClose)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            if (Response != null)
            {
                addProperty("Seed Updated", Response.SeedUpdated.ToString());
                addProperty("Format Updated", Response.FormatUpdated.ToString());
            }
            return dictionary;
        }
    }
}