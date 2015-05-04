#region

using System.Collections.Generic;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Application.Shared;

#endregion

namespace JosephM.Record.Application.Fakes
{
    public class FakeCompletionScreenViewModel : CompletionScreenViewModel
    {
        public FakeCompletionScreenViewModel()
            : base(
                () => { },
                "This Has Coimpleted Processing",
                CreateFakeCompletionOptions(),
                null,
                new FakeApplicationController())
        {
        }

        public static IEnumerable<XrmButtonViewModel> CreateFakeCompletionOptions()
        {
            return new XrmButtonViewModel[]
            {
                new XrmButtonViewModel("This Is To Do Something", () => { }, new FakeApplicationController())
            };
        }
    }
}