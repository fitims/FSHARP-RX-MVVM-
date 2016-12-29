using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CommandExample.ViewModels
{
    //public interface IMessageBus
    //{
    //    IDisposable Subscribe<T>(Action<T> action);
    //    void Publish<T>(T item);
    //}

    //public class MessageBus : IMessageBus
    //{
    //    private readonly Subject<object> _messageBus;  

    //    public MessageBus()
    //    {
    //        _messageBus = new Subject<object>();
    //    }

    //    public IDisposable Subscribe<T>(Action<T> action)
    //    {
    //        return _messageBus.OfType<T>().Subscribe(action);
    //    }

    //    public void Publish<T>(T item)
    //    {
    //        _messageBus.OnNext((object) item);
    //    }
    //}
}
