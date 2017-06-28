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
using $safeprojectname$.Core;

namespace $safeprojectname$.Xrm
{
    public class XrmSetting : IXrmConfiguration
    {
        #region IXrmConfiguration Members

        public AuthenticationProviderType AuthenticationProviderType { get; set; }
        public string DiscoveryServiceAddress { get; set; }
        public string OrganizationUniqueName { get; set; }
        public string Domain { get; set; }
        public string Username { get; set; }
        public Password Password { get; set; }
        string IXrmConfiguration.Password => Password == null ? null : Password.GetRawPassword();

        #endregion
    }
}