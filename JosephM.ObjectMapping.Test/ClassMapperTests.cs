using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.ObjectMapping.Test
{
    [TestClass]
    public abstract class ClassMapperTests<TMapper, TType1, TType2> : MappingTest
        where TType1 : new()
        where TType2 : new()
        where TMapper : ClassMapperFor<TType1, TType2>, new()
    {
        protected ClassMapperTests()
            : base(new TMapper())
        {
        }

        private TMapper ClassMapper
        {
            get { return (TMapper) Mapper; }
        }

        protected void ClassMapperTest()
        {
            var typeFrom = new TType1();
            var mapped = ClassMapper.Map(typeFrom);
            ValidateMapped(mapped, typeFrom);
            PopulateObject(typeFrom);
            mapped = ClassMapper.Map(typeFrom);
            ValidateMapped(mapped, typeFrom);
            var typeFrom2 = new TType2();
            var type2MappedTo1 = ClassMapper.Map(typeFrom2);
            ValidateMapped(type2MappedTo1, typeFrom2);
            PopulateObject(typeFrom2);
            type2MappedTo1 = ClassMapper.Map(typeFrom2);
            ValidateMapped(type2MappedTo1, typeFrom2);
        }
    }
}