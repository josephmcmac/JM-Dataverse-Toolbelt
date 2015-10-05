namespace JosephM.Application.ViewModel.Navigation
{
    public interface INavigationProvider
    {
        string GetValue(string key);
        T GetObject<T>(string key);
    }
}