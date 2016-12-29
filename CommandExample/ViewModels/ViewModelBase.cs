using System;
using System.ComponentModel;
using System.Windows.Input;

namespace CommandExample.ViewModels
{
    //public abstract class ViewModelBase : INotifyPropertyChanged 
    //{

    //    public void OnPropertyChanged(params string[] propertyNames)
    //    {
    //        foreach(var propName in propertyNames)
    //            DoPropertyChanged(propName);
    //    }

    //    protected virtual void DoPropertyChanged(string propertyName)
    //    {
    //        RaisePropertyChanged(propertyName);
    //    }

    //    private void RaisePropertyChanged(string propertyName)
    //    {
    //        if(PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //        }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //}


    //public class RelayCommand : ICommand
    //{
    //    private readonly Action _execute;
    //    private readonly Func<bool> _canExecute;

    //    public RelayCommand(Action execute, Func<bool> canExecute)
    //    {
    //        _execute = execute;
    //        _canExecute = canExecute;
    //    }

    //    public void Execute(object parameter)
    //    {
    //        _execute();
    //    }

    //    public bool CanExecute(object parameter)
    //    {
    //        return _canExecute();
    //    }

    //    public void Refresh()
    //    {
    //        if (CanExecuteChanged != null)
    //            CanExecuteChanged(this, EventArgs.Empty);
    //    }

    //    public event EventHandler CanExecuteChanged;
    //}
}
