﻿namespace FsTestStack.AspNetCore

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

type ConfigBuilder = WebApplicationBuilder -> WebApplicationBuilder
type ConfigApp = WebApplication -> WebApplication

module private TestHttpServer =
  let New<'TScope> (configBuilder: ConfigBuilder) (configApp: ConfigApp) castScope =
    WebApplication.CreateBuilder()
    |> (fun b ->
      b.WebHost.UseTestServer() |> ignore
      b.Logging.AddFilter<ConsoleLoggerProvider>(fun lb -> lb >= LogLevel.Warning) |> ignore
      configBuilder(b) |> ignore
      b.Build())
    |> configApp
    |> (fun h ->
        h.Start()
        new TestHttpServer<'TScope>(h, castScope))

type IContainerType<'TContainer, 'TScope> =
  abstract member CastScope : IServiceScope -> 'TScope
  abstract member ConfigBuilder : WebApplicationBuilder -> WebApplicationBuilder
  abstract member ConfigApp : WebApplication -> WebApplication

type ApiFactFactory<'TContainer, 'TScope> (configBuilder: ConfigBuilder, configApp: ConfigApp, castScope: IServiceScope -> 'TScope) =
  member this.Launch(launchConfigBuilder: Func<WebApplicationBuilder, WebApplicationBuilder>, launchConfigApp: Func<WebApplication, WebApplication>) =
    TestHttpServer.New<'TScope> (configBuilder >> launchConfigBuilder.Invoke) (configApp >> launchConfigApp.Invoke) castScope
  
    
