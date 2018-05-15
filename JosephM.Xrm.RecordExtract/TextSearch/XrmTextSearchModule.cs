using JosephM.Application.Modules;
using JosephM.Core.Attributes;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.XrmModule.XrmConnection;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModule(typeof(XrmConnectionModule))]
    [DependantModule(typeof(XrmRecordExtractModule))]
    [MyDescription("Search Records In Dynamics For A Specific Piece Of Text")]
    public class XrmTextSearchModule :
        TextSearchModuleBase
            <XrmTextSearchDialog, XrmTextSearchService>
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