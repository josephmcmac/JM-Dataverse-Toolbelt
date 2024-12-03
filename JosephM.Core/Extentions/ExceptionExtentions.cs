using System;
using System.Linq;

namespace JosephM.Core.Extentions
{
    public static class ExceptionExtentions
    {
        /// <summary>
        ///     Returns a string with a text description of a CLR exception
        ///     Includes the main exception, and the inner exception
        /// </summary>
        public static string DisplayString(this Exception ex)
        {
            if (ex == null)
            {
                return "An unidentified error occured";
            }
            var result = "";
            result = string.Concat(ex.GetType(), ": ", ex.Message, "\n", ex.StackTrace);
            if (ex.InnerException != null)
            {
                result = string.Concat(result, "\nInner Exception: ", ex.InnerException.DisplayString());
            }
            if(ex is AggregateException aggregateException && aggregateException.InnerExceptions != null)
            {
                result = string.Concat(result, "\nInner Exceptions: ", string.Join("\n",  aggregateException.InnerExceptions.Select(ie => ie.DisplayString())));
            }
            return result;
        }
    }
}