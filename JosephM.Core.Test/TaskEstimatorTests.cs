using JosephM.Core.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;

namespace JosephM.Core.Test
{
    [TestClass]
    public class TaskEstimatorTests : CoreTest
    {
        [TestMethod]
        [DeploymentItem("app.config")]
        public void TaskEstimatorTestsTest()
        {
            var iterations = 2;
            var taskEstimator = new TaskEstimator(iterations);
            for(var i = 0; i < iterations; i++)
            {
                Thread.Sleep(1000);
                Debug.WriteLine($"Remaining {taskEstimator.GetProgressString(i + 1, taskName: "Executing Test")}");
            }
        }
    }
}