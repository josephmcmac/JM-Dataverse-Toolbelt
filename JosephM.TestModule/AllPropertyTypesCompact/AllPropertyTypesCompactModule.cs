using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.TestModule.AllPropertyTypesCompact
{
    [MyDescription("Just a fake dialog for AllPropertyTypesCompacting")]
    public class AllPropertyTypesCompactModule :
        ServiceRequestModule
            <AllPropertyTypesCompactDialog, AllPropertyTypesCompactService, AllPropertyTypesCompactRequest, AllPropertyTypesCompactResponse, AllPropertyTypesCompactResponseItem>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            RegisterInstance(TestSettingsTypeCompact.Create());
        }
    }
}