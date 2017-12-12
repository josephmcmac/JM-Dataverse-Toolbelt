using System;
using System.Collections.Generic;

namespace JosephM.Core.Service
{
    public interface IProcessCompletion
    {
        bool Success { get; }
        Exception Exception { get; }

        IEnumerable<object> GetResponseItemsWithError();
    }
}
