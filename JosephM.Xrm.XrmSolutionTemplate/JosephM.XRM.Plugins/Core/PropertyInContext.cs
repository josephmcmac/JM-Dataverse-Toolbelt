using System;

namespace $safeprojectname$.Core
{
    /// <summary>
    ///     Base Attribute Class To Define Rules For A Class Property Being In Context
    ///     Used Too Show/Hide The Property When In/Out Of context
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public abstract class PropertyInContext : Attribute
    {
        /// <summary>
        ///     If In Context Resolves To True For The Objects Current State
        /// </summary>
        public abstract bool IsInContext(object instance);
    }
}