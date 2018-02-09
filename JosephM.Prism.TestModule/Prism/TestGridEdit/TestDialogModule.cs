using JosephM.Core.Attributes;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Prism.TestModule.Prism.TestGridEdit
{
    [MyDescription("Just a fake dialog for testing")]
    public class TestGridEditModule :
        ServiceRequestModule
            <TestGridEdit, TestGridEditService, TestGridEditRequest, TestGridEditResponse, TestGridEditResponseItem>
    {
    }
}