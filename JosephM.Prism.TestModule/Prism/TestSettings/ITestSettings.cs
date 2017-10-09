using System.Collections.Generic;

namespace JosephM.Prism.TestModule.Prism.TestSettings
{
    public interface ITestSettings
    {
        IEnumerable<TestSetting> Settings { get; set; }
    }
}