using System;
using System.Collections.Generic;

namespace JosephM.Record.IService
{
    public class ExecuteQueryResponse
    {
        public IEnumerable<IRecord> Records { get; set; }

        public Exception Exception { get; set; }
    }
}
