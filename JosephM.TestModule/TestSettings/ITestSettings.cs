using System.Collections.Generic;

namespace JosephM.TestModule.TestSettings
{
    public interface ITestSettings
    {
        IEnumerable<TestSetting> Settings { get; set; }
    }
}