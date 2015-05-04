using System;
namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Defines The Property With The Attribute In Context If The Property With A Given Name Has One Of The Given Values
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class FormEntry : Attribute
    {
    }
}