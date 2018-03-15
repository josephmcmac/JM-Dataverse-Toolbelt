using System.Diagnostics;
using $ext_safeprojectname$.Plugins.Core;

namespace $safeprojectname$
{
    public class DebugUserInterface : IUserInterface
    {
        private readonly bool _logDetail;

        public DebugUserInterface()
        {
            _logDetail = true;
            UiActive = true;
        }

        public void LogMessage(string message)
        {
            Debug.WriteLine(message);
        }

        public void LogDetail(string stage)
        {
            if(_logDetail)
                LogMessage(stage);
        }

        public void UpdateProgress(int countCompleted, int countOutOf, string message)
        {
            LogMessage(string.Format("{0}/{1} - {2}", countCompleted, countOutOf, message));
        }

        public bool UiActive { get; set; }
    }
}
