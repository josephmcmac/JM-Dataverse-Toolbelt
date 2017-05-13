using System;
using System.Diagnostics;

namespace JosephM.XRM.VSIX.Commands.OpenWeb
{
    internal sealed class OpenCrmSolutionCommand : CommandBase<OpenCrmSolutionCommand>
    {
        public override int CommandId
        {
            get { return 0x010C; }
        }

        public override void DoDialog()
        {
            var xrmRecordService = GetXrmRecordService();
            var url = xrmRecordService.WebUrl;
            var solution = GetPackageSettings().Solution;
            if (solution == null)
                throw new NullReferenceException("No solution selected in the settings");

            var solutionUrl = string.Format("{0}{1}tools/solution/edit.aspx?id={2}", url, url.EndsWith("/") ? "" : "/", solution.Id.ToString());
            Process.Start(solutionUrl);
        }
    }
}
