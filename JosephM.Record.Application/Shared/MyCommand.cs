using System;
using System.Windows.Input;

namespace JosephM.Application.ViewModel.Shared
{
    public class MyCommand : ICommand
    {
        private Action Action { get; set; }


        public MyCommand(Action action)
        {
            Action = action;
        }

        public void Execute()
        {
            Action();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
            }

            remove
            {
            }
        }
    }
}
