namespace JosephM.ObjectMapping
{
    public class ClassMapperFor<TType1, TType2>
        : MapperBase
        where TType1 : new()
        where TType2 : new()
    {
        public TType1 Map(TType2 type2)
        {
            var type1 = new TType1();
            Map(type2, type1);
            return type1;
        }

        public TType2 Map(TType1 type1)
        {
            var type2 = new TType2();
            Map(type1, type2);
            return type2;
        }
    }
}