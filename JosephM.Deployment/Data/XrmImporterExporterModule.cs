#region

using JosephM.Prism.Infrastructure.Module;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.ImportExporter.Service;

#endregion

namespace JosephM.Xrm.ImportExporter.Prism
{
    public class XrmImporterExporterModule
        : ServiceRequestModule<XrmImporterExporterDialog, XrmImporterExporterService<XrmRecordService>, XrmImporterExporterRequest, XrmImporterExporterResponse, XrmImporterExporterResponseItem>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelpUrl("Data Import / Export", "DataImportExport");
        }

        protected override string MainOperationName { get { return "Data Import / Export"; } }

        public override string MenuGroup => "Deployment / Migration";
    }
}