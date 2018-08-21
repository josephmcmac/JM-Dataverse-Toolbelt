using System;

namespace JosephM.Application.ViewModel.Attributes
{
    /// <summary>
    /// Attribute declares that the dialog requires a connection and should be redirected to enter one if not present
    /// </summary>
    public class RequiresConnection : Attribute
    {
        public RequiresConnection()
        {
            
        }

        public RequiresConnection(string processEnteredSettingsMethodName)
        {
            ProcessEnteredSettingsMethodName = processEnteredSettingsMethodName;
        }

        public string ProcessEnteredSettingsMethodName { get; }
    }
}
