
module MVVMTests 


open System 
open System.Linq.Expressions 
open System.ComponentModel 
open System.Windows.Input 
open NUnit.Framework 
open KosData.Mvvm.Core
open NUnit.Framework 
open FsUnit 
open System.Reactive
open System.Reactive.Subjects
open System.Reactive.Disposables
open System.Reactive.Linq 
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Functions
open Stubs
open System.Windows.Input

type TestCommand(execute : unit -> unit) =
    let event = Event<_, _>()
    let mutable canExecute = true  
    member x.Execute() =
        execute()
    member x.ChangeCanExecute(value : bool) =
        canExecute <- value
        event.Trigger(None, EventArgs.Empty)
    member x.CanExecute with get() = canExecute
    interface ICommand with
        member x.add_CanExecuteChanged(e) = event.Publish.AddHandler(e)                                                                               
        member x.remove_CanExecuteChanged(e) = event.Publish.RemoveHandler(e)
        member x.Execute parameter =
            execute()
        member x.CanExecute parameter =
            canExecute

type ITestInterface = 
    abstract member StrProperty : string with get, set 
    abstract member IntProperty : int with get, set 
    abstract member Command : ICommand with get 

type TestVm(execute : unit -> unit) =
    inherit ViewModelBase()
    let mutable strValue = ""
    let mutable intValue = 0
    let testCmd = new TestCommand(execute)
    member x.ExecuteCommand() =
        testCmd.Execute()
    member x.CanExecute with get() = testCmd.CanExecute
    member x.ChangeCanExecute v =
        testCmd.ChangeCanExecute v 
    interface ITestInterface with
        member x.StrProperty with get() = strValue and set v = strValue <- v
        member x.IntProperty with get() = intValue and set v = intValue <- v
        member x.Command with get() = testCmd :> ICommand



[<TestFixture>] 
type ``ViewModelBase Tests`` () = 
    [<Test>]
    member x.``When calling OnPropertyChanged, should invoke PropertyChanged event`` () = 
        let propertyName = "Property Name"
        let viewModelBase = new ViewModelBase() 
        let notifylntf = viewModelBase :> INotifyPropertyChanged 
        let value = ref ""
        notifylntf.PropertyChanged.AddHandler(PropertyChangedEventHandler(fun s e -> value := e.PropertyName)) 
        viewModelBase.OnPropertyChanged (propertyName) 
        !value |> should equal propertyName 



[<TestFixture>] 
type ``PropertySubject Tests`` () = 
    [<Test>] 
    member x.``When setting the Value property, the value should be pushed into observable`` () = 
        let value = ref 0 
        let expectedValue = 20 
        let propertySubject = new PropertySubject<int>() :> IPropertySubject<int> 
        propertySubject.Subscribe(fun v -> value := v) |> ignore 
        propertySubject .Value <- expectedValue 
        !value |> should equal expectedValue 

    [<Test>] 
    member x.``When calling SetValue method, the value should be pushed into observable`` () = 
        let value = ref 0 
        let expectedValue = 20 
        let propertySubject = new PropertySubject<int>() 
        propertySubject.Subscribe(fun v -> value := v) |> ignore 
        propertySubject.SetValue expectedValue
        !value |> should equal expectedValue 

    [<Test>] 
    member x.``When calling SetValues method, values pushed into provided observable should be pushed into property`` () = 
        let value = ref 0 
        let expectedValue1 = 20 
        let expectedValue2 = 30 
        let propertySubject = new PropertySubject<int>() 
        let subject = new Subject<int>()

        propertySubject.Subscribe(fun v -> value := v) |> ignore 
        propertySubject.SetValues(subject) |> ignore

        subject.OnNext(expectedValue1)
        !value |> should equal expectedValue1 

        subject.OnNext(expectedValue2)
        !value |> should equal expectedValue2 


