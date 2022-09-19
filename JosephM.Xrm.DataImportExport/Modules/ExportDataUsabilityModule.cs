using JosephM.Application.Modules;

namespace JosephM.Xrm.DataImportExport.Modules
{
    [DependantModule(typeof(AddPortalDataModule))]
    [DependantModule(typeof(ExportDataTypeUsabilitiesModule))]
    public class ExportDataUsabilityModule : ModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
        }
    }
}