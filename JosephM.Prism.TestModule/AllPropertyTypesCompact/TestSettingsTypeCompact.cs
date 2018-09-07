using System.Collections.Generic;

namespace JosephM.TestModule.AllPropertyTypesCompact
{
    public class TestSettingsTypeCompact
    {
        public IEnumerable<TestSettingsTypeCompactEnumerated> Items { get; set; }

        public static TestSettingsTypeCompact Create()
        {
            return new TestSettingsTypeCompact
            {
                Items = new[]
                {
                new TestSettingsTypeCompactEnumerated("Foo1"),
                new TestSettingsTypeCompactEnumerated("Foo2"),
                new TestSettingsTypeCompactEnumerated("Foo3"),
            }
            };
        }
    }
}
