using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.CodeGenerator.CSharp;
using JosephM.Core.FieldType;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Utilities;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.RefreshSchema
{
    [MenuItemVisibleSchemaCs]
    [DependantModule(typeof(XrmConnectionModule))]
    public class RefreshSchemaModule : ActionModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
        }

        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            var selectedItems = visualStudioService.GetSelectedFileNamesQualified();
            if (selectedItems.Count() != 1)
            {
                ApplicationController.UserMessage("Only one file may be selected to refresh");
                return;
            }

            //refresh cache in case customisation changes have been made
            var xrmService = ApplicationController.ResolveType(typeof(XrmRecordService)) as XrmRecordService;
            if (xrmService == null)
                throw new NullReferenceException("xrmService");
            xrmService.ClearCache();

            if (selectedItems.Count() != 1)
            {
                ApplicationController.UserMessage("Only one file may be selected to refresh");
                return;
            }
            FileInfo fileInfo = new FileInfo(selectedItems.First());

            var request = new CSharpRequest()
            {
                Folder = new Folder(fileInfo.DirectoryName),
                FileName = fileInfo.Name,
                Namespace = "Schema",
                Actions = true,
                Entities = true,
                Fields = true,
                FieldOptions = true,
                Relationships = true,
                SharedOptions = true,
                IncludeAllRecordTypes = true
            };

            var uri = new UriQuery();
            uri.AddObject(nameof(CSharpDialog.Request), request);
            uri.AddObject(nameof(CSharpDialog.SkipObjectEntry), true);
            //todo need to pass through to skip the entry screen
            ApplicationController.RequestNavigate("Main", typeof(CSharpDialog), uri);
        }
    }
}
