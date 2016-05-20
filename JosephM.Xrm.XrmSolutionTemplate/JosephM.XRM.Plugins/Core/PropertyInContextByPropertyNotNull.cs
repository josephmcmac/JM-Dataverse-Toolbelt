using System;

namespace $safeprojectname$.Core
{
    /// <summary>
    ///     Defines The Property With The Attribute In Context If The Property With A Given Name Is Not Empty
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class PropertyInContextByPropertyNotNull : PropertyInContext
    {
        public string PropertyDependency { get; set; }

        public PropertyInContextByPropertyNotNull(string prepertyDependency)
        {
            PropertyDependency = prepertyDependency;
        }

        public override bool IsInContext(object instance)
        {
            var propertyValue = instance.GetPropertyValue(PropertyDependency);
            return (propertyValue != null);
        }
    }
}