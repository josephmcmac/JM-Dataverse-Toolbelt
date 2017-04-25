using System;
using System.Linq;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Utilities;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.ObjectMapping;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Core.FieldType;

namespace JosephM.XRM.VSIX.Commands.GetSolution
{
    public class GetSolutionDialog : VsixServiceDialog<XrmSolutionImporterExporterService, XrmSolutionImporterExporterRequest, XrmSolutionImporterExporterResponse, XrmSolutionImporterExporterResponseItem>
    {
        public XrmRecordService XrmRecordService { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }
        public IVisualStudioService VisualStudioService { get; set; }

        public GetSolutionDialog(XrmSolutionImporterExporterService service, XrmSolutionImporterExporterRequest request, IDialogController dialogController, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings, IVisualStudioService visualStudioService)
            : base(service, request, dialogController)
        {
            XrmRecordService = xrmRecordService;
            PackageSettings = packageSettings;
            VisualStudioService = visualStudioService;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        public override void PostExecute()
        {
            if (Response.ExportedSolutions.Any())
            {
                VisualStudioService.AddSolutionItem(Response.ExportedSolutions.First().SolutionFile.FileName);
                CompletionMessage = "Solution Updated In Visual Studio Solution";
            }
            else
            {
                throw new Exception("Error the response did not contain exported solutions");
            }
        }
    }
}