using System;

namespace $safeprojectname$.Core
{
    /// <summary>
    ///     Attribute To Define An Alternative Display Name For A Class Type Through The TypeEntentions.GetDisplayName Method
    /// </summary>
    public class DisplayNameAttribute : Attribute
    {
        public string Label { get; private set; }

        public DisplayNameAttribute(string label)
        {
            Label = label;
        }
    }
}