
namespace KosData.Mvvm.Core

open System
open System.Threading
open System.Linq.Expressions
open System.Reactive
open System.Reactive.Subjects
open System.Reactive.Concurrency
open System.ComponentModel
open System.Reactive.Disposables
open System.Windows.Input
open System.Reactive.Linq
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Functions


type RelayCommand(canExecute : Object -> bool, execute : Object -> unit)  =        
    interface ICommand with
        member x.CanExecute param = canExecute param        
        member x.Execute param = execute param
        member x.add_CanExecuteChanged(e) = CommandManager.RequerySuggested.AddHandler(e)                                                                               
        member x.remove_CanExecuteChanged(e) = CommandManager.RequerySuggested.RemoveHandler(e)


type ViewModelBase() =
    let event = Event<_, _>()
    member x.OnPropertyChanged(propName : string) =
        event.Trigger(x, new PropertyChangedEventArgs(propName))

    interface INotifyPropertyChanged with
        member x.add_PropertyChanged(e) = event.Publish.AddHandler(e)
        member x.remove_PropertyChanged(e) = event.Publish.RemoveHandler(e)


type IGuiSynchronizationContext =
    abstract member SynchronizationContext : SynchronizationContext with get
    abstract member Invoke : (obj -> unit) -> unit
    abstract member BeginInvoke : (obj -> unit) -> unit



type IBusyToken =
    inherit IObservable<Unit> 
    abstract member Set: unit -> unit 
    abstract member Reset : unit -> unit

type BusyToken() =
    let mutable count = 1
    let _subject = new Subject<Unit>()
    interface IBusyToken with
        member x.Set() =
            count <- inc count   
        member x.Reset() =
            count <- dec count
            match count with
            | 0 -> _subject.OnNext(Unit.Default)
            | _ -> ()
        member x.Subscribe observer =
            _subject.Subscribe observer


type IBusyTokenProvider =
    abstract member GetToken : unit -> IBusyToken

type BusyTokeProvider() =
    interface IBusyTokenProvider with
        member x.GetToken() =
            new BusyToken() :> IBusyToken



type GuiSynchronizationContext() =
    let _syncContext = SynchronizationContext.Current
    interface IGuiSynchronizationContext with
        member x.SynchronizationContext with get() = _syncContext
        member x.Invoke action =
            _syncContext.Send(SendOrPostCallback(action), null)
        member x.BeginInvoke action =
            _syncContext.Post(SendOrPostCallback(action), null)


type ISchedulers =
    abstract member Dispatcher : IScheduler with get
    abstract member NewThread : IScheduler with get
    abstract member NewTask : IScheduler with get
    abstract member ThreadPool : IScheduler with get
    abstract member Timer : IScheduler with get


type Schedulers(guiSynchronizationContext : IGuiSynchronizationContext) =
    let _dispatcher = new SynchronizationContextScheduler(guiSynchronizationContext.SynchronizationContext)
    interface ISchedulers with
        member x.Dispatcher with get() = _dispatcher :> IScheduler
        member x.NewThread with get() = Scheduler.NewThread :> IScheduler
        member x.NewTask with get() = Scheduler.TaskPool :> IScheduler
        member x.ThreadPool with get() = Scheduler.ThreadPool :> IScheduler
        member x.Timer with get() = Scheduler.Immediate :> IScheduler



type IMessageBus = 
    abstract member Subscribe<'t> : System.Action<'t> -> IDisposable
    abstract member Publish<'t> : 't -> unit



type MessageBus() =
    let _messageBus = new Subject<Object>()
    interface IMessageBus with
        member x.Subscribe<'t> onNext =
            _messageBus.OfType<'t>().Subscribe(onNext) 
        member x.Publish<'t>(value : 't)  =
            _messageBus.OnNext(value :> Object)


type IPropertySubject<'t> =
    inherit IObservable<'t> 
    abstract member Value : 't with get, set



