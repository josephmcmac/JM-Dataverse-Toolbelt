using System.Collections.Generic;

namespace JosephM.Prism.TestModule
{
    public class TestViewModel
    {
        public string Blurb
        {
            get { return "This view model is for experimenting"; }
        }

        public TestNestedViewModel TestViewmOdelProperty { get; set; }
        public List<TestNestedViewModel> TestNesteds { get; set; }
    }

    public class TestNestedViewModel
    {
        public string TestNested { get; set; }
    }
}