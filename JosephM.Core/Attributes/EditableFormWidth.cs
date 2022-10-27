using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute to define the initial and minimum width for the property when displayed editable on forms
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class EditableFormWidth : Attribute
    {
        public int Width { get; private set; }

        public EditableFormWidth(int width)
        {
            Width = width;
        }
    }
}