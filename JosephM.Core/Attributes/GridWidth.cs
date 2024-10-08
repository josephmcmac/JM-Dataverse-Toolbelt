﻿using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute to define the width for the property when displayed in views
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class GridWidth : Attribute
    {
        public int Width { get; private set; }

        public GridWidth(int width)
        {
            Width = width;
        }
    }
}