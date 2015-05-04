#region

using System;

#endregion

namespace JosephM.Core.Log
{
    /// <summary>
    ///     Outputs to a log file
    /// </summary>
    public class ConsoleUserInterface : IUserInterface
    {
        private readonly bool _logDetail;

        public ConsoleUserInterface(bool logDetail)
        {
            _logDetail = logDetail;
            UiActive = true;
        }

        private void WriteLine(object output)
        {
            Console.WriteLine(output);
        }

        public void UpdateProgress(int countCompleted, int countOutOf, string message)
        {
        }

        public void LogDetail(string detail)
        {
            if (_logDetail)
                WriteLine(detail);
        }


        public void LogMessage(string message)
        {
            WriteLine(message);
        }

        public bool UiActive { get; set; }
    }
}