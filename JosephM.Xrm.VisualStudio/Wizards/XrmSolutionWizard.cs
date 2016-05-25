using System;
using EnvDTE;
using EnvDTE80;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.VisualStudio.TemplateWizard;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using JosephM.XRM.VSIX.Commands.PackageSettings;
using JosephM.XRM.VSIX.Commands.RefreshConnection;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using Microsoft.VisualStudio.OLE.Interop;

namespace JosephM.XRM.VSIX.Wizards
{
    public class XrmSolutionWizard : MyWizardBase
    {
        private string Directory { get; set; }

        private string SolutionName { get; set; }

        public XrmPackageSettings XrmPackageSettings { get; set; }

        public override void RunStartedExtention(Dictionary<string, string> replacementsDictionary)
        {
            XrmPackageSettings = new XrmPackageSettings();
            #if DEBUG
                XrmPackageSettings.SolutionDynamicsCrmPrefix = "template";
                XrmPackageSettings.SolutionObjectPrefix = "Template";
            #endif
            var settingsDialog = new XrmPackageSettingDialog(DialogUtility.CreateDialogController(), XrmPackageSettings, null, Directory, false);
            DialogUtility.LoadDialog(settingsDialog, showCompletion: false);

            AddReplacements(replacementsDictionary, XrmPackageSettings);

            Directory = replacementsDictionary.ContainsKey("$solutiondirectory$")
                ? replacementsDictionary["$solutiondirectory$"]
                : null;

            SolutionName = replacementsDictionary.ContainsKey("$specifiedsolutionname$")
                ? replacementsDictionary["$specifiedsolutionname$"]
                : replacementsDictionary["safeprojectname"];


            replacementsDictionary.Add("$myprefix$", SolutionName);
        }

        public override void RunFinishedExtention()
        {
            var solution = DTE == null ? null : DTE.Solution as Solution2;
            
            if (solution != null)
            {
                VsixUtility.AddSolutionItem(solution, "xrmpackage.xrmsettings", XrmPackageSettings, Directory);

                var xrmConfig = new XrmRecordConfiguration();
                #if DEBUG
                    xrmConfig.AuthenticationProviderType = XrmRecordAuthenticationProviderType.ActiveDirectory;
                    xrmConfig.DiscoveryServiceAddress = "http://qa2012/XRMServices/2011/Discovery.svc";
                    xrmConfig.Name = "TEST";
                    xrmConfig.OrganizationUniqueName = "TEST";
                    xrmConfig.Username = "joseph";
                    xrmConfig.Domain = "auqa2012";
                #endif
                var dialog = new ConnectionEntryDialog(DialogUtility.CreateDialogController(), xrmConfig, solution,
                    Directory);

                DialogUtility.LoadDialog(dialog, false);

                //DTE.Solution.Projects.Item(0).
            }
        }

        //private void GenerateStrongNameKeyAndToken()
        //{
        //    var clrStrongName = (ICLRStrongName)RuntimeEnvironment.GetRuntimeInterfaceAsObject(new Guid("B79B0ACD-F5CD-409b-B5A5-A16244610B92"), new Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D"));
        //    IntPtr ppbKeyBlob;
        //    int pcbKeyBlob;
        //    clrStrongName.StrongNameKeyGen((string)null, 0, out ppbKeyBlob, out pcbKeyBlob);
        //    VSXTemplateWizard.strongNameKey = new byte[pcbKeyBlob];
        //    Marshal.Copy(ppbKeyBlob, VSXTemplateWizard.strongNameKey, 0, pcbKeyBlob);
        //    IntPtr ppbPublicKeyBlob;
        //    int pcbPublicKeyBlob;
        //    clrStrongName.StrongNameGetPublicKey((string)null, ppbKeyBlob, pcbKeyBlob, out ppbPublicKeyBlob, out pcbPublicKeyBlob);
        //    IntPtr ppbStrongNameToken;
        //    int pcbStrongNameToken;
        //    clrStrongName.StrongNameTokenFromPublicKey(ppbPublicKeyBlob, pcbPublicKeyBlob, out ppbStrongNameToken, out pcbStrongNameToken);
        //    byte[] destination = new byte[pcbStrongNameToken];
        //    Marshal.Copy(ppbStrongNameToken, destination, 0, pcbStrongNameToken);
        //    StringBuilder stringBuilder = new StringBuilder();
        //    for (int index = 0; index < pcbStrongNameToken; ++index)
        //        stringBuilder.Append(destination[index].ToString("x02", (IFormatProvider)CultureInfo.InvariantCulture));
        //    VSXTemplateWizard.publicKeyToken = stringBuilder.ToString();
        //    clrStrongName.StrongNameFreeBuffer(ppbKeyBlob);
        //    clrStrongName.StrongNameFreeBuffer(ppbPublicKeyBlob);
        //    clrStrongName.StrongNameFreeBuffer(ppbStrongNameToken);
        //}
    }
}
