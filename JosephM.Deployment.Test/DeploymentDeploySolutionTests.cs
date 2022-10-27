using JosephM.Deployment.SolutionTransfer;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentDeploySolutionTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentDeploySolutionTest()
        {
            Assert.Inconclusive();
            var altConnection = GetAltSavedXrmRecordConfiguration();

            var sourceSolution = ReCreateTestSolution();
            var request = new SolutionTransferRequest();
            request.SourceConnection = GetSavedXrmRecordConfiguration();
            request.TargetConnection = GetAltSavedXrmRecordConfiguration();
            request.Solution = sourceSolution.ToLookup();
            request.SourceVersionForRelease = "3.0.0.0";
            request.SetSourceVersionPostRelease = "4.0.0.0";

            var altService = new XrmRecordService(altConnection, ServiceFactory);
            var targetSolution = altService.GetFirst(Entities.solution, Fields.solution_.uniquename, sourceSolution.GetStringField(Fields.solution_.uniquename));
            if (targetSolution != null)
                altService.Delete(targetSolution);

            var createApplication = CreateAndLoadTestApplication<SolutionTransferModule>();
            var response = createApplication.NavigateAndProcessDialog<SolutionTransferModule, SolutionTransferDialog, SolutionTransferResponse>(request);
            Assert.IsFalse(response.HasError);

            targetSolution = altService.GetFirst(Entities.solution, Fields.solution_.uniquename, sourceSolution.GetStringField(Fields.solution_.uniquename));
            Assert.IsTrue(targetSolution != null);
            Assert.AreEqual("3.0.0.0", targetSolution.GetStringField(Fields.solution_.version));

            sourceSolution = XrmRecordService.Get(sourceSolution.Type, sourceSolution.Id);
            Assert.AreEqual("4.0.0.0", sourceSolution.GetStringField(Fields.solution_.version));
        }
    }
}
