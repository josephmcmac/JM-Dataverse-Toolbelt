using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Extentions;

namespace JosephM.ObjectMapping.Test
{
    [TestClass]
    public abstract class InterfaceMapperTests<TMapper, TInterface, TType> : MappingTest
        where TType : TInterface, new()
        where TMapper : InterfaceMapperFor<TInterface, TType>, new()
    {
        protected InterfaceMapperTests()
            : base(new TMapper())
        {
        }

        protected void ClassMapperTest()
        {
            if (!typeof (TMapper).HasParameterlessConstructor())
                throw new Exception(string.Format("Type {0} Must Have Parameterless Constructor To Create",
                    typeof (TMapper).Name));
            var lookupMapper = (TMapper) typeof (TMapper).CreateFromParameterlessConstructor();
            var typefrom = new TType();
            var mapped = lookupMapper.Map(typefrom);
            ValidateMapped(mapped, typefrom);
            PopulateObject(typefrom);
            mapped = lookupMapper.Map(typefrom);
            ValidateMapped(mapped, typefrom);
        }
    }
}