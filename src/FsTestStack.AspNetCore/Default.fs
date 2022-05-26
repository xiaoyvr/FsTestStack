namespace FsTestStack.AspNetCore.Default

open System
open FsTestStack.AspNetCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.TestHost


type private DefaultContainer() =
  interface IContainerType<IServiceCollection, IServiceScope> with
    member this.CastScope scope =
      scope
    member this.ConfigHost b =
      b
    member this.ConfigWebHost b configTestContainer =
      b.ConfigureTestContainer(configTestContainer)


 type DefaultApiFactFactory() =
   inherit ApiFactFactory<IServiceCollection, IServiceScope>(DefaultContainer())
