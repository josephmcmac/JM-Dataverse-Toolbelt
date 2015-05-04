namespace JosephM.Record.IService
{
    /// <summary>
    ///     Object For Response When Attempting To Convert A Value To The Required Type By The IService Through The ParseField
    ///     Method
    /// </summary>
    public class ParseFieldResponse
    {
        /// <summary>
        ///     Creates A Successful Response Containing The New Field Value
        /// </summary>
        public ParseFieldResponse(object parsedValue)
        {
            Success = true;
            ParsedValue = parsedValue;
        }

        /// <summary>
        ///     Creates An Unsuccessful Response Containing A Description Of The Error Captured When Converting The Value
        /// </summary>
        public ParseFieldResponse(string error)
        {
            Success = false;
            Error = error;
        }

        /// <summary>
        ///     If No Error Was Captured Converting The Value
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        ///     A Description Of The Error If One Was Captured Converting The Value
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        ///     The Value Which May Have Been Converted To The Correct Type
        /// </summary>
        public object ParsedValue { get; private set; }
    }
}