using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.TestModule.AllPropertyTypes;

namespace JosephM.AllPropertyTypesModule.AllPropertyTypesDialog
{
    [MyDescription("Just a fake dialog for AllPropertyTypesing")]
    public class AllPropertyTypesDialogModule :
        ServiceRequestModule
            <AllPropertyTypesDialog, AllPropertyTypesService, AllPropertyTypesRequest, AllPropertyTypesResponse, AllPropertyTypesResponseItem>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            RegisterInstance(TestSettingsType.Create());
        }
    }
}