using JosephM.Core.Attributes;

namespace JosephM.Core.Log
{
    public class LogConfiguration : ILogConfiguration
    {
        [RequiredProperty]
        public bool Log { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("Log", true)]
        public string LogFilePath { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("Log", true)]
        public bool LogDetail { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("Log", true)]
        public int LogsPerFile { get; set; }
    }
}