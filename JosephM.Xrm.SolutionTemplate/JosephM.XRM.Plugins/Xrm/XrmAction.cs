using $safeprojectname$.Core;
using Microsoft.Xrm.Sdk;
using System;

namespace $safeprojectname$.Xrm
{
    public abstract class XrmAction
    {
        private IPluginExecutionContext _context;
        private LogController _controller;
        private XrmService _xrmService;
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        ///     The Context provided by the crm plugin service provider
        /// </summary>
        public IPluginExecutionContext Context
        {
            get
            {
                if (_context == null)
                    _context = (IPluginExecutionContext)ServiceProvider.GetService(typeof(IPluginExecutionContext));
                return _context;
            }
            set { _context = value; }
        }
        public LogController Controller
        {
            get
            {
                if (_controller == null)
                {
                    var trace = (ITracingService)ServiceProvider.GetService(typeof(ITracingService));
                    _controller = new LogController();
                    _controller.AddUi(new XrmTraceUserInterface(trace));
                }
                return _controller;
            }
            set { _controller = value; }
        }

        public XrmService XrmService
        {
            get
            {
                if (_xrmService == null)
                {
                    var factory =
                        (IOrganizationServiceFactory)ServiceProvider.GetService(typeof(IOrganizationServiceFactory));
                    _xrmService = new XrmService(factory.CreateOrganizationService(Context.UserId), Controller);
                }
                return _xrmService;
            }
            set { _xrmService = value; }
        }

        public string UnsecureConfig { get; internal set; }
        public string SecureConfig { get; internal set; }

        public string GetStringInput(string inputName)
        {
            return Context.InputParameters.Contains(inputName)
                && Context.InputParameters[inputName] is string
                ? (string)Context.InputParameters[inputName]
                : null;
        }

        public int GetOptionValueInput(string inputName)
        {
            return Context.InputParameters.Contains(inputName)
                && Context.InputParameters[inputName] is OptionSetValue osv
                ? osv.Value
                : -1;
        }

        public Guid? GetGuidFromInput(string inputName)
        {
            var stringInput = GetStringInput(inputName);
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }
            Guid outGuid;
            if (!Guid.TryParse(stringInput, out outGuid))
            {
                throw new InvalidPluginExecutionException($"Input {inputName} not of {nameof(Guid)} form. Value is '{stringInput}'");
            }
            else
            {
                return outGuid;
            }
        }

        public DateTime? GetDateTimeInput(string inputName)
        {
            return Context.InputParameters.Contains(inputName)
                && Context.InputParameters[inputName] is DateTime
                ? (DateTime)Context.InputParameters[inputName]
                : (DateTime?)null;
        }

        public void SetOutput(string outputName, object value)
        {
            Context.OutputParameters[outputName] = value;
        }

        public abstract void PostActionSynch();
    }
}
