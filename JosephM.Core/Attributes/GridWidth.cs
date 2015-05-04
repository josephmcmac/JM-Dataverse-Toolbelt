using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define An Alternative Display Name For A Class Type Through The TypeEntentions.GetDisplayName Method
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class GridWidth : Attribute
    {
        public int Width { get; private set; }

        public GridWidth(int width)
        {
            Width = width;
        }
    }
}