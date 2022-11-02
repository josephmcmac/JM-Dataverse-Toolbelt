﻿using System;

namespace JosephM.Core.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class AllowDownload : Attribute
    {
    }
}