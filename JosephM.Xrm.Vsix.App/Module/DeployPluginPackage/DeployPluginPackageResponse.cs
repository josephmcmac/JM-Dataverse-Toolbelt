using JosephM.Core.Attributes;

namespace JosephM.Xrm.Vsix.Module.DeployPluginPackage
{
    [Group(Sections.Message, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly, order: -1, displayLabel: false)]
    public class DeployPluginPackageResponse
    {
        [Group(Sections.Message)]
        public string CompletionMessage { get; set; }

        private static class Sections
        {
            public const string Message = "Message";
        }
    }
}