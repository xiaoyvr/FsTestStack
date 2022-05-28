namespace FsTestStack.AspNetCore.Autofac

open System
open Autofac.Extensions.DependencyInjection
open FsTestStack.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Autofac
open Microsoft.AspNetCore.Hosting
type private CustomServiceScope(scope) =
  let serviceProvider = new AutofacServiceProvider(scope)
  let mutable disposed = false
  abstract member Dispose : bool -> unit
  default this.Dispose(disposing) =
    if not disposed then
      if disposing then
        serviceProvider.Dispose();
      disposed <- true

  member this.Scope = scope

  interface IServiceScope with
    member this.ServiceProvider = serviceProvider
    member this.Dispose() =


      this.Dispose(true)
      GC.SuppressFinalize(this)

type private CustomServiceScopeFactory(scope: ILifetimeScope) =
  interface IServiceScopeFactory with
    member this.CreateScope() =
      let customizer = scope.Resolve<ScopeCustomizer<ContainerBuilder, ILifetimeScope>>()
      new CustomServiceScope(customizer.Customize(scope))

#nowarn "44"
type private CustomServiceProviderFactory() =
  let wrapped = AutofacServiceProviderFactory()
  [<DefaultValue>]
  val mutable savedServices: IServiceCollection
  interface IServiceProviderFactory<ContainerBuilder> with
    member this.CreateBuilder (services: IServiceCollection) : ContainerBuilder =
      this.savedServices <- services;
      wrapped.CreateBuilder(services)
      |> fun builder ->
        builder.RegisterType<CustomServiceScopeFactory>().As<IServiceScopeFactory>() |> ignore
        builder

    member this.CreateServiceProvider (containerBuilder: ContainerBuilder): IServiceProvider =
      this.savedServices.BuildServiceProvider()
        .GetRequiredService< IStartupConfigureContainerFilter<ContainerBuilder> seq>()
      |> Seq.iter (fun filter -> filter.ConfigureContainer(fun b -> ()).Invoke(containerBuilder))
      wrapped.CreateServiceProvider(containerBuilder)
#endnowarn "44"

open FuncConvert
module AutofacContainer =
  let ConfigBuilder (b: WebApplicationBuilder) =
    b.Host.UseServiceProviderFactory(CustomServiceProviderFactory()) |> ignore
    ScopeCustomizer<_, ILifetimeScope> (fun action scope -> scope.BeginLifetimeScope action)
      |> b.Services.AddSingleton |> ignore
    b

 type AutofacApiFactFactory(configBuilder: Func<WebApplicationBuilder, WebApplicationBuilder>,
                            configApp: Func<WebApplication, WebApplication>) =
   inherit ApiFactFactory<ContainerBuilder, ILifetimeScope>(
     (DefaultFunc2 id configBuilder) >> AutofacContainer.ConfigBuilder,
     DefaultFunc2 id configApp,
     (fun scope -> (scope :?> CustomServiceScope).Scope))
