#region

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    /// <summary>
    ///     Not actually showing yet - was attempting to show a sample form in vs
    /// </summary>
    public class FakeMultiSelectDialogViewModel : MultiSelectDialogViewModel<PicklistOption>
    {
        public FakeMultiSelectDialogViewModel()
            : base(GetFakeOptions(), GetFakeOptions().Take(10), (x) => { }, () => { },  new FakeApplicationController())
        {
        }

        private static IEnumerable<PicklistOption> GetFakeOptions()
        {
            for (int i = 1; i <= 50; i++)
            {
                yield return new PicklistOption(i.ToString(), "Option " + i);
            }
        }
    }
}