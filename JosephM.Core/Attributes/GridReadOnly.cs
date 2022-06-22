using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Defines field will always be read only when displayed in a grid
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class GridReadOnly : Attribute
    {
    }
}