using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.Prism.TestModule.Prism.TestGridEdit
{
    [MyDescription("Just a fake dialog for testing")]
    public class TestGridEditModule :
        ServiceRequestModule
            <TestGridEdit, TestGridEditService, TestGridEditRequest, TestGridEditResponse, TestGridEditResponseItem>
    {
    }
}