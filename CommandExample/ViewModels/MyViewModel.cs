using System;
using System.Reactive;
using System.Windows;
using System.Windows.Input;
using KosData.Mvvm.Core;


namespace CommandExample.ViewModels
{
    public interface IMyViewModel
    {
        string Message { get; set; }    
    }


    public class Msg1
    {
        public string Message { get; set; }
    }


    public class Msg2
    {
        public string Message { get; set; }
    }


    public class MyViewModel : ViewModelBase
    {
        private readonly ICommandObserver<Unit> _saveCommand;
        private readonly ICommandObserver<Unit> _loadCommand;
        private readonly IPropertySubject<string> _messageSubject;


        public MyViewModel()
        {
            var pp = (IPropertyProvider<MyViewModel>)(new PropertyProvider<MyViewModel>(this, new Schedulers(new GuiSynchronizationContext())));

            var messageBus = (new MessageBus()) as IMessageBus;

            messageBus.Subscribe<string>(s => MessageBox.Show(s));
            messageBus.Subscribe<Msg1>(m => MessageBox.Show("msg1 published"));

            _loadCommand = pp.CreateCommand<Unit>(v => v.LoadCommand, true);
            _loadCommand.Execute.Subscribe(_ => messageBus.Publish("load has been clicked"));

            _saveCommand = pp.CreateCommand<Unit>(v => v.SaveCommand, false);
            _saveCommand.Execute.Subscribe(_ =>
                                               {
                                                   Message = "Clicked !!!";
                                                   messageBus.Publish(new Msg1 { Message = "this is a message"});
                                               });

            _messageSubject = pp.CreateProperty(v => v.Message);
            //_messageSubject.Subscribe(_ => _saveCommand.CanExecute.OnNext(!string.IsNullOrEmpty(Message)));
        }

        public string Message
        {
            get { return _messageSubject.Value; }
            set { _messageSubject.Value = value; }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand; }
        }

        public ICommand LoadCommand
        {
            get { return _loadCommand; }
        }

    }
}
 
