namespace FsTestStack.AspNetCore.Default

open System
open System.Runtime.InteropServices
open FsTestStack.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FuncUtil

 type DefaultApiFactFactory(configBuilder: Func<WebApplicationBuilder, WebApplicationBuilder>,
                            configApp: Func<WebApplication, WebApplication>, [<Optional>]options: WebApplicationOptions option) =
   inherit ApiFactFactory<IServiceCollection, IServiceScope>(
     DefaultFunc2 id configBuilder,
     DefaultFunc2 id configApp,
     id,
     options)
