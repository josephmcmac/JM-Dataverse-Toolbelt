using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JosephM.Core.AppConfig
{
    public interface IResolveObject
    {
        object ResolveType(Type type);
    }
}
