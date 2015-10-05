#region

using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Xrm.ImportExporter.Prism
{
    public class XrmSolutionImporterExporterModule : DialogModule<XrmSolutionImporterExporterDialog>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelp("Solution Import / Export", "Solution Importer Exporter Help.htm");
        }

        protected override string MainOperationName { get { return "Solution Import / Export"; } }
    }
}