[<TestFixture>] 
type ``CommandObserver Tests`` () = 
    [<Test>]
    member x.``When calling Execute Method, new Unit value should be pushed into observable`` () = 
        let cmdObserver = new CommandObserver<int>() 
        let cmdObslntf = cmdObserver :> ICommandObserver<int> 
        let cmdlntf = cmdObserver :> ICommand 
        let stringValue = "called"
        let value = ref ""
        cmdObslntf.Execute.Subscribe(fun _ -> value := stringValue) |> ignore 
        cmdlntf. Execute() 
        !value |> should equal stringValue 

    [<Test>] 
    member x.``When pushing true to CanExecute observer, CanExecuteChanged event should be fired and CanExecute method return true`` () = 
        let cmdObserver = new CommandObserver<int>() 
        let cmdObslntf = cmdObserver :> ICommandObserver<int> 
        let cmdlntf = cmdObserver :> ICommand 
        let value = ref ""
        let stringValue = "event fired" 
        let boolValue = true 
        cmdlntf.CanExecuteChanged.AddHandler(EventHandler(fun s e -> value := stringValue)) 
        cmdObslntf.CanExecute.OnNext(boolValue) 
        let retValue = cmdlntf.CanExecute(null) 
        !value |> should equal stringValue 
        retValue |> should equal boolValue 

    [<Test>] 
    member x.``When pushing false to CanExecute observer, CanExecuteChanged event should be fired and CanExecute method return false`` () = 
        let cmdObserver = new CommandObserver<int>() 
        let cmdobsIntf = cmdObserver :> ICommandObserver<int> 
        let cmdIntf = cmdObserver :> ICommand 
        let value = ref ""
        let stringValue = "event fired" 
        let boolValue = false 
        cmdIntf.CanExecuteChanged.AddHandler(EventHandler(fun s e -> value := stringValue)) 
        cmdobsIntf.CanExecute.OnNext(boolValue) 
        let retValue = cmdIntf.CanExecute(null) 
        !value |> should equal stringValue 
        retValue |> should equal boolValue 

    [<Test>] 
    member x.``When pushing same value to CanExecute observer, CanExecuteChanged event should be fired only once`` () = 
        let cmdObserver = new CommandObserver<int>() 
        let cmdObslntf = cmdObserver :> ICommandObserver<int> 
        let cmdlntf = cmdObserver :> ICommand 
        let value = ref 0 
        cmdlntf.CanExecuteChanged.AddHandler(EventHandler(fun s e -> value := inc value.Value)) 
        Observable. Range(1,5).Subscribe(fun _ -> cmdObslntf.CanExecute.OnNext(true)) |> ignore 
        !value |> should equal 1 

    [<Test>] 
    member x.``When pushing different value to CanExecute observer, CanExecuteChanged event should be fired everytime the value changes`` () = 
        let cmdObserver = new CommandObserver<int>() 
        let cmdObslntf = cmdObserver :> ICommandObserver<int> 
        let cmdlntf = cmdObserver :> ICommand 
        let value = ref 0 
        cmdlntf.CanExecuteChanged.AddHandler(EventHandler(fun s e -> value := inc value.Value)) 
        Observable.Range(1,5).Subscribe(fun _ -> 
                                                cmdObslntf.CanExecute.OnNext (true) 
                                                cmdObslntf.CanExecute.OnNext(false)) |> ignore 
        !value |> should equal 10 

    [<Test>] 
    member x.``When calling SubscribeOnValues(IObservable<boolo>), CanExecute should have same values as the passed observable`` () = 
        let cmdObserver = new CommandObserver<int>() 
        let cmdObslntf = cmdObserver :> ICommandObserver<int> 
        let cmdlntf = cmdObserver :> ICommand 
        let subject = new Subject<bool>()
        cmdObserver.SubscribeOnValues(subject) |> ignore

        Observable.Range(1,5).Subscribe(fun _ -> 
                                                subject.OnNext(false)
                                                cmdlntf.CanExecute(null) |> should equal false
                                                subject.OnNext(true)
                                                cmdlntf.CanExecute(null) |> should equal true) 
        

[<TestFixture>] 
type ``PropertyProvider Tests`` () = 
    [<Test>] 
    member x.``When calling CreateProperty(), it should return a valid property subject`` () = 
        let vm = new TestVm(fun () -> ())
        let vmIntf = vm :> ITestInterface
        let pp = new PropertyProvider<ITestInterface>(vm, new TestSchedulers())
        let ppIntf = pp :> IPropertyProvider<ITestInterface>
        let exp = getExpression<ITestInterface, int> "IntProperty"
        let property = ppIntf.CreateProperty<int>(exp) 
        
        Assert.IsNotNull(property)

    [<Test>] 
    member x.``When calling CreateProperty(x), it should return a valid property subject and set its value and it should raise PropertyChanged on viewModel`` () = 
        let vm = new TestVm(fun () -> ())        
        let vmIntf = vm :> ITestInterface
        let vmPropChanged = vm :> INotifyPropertyChanged
        let pp = new PropertyProvider<ITestInterface>(vm, new TestSchedulers())
        let ppIntf = pp :> IPropertyProvider<ITestInterface>

        let propName = ref ""
        let isRaised = ref false
        vmPropChanged.PropertyChanged.Add(fun p -> propName := p.PropertyName; isRaised := true)

        let expectedValue = 10
        let exp = getExpression<ITestInterface, int> "IntProperty"
        let property = ppIntf.CreateProperty<int>(exp, expectedValue)

        Assert.IsNotNull(property)
        
        property.Value |> should equal expectedValue
        propName.Value |> should equal "IntProperty"
        isRaised.Value |> should equal true

    [<Test>] 
    member x.``When calling CreateProperty(IObservable<x>), it should return a valid property subject and its values should be same as provided observable and it should raise PropertyChanged on viewModel`` () = 
        let vm = new TestVm(fun () -> ())
        let vmIntf = vm :> ITestInterface
        let pp = new PropertyProvider<ITestInterface>(vm, new TestSchedulers())
        let ppIntf = pp :> IPropertyProvider<ITestInterface>
        let vmPropChanged = vm :> INotifyPropertyChanged

        let propName = ref ""
        let isRaised = ref false
        vmPropChanged.PropertyChanged.Add(fun p -> propName := p.PropertyName; isRaised := true)

        let valuesSubject = new Subject<int>()
        let exp = getExpression<ITestInterface, int> "IntProperty"
        let property = ppIntf.CreateProperty<int>(exp, valuesSubject)

        Assert.IsNotNull(property)

        Observable.Range(1,5).Subscribe(fun v -> 
                                                propName := ""
                                                isRaised := false
                                                valuesSubject.OnNext(v)
                                                property.Value |> should equal v
                                                propName.Value |> should equal "IntProperty"
                                                isRaised.Value |> should equal true) |> ignore


    [<Test>] 
    member x.``When disposing PropertyProvider, it should stop listening to the provided observable and should not raise PropertyChanged`` () = 
        let vm = new TestVm(fun () -> ())
        let vmIntf = vm :> ITestInterface
        let pp = new PropertyProvider<ITestInterface>(vm, new TestSchedulers())
        let ppIntf = pp :> IPropertyProvider<ITestInterface>
        let ppDispose = pp:> IDisposable
        let valuesSubject = new Subject<int>()
        let exp = getExpression<ITestInterface, int> "IntProperty"
        let property = ppIntf.CreateProperty<int>(exp, valuesSubject)
        let vmPropChanged = vm :> INotifyPropertyChanged

        let propName = ref ""
        let isRaised = ref false
        vmPropChanged.PropertyChanged.Add(fun p -> propName := p.PropertyName; isRaised := true)

        Assert.IsNotNull(property)
        Observable.Range(1,5).Subscribe(fun v -> 
                                            propName := ""
                                            isRaised := false
                                            valuesSubject.OnNext(v)
                                            property.Value |> should equal v
                                            propName.Value |> should equal "IntProperty"
                                            isRaised.Value |> should equal true) |> ignore

        ppDispose.Dispose()

        Observable.Range(10,5).Subscribe(fun v -> 
                                            propName := ""
                                            isRaised := false
                                            valuesSubject.OnNext(v)
                                            property.Value |> should equal 5
                                            propName.Value |> should equal ""
                                            isRaised.Value |> should equal false) |> ignore

    [<Test>] 
    member x.``When calling CreateCommand(), it should return a valid CommandObserver`` () = 
        let vm = new TestVm(fun () -> ())
        
        let vmIntf = vm :> ITestInterface
        let pp = new PropertyProvider<ITestInterface>(vm, new TestSchedulers())
        let ppIntf = pp :> IPropertyProvider<ITestInterface>
        let exp = getExpression<ITestInterface, ICommand> "Command"
        let cmd = ppIntf.CreateCommand<unit>(exp) 
        let cmdIntf = cmd :> ICommand

        Assert.IsNotNull(cmd)
        cmdIntf.CanExecute(null) |> should equal true

    [<Test>] 
    member x.``When calling CreateCommand(false), it should return a valid CommandObserver and CanExecute should be false`` () = 
        let vm = new TestVm(fun () -> ())
        
        let vmIntf = vm :> ITestInterface
        let pp = new PropertyProvider<ITestInterface>(vm, new TestSchedulers())
        let ppIntf = pp :> IPropertyProvider<ITestInterface>
        let exp = getExpression<ITestInterface, ICommand> "Command"
        let cmd = ppIntf.CreateCommand<unit>(exp, false) 
        let cmdIntf = cmd :> ICommand
        
        Assert.IsNotNull(cmd)
        cmdIntf.CanExecute(null) |> should equal false

    [<Test>] 
    member x.``When calling CreateCommand(IObservable<bool>), CanExecute should return values pushed into provided observable`` () = 
        let vm = new TestVm(fun () -> ())        
        let vmIntf = vm :> ITestInterface
        let pp = new PropertyProvider<ITestInterface>(vm, new TestSchedulers())
        let ppIntf = pp :> IPropertyProvider<ITestInterface>
        let exp = getExpression<ITestInterface, ICommand> "Command"
        
        let subject = new Subject<bool>()
        let cmd = ppIntf.CreateCommand<unit>(exp, subject) 
        let cmdIntf = cmd :> ICommand
        
        Assert.IsNotNull(cmd)
        cmdIntf.CanExecute(null) |> should equal true

        Observable.Range(1,5).Subscribe(fun v -> 
                                            subject.OnNext(false)
                                            cmdIntf.CanExecute(null) |> should equal false
                                            subject.OnNext(true)
                                            cmdIntf.CanExecute(null) |> should equal true) |> ignore

    [<Test>] 
    member x.``When disposing PropertyProvider, it should stop listening to the provided observable and should not push values to the CanExecute`` () = 
        let vm = new TestVm(fun () -> ())
        let vmIntf = vm :> ITestInterface
        let pp = new PropertyProvider<ITestInterface>(vm, new TestSchedulers())
        let ppIntf = pp :> IPropertyProvider<ITestInterface>
        let ppDispose = pp:> IDisposable
        let ppIntf = pp :> IPropertyProvider<ITestInterface>
        let exp = getExpression<ITestInterface, ICommand> "Command"

        let subject = new Subject<bool>()
        let cmd = ppIntf.CreateCommand<unit>(exp, subject) 
        let cmdIntf = cmd :> ICommand

        Assert.IsNotNull(cmd)
        cmdIntf.CanExecute(null) |> should equal true
        Observable.Range(1,5).Subscribe(fun v -> 
                                            subject.OnNext(false)
                                            cmdIntf.CanExecute(null) |> should equal false
                                            subject.OnNext(true)
                                            cmdIntf.CanExecute(null) |> should equal true) |> ignore

        
        ppDispose.Dispose();

        subject.OnNext(false)
        cmdIntf.CanExecute(null) |> should equal true




