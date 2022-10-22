using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.TestModule.AllPropertyTypesCentered
{
    [MyDescription("Just a fake dialog for AllPropertyTypesCompacting")]
    public class AllPropertyTypesCenteredModule :
        ServiceRequestModule
            <AllPropertyTypesCenteredDialog, AllPropertyTypesCenteredService, AllPropertyTypesCenteredRequest, AllPropertyTypesCenteredResponse, AllPropertyTypesCenteredResponseItem>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            RegisterInstance(TestSettingsTypeCentered.Create());
        }
    }
}