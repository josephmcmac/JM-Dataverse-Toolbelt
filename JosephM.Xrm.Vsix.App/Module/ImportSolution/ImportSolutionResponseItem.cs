using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Deployment.DeployPackage;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    public class ImportSolutionResponseItem : ServiceResponseItem
    {
        public ImportSolutionResponseItem(SolutionImportResult solutionImportResult)
        {
            SolutionImportResult = solutionImportResult;
        }

        private SolutionImportResult SolutionImportResult { get; }
        [DisplayOrder(10)]
        public string Type => SolutionImportResult.Type;
        [DisplayOrder(20)]
        public string Name => SolutionImportResult.Name;
        [DisplayOrder(30)]
        public string Result => SolutionImportResult.Result;
        [DisplayOrder(40)]
        public string ErrorCode => SolutionImportResult.ErrorCode;
        [DisplayOrder(50)]
        [GridWidth(400)]
        public string ErrorText => SolutionImportResult.ErrorText;

        private Url _url;
        private bool _urlLoaded;
        [DisplayOrder(60)]
        [GridWidth(55)]
        [PropertyInContextByPropertyNotNull(nameof(Url))]
        public Url Url
        {
            get
            {
                if(!_urlLoaded)
                {
                    _url = SolutionImportResult.GetUrl();
                    _urlLoaded = true;
                }
                return _url;
            }
        }
    }
}