using JosephM.Application.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;

namespace JosephM.Xrm.Vsix.Module.Web
{
    public class MenuItemVisibleSolutionConfigured : MenuItemVisible
    {
        public override bool IsVisible(IApplicationController applicationController)
        {
            var settings = applicationController.ResolveType(typeof(XrmPackageSettings)) as XrmPackageSettings;
            return settings.AddToSolution && settings.Solution != null;
        }
    }
}
