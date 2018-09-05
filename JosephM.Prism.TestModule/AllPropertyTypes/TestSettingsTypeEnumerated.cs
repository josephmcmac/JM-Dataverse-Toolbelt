namespace JosephM.TestModule.AllPropertyTypes
{
    public class TestSettingsTypeEnumerated
    {
        private string Name { get; set; }

        public TestSettingsTypeEnumerated()
        {

        }

        public TestSettingsTypeEnumerated(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}
