using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.SavedViewsExport
{
    [MyDescription("Export personal saved views")]
    public class SavedViewsExportModule :
        ServiceRequestModule
            <SavedViewsExportDialog, SavedViewsExportService, SavedViewsExportRequest,
                SavedViewsExportResponse, SavedViewsExportResponseItem>
    {
        public override string MenuGroup => "Customisations";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
        }
    }
}