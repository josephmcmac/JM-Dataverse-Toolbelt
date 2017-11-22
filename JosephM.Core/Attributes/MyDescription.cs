using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define An Alternative Display Name For A Class Type Through The TypeEntentions.GetDisplayName Method
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Property,
        AllowMultiple = false)]
    public class MyDescription : Attribute
    {
        public string Text { get; private set; }

        public MyDescription(string text)
        {
            Text = text;
        }
    }
}