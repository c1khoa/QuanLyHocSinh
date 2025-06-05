using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged; //

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); //
        }
    }

    // RelayCommand không generic
    public class RelayCommand : ICommand
    {
        private readonly Action _execute; //
        private readonly Func<bool>? _canExecute; //

        public event EventHandler? CanExecuteChanged //
        {
            add { CommandManager.RequerySuggested += value; } //
            remove { CommandManager.RequerySuggested -= value; } //
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null) //
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute)); //
            _canExecute = canExecute; //
        }

        public bool CanExecute(object? parameter) //
        {
            return _canExecute?.Invoke() ?? true; //
        }

        public void Execute(object? parameter) //
        {
            _execute(); //
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T>? _canExecute; //
        private readonly Action<T> _execute; //

        public event EventHandler? CanExecuteChanged //
        {
            add { CommandManager.RequerySuggested += value; } //
            remove { CommandManager.RequerySuggested -= value; } //
        }

        public RelayCommand(Predicate<T>? canExecute, Action<T> execute) //
        {
            if (execute == null) //
                throw new ArgumentNullException("execute"); //
            _canExecute = canExecute; //
            _execute = execute; //
        }

        public RelayCommand(Action<T> execute) : this(null, execute) { } //

        public bool CanExecute(object? parameter) //
        {
            try //
            {
                return _canExecute == null ? true : _canExecute((T)parameter!); //
            }
            catch //
            {
                return true; //
            }
        }

        public void Execute(object? parameter) //
        {
            _execute((T)parameter!); //
        }
    }
}