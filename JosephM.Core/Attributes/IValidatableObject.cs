using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JosephM.Core.Service;

namespace JosephM.Core.Attributes
{
    public interface IValidatableObject
    {
        IsValidResponse Validate();
    }
}
