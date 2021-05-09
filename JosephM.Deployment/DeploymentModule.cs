using JosephM.Application.Modules;
using JosephM.Deployment.CreatePackage;
using JosephM.Deployment.DeployPackage;
using JosephM.Deployment.DeploySolution;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.ImportCsvs;
using JosephM.Deployment.ImportExcel;
using JosephM.Deployment.ImportSql;
using JosephM.Deployment.ImportXml;
using JosephM.Deployment.MigrateInternal;
using JosephM.Deployment.MigrateRecords;

namespace JosephM.Deployment
{
    [DependantModule(typeof(ImportXmlModule))]
    [DependantModule(typeof(ImportCsvsModule))]
    [DependantModule(typeof(ImportExcelModule))]
    [DependantModule(typeof(ImportSqlModule))]
    [DependantModule(typeof(ExportXmlModule))]
    [DependantModule(typeof(CreatePackageModule))]
    [DependantModule(typeof(DeployPackageModule))]
    [DependantModule(typeof(MigrateRecordsModule))]
    [DependantModule(typeof(MigrateInternalModule))]
    [DependantModule(typeof(DeploySolutionModule))]
    [DependantModule(typeof(AddPortalDataModule))]
    [DependantModule(typeof(ExportDataTypeUsabilitiesModule))] 
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