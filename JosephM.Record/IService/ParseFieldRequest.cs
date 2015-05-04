namespace JosephM.Record.IService
{
    /// <summary>
    ///     Object For Attempting To Convert A Value To The Required Type By The IService Through The ParseField Method
    /// </summary>
    public class ParseFieldRequest
    {
        public ParseFieldRequest(string fieldName, string recordType, object fieldValue)
        {
            FieldName = fieldName;
            RecordType = recordType;
            Value = fieldValue;
        }

        /// <summary>
        ///     The Name Of The Field Which Will Contain The Converted Value
        /// </summary>
        public string FieldName { get; private set; }

        /// <summary>
        ///     The Name Of The Record Type Containing The Field Which Will Contain The Converted Value
        /// </summary>
        public string RecordType { get; private set; }

        /// <summary>
        ///     The Field Value Which May Require Converting To A Type Required by The Service To Save The Field
        /// </summary>
        public object Value { get; private set; }
    }
}