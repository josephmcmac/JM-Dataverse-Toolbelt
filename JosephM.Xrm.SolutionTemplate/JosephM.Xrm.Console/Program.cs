using $ext_safeprojectname$.Plugins.Core;
using $ext_safeprojectname$.Plugins.Xrm;
using System;

namespace $ext_safeprojectname$
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                var xrmConfiguration = new VsixActiveXrmConnection();
                var controller = new LogController();
                controller.AddUi(new ConsoleUserInterface(false));
                var xrmService = new XrmService(xrmConfiguration, controller);
                var me = xrmService.WhoAmI();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.XrmDisplayString());
            }
            Console.WriteLine("Press Any Key To Close");
            Console.ReadKey();
        }
    }
}
