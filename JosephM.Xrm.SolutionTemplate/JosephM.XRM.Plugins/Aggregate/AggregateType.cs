namespace $safeprojectname$.Aggregate
{
    /// <summary>
    ///     The type of aggregate
    /// </summary>
    public enum AggregateType
    {
        Exists = 1,
        Count = 2,
        Sum = 3,
        Min = 4,
        CSV = 5,
        PSV = 6
    }
}