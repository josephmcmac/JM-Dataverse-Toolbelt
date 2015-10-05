using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JosephM.Record.Metadata;

namespace JosephM.Record.Service
{
    public class ObjectRecordMetadata : RecordMetadata
    {
        public override string PrimaryFieldSchemaName { get { return "ToString"; } }
        public override string PrimaryFieldDescription { get { return "ToString Method"; } }
        public override string PrimaryFieldDisplayName { get { return "To String"; } }
    }
}
