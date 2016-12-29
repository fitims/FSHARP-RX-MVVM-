using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace CommandExample.ViewModels
{
    //public interface ICommandObserver<out T> : ICommand
    //{
    //    IObservable<T> Execute { get; }
    //    IObserver<bool> CanExecute { get; }
    //}

    //public class CommandObserver<T> : ICommandObserver<T> 
    //{
    //    private readonly Subject<T> _executeSubject;
    //    private readonly Subject<bool> _canExecuteSubject;
    //    private bool _canExecute = true;

    //    public CommandObserver()
    //    {
    //        _executeSubject = new Subject<T>();
    //        _canExecuteSubject = new Subject<bool>();

    //        _canExecuteSubject.DistinctUntilChanged().Subscribe(v =>
    //                                                                {
    //                                                                    _canExecute = v;
    //                                                                    Refresh();
    //                                                                });
    //    }

    //    public IObservable<T> Execute
    //    {
    //        get { return _executeSubject.AsObservable(); }
    //    }

    //    public IObserver<bool> CanExecute
    //    {
    //        get { return _canExecuteSubject.AsObserver(); }
    //    }

    //    void ICommand.Execute(object parameter)
    //    {
    //        _executeSubject.OnNext((T)parameter);
    //    }

    //    bool ICommand.CanExecute(object parameter)
    //    {
    //        return _canExecute;
    //    }

    //    private void Refresh()
    //    {
    //        if (CanExecuteChanged != null)
    //            CanExecuteChanged(this, EventArgs.Empty);
    //    }

    //    public event EventHandler CanExecuteChanged;
    //}
}