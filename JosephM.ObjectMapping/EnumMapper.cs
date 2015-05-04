#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace JosephM.ObjectMapping
{
    public class EnumMapper<TEnum1, TEnum2>
    {
        protected virtual bool AllowUnmapped
        {
            get { return true; }
        }

        protected virtual IEnumerable<TypeMapping<TEnum1, TEnum2>> Mappings
        {
            get { return new TypeMapping<TEnum1, TEnum2>[0]; }
        }

        public virtual TEnum1 Map(TEnum2 tEnum2)
        {
            if (Mappings != null && Mappings.Any(m => m.Type2.Equals(tEnum2)))
                return Mappings.First(m => m.Type2.Equals(tEnum2)).Type1;
            if (GetEnum1Options().Any(e => e.ToString().ToLower() == tEnum2.ToString().ToLower()))
                return GetEnum1Options().First(e => e.ToString().ToLower() == tEnum2.ToString().ToLower());
            if (AllowUnmapped)
                return DefaultEnum1Option;
            throw new ArgumentOutOfRangeException("tEnum2", string.Format("No Mapped Type For Value {0}", tEnum2));
        }

        public virtual TEnum2 Map(TEnum1 tEnum1)
        {
            if (Mappings != null && Mappings.Any(m => m.Type1.Equals(tEnum1)))
                return Mappings.First(m => m.Type1.Equals(tEnum1)).Type2;
            if (GetEnum2Options().Any(e => e.ToString() == tEnum1.ToString()))
                return GetEnum2Options().First(e => e.ToString() == tEnum1.ToString());
            if (AllowUnmapped)
                return DefaultEnum2Option;
            throw new ArgumentOutOfRangeException("tEnum1", string.Format("No Mapped Type For Value {0}", tEnum1));
        }

        protected virtual TEnum1 DefaultEnum1Option
        {
            get { return GetEnum1Options().First(); }
        }

        protected virtual TEnum2 DefaultEnum2Option
        {
            get { return GetEnum2Options().First(); }
        }

        internal IEnumerable<TEnum1> GetEnum1Options()
        {
            var items = new List<TEnum1>();
            var options = typeof (TEnum1).GetEnumValues();
            foreach (var option in options)
            {
                items.Add((TEnum1) option);
            }
            return items;
        }

        internal IEnumerable<TEnum2> GetEnum2Options()
        {
            var items = new List<TEnum2>();
            var options = typeof (TEnum2).GetEnumValues();
            foreach (var option in options)
            {
                items.Add((TEnum2) option);
            }
            return items;
        }

        protected class TypeMapping<TType1, TType2>
        {
            public TypeMapping(TType1 t, TType2 u)
            {
                Type1 = t;
                Type2 = u;
            }

            internal TType1 Type1 { get; set; }
            internal TType2 Type2 { get; set; }
        }
    }
}