#region

using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Xrm.ImportExporter.Prism
{
    public class XrmSolutionImporterExporterModule : DialogModule<XrmSolutionImporterExporterDialog>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelpUrl("Solution Import / Export", "SolutionImportExport");
        }

        protected override string MainOperationName { get { return "Solution Import / Export"; } }
    }
}