using System;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace CommandExample.ViewModels
{
    //public interface IViewModelProvider<TViewModel> : IDisposable
    //{
    //    IPropertySubject<T> CreateProperty<T>(Expression<Func<TViewModel, T>> expr);
    //}

    //public class ViewModelProvider<TViewModel> : IViewModelProvider<TViewModel> where TViewModel : ViewModelBase
    //{
    //    private readonly TViewModel _viewModelBase;
    //    private readonly CompositeDisposable _disposables;

    //    public ViewModelProvider(TViewModel viewModelBase)
    //    {
    //        _viewModelBase = viewModelBase;
    //        _disposables = new CompositeDisposable();
    //    }

    //    public IPropertySubject<TType> CreateProperty<TType>(Expression<Func<TViewModel, TType>> expr)
    //    {
    //        var propName = ((MemberExpression) expr.Body).Member.Name;
    //        var propSubject = new PropertySubject<TType>();

    //        var propDisposable = propSubject.Subscribe(_ => _viewModelBase.OnPropertyChanged(propName));
    //        _disposables.Add(propDisposable);
    //        return propSubject;
    //    }

    //    public ICommandObserver<T> CreateCommand<T>(Expression<Func<TViewModel, ICommand>> expr)
    //    {
    //        return new CommandObserver<T>();
    //    }


    //    public void Dispose()
    //    {
    //        _disposables.Dispose();
    //    }
    //}



}
