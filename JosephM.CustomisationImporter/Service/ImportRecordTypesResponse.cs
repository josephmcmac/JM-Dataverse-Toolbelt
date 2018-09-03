using System.Collections.Generic;

namespace JosephM.CustomisationImporter.Service
{
    public class ImportRecordTypesResponse
    {
        private List<string> _createdRecordTypes = new List<string>();
        public IEnumerable<string> CreatedRecordTypes
        {
            get
            {
                return _createdRecordTypes;
            }
        }

        public void AddCreatedRecordType(string recordType)
        {
            _createdRecordTypes.Add(recordType);
        }
    }
}
