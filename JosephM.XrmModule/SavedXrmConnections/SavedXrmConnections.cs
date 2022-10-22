using System.Collections.Generic;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;

namespace JosephM.XrmModule.SavedXrmConnections
{
    [Group(Sections.Main, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly)]
    [GridOnlyEntry(nameof(Connections))]
    public class SavedXrmConnections : ISavedXrmConnections
    {
        [Group(Sections.Main)]
        [FormEntry]
        public IEnumerable<SavedXrmRecordConfiguration> Connections { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
        }
    }
}
