using System.Runtime.Serialization;

namespace JosephM.Core.FieldType
{
    [DataContract]
    public class PicklistOption
    {
        protected bool Equals(PicklistOption other)
        {
            return string.Equals(Key, other.Key);
        }

        public override int GetHashCode()
        {
            return (Key != null ? Key.GetHashCode() : 0);
        }

        public static bool operator ==(PicklistOption left, PicklistOption right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PicklistOption left, PicklistOption right)
        {
            return !Equals(left, right);
        }

        public PicklistOption()
        {
        }

        public PicklistOption(string key, string value)
        {
            Key = key;
            Value = value;
        }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as PicklistOption;
            return other != null && Equals(other);
        }
    }
}