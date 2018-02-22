namespace JosephM.Application.Prism.Module.ReleaseCheckModule
{
    public class UpdateSettings
    {
        public UpdateSettings()
        {
            CheckForNewReleaseOnStartup = true;
        }

        public bool CheckForNewReleaseOnStartup { get; set; }
    }
}
