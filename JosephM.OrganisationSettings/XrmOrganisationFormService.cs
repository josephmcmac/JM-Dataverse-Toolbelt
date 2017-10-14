using System;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Record.IService;

namespace JosephM.OrganisationSettings
{
    public class XrmOrganisationFormService : FormServiceBase
    {
        public override FormMetadata GetFormMetadata(string recordType, IRecordService recordService = null)
        {
            return new FormMetadata(new[] {"maxrecordsforexporttoexcel"});
        }
    }
}