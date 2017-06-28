#region

using System;

#endregion

namespace $safeprojectname$.Core
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

        private int LastPercentProgress { get; set; }
        public void UpdateProgress(int countCompleted, int countOutOf, string message)
        {
            var percentProgress = Convert.ToInt32(Convert.ToDecimal(countCompleted) / Convert.ToDecimal(countOutOf) * (decimal)100);
            if (percentProgress < LastPercentProgress)
                LastPercentProgress = 0;
            while (LastPercentProgress < percentProgress)
            {
                LastPercentProgress++;
                if (LastPercentProgress % 10 == 0)
                    Console.Write(LastPercentProgress + "%");
                else
                    Console.Write(".");
            }
            if (LastPercentProgress == 100)
                Console.WriteLine();
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