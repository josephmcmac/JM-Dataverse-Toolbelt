using System;
using System.Collections;
using JosephM.Core.Extentions;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define A Property As Required To Be Non-Empty To Be Valid
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class RequiredProperty : PropertyValidator
    {
        public override bool IsValid(object value)
        {
            if (value is IEnumerable)
            {
                var enumerator = ((IEnumerable)value).GetEnumerator();
                if (enumerator.MoveNext())
                {
                    var firstOne = enumerator.Current;
                    if (firstOne is ISelectable)
                    {
                        var isOneSelelected = ((ISelectable)firstOne).Selected;
                        while(enumerator.MoveNext())
                        {
                            var nextOne = enumerator.Current;
                            if (((ISelectable)nextOne).Selected)
                                isOneSelelected = true;
                        }
                        return isOneSelelected;
                    }
                }
            }
            return value.IsNotEmpty();
        }

        public override string GetErrorMessage(string propertyLabel)
        {
            return string.Format("{0} Is Required", propertyLabel);
        }
    }
}