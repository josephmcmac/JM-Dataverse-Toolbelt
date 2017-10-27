using JosephM.CodeGenerator.CSharp;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.XRM.VSIX.Dialogs;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.XRM.VSIX.Commands.RefreshSchema
{
    internal sealed class RefreshSchemaCommand : SolutionItemCommandBase<RefreshSchemaCommand>
    {
        public override IEnumerable<string> ValidFileNames
        {
            get { return new[] { "Schema.cs" }; }
        }
        public override IEnumerable<string> ValidExtentions
        {
            get { return new[] { "cs" }; }
        }
        public override int CommandId
        {
            get { return 0x0100; }
        }

        public override void DoDialog()
        {
            var xrmService = GetXrmRecordService();

            var codeGeneratorService = new CSharpService(xrmService);

            var selectedItems = GetSelectedFileNamesQualified();
            if (selectedItems.Count() != 1)
            {
                VsShellUtilities.ShowMessageBox(
                    this.ServiceProvider,
                    "Only one file may be selected to refresh",
                    "XRM",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
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
            var dialog = new VsixServiceDialog<CSharpService, CSharpRequest, CSharpResponse, ServiceResponseItem>(
                codeGeneratorService,
                request,
                CreateDialogController());
            //refresh cache in case customisation changes have been made
            xrmService.ClearCache();
            DialogUtility.LoadDialog(dialog);
        }
    }
}
