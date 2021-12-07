using JosephM.Xrm.Vsix.Application;
using System;

namespace JosephM.Xrm.Vsix.App.Extensions
{
    public static class VisualStudioServiceExtensions
    {
        public static string GetSelectedProjectName(this IVisualStudioService visualStudioService)
        {
            var selectedItems = visualStudioService.GetSelectedItems();
            string selectedItemName = null;
            foreach (var item in selectedItems)
            {
                selectedItemName = item.Name;
            }

            return selectedItemName ?? throw new NullReferenceException("Error getting selected project name");
        }
    }
}
