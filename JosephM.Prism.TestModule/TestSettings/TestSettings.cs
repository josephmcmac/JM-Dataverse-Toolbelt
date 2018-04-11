using System.Collections.Generic;
using JosephM.Core.Attributes;

namespace JosephM.TestModule.TestSettings
{
    public class TestSettings : ITestSettings
    {
        [FormEntry]
        public IEnumerable<TestSetting> Settings { get; set; }
    }
}
