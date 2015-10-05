using System;

namespace JosephM.Application.Modules
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = true)]
    public class DependantModuleAttribute : Attribute
    {
        public Type DependantModule { get; private set; }

        public DependantModuleAttribute(Type module)
        {
            DependantModule = module;
        }
    }
}
