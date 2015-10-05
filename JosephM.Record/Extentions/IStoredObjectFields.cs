namespace JosephM.Record.Extentions
{
    public interface IStoredObjectFields
    {
        string RecordType { get; }

        string ValueField { get; }

        string AssemblyField { get; }

        string TypeQualfiedNameField { get; }

        string TypeField { get; }
    }
}
