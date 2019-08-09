using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EasyDeploy
{
    class DelegateCommand<T> : ICommand
    {
        public DelegateCommand(Action<T> action)
        {
            ExecuteAction = action;
        }

        public bool CanExecute(object parameter)
        {
            if (this.CanExecuteFunc == null)
            {
                return true;
            }

            return this.CanExecuteFunc((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (this.ExecuteAction == null)
            {
                return;
            }
            this.ExecuteAction((T)parameter);
        }

        public Action<T> ExecuteAction { get; set; }
        public Func<T, bool> CanExecuteFunc { get; set; }
    }

    class DelegateCommand : ICommand
    {
        public DelegateCommand(Action action)
        {
            ExecuteAction = action;
        }

        public bool CanExecute(object parameter)
        {
            if (this.CanExecuteFunc == null)
            {
                return true;
            }

            return this.CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (this.ExecuteAction == null)
            {
                return;
            }
            this.ExecuteAction();
        }

        public Action ExecuteAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }
    }
}
