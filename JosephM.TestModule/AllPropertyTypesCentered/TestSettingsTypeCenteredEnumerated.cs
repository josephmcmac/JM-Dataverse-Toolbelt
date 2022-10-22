namespace JosephM.TestModule.AllPropertyTypesCentered
{
    public class TestSettingsTypeCenteredEnumerated
    {
        private string Name { get; set; }

        public TestSettingsTypeCenteredEnumerated()
        {

        }

        public TestSettingsTypeCenteredEnumerated(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}
