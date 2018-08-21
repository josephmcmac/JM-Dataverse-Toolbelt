using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Diagnostics;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [RequiresConnection]
    public class OpenSolutionDialog : OpenWebDialog
    {
        public OpenSolutionDialog(XrmRecordService xrmRecordService, XrmPackageSettings xrmPackageSettings, IDialogController controller)
            : base(xrmRecordService, controller)
        {
            XrmPackageSettings = xrmPackageSettings;
        }

        public XrmPackageSettings XrmPackageSettings { get; }

        protected override string GetUrl()
        {
            if (XrmPackageSettings.Solution == null)
                throw new NullReferenceException($"{nameof(XrmPackageSettings.Solution)} Is Not Populated In The {nameof(XrmPackageSettings)}");
            var url = XrmRecordService.WebUrl;
            var solutionUrl = string.Format("{0}{1}tools/solution/edit.aspx?id={2}", url, url.EndsWith("/") ? "" : "/", XrmPackageSettings.Solution.Id.ToString());
            return solutionUrl;
        }
    }
}
