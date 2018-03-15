using System;

namespace $safeprojectname$.Core
{
    public static class ExceptionExtentions
    {
        /// <summary>
        ///     Returns a string with a text description of a CLR exception
        ///     Includes the main exception, and the inner exception
        /// </summary>
        public static string DisplayString(this Exception ex)
        {
            var result = "";
            if (ex != null)
            {
                result = string.Concat(ex.GetType(), ": ", ex.Message, "\n", ex.StackTrace);
                if (ex.InnerException != null)
                    result = string.Concat(result, "\n", ex.InnerException.DisplayString());
            }
            return result;
        }
    }
}