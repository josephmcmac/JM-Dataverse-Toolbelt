using System;
using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModule(typeof(XrmModuleModule))]
    [DependantModule(typeof(XrmRecordExtractModule))]
    [DependantModule(typeof(XrmTextSearchSettingsModule))]
    public class XrmTextSearchModule :
        ServiceRequestModule
            <XrmTextSearchDialog, XrmTextSearchService, TextSearchRequest, TextSearchResponse, TextSearchResponseItem>
    {
        protected override string MainOperationName
        {
            get { return "CRM Record Text Search"; }
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelp("CRM Text Search", "Text Search Help.htm");
        }
    }
}