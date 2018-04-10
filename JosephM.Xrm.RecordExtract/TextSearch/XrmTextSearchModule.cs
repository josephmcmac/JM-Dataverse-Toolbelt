using JosephM.Application.Modules;
using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModule(typeof(XrmConnectionModule))]
    [DependantModule(typeof(XrmRecordExtractModule))]
    [DependantModule(typeof(XrmTextSearchSettingsModule))]
    public class XrmTextSearchModule :
        ServiceRequestModule
            <XrmTextSearchDialog, XrmTextSearchService, TextSearchRequest, TextSearchResponse, TextSearchResponseItem>
    {
        public override string MainOperationName
        {
            get { return "CRM Record Text Search"; }
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
        }
    }
}