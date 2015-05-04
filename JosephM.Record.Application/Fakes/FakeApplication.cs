#region

using System.Threading;
using JosephM.Record.Application.ApplicationOptions;

#endregion

namespace JosephM.Record.Application.Fakes
{
    /// <summary>
    ///     Object for access to the main UI thread and adding or removing UI items
    /// </summary>
    public class FakeApplication : ApplicationViewModel
    {
        public FakeApplication()
            : base(new FakeApplicationController())
        {
        }
    }
}