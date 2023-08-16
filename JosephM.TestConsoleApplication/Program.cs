using JosephM.XrmModule.ToolingConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.TestConsoleApplication
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                var loginFrm = new ToolingConnectorForm(Guid.NewGuid().ToString(), "I Connect In Console");
                // Login process is Async, thus we need to detect when login is completed and close the form. 
                loginFrm.ConnectionToCrmCompleted += LoginFrm_ConnectionToCrmCompleted;
                // Show the dialog here. 
                loginFrm.ShowDialog();

                // If the login process completed, assign the connected service to the CRMServiceClient var 
                if (loginFrm.CrmConnectionMgr != null && loginFrm.CrmConnectionMgr.CrmSvc != null && loginFrm.CrmConnectionMgr.CrmSvc.IsReady)
                {
                    
                }
                else
                {
                    throw new Exception("A Successful Connection Was Not Made By The Tooling Connector");
                }
            }
            catch(Exception)
            {

            }

        }

        private static void LoginFrm_ConnectionToCrmCompleted(object sender, EventArgs e)
        {
            if (sender is ToolingConnectorForm)
            {
                ((ToolingConnectorForm)sender).Close();
            }
        }
    }
}