type PropertySubject<'t>() =
    let _subject : Subject<'t> = new Subject<'t>()
    let mutable _value : 't = Unchecked.defaultof<'t>
    let setValue v =
        _value <- v
        _subject.OnNext(v)

    member x.SetValues(obs : IObservable<'t>) =
        obs.Subscribe(fun v -> setValue v)       
    member x.SetValue(value : 't) = setValue value
        
    interface IPropertySubject<'t> with
        member x.Value with get() = _value and set v = setValue v
        member x.Subscribe observer = 
            _subject.Subscribe observer


type ICommandObserver<'t> =
    inherit ICommand
    abstract member Execute : IObservable<'t>  with get
    abstract member CanExecute : IObserver<bool> with get



type CommandObserver<'t>(value) as self =
    let event = Event<_, _>()
    let _executeSubject = new Subject<'t>()
    let _canExecuteSubject = new Subject<bool>()
    let mutable _canExecute = value    

    let disp = _canExecuteSubject.DistinctUntilChanged().Subscribe(fun v -> _canExecute <- v 
                                                                            event.Trigger(self, EventArgs.Empty))
    new() = new CommandObserver<'t>(true)        

    member x.SubscribeOnValues (values : IObservable<bool>)  = 
        values.Subscribe(_canExecuteSubject)

    interface ICommandObserver<'t> with
        member x.Execute with get() = _executeSubject.AsObservable()
        member x.CanExecute with get() = _canExecuteSubject.AsObserver()
                                                                          
    interface ICommand with
        member x.add_CanExecuteChanged(e) = event.Publish.AddHandler(e)                                                                               
        member x.remove_CanExecuteChanged(e) = event.Publish.RemoveHandler(e)        
        member x.Execute parameter = 
            match parameter with
            | :? 't -> _executeSubject.OnNext(parameter :?> 't)
            | _ -> _executeSubject.OnNext(Unchecked.defaultof<'t>)            
        member x.CanExecute parameter =
            _canExecute
                                                                                                       

type IPropertyProvider<'viewmodel> =
    inherit IDisposable
    abstract member CreateProperty<'ttype> : Expression<System.Func<'viewmodel,'ttype>> -> IPropertySubject<'ttype> 
    abstract member CreateProperty<'ttype> : Expression<System.Func<'viewmodel,'ttype>> * 'ttype -> IPropertySubject<'ttype> 
    abstract member CreateProperty<'ttype> : Expression<System.Func<'viewmodel,'ttype>> * IObservable<'ttype> -> IPropertySubject<'ttype> 
    abstract member CreateCommand<'ttype> : Expression<System.Func<'viewmodel, ICommand>> -> ICommandObserver<'ttype>
    abstract member CreateCommand<'ttype> : Expression<System.Func<'viewmodel, ICommand>> * bool -> ICommandObserver<'ttype>
    abstract member CreateCommand<'ttype> : Expression<System.Func<'viewmodel, ICommand>> * IObservable<bool> -> ICommandObserver<'ttype>


type PropertyProvider<'viewmodel>(viewModelBase : ViewModelBase, schedulers : ISchedulers) =
    let _viewModelBase = viewModelBase
    let _disposables = new CompositeDisposable()

    let getProperty (expr : Expression<System.Func<'viewmodel,'ttype>>) =
        let propName = (expr.Body :?> MemberExpression).Member.Name
        let propSubject = new PropertySubject<'t>()
        propSubject.SubscribeOn(schedulers.Dispatcher).Subscribe(fun _ -> _viewModelBase.OnPropertyChanged(propName))
        |> _disposables.Add 
        propSubject

    interface IPropertyProvider<'viewmodel> with
        member x.CreateProperty<'ttype> expr =
            let propSubject = getProperty expr
            propSubject :> IPropertySubject<'ttype>

        member x.CreateProperty<'ttype> (expr : Expression<System.Func<'viewmodel,'ttype>>, value : IObservable<'ttype>) =
            let propSubject = getProperty expr
            propSubject.SetValues(value) |> _disposables.Add
            propSubject :> IPropertySubject<'ttype>

        member x.CreateProperty<'ttype> (expr : Expression<System.Func<'viewmodel,'ttype>>, value : 'ttype) =
            let propSubject = getProperty expr
            propSubject.SetValue(value)
            propSubject :> IPropertySubject<'ttype>
                        
        member x.CreateCommand<'ttype> expr =
            new CommandObserver<'ttype>(true) :> ICommandObserver<'ttype>

        member x.CreateCommand<'ttype>(expr : Expression<System.Func<'viewmodel, ICommand>>, value : bool) =
            new CommandObserver<'ttype>(value) :> ICommandObserver<'ttype>

        member x.CreateCommand<'ttype>(expr : Expression<System.Func<'viewmodel, ICommand>>, values : IObservable<bool>) =
            let cmdObserver = new CommandObserver<'ttype>() 
            cmdObserver.SubscribeOnValues(values) |> _disposables.Add
            cmdObserver :> ICommandObserver<'ttype>
                
    interface IDisposable with
        member x.Dispose() =
            _disposables.Dispose()


type IPropertyProviderFactory = 
    abstract member Create<'t> : ViewModelBase -> IPropertyProvider<'t>  

type PropertyProviderFactory(schedulers : ISchedulers) =
    interface IPropertyProviderFactory with
        member x.Create<'t> (vm : ViewModelBase) =
            new PropertyProvider<'t>(vm, schedulers) :> IPropertyProvider<'t>



