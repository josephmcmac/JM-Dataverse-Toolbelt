using System;
using System.Collections.Generic;

namespace JosephM.Record.Test
{
    public class TestClass
    {
        public string String { get; set; }
        public int Int { get; set; }
        public bool Bool { get; set; }

        public IEnumerable<NestedClass> EnumerableObjects { get; set; }

        public class NestedClass
        {
            public string String { get; set; }
            public int Int { get; set; }
            public bool Bool { get; set; }
        }
    }
}