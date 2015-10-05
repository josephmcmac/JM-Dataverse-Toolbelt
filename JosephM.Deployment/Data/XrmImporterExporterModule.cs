#region

using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Xrm.ImportExporter.Prism
{
    public class XrmImporterExporterModule : DialogModule<XrmImporterExporterDialog>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelp("Data Import / Export", "Xrm Importer Exporter Help.htm");
        }

        protected override string MainOperationName { get { return "Data Import / Export"; } }
    }
}