namespace FsTestStack.AspNetCore

open System
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Hosting.Server
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Console

type ScopeCustomizer<'TContainer, 'TScope> internal (newScope) =
  let customizeList = System.Collections.Generic.List<'TContainer -> unit>();
  member x.Dirty =
    customizeList.Count > 0

  member x.Customize (scope: 'TScope): 'TScope =
    scope |> newScope (fun b ->(customizeList |> Seq.iter (fun a -> a b ) ))

  member x.Take action =
    customizeList.Add action
  member x.CSTake (action: Action<'TContainer>) =
    customizeList.Add action.Invoke

type TestHttpServer<'TScope> internal (server: TestServer, host: IHost, castScope) =
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
  let New<'TScope> (configHost: IHostBuilder -> IHostBuilder) (configWebHost: IWebHostBuilder -> IWebHostBuilder) castScope =
    configHost(Host.CreateDefaultBuilder())
      .ConfigureWebHostDefaults(fun x ->
          x.UseTestServer()
            .ConfigureLogging(fun log ->
              log.AddFilter<ConsoleLoggerProvider>(fun lb -> lb >= LogLevel.Warning) |> ignore)
          |> configWebHost |> ignore)
    |> fun hostBuilder -> hostBuilder.Start()
    |> fun host -> new TestHttpServer<'TScope>(host.Services.GetRequiredService<IServer>() :?> TestServer,
                                               host, castScope)

type IContainerType<'TContainer, 'TScope> =
  abstract member CastScope : IServiceScope -> 'TScope
  abstract member ConfigHost : IHostBuilder -> IHostBuilder
  abstract member ConfigWebHost : IWebHostBuilder -> ('TContainer -> unit) ->IWebHostBuilder



type ApiFactFactory<'TContainer, 'TScope> (containerType: IContainerType<'TContainer, 'TScope>) =

  member this.Launch (configWebHost: IWebHostBuilder -> IWebHostBuilder) (configTestContainer: 'TContainer -> unit) =
    TestHttpServer.New<'TScope>
      containerType.ConfigHost
      (fun b -> containerType.ConfigWebHost b configTestContainer |> configWebHost)
      containerType.CastScope

  member this.CSLaunch (configWebHost: Func<IWebHostBuilder, IWebHostBuilder>, configTestContainer: Action<'TContainer>) =
    this.Launch configWebHost.Invoke configTestContainer.Invoke

  member this.CSLaunch (configWebHost: Func<IWebHostBuilder, IWebHostBuilder>) =
    this.Launch configWebHost.Invoke (fun _ -> ())

