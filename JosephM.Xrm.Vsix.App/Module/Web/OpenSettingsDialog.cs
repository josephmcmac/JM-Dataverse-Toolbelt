using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.App.Module.Web;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [RequiresConnection]
    public class OpenSettingsDialog : OpenWebDialog
    {
        public OpenSettingsDialog(XrmRecordService xrmRecordService, OpenWebSettings openWebSettings, IDialogController controller)
            : base(xrmRecordService, controller)
        {
            OpenWebSettings = openWebSettings;
        }

        public OpenWebSettings OpenWebSettings { get; }

        protected override string GetUrl()
        {
            if (OpenWebSettings.UseClassicSettings)
            {
                var url = XrmRecordService.WebUrl;
                return $"{url}/main.aspx?settingsonly=true";
            }
            else
            {
                return $"https://admin.powerplatform.microsoft.com/environments/{XrmRecordService.OrganisationId}/settings";
            }
        }
    }
}
