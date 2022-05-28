module FsTestStack.Test.Tests

open System
open System.Net
open FsTestStack.AspNetCore.InMemoryDb
open FsTestStack.AspNetCore.Default
open FsTestStack.Test.App.Domain
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.FSharp.Core
open NHibernate
open Xunit
open System.Linq
open System.Net.Http.Json
open Microsoft.AspNetCore.Builder


[<Fact>]
let ``should be able to create a simple test server`` () =
  let apiFactory = DefaultApiFactFactory(id, (fun a -> a.MapGet("/abc", Func<_>(fun _ -> "Hello world!")) |> ignore; a))
  
  use testServer = apiFactory.Launch(id, id)
  use httpClient = testServer.CreateClient()
  
  let response, body =
    task {
      let! response = httpClient.GetAsync("/abc")
      let! body = response.Content.ReadAsStringAsync()
      return (response, body)
    } |> Async.AwaitTask |> Async.RunSynchronously

  Assert.Equal (response.StatusCode, HttpStatusCode.OK)
  Assert.Equal("Hello world!", body)
  
[<Fact>]
let ``should be able to create an in-memory database`` () =
  let dbFactory = InMemoryDbFactory(fun m -> m.FluentMappings.AddFromAssemblyOf<PeopleMapping>() |> ignore)
  use db = dbFactory.Create()
  do
    use session = db.CreateSession()
    session.SaveOrUpdate(People("John", "Doe"))

  do
    use session = db.CreateSession()
    let people = session.Query<People>().First()
    Assert.NotEqual(0, people.Id)
    Assert.Equal("John", people.FirstName)
    Assert.Equal("Doe", people.LastName)


[<Fact>]
let ``should be able to run test for application logic`` () =
    
    let dbFactory = InMemoryDbFactory(fun m -> m.FluentMappings.AddFromAssemblyOf<PeopleMapping>() |> ignore)    
    use db = dbFactory.Create()
    
    let apiFactory = DefaultApiFactFactory((fun b -> b.Services.AddScoped<ISession>(fun sp -> db.CreateSession() ) |> ignore; b),
                                           fun a -> a.MapGet("/people", Func<_,_>(fun (s:ISession) -> Results.Json(s.Query<People>().ToList()) )  ) |> ignore; a)
    let session = db.CreateSession()
    session.SaveOrUpdate(People("John", "Doe"))
    
    use testServer = apiFactory.Launch(id, id)
                       
    use httpClient = testServer.CreateClient()
    let response, body =
        task {
            let! response = httpClient.GetAsync("/people")
            let! body = response.Content.ReadFromJsonAsync<{|firstName: string; lastName: string; id: int|} list>()
            return (response, body)
        } |> Async.AwaitTask |> Async.RunSynchronously
            
    Assert.Equal (HttpStatusCode.OK, response.StatusCode)
    let people = body[0]
    Assert.Equal("John", people.firstName)
    Assert.Equal("Doe", people.lastName)
    Assert.NotEqual(0, people.id)
    

type IGreetings = abstract Get : unit -> string
type ProdImpl() = interface IGreetings with member x.Get() = "Greetings from Prod!"
type TestImpl() = interface IGreetings with member x.Get() = "Greetings from Test!"

[<Fact>]
let ``should be able to mock app service`` () =
    
    let apiFactory = DefaultApiFactFactory((fun b -> b.Services.AddTransient<IGreetings, ProdImpl>( ) |> ignore; b),
                                           fun a -> a.MapGet("/greetings", Func<_,_>(fun (s:IGreetings) -> s.Get() )  ) |> ignore; a)
    
    use testServer = apiFactory.Launch((fun b -> b.Services.AddTransient<IGreetings, TestImpl>( ) |> ignore; b) , id)

    use httpClient = testServer.CreateClient()
    let response, body =
        task {
            let! response = httpClient.GetAsync("/greetings")
            let! body = response.Content.ReadAsStringAsync()
            return (response, body)
        } |> Async.AwaitTask |> Async.RunSynchronously
            
    Assert.Equal(HttpStatusCode.OK, response.StatusCode)
    Assert.Equal("Greetings from Test!", body)
      