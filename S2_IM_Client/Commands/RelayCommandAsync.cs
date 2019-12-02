using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace S2_IM_Client.Commands
{
    public class RelayCommandAsync : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Predicate<object> _canExecute;
        private bool _isExecuting;

        public RelayCommandAsync(Func<Task> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && _canExecute == null || !_isExecuting && _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public async void Execute(object parameter)
        {
            _isExecuting = true;
            try { await _execute(); }
            finally { _isExecuting = false; }
        }
    }
}
