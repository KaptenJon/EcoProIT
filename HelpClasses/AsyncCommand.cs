using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HelpClasses
{
    public class AsyncCommand<T> : ICommand
    {
        private readonly Func<T,Task<T>> execute;
        private readonly Predicate<T> canExecute;
        private bool isExecuting;

        public AsyncCommand(Func<T, Task<T>> execute) : this(execute, s => true) { }

        public AsyncCommand(Func<T, Task<T>> execute, Predicate<T> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(T parameter)
        {
            // if the command is not executing, execute the users' can execute logic
            return !isExecuting && canExecute(parameter);
        }

        public  bool CanExecute(object parameter)
        {
            
            return !isExecuting && canExecute((T)parameter);
        }

        public async void Execute(object parameter)
        {
            isExecuting = true;
            OnCanExecuteChanged();

            try
            {
                // execute user code
                await execute((T)parameter);
            }
            finally
            {
                // tell the button we're done
                isExecuting = false;
                OnCanExecuteChanged();
            }
            
        }

        public event EventHandler CanExecuteChanged;

        public async void Execute(T parameter)
        {
            // tell the button that we're now executing...
            isExecuting = true;
            OnCanExecuteChanged();
            
            try
            {
                // execute user code
                await execute(parameter);
            }
            finally
            {
                // tell the button we're done
                isExecuting = false;
                OnCanExecuteChanged();
            }
            
        }

        protected virtual void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
        }
    }
}
