using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
