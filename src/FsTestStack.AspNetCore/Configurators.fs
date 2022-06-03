module FsTestStack.AspNetCore.Configurators

open System
open System.Diagnostics.CodeAnalysis
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection

[<NotNull>]
let ConfigServices (config: Action<IServiceCollection>) : Func<WebApplicationBuilder, WebApplicationBuilder> =
  Func<_,_>(fun (b:WebApplicationBuilder) -> config.Invoke(b.Services); b)
