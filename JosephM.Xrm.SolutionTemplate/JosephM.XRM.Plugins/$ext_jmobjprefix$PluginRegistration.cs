
using $safeprojectname$.Plugins;
using $safeprojectname$.Xrm;
using Schema;

namespace $safeprojectname$
{
    /// <summary>
    /// This is the class for registering plugins in CRM
    /// Each entity plugin type needs to be instantiated in the CreateEntityPlugin method
    /// </summary>
    public class $ext_jmobjprefix$PluginRegistration : XrmPluginRegistration
    {
        public override XrmPlugin CreateEntityPlugin(string entityType, bool isRelationship)
        {
            switch (entityType)
            {
                //case Entities.account: return new AccountPlugin();
            }
            return null;
        }
    }
}
