using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.App.Module.Web;
using System;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [RequiresConnection]
    public class OpenDefaultSolutionDialog : OpenWebDialog
    {
        public OpenDefaultSolutionDialog(XrmRecordService xrmRecordService, OpenWebSettings openWebSettings, IDialogController controller)
            : base(xrmRecordService, controller)
        {
            OpenWebSettings = openWebSettings;
        }

        public OpenWebSettings OpenWebSettings { get; }

        protected override string GetUrl()
        {
            var solution = XrmRecordService.GetFirst(Entities.solution, Fields.solution_.uniquename, "default");
            if (solution == null)
                throw new NullReferenceException($"Could Not Find Solution named 'default'");
            if (OpenWebSettings.UseClassicSettings)
            {
                var url = XrmRecordService.WebUrl;
                var solutionUrl = string.Format("{0}/tools/solution/edit.aspx?id={1}", url, solution.Id.ToString());
                return solutionUrl;
            }
            else
            {
                return $"https://make.powerapps.com/environments/{XrmRecordService.EnvironmentId}/solutions/{solution.Id}";
            }
        }
    }
}