[<TestFixture>] 
type ``MessageBus Tests`` () = 
    [<Test>] 
    member x.``When calling Subscribe<int>, only the int values should be received`` () =         
        let value = ref 0
        let isCalled = ref false         
        let bus = new MessageBus() :> IMessageBus
        bus.Subscribe<int>(Action<int>(fun v -> value := v; isCalled := true)) |> ignore

        bus.Publish<int>(5)
        !value |> should equal 5
        !isCalled |> should equal true 

        isCalled := false;
        bus.Publish<string>("test")
        !value |> should equal 5
        !isCalled |> should equal false 

        isCalled := false;
        bus.Publish<double>(12.5)
        !value |> should equal 5
        !isCalled |> should equal false 

        isCalled := false;
        bus.Publish<int>(15)
        !value |> should equal 15
        !isCalled |> should equal true 




[<TestFixture>] 
type ``PropertyProviderFactory Tests`` () = 
    [<Test>] 
    member x.``When calling Create<IViewModel>(ViewModel), it should return a valid PropertyProvider`` () = 
        let vm = new TestVm(fun () -> ())    
        let vmIntf = vm :> ITestInterface    
        let ppFactory = new PropertyProviderFactory(new TestSchedulers()) :> IPropertyProviderFactory
        let pp = ppFactory.Create<ITestInterface>(vm)

        Assert.IsNotNull(pp)

