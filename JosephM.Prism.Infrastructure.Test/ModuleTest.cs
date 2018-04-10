using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Core.Test;

namespace JosephM.Application.Prism.Test
{
    public class ModuleTest : CoreTest
    {
        protected ModuleTest()
            : base()
        {
        }

        protected virtual TestApplication CreateAndLoadTestApplication<TModule>(ApplicationControllerBase applicationController = null, ISettingsManager settingsManager = null, bool loadXrmConnection = true)
            where TModule : ModuleBase, new()
        {
            var testApplication = TestApplication.CreateTestApplication(applicationController, settingsManager);
            testApplication.AddModule<TModule>();
            return testApplication;
        }
    }
}
