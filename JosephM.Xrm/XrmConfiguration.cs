#region

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xrm.Sdk.Client;

#endregion

// These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
// located in the SDK\bin folder of the SDK download.

namespace JosephM.Xrm
{
    public class XrmConfiguration : IXrmConfiguration
    {
        #region IXrmConfiguration Members

        public AuthenticationProviderType AuthenticationProviderType { get; set; }
        public string DiscoveryServiceAddress { get; set; }
        public string OrganizationUniqueName { get; set; }
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        #endregion
    }
}