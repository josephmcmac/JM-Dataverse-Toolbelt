#region

using JosephM.Core.FieldType;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Application.Dialog;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.ImportExporter.Service;

#endregion

namespace JosephM.Xrm.ImportExporter.Prism
{
    public class XrmSolutionImporterExporterDialog :
        ServiceRequestDialog
            <XrmSolutionImporterExporterService<XrmRecord, XrmRecordService>, XrmSolutionImporterExporterRequest,
                XrmSolutionImporterExporterResponse, XrmSolutionImporterExporterResponseItem>
    {
        public XrmSolutionImporterExporterDialog(XrmSolutionImporterExporterService<XrmRecord, XrmRecordService> service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            if (Response.Success)
            {
                if ((Request.ImportExportTask == SolutionImportExportTask.ExportSolutions)
                    && Request.FolderPath != null)
                    AddCompletionOption("Open Folder", () => OpenFolder(Request.FolderPath.FolderPath));
            }
        }


    }
}