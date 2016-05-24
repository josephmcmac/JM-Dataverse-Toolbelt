
using JosephM.Core.Attributes;

namespace JosephM.XRM.VSIX
{
    public class XrmPackageSettings
    {
        public string SolutionObjectPrefix { get; set; }
        public string SolutionDynamicsCrmPrefix { get; set; }

        [Hidden]
        public string SolutionObjectInstancePrefix
        {
            get
            {
                if (string.IsNullOrEmpty(SolutionObjectPrefix))
                    return SolutionObjectPrefix;
                if (char.IsLower(SolutionObjectPrefix[0]))
                    return "" + char.ToUpper(SolutionObjectPrefix[0]) + (SolutionObjectPrefix.Length == 1 ? "" : SolutionObjectPrefix.Substring(1));
                if (char.IsUpper(SolutionObjectPrefix[0]))
                    return "" + char.ToLower(SolutionObjectPrefix[0]) + (SolutionObjectPrefix.Length == 1 ? "" : SolutionObjectPrefix.Substring(1));
                return SolutionObjectPrefix;
            }
        }
    }
}
