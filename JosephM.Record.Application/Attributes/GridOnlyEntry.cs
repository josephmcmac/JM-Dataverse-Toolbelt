using System;

namespace JosephM.Application.ViewModel.Attributes
{
    public class GridOnlyEntry : Attribute
    {
        public string EnumerableProperty;

        public GridOnlyEntry(string enumerableProperty)
        {
            EnumerableProperty = enumerableProperty;
        }
    }
}