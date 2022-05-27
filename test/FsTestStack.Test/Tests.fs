module FsTestStack.Test.Tests

open System
open System.Net
open FsTestStack.AspNetCore.InMemoryDb
open FsTestStack.AspNetCore.Default
open Microsoft.Extensions.DependencyInjection
open Microsoft.FSharp.Core
open NHibernate
open Xunit
open System.Linq
open System.Net.Http.Json
open FsTestStack.Test.App.Model
open FsTestStack.Test
open Microsoft.AspNetCore.Builder


[<Fact>]
let ``should be able to create a simple test server`` () =
  let apiFactory = DefaultApiFactFactory()
  use testServer = apiFactory.Launch id (fun a ->
    a.MapGet("/abc", Func<string>(fun _ -> "Hello world!")) |> ignore; a)
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
    
    let apiFactory = DefaultApiFactFactory()
    let dbFactory = InMemoryDbFactory(fun m -> m.FluentMappings.AddFromAssemblyOf<PeopleMapping>() |> ignore)
    
    use db = dbFactory.Create()
    let session = db.CreateSession()
    session.SaveOrUpdate(People("John", "Doe"))
    
    use testServer = apiFactory.Launch
                       (fun b -> b.Services.AddScoped<ISession>(fun sp -> db.CreateSession() ) |> ignore; b)
                       App.Application.configApp
                       
    use httpClient = testServer.CreateClient()
    let response, body =
        task {
            let! response = httpClient.GetAsync("/people")
            let! body = response.Content.ReadFromJsonAsync<{|firstName: string; lastName: string; id: int|} list>()
            return (response, body)
        } |> Async.AwaitTask |> Async.RunSynchronously
            
    Assert.Equal (response.StatusCode, HttpStatusCode.OK)
    let people = body[0]
    Assert.Equal("John", people.firstName)
    Assert.Equal("Doe", people.lastName)
    Assert.NotEqual(0, people.id)
  