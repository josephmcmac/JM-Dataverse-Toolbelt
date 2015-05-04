#region

using JosephM.Record.Application.Shared;

#endregion

namespace JosephM.Record.Application.Fakes
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
            LoadingObjects.Add(new object(), "Fake Loading Message");
            OnPropertyChanged("IsLoading");
        }
    }
}