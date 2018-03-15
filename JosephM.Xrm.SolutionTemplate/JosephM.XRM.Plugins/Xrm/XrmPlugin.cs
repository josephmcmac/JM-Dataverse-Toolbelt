#region

using Microsoft.Xrm.Sdk;
using $safeprojectname$.Core;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace $safeprojectname$.Xrm
{
    /// <summary>
    ///     Class storing properties and methods common for plugins on all entity types
    ///     A specific entity may extend this class to implement plugins specific for that entity type
    /// </summary>
    public abstract class XrmPlugin
    {
        public abstract string TargetType { get; }

        public static void Go(XrmPlugin plugin)
        {
            try
            {
                plugin.GoExtention();
            }
            catch (InvalidPluginExecutionException ex)
            {
                //ADDED 10000 Limit To Trace As Was Receiving Buffer Exceeded Error
                //Couldn;t figure out where to increase buffer size
                plugin.Trace(ex.DisplayString().Left(10000));
                throw;
            }
            catch (Exception ex)
            {
                plugin.Trace(ex.DisplayString().Left(10000));
                throw new InvalidPluginExecutionException(ex.Message, ex);
            }
        }

        public abstract void GoExtention();

        #region instance properties

        private IPluginExecutionContext _context;

        private LogController _controller;

        private XrmService _service;
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

        /// <summary>
        ///     The Service for connecting to the crm instance provided by the crm plugin service provider
        /// </summary>
        public XrmService XrmService
        {
            get
            {
                if (_service == null)
                {
                    var factory =
                        (IOrganizationServiceFactory)ServiceProvider.GetService(typeof(IOrganizationServiceFactory));
                    _service = new XrmService(factory.CreateOrganizationService(Context.UserId), Controller);
                }
                return _service;
            }
            set { _service = value; }
        }

        /// <summary>
        ///     The message the plugin is firing for e.g. Create, Update
        /// </summary>
        public string MessageName
        {
            get { return Context.MessageName; }
        }

        /// <summary>
        ///     The stage the plugin is firing for e.g. Preoperation, PostOperation
        /// </summary>
        public int Stage
        {
            get { return Context.Stage; }
        }

        /// <summary>
        ///     The mode the plugin is firing in e.g. Synchronous
        /// </summary>
        public int Mode
        {
            get { return Context.Mode; }
        }

        #endregion

        #region instance methods

        /// <summary>
        ///     true if the plugin context is either of the input messages
        /// </summary>
        public bool IsMessage(string message1, string message2, string message3)
        {
            return
                IsMessage(message1)
                || IsMessage(message2)
                || IsMessage(message3);
        }

        public bool IsStage(int stage)
        {
            return Stage == stage;
        }

        public bool IsMode(int mode)
        {
            return Mode == mode;
        }

        /// <summary>
        ///     true if the plugin context is either of the input messages
        /// </summary>
        public bool IsMessage(string message1, string message2)
        {
            return
                IsMessage(message1)
                || IsMessage(message2);
        }

        /// <summary>
        ///     true if the plugin context is either of the input messages
        /// </summary>
        public bool IsMessage(string message)
        {
            return MessageName == message;
        }

        public void Trace(string message)
        {
            Controller.LogLiteral(message);
        }

        #endregion
    }

    /// <summary>
    ///     The Crm plugin messages
    /// </summary>
    public static class PluginMessage
    {
        public const string Lose = "Lose";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string SetStateDynamicEntity = "SetStateDynamicEntity";
        public const string Associate = "Associate";
        public const string Disassociate = "Disassociate";
        public const string Merge = "Merge";
        public const string QualifyLead = "QualifyLead";
        public const string Send = "Send";
        public const string Assign = "Assign";
        public const string AddMember = "AddMember";
        public const string AddListMember = "AddListMember";
        public const string AddMembers = "AddMembers";
        public const string RetrieveMultiple = "RetrieveMultiple";
        public const string Cancel = "Cancel";
        public const string Win = "Win";
        public const string Lost = "Lost";
    }

    /// <summary>
    ///     The Crm event pipeline stages
    /// </summary>
    public static class PluginMode
    {
        public const int Synchronous = 0;
        public const int Asynchronous = 1;
    }

    /// <summary>
    ///     The Crm event pipeline stages
    /// </summary>
    public static class PluginStage
    {
        public const int PreValidationEvent = 10;
        public const int PreOperationEvent = 20;
        public const int PostEvent = 40;
    }
}