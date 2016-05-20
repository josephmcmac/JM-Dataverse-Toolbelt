#region

using System;
using System.IO;

#endregion

namespace $safeprojectname$.Core
{
    /// <summary>
    ///     Outputs to a log file
    /// </summary>
    public class LogFileUserInterface : IUserInterface
    {
        private readonly Object _lockObject = new Object();
        private readonly string _fileNamePrefix;
        private readonly string _filepath;
        private readonly bool _logDetail;
        private int _currentEntryCount;
        private string _currentFile;

        public LogFileUserInterface(string filepath, string fileNamePrefix, bool logDetail)
        {
            _logDetail = logDetail;
            _filepath = filepath;
            _fileNamePrefix = fileNamePrefix;
            UiActive = true;
            CreateNewPhysicalLogFile();
            LogsPerFile = 5000;
        }

        private void CreateNewPhysicalLogFile()
        {
            lock (_lockObject)
            {
                var newFileName = string.Concat(_fileNamePrefix, " - ", DateTime.Now.ToFileTime().ToString());

                if (!Directory.Exists(_filepath))
                    Directory.CreateDirectory(_filepath);

                _currentFile = _filepath + "\\" + newFileName + ".txt";

                // Create a file to write to.
                using (var sw = File.CreateText(_currentFile))
                {
                    sw.WriteLine("Beginning Logging");
                }
            }
        }

        private void WriteLine(object output)
        {
            lock (_lockObject)
            {
                using (var sw = File.AppendText(_currentFile))
                {
                    sw.WriteLine(DateTime.Now + ": " + output);
                    _currentEntryCount++;
                }
                if (_currentEntryCount > LogsPerFile)
                {
                    using (var sw = File.AppendText(_currentFile))
                    {
                        sw.WriteLine("Creating new log file");
                    }
                    CreateNewPhysicalLogFile();
                    _currentEntryCount = 0;
                }
            }
        }

        public void UpdateProgress(int countCompleted, int countOutOf, string message)
        {
            if (_logDetail)
                WriteLine(string.Format("{0} {1}/{2}", message, countCompleted, countOutOf));
        }

        public bool UiActive { get; set; }

        public int LogsPerFile { get; set; }

        public void LogDetail(string detail)
        {
            if (_logDetail)
                WriteLine(detail);
        }


        public void LogMessage(string message)
        {
            WriteLine(message);
        }
    }
}