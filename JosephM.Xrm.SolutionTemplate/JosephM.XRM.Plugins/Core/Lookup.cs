namespace $safeprojectname$.Core
{
    public class Lookup
    {
        protected bool Equals(Lookup other)
        {
            return string.Equals(RecordType, other.RecordType) && string.Equals(Id, other.Id);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((RecordType != null ? RecordType.GetHashCode() : 0) * 397) ^ (Id != null ? Id.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Lookup left, Lookup right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Lookup left, Lookup right)
        {
            return !Equals(left, right);
        }

        public Lookup()
        {
        }

        public Lookup(string recordType, string id, string name)
        {
            RecordType = recordType;
            Id = id;
            Name = name;
        }

        public string RecordType { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Lookup;
            return other != null && Equals(other);
        }
    }
}