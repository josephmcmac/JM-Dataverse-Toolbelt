using Microsoft.VisualStudio.Shell;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("JosephM.Xrm.Vsix")]
[assembly: AssemblyCompany("JosephM")]
[assembly: AssemblyProduct("JosephM.Xrm.Vsix")]
[assembly: ProvideBindingRedirection(AssemblyName = "Microsoft.IdentityModel.Clients.ActiveDirectory",
        PublicKeyToken = "31bf3856ad364e35",
        Culture = "neutral",
        NewVersion = "5.2.2.0",
        OldVersionLowerBound = "0.0.0.0",
        OldVersionUpperBound = "5.2.2.0")]


//WARNING!! DON:T CHANGE THIS UNLESS ALSO CHANGING THE REFERENCING .vstemplate files
[assembly: AssemblyVersion("2.0.0.0")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

