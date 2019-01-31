using System;

namespace JosephM.Core.AppConfig
{
    public interface IResolveObject
    {
        object ResolveType(Type type);
    }
}
