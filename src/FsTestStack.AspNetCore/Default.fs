namespace FsTestStack.AspNetCore.Default

open System
open FsTestStack.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection

 type DefaultApiFactFactory(configBuilder: Func<WebApplicationBuilder, WebApplicationBuilder>,
        configApp: Func<WebApplication, WebApplication>) =
   inherit ApiFactFactory<IServiceCollection, IServiceScope>(configBuilder.Invoke, configApp.Invoke, id)
