#region



#endregion

using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.ObjectEncryption
{
    public class ObjectEncryptModule<TDialog,TTypeToEnter> : PrismModuleBase
        where TDialog : ObjectEncryptDialog<TTypeToEnter>
        where TTypeToEnter : new()
    {
        public override void RegisterTypes()
        {
            //how to register generic type for navigation
            RegisterTypeForNavigation<TDialog>();
        }

        public override void InitialiseModule()
        {
            //aah cant remember
            //think need create dialog with
            //enter object then encrypt and store on file
            //and somehow enter the location
            ApplicationOptions.AddOption(OptionLabel, MenuNames.Test, StartDialog);
        }

        private void StartDialog()
        {
            NavigateTo<TDialog>();
        }

        public virtual string OptionLabel
        {
            get { return string.Format("Encrypt {0}", typeof(TTypeToEnter).Name); }
        }
    }
}