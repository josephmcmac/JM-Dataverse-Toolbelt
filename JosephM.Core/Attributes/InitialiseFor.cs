using System;
namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Defines The Property With The Attribute In Context If The Property With A Given Name Has One Of The Given Values
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class InitialiseFor : Attribute
    {
        public string PropertyDependency { get; set; }
        public object ForValue { get; set; }
        public object InitialValue { get; set; }

        public InitialiseFor(string propertyDependency, object forValue, object initialValue)
        {
            PropertyDependency = propertyDependency;
            ForValue = forValue;
            InitialValue = initialValue;
        }
    }
}