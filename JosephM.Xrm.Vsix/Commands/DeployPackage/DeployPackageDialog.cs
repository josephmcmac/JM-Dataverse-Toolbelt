using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System;
using System.IO;

namespace JosephM.XRM.VSIX.Commands.DeployPackage
{
    public class DeployPackageDialog : VsixServiceDialog<XrmSolutionImporterExporterService, XrmSolutionImporterExporterRequest, XrmSolutionImporterExporterResponse, XrmSolutionImporterExporterResponseItem>
    {
        public XrmRecordService XrmRecordService { get { return Service.XrmRecordService; } }
        public XrmPackageSettings PackageSettings { get; set; }
        public IVisualStudioService VisualStudioService { get; set; }

        public DeployPackageDialog(XrmSolutionImporterExporterService service, XrmSolutionImporterExporterRequest request, IDialogController dialogController, XrmPackageSettings packageSettings, IVisualStudioService visualStudioService)
            : base(service, request, dialogController, lookupService: service.XrmRecordService, showRequestEntryForm: true)
        {
            PackageSettings = packageSettings;
            VisualStudioService = visualStudioService;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }
    }
}