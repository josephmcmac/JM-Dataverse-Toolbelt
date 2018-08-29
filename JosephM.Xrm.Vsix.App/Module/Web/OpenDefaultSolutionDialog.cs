using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [RequiresConnection]
    public class OpenDefaultSolutionDialog : OpenWebDialog
    {
        public OpenDefaultSolutionDialog(XrmRecordService xrmRecordService, IDialogController controller)
            : base(xrmRecordService, controller)
        {
        }

        protected override string GetUrl()
        {
            var solution = XrmRecordService.GetFirst(Entities.solution, Fields.solution_.uniquename, "default");
            if (solution == null)
                throw new NullReferenceException($"Could Not Find Solution named 'default'");
            var url = XrmRecordService.WebUrl;
            var solutionUrl = string.Format("{0}{1}tools/solution/edit.aspx?id={2}", url, url.EndsWith("/") ? "" : "/", solution.Id.ToString());
            return solutionUrl;
        }
    }
}
