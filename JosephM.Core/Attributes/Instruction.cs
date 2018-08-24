using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define Instructions To The User In The User Interface When Entering An Object
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Property,
        AllowMultiple = false)]
    public class Instruction : Attribute
    {
        public string Text { get; private set; }

        public Instruction(string text)
        {
            Text = text;
        }
    }
}