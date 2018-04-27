using JosephM.Core.Service;

namespace JosephM.Core.Attributes
{
    public interface IValidatableObject
    {
        IsValidResponse Validate();
    }
}
