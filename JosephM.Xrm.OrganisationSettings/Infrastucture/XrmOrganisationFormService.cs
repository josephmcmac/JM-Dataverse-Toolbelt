using System;
using JosephM.Record.Application.RecordEntry.Metadata;

namespace JosephM.Xrm.OrganisationSettings.Infrastucture
{
    public class XrmOrganisationFormService : FormServiceBase
    {
        public override FormMetadata GetFormMetadata(string recordType)
        {
            if (recordType == "organization")
                return new FormMetadata(new[] {"maxrecordsforexporttoexcel"});
            if (string.IsNullOrWhiteSpace(recordType))
                throw new ArgumentOutOfRangeException("recordType", "Empty String");
            throw new ArgumentOutOfRangeException("recordType", "Form not defined for record type: " + recordType);
        }
    }
}