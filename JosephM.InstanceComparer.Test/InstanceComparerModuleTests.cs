using JosephM.Core.FieldType;
using JosephM.Prism.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.InstanceComparer.Test
{
    [TestClass]
    public class InstanceComparerModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void InstanceComparerModuleTest()
        {
            var request = new InstanceComparerRequest();
            request.ConnectionOne = GetSavedXrmRecordConfiguration();
            request.ConnectionTwo = GetSavedXrmRecordConfiguration();

            var application = CreateAndLoadTestApplication<InstanceComparerModule>();
            var response = application.NavigateAndProcessDialog<InstanceComparerModule, InstanceComparerDialog, InstanceComparerResponse>(request);
            Assert.IsFalse(response.HasError);
        }
    }
}
