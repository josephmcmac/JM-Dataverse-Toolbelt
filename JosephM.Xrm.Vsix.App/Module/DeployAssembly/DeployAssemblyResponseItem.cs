using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    public class DeployAssemblyResponseItem : ServiceResponseItem
    {
        public DeployAssemblyResponseItem(string changeMade, string name, Exception exception = null)
        {
            Exception = exception;
            ChangeMade = changeMade;
            Name = name;
        }


        [DisplayOrder(10)]
        [GridWidth(135)]
        public string ChangeMade { get; set; }

        [DisplayOrder(20)]
        [GridWidth(350)]
        public string Name { get; set; }
    }
}