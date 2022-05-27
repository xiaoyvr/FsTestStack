namespace FsTestStack.AspNetCore

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Hosting.Server
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Console

type ScopeCustomizer<'TContainer, 'TScope> (newScope) =
  let customizeList = System.Collections.Generic.List<'TContainer -> unit>();
  member x.Dirty =
    customizeList.Count > 0

  member x.Customize (scope: 'TScope): 'TScope =
    scope |> newScope (fun b ->(customizeList |> Seq.iter (fun a -> a b ) ))

  member x.Take action =
    customizeList.Add action
  member x.CSTake (action: Action<'TContainer>) =
    customizeList.Add action.Invoke

type TestHttpServer<'TScope> internal (host: IHost, castScope) =
  
  let server = host.Services.GetRequiredService<IServer>() :?> TestServer
  member this.CreateScope(): 'TScope =
      server.Services.CreateScope() |> castScope

  member this.CreateClient() =
      server.CreateClient()

  interface IDisposable with
    member this.Dispose() =
      server.Dispose()
      host.StopAsync().ConfigureAwait(true).GetAwaiter().GetResult()
      host.Dispose()
      GC.SuppressFinalize(this)

module private TestHttpServer =
  let New<'TScope> configBuilder configApp castScope =
    let builder = WebApplication.CreateBuilder()
    builder.WebHost.UseTestServer() |> ignore
    builder.Logging.AddFilter<ConsoleLoggerProvider>(fun lb -> lb >= LogLevel.Warning) |> ignore
    configBuilder(builder) |> ignore
    let host = builder.Build()
    configApp(host) |> ignore
    host.Start()
    new TestHttpServer<'TScope>(host, castScope)    

type IContainerType<'TContainer, 'TScope> =
  abstract member CastScope : IServiceScope -> 'TScope
  abstract member ConfigBuilder : WebApplicationBuilder -> WebApplicationBuilder
  abstract member ConfigApp : WebApplication -> WebApplication

type ApiFactFactory<'TContainer, 'TScope> (containerType: IContainerType<'TContainer, 'TScope>) =

  member this.Launch configBuilder configApp =
    TestHttpServer.New<'TScope> (containerType.ConfigBuilder >> configBuilder) (containerType.ConfigApp >> configApp) containerType.CastScope

  member this.CSLaunch (configBuilder: Func<WebApplicationBuilder, WebApplicationBuilder>, configApp: Func<WebApplication,WebApplication>) =
    this.Launch configBuilder.Invoke configApp.Invoke
