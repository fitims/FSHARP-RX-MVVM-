module Stubs

open KosData.Mvvm.Core
open Microsoft.Reactive.Testing
open System.Reactive
open System.Reactive.Concurrency

type TestSchedulers() =
    let scheduler = Scheduler.CurrentThread
    member x.Dispatcher with get() = scheduler
    member x.NewThread with get() = scheduler
    member x.NewTask with get() = scheduler
    member x.ThreadPool with get() = scheduler
    member x.Timer with get() = scheduler
    interface ISchedulers with
        member x.Dispatcher with get() = scheduler :> IScheduler
        member x.NewThread with get() = scheduler :> IScheduler
        member x.NewTask with get() = scheduler :> IScheduler
        member x.ThreadPool with get() = scheduler :> IScheduler
        member x.Timer with get() = scheduler :> IScheduler

