namespace JosephM.Xrm.Test
{
    public class TestXrmTest : XrmTest
    {
        public override IXrmConfiguration XrmConfiguration
        {
            get { return new TestXrmConfiguration(); }
        }
    }
}