using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.XmlExport;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    public class ImportRecordsDialog
        : ServiceRequestDialog<ImportRecordsService, ImportRecordsRequest, ImportRecordsResponse, DataImportResponseItem>
    {
        public ImportRecordsDialog(ImportRecordsService service, IDialogController dialogController)
            : base(service, dialogController)
        {
            var validationDialog = new ImportXmlValidationDialog(this, Request);
            SubDialogs = SubDialogs.Union(new[] { validationDialog }).ToArray();
        }
    }
}
