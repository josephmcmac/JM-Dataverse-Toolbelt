using JosephM.Application.ViewModel.Dialog;
using JosephM.CodeGenerator.CSharp;
using JosephM.Xrm.Vsix.Application.Extentions;
using JosephM.Xrm.Vsix.Module.PackageSettings;

namespace JosephM.Xrm.Vsix.Module.RefreshSchema
{
    public class VsixCSharpDialog : CSharpDialog
    {
        public VsixCSharpDialog(CSharpService service, IDialogController dialogController, XrmPackageSettings xrmPackageSettings)
            : base(service, dialogController, service.Service, true)
        {
            this.AddRedirectToPackageSettingsEntryWhenNotConnected(service.Service, xrmPackageSettings);
        }
    }
}
