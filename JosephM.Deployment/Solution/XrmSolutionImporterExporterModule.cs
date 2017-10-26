#region

using JosephM.Prism.Infrastructure.Module;
using JosephM.Xrm.ImportExporter.Service;

#endregion

namespace JosephM.Xrm.ImportExporter.Prism
{
    public class XrmSolutionImporterExporterModule
        : ServiceRequestModule<XrmSolutionImporterExporterDialog, XrmSolutionImporterExporterService, XrmSolutionImporterExporterRequest, XrmSolutionImporterExporterResponse, XrmSolutionImporterExporterResponseItem>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelpUrl("Solution Deployment", "SolutionImportExport");
        }

        protected override string MainOperationName { get { return "Solution Deployment"; } }

        public override string MenuGroup => "Deployment / Migration";
    }
}