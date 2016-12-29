using System;
using System.Reactive.Subjects;

namespace CommandExample.ViewModels
{
    //public interface IPropertySubject<T> : IObservable<T>
    //{
    //    T Value { get; set; }
    //}


    //public class PropertySubject<T> : IPropertySubject<T>
    //{
    //    private readonly Subject<T> _subject;
    //    private T _value;

    //    public PropertySubject()
    //    {
    //        _subject = new Subject<T>();
    //    }

    //    public IDisposable Subscribe(IObserver<T> observer)
    //    {
    //        return _subject.Subscribe(observer);
    //    }

    //    public T Value
    //    {
    //        get { return _value; }
    //        set 
    //        { 
    //            _value = value;
    //            _subject.OnNext(value);
    //        }
    //    }
    //}
}