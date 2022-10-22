﻿using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Xrm;
using System.Collections.Generic;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerDialog :
        ServiceRequestDialog<InstanceComparerService, InstanceComparerRequest, InstanceComparerResponse, InstanceComparerResponseItem>
    {
        public InstanceComparerDialog(IOrganizationConnectionFactory serviceFactory, DialogController dialogController)
            : base(new InstanceComparerService(serviceFactory), dialogController)
        {
        }

        protected override bool UseProgressControlUi => true;

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            if(Response.Summary != null)
            {
                foreach (var summaryItem in Response.Summary)
                {
                    addProperty($"{summaryItem.Type} Difference Count", summaryItem.Total.ToString());
                }
            }
            return dictionary;
        }
    }
}