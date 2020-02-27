using System;

namespace JosephM.Application.ViewModel.Attributes
{
    /// <summary>
    /// Attribute declares that the dialog requires a connection and should be redirected to enter one if not present
    /// </summary>
    public class RequiresConnection : Attribute
    {
        public RequiresConnection(string processEnteredSettingsMethodName = null, string escapeConnectionCheckProperty = null)
        {
            ProcessEnteredSettingsMethodName = processEnteredSettingsMethodName;
            EscapeConnectionCheckProperty = escapeConnectionCheckProperty;
        }

        public string ProcessEnteredSettingsMethodName { get; }
        public string EscapeConnectionCheckProperty { get; }
    }
}
