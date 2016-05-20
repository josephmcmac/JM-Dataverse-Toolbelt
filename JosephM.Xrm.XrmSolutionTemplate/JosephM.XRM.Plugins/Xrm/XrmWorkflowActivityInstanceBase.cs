using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using $safeprojectname$.Core;
using System;
using System.Activities;

namespace $safeprojectname$.Xrm
{
    public abstract class XrmWorkflowActivityInstanceBase
    {
        protected XrmWorkflowActivityRegistration Activity { get; set; }

        protected CodeActivityContext ExecutionContext { get; set; }

        private IWorkflowContext _context;

        private IWorkflowContext Context
        {
            get
            {
                if (_context == null)
                {
                    _context = ExecutionContext.GetExtension<IWorkflowContext>();
                    if (_context == null)
                        throw new InvalidPluginExecutionException("Failed to retrieve workflow context.");
                }
                return _context;
            }
        }

        private bool? _isSandboxIsolated;
        public bool IsSandboxIsolated
        {
            get
            {
                if (_isSandboxIsolated.HasValue)
                    return _isSandboxIsolated.Value;
                else
                    return Context.IsolationMode == 2;
            }
            set { _isSandboxIsolated = value; }
        }

        private int _maxSandboxIsolationExecutionSeconds = 120;
        public int MaxSandboxIsolationExecutionSeconds
        {
            get { return _maxSandboxIsolationExecutionSeconds; }
            set { _maxSandboxIsolationExecutionSeconds = value; }
        }

        private XrmService _xrmService;

        public XrmService XrmService
        {
            get
            {
                if (_xrmService == null)
                {
                    var serviceFactory = ExecutionContext.GetExtension<IOrganizationServiceFactory>();
                    _xrmService = new XrmService(serviceFactory.CreateOrganizationService(Context.UserId), LogController);
                }
                return _xrmService;
            }
            set { _xrmService = value; }
        }

        private ITracingService _tracingService;

        private ITracingService TracingService
        {
            get
            {
                if (_tracingService == null)
                {
                    _tracingService = ExecutionContext.GetExtension<ITracingService>();
                    if (_tracingService == null)
                        throw new InvalidPluginExecutionException("Failed to retrieve tracing service.");
                }
                return _tracingService;
            }
        }


        private LogController _logController;

        public LogController LogController
        {
            get
            {
                if (_logController == null)
                {
                    _logController = new LogController(new XrmTraceUserInterface(TracingService));
                }
                return _logController;
            }
            set { _logController = value; }
        }

        internal void ExecuteBase(CodeActivityContext executionContext,
            XrmWorkflowActivityRegistration xrmWorkflowActivityRegistration)
        {
            try
            {
                Activity = xrmWorkflowActivityRegistration;
                ExecutionContext = executionContext;

                TracingService.Trace(
                    "Entered Workflow {0}\nActivity Instance Id: {1}\nWorkflow Instance Id: {2}\nCorrelation Id: {3}\nInitiating User: {4}",
                    GetType().Name,
                    ExecutionContext.ActivityInstanceId,
                    ExecutionContext.WorkflowInstanceId,
                    Context.CorrelationId,
                    Context.InitiatingUserId);
                Execute();
            }
            catch (InvalidPluginExecutionException ex)
            {
                LogController.LogLiteral(ex.XrmDisplayString());
                throw;
            }
            catch (Exception ex)
            {
                LogController.LogLiteral(ex.XrmDisplayString());
                throw new InvalidPluginExecutionException(ex.Message, ex);
            }
        }

        protected abstract void Execute();

        private Guid? _targetId;
        public Guid TargetId
        {
            get
            {
                if (_targetId.HasValue)
                    return _targetId.Value;
                else
                    return Context.PrimaryEntityId;
            }
            set { _targetId = value; }
        }

        private string _targetType;
        public string TargetType
        {
            get
            {
                if (_targetType != null)
                    return _targetType;
                else
                    return Context.PrimaryEntityName;
            }
            set { _targetType = value; }
        }

        public void Trace(string message)
        {
            TracingService.Trace(message);
        }
    }
}