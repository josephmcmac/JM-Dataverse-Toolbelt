using JosephM.Core.Attributes;

namespace JosephM.Xrm.Vsix.Module.UpdateAssembly
{
    [Group(Sections.Message, Group.DisplayLayoutEnum.HorizontalInputOnly, order: -1, displayLabel: false)]
    public class UpdateAssemblyResponse
    {
        [Group(Sections.Message)]
        public string CompletionMessage { get; set; }

        private static class Sections
        {
            public const string Message = "Message";
        }
    }
}