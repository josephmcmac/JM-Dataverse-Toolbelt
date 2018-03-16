using System.Collections.Generic;

namespace $safeprojectname$.Core
{
    public class LogController
    {
        private readonly List<IUserInterface> _uis = new List<IUserInterface>();

        public void LogLiteral(string literal)
        {
            foreach (var ui in _uis)
                ui.LogMessage(literal);
        }

        public void LogDetail(string text)
        {
            foreach (var ui in _uis)
                ui.LogDetail(text);
        }

        public void AddUi(IUserInterface userInterface)
        {
            _uis.Add(userInterface);
        }

        public void UpdateProgress(int countCompleted, int countOutOf, string message)
        {
            foreach (var ui in _uis)
                ui.UpdateProgress(countCompleted, countOutOf, message);
        }
    }
}