namespace JosephM.Core.Log
{
    /// <summary>
    ///     Interface for a user interface
    /// </summary>
    public interface IUserInterface
    {
        void LogMessage(string message);
        void LogDetail(string stage);
        void UpdateProgress(int countCompleted, int countOutOf, string message);
        bool UiActive { get; set; }
    }
}