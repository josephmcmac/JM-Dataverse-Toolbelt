using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.TestModule.TestGridEdit
{
    [MyDescription("Just a fake dialog for testing")]
    public class TestGridEditModule :
        ServiceRequestModule
            <TestGridEdit, TestGridEditService, TestGridEditRequest, TestGridEditResponse, TestGridEditResponseItem>
    {
    }
}