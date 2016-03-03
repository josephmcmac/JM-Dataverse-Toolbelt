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
            AddHelp("Solution Import / Export", "Solution Importer Exporter Help.docx");
        }

        protected override string MainOperationName { get { return "Solution Import / Export"; } }
    }
}