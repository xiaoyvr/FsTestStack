namespace FsTestStack.AspNetCore

open System
open System.Diagnostics.CodeAnalysis
open System.Runtime.InteropServices
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Hosting.Server
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Console
open Microsoft.FSharp.Core

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
    
module Configurators =
  [<NotNull>]
  let ConfigServices (config: Action<IServiceCollection>) : Func<WebApplicationBuilder, WebApplicationBuilder> =
    Func<_,_>(fun (b:WebApplicationBuilder) -> config.Invoke(b.Services); b)

module FuncConvert =  
  let rec Compose<'T1> (funcList: ('T1 -> 'T1) list) =
    match funcList with
      | [] -> id
      | x :: xs -> x >> Compose xs
      
  [<NotNull>]
  let ComposeFunc<'T1> ([<ParamArray>]funcArray: Func<'T1, 'T1> array)=
    funcArray
    |> List.ofArray
    |> List.map (fun func -> func.Invoke)
    |> Compose
    |> fun t1 -> Func<'T1,'T1>(t1)
    
  let DefaultFunc2 def (func:Func<_, _>)  =
    match func with null -> def | _ -> func.Invoke
  let FuncId<'T>(w:'T) :'T = w;
  
open FuncConvert

type ApiFactFactory<'TContainer, 'TScope> (configBuilder: ConfigBuilder, configApp: ConfigApp, castScope: IServiceScope -> 'TScope) =
  member this.Launch([<Optional>]launchConfigBuilder: Func<WebApplicationBuilder, WebApplicationBuilder>,
                     [<Optional>]launchConfigApp: Func<WebApplication, WebApplication>) : [<NotNull>]_=
    TestHttpServer.New<'TScope> (configBuilder >> (DefaultFunc2 id launchConfigBuilder)) (configApp >> (DefaultFunc2 id launchConfigApp)) castScope
  
