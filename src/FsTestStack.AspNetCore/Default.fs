namespace FsTestStack.AspNetCore.Default

open FsTestStack.AspNetCore
open Microsoft.Extensions.DependencyInjection


type private DefaultContainer() =
  interface IContainerType<IServiceCollection, IServiceScope> with
    member this.CastScope scope = scope
    member this.ConfigBuilder b = b
    member this.ConfigApp a  = a


 type DefaultApiFactFactory() =
   inherit ApiFactFactory<IServiceCollection, IServiceScope>(DefaultContainer())
