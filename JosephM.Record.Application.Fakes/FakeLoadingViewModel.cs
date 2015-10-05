#region

using JosephM.Application.ViewModel.Shared;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    /// <summary>
    ///     Not actually showing yet - was attempting to show a sample form in vs
    /// </summary>
    public class FakeLoadingViewModel : LoadingViewModel
    {
        public FakeLoadingViewModel()
            : base(new FakeApplicationController())
        {
            LoadingMessage = "Fake Loading Message";
            IsLoading = true;
        }
    }
}