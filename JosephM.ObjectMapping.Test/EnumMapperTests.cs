using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.ObjectMapping.Test
{
    [TestClass]
    public abstract class EnumMapperTests<TMapper, TType1, TType2>
        where TMapper : EnumMapper<TType1, TType2>, new()
        where TType1 : new()
        where TType2 : new()
    {
        protected void EnumMapperTest()
        {
            var mapper = new TMapper();

            foreach (var enumOption in mapper.GetEnum1Options())
            {
                var mappedOption = mapper.Map(enumOption);
                Assert.IsNotNull(mappedOption);
            }

            foreach (var enumOption in mapper.GetEnum2Options())
            {
                var mappedOption = mapper.Map(enumOption);
                Assert.IsNotNull(mappedOption);
            }
        }
    }
}