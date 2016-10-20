#region

using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Xrm.ImportExporter.Prism
{
    public class XrmImporterExporterModule : DialogModule<XrmImporterExporterDialog>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelpUrl("Data Import / Export", "DataImportExport");
        }

        protected override string MainOperationName { get { return "Data Import / Export"; } }
    }
}