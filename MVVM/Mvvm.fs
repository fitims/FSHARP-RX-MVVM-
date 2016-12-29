module Mvvm

open KosData.Mvvm.Core
open Autofac


let Register (containerBuilder : ContainerBuilder) =
    containerBuilder.RegisterGeneric(typedefof<PropertySubject<_>>).As(typedefof<IPropertySubject<_>>) |> ignore
    containerBuilder.RegisterGeneric(typedefof<CommandObserver<_>>).As(typedefof<ICommandObserver<_>>) |> ignore
    containerBuilder.RegisterGeneric(typedefof<PropertyProvider<_>>).As(typedefof<IPropertyProvider<_>>) |> ignore
    containerBuilder.RegisterType<PropertyProviderFactory>().As<IPropertyProviderFactory>().SingleInstance() |> ignore
    containerBuilder.RegisterType<Schedulers>().As<ISchedulers>().SingleInstance() |> ignore
    containerBuilder.RegisterType<GuiSynchronizationContext>().As<IGuiSynchronizationContext>().SingleInstance() |> ignore
    containerBuilder.RegisterType<MessageBus>().As<IMessageBus>().SingleInstance() |> ignore
    containerBuilder.RegisterType<ViewModelBase>() |> ignore

