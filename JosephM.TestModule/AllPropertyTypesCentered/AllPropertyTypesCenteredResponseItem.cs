﻿using System;
using JosephM.Core.Service;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.TestModule.AllPropertyTypesCentered
{
    public class AllPropertyTypesCenteredResponseItem : ServiceResponseItem
    {
        public string Type { get; set; }
        public string Type1 { get; set; }
        public string Type2 { get; set; }
        public string Type3 { get; set; }
        [Multiline]
        public string MultipleField { get; set; }

        public Url url {  get { return new Url("http://google.com", "Open Google"); } }

        public AllPropertyTypesCenteredResponseItem(string type, Exception ex)
        {
            Type = type;
            Exception = ex;
        }
    }
}