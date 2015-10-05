#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.FieldType;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.ImportExporter.Service;

#endregion

namespace JosephM.Xrm.ImportExporter.Prism
{
    public class XrmImporterExporterDialog :
        ServiceRequestDialog
            <XrmImporterExporterService<XrmRecordService>, XrmImporterExporterRequest,
                XrmImporterExporterResponse, XrmImporterExporterResponseItem>
    {
        public XrmImporterExporterDialog(XrmImporterExporterService<XrmRecordService> service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            if (Response.Success)
            {
                if (
                    (Request.ImportExportTask == ImportExportTask.ExportXml)
                    && Request.FolderPath != null)
                    AddCompletionOption("Open Folder", () => OpenFolder(Request.FolderPath.FolderPath));
            }
        }


    }
}