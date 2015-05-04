using System;
using JosephM.Prism.Infrastructure.Attributes;
using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModuleAttribute(typeof(XrmModuleModule))]
    [DependantModuleAttribute(typeof(XrmRecordExtractModule))]
    [DependantModuleAttribute(typeof(XrmTextSearchSettingsModule))]
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
            ApplicationOptions.AddHelp("CRM Text Search", "Text Search Help.htm");
        }
    }
}