﻿using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Xrm.DataImportExport.Import;
using System.Collections.Generic;

namespace JosephM.Xrm.MigrateRecords
{
    public class MigrateRecordsDialog :
        ServiceRequestDialog
            <MigrateRecordsService, MigrateRecordsRequest,
                MigrateRecordsResponse, DataImportResponseItem>
    {
        public MigrateRecordsDialog(MigrateRecordsService service,
            IDialogController dialogController)
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
            addProperty("Include NN", Request.IncludeNNRelationshipsBetweenEntities.ToString());
            addProperty("Include Notes", Request.IncludeNotes.ToString());
            addProperty("Include Owner", Request.IncludeOwner.ToString());
            addProperty("Mask Emails", Request.MaskEmails.ToString());
            addProperty("Match By Name", Request.MatchByName.ToString());
            if (Response.IsImportSummary)
            {
                foreach (var typeGroup in Response.ImportSummary)
                {
                    addProperty($"Import Count {typeGroup.Type}", typeGroup.Total.ToString());
                    addProperty($"Import Errors {typeGroup.Type}", typeGroup.Errors.ToString());
                }
            }
            return dictionary;
        }
    }
}