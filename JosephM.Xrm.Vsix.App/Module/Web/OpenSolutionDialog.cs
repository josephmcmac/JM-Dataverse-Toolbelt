using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.App.Module.Web;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [RequiresConnection]
    public class OpenSolutionDialog : OpenWebDialog
    {
        public OpenSolutionDialog(XrmRecordService xrmRecordService, XrmPackageSettings xrmPackageSettings, OpenWebSettings openWebSettings, IDialogController controller)
            : base(xrmRecordService, controller)
        {
            XrmPackageSettings = xrmPackageSettings;
            OpenWebSettings = openWebSettings;
        }

        public XrmPackageSettings XrmPackageSettings { get; }
        public OpenWebSettings OpenWebSettings { get; }

        protected override string GetUrl()
        {
            if (XrmPackageSettings.Solution == null)
            {
                throw new NullReferenceException($"{nameof(XrmPackageSettings.Solution)} Is Not Populated In The {nameof(XrmPackageSettings)}");
            }
            if (OpenWebSettings.UseClassicSettings)
            {
                var url = XrmRecordService.WebUrl;
                var solutionUrl = string.Format("{0}/tools/solution/edit.aspx?id={1}", url, XrmPackageSettings.Solution.Id.ToString());
                return solutionUrl;
            }
            else
            {
                return $"https://make.powerapps.com/environments/{XrmRecordService.EnvironmentId}/solutions/{XrmPackageSettings.Solution.Id}";
            }
        }
    }
}
