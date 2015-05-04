#region

using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.CustomisationImporter.Service
{
    public class XrmCustomisationImportService : CustomisationImportService
    {
        public XrmCustomisationImportService(XrmRecordService customisationService)
            : base(customisationService)
        {
        }
    }
}