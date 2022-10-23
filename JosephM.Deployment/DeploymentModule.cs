using JosephM.Application.Modules;
using JosephM.Deployment.CreatePackage;
using JosephM.Deployment.DeployPackage;
using JosephM.Deployment.ImportSolution;
using JosephM.Deployment.SolutionTransfer;

namespace JosephM.Deployment
{
    [DependantModule(typeof(CreatePackageModule))]
    [DependantModule(typeof(DeployPackageModule))]
    [DependantModule(typeof(SolutionTransferModule))]
    [DependantModule(typeof(ImportSolutionModule))]
    public class DeploymentModule : ModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
        }
    }
}