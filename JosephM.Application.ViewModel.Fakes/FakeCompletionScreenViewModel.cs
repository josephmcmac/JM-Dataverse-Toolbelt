#region

using System.Collections.Generic;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Shared;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeCompletionScreenViewModel : CompletionScreenViewModel
    {
        public FakeCompletionScreenViewModel()
            : base(
                () => { },
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