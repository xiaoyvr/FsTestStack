namespace FsTestStack.AspNetCore.Default

open System
open FsTestStack.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FuncConvert

 type DefaultApiFactFactory(configBuilder: Func<WebApplicationBuilder, WebApplicationBuilder>,
                            configApp: Func<WebApplication, WebApplication>) =
   inherit ApiFactFactory<IServiceCollection, IServiceScope>(
     DefaultFunc2 id configBuilder,
     DefaultFunc2 id configApp, id)
