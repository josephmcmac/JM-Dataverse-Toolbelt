using System.Collections.Generic;
using JosephM.Core.Attributes;

namespace JosephM.Prism.TestModule.Prism.TestSettings
{
    public class TestSettings : ITestSettings
    {
        [FormEntry]
        public IEnumerable<TestSetting> Settings { get; set; }
    }
}
