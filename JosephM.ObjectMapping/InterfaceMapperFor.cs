namespace JosephM.ObjectMapping
{
    public class InterfaceMapperFor<TInterface, TClass>
        : MapperBase
        where TClass : TInterface, new()
    {
        public TClass Map(TInterface from)
        {
            var to = new TClass();
            Map(from, to);
            return to;
        }
    }
}