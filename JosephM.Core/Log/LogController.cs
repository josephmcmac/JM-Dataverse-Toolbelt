#region

using System.Collections.Generic;

#endregion

namespace JosephM.Core.Log
{
    public class LogController
    {
        private readonly List<IUserInterface> _uis = new List<IUserInterface>();
        private readonly List<IUserInterface> _uisLevel2 = new List<IUserInterface>();

        public LogController(ILogConfiguration logConfiguration, string applicationName)
        {
            if (logConfiguration.Log)
            {
                var logFile = new LogFileUserInterface(logConfiguration.LogFilePath, applicationName,
                    logConfiguration.LogDetail, logConfiguration.LogsPerFile);
                _uis.Add(logFile);
            }
        }

        public LogController()
        {
        }

        public LogController(IUserInterface ui)
        {
            _uis.Add(ui);
        }

        public void AddUi(IUserInterface uis)
        {
            _uis.Add(uis);
        }

        public LogController(IUserInterface ui, IUserInterface ui2)
        {
            _uis.Add(ui);
            _uis.Add(ui2);
        }

        public LogController(IEnumerable<IUserInterface> uis)
        {
            _uis.AddRange(uis);
        }

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

        public void UpdateProgress(int countCompleted, int countOutOf, string message)
        {
            foreach (var ui in _uis)
                ui.UpdateProgress(countCompleted, countOutOf, message);
        }

        public void UpdateLevel2Progress(int countCompleted, int countOutOf, string message)
        {
            foreach (var ui in _uisLevel2)
                ui.UpdateProgress(countCompleted, countOutOf, message);
        }

        public void AddLevel2Ui(IUserInterface userInterface)
        {
            _uisLevel2.Add(userInterface);
        }

        public LogController GetLevel2Controller()
        {
            return new LogController(_uisLevel2);
        }

        public void TurnOffLevel2()
        {
            foreach (var ui in _uisLevel2)
                ui.UiActive = false;
        }
    }
}