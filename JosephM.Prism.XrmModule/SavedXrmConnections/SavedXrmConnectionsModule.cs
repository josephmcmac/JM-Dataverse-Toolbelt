#region

using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.XrmConnection;

#endregion

namespace JosephM.Prism.XrmModule.SavedXrmConnections
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class SavedXrmConnectionsModule : SettingsModule<SavedXrmConnectionsDialog, ISavedXrmConnections, SavedXrmConnections>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelp("Saved Connections", "Saved CRM Connections Help.htm");
        }

        public override void RegisterTypes()
        {
            base.RegisterTypes();
        }
    }
}