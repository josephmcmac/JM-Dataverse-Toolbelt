namespace JosephM.TestModule.AllPropertyTypesCompact
{
    public class TestSettingsTypeCompactEnumerated
    {
        private string Name { get; set; }

        public TestSettingsTypeCompactEnumerated()
        {

        }

        public TestSettingsTypeCompactEnumerated(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}
