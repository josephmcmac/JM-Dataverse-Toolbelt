using JosephM.Record.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.CustomisationImporter.Service
{
    public class ImportFieldsResponse
    {
        private List<FieldMetadata> _created = new List<FieldMetadata>();
        public IEnumerable<FieldMetadata> CreatedFields
        {
            get
            {
                return _created;
            }
        }

        public void AddCreatedField(FieldMetadata fieldMetadata)
        {
            _created.Add(fieldMetadata);
        }
    }
}
