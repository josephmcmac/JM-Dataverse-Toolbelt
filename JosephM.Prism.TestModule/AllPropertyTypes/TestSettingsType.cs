using System.Collections.Generic;

namespace JosephM.TestModule.AllPropertyTypes
{
    public class TestSettingsType
    {
        public IEnumerable<TestSettingsTypeEnumerated> Items { get; set; }

        public static TestSettingsType Create()
        {
            return new TestSettingsType
            {
                Items = new[]
                {
                new TestSettingsTypeEnumerated("Foo1"),
                new TestSettingsTypeEnumerated("Foo2"),
                new TestSettingsTypeEnumerated("Foo3"),
            }
            };
        }
    }
}
