using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.XrmModule.XrmConnection;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.Core.Attributes;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModule(typeof(XrmConnectionModule))]
    [DependantModule(typeof(XrmRecordExtractModule))]
    [MyDescription("Search Records In Dynamics For A Specific Piece Of Text")]
    public class XrmTextSearchModule :
        ServiceRequestModule
            <XrmTextSearchDialog, XrmTextSearchService, TextSearchRequest, TextSearchResponse, TextSearchResponseItem>
    {
        public override string MainOperationName
        {
            get { return "Text Search"; }
        }

        public override string MenuGroup => "Reports";

        public override void InitialiseModule()
        {
            base.InitialiseModule();
        }
    }
}