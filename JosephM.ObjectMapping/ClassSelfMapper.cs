using System;
using JosephM.Core.Extentions;

namespace JosephM.ObjectMapping
{
    public class ClassSelfMapper : MapperBase
    {
        public T Map<T>(T from)
            where T : new()
        {
            var targetType = from.GetType();
            object to;
            if (targetType.HasParameterlessConstructor())
            {
                to = targetType.CreateFromParameterlessConstructor();
                Map(from, to);
            }
            else
                to = from;
            //throw new NullReferenceException(
            //    string.Format(
            //        "Type {0} Is Required To Have A Parameterless Constructor To Be Mapped By This Class",
            //        targetType.Name));

            return (T) to;
        }
    }
}