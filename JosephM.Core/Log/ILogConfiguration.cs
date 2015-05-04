namespace JosephM.Core.Log
{
    public interface ILogConfiguration
    {
        bool Log { get; }
        string LogFilePath { get; }
        bool LogDetail { get; }
        int LogsPerFile { get; }
    }
}