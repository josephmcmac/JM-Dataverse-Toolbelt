using JosephM.TestModule.AllPropertyTypesCentered;
using System.Collections.Generic;

namespace JosephM.TestModule.AllPropertyTypesCentered
{
    public class TestSettingsTypeCentered
    {
        public IEnumerable<TestSettingsTypeCenteredEnumerated> Items { get; set; }

        public static TestSettingsTypeCentered Create()
        {
            return new TestSettingsTypeCentered
            {
                Items = new[]
                {
                new TestSettingsTypeCenteredEnumerated("Foo1"),
                new TestSettingsTypeCenteredEnumerated("Foo2"),
                new TestSettingsTypeCenteredEnumerated("Foo3"),
            }
            };
        }
    }
}
