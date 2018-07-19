using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Extentions;
using System.Linq;
using JosephM.Core.FieldType;
using System;

namespace JosephM.InstanceComparer
{
    [MyDescription("Compare The State Of Customisations And Data Between 2 CRM Instances. Note This Is Not A Complete Comparison")]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class InstanceComparerModule :
        ServiceRequestModule
            <InstanceComparerDialog, InstanceComparerService, InstanceComparerRequest, InstanceComparerResponse, InstanceComparerResponseItem>
    {
        public override string MainOperationName
        {
            get { return "Instance Compare"; }
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddPortalDataButtonToRequestFormGrid();
        }

        private void AddPortalDataButtonToRequestFormGrid()
        {
            var customFormFunction = new CustomFormFunction("ADDPORTALDATA", "Add Portal Types", (r) =>
            {
                try
                {
                    r.GetBooleanFieldFieldViewModel(nameof(InstanceComparerRequest.Data)).Value = true;
                    var typesGrid = r.GetEnumerableFieldViewModel(nameof(InstanceComparerRequest.DataComparisons));
                    var typesToAdd = new[]
                    {
                        "adx_contentsnippet",
                        "adx_entityform",
                        "adx_entityformmetadata",
                        "adx_entitylist",
                        "adx_entitypermission",
                        "adx_pagetemplate",
                        "adx_sitemarker",
                        "adx_sitesetting",
                        "adx_webfile",
                        "adx_weblink",
                        "adx_weblinkset",
                        "adx_webpage",
                        "adx_webrole",
                        "adx_webtemplate",
                    };
                    var typesGridService = typesGrid.GetRecordService();
                    foreach (var item in typesToAdd.Reverse())
                    {
                        var newRecord = typesGridService.NewRecord(typeof(InstanceComparerRequest.InstanceCompareDataCompare).AssemblyQualifiedName);
                        newRecord.SetField(nameof(InstanceComparerRequest.InstanceCompareDataCompare.RecordType), new RecordType(item, item), typesGridService);
                        typesGrid.InsertRecord(newRecord, 0);
                    }
                }
                catch(Exception ex)
                {
                    r.ApplicationController.ThrowException(ex);
                }
            }, (r) => true);
            this.AddCustomFormFunction(customFormFunction, typeof(InstanceComparerRequest));
        }
    }
}