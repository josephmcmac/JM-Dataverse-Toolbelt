using System;

namespace JosephM.Application.Options
{
    public interface IApplicationOption
    {
        string Description { get; }
        string Label { get; }
        void InvokeMethod();
    }
}
