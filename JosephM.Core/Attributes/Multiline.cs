using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define A String As Multiline
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Property,
        AllowMultiple = false)]
    public class Multiline : Attribute
    {
        public int NumberOfLines { get; set; }

        public Multiline()
        {
            
        }

        public Multiline(int numberOfLines)
        {
            NumberOfLines = numberOfLines;
        }
    }
}