module FsTestStack.Test.Tests

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
open FsTestStack.Test.App.Application


[<Fact>]
let ``My test`` () =
    let p = People("John", "Doe")
    
    let apiFactory = DefaultApiFactFactory()
    let dbFactory = InMemoryDbFactory<PeopleMapping>()
    
    use db = dbFactory.Create()
    
    
    use testServer = apiFactory.Launch
                         (fun b -> b.Services.AddScoped<ISession>(fun sp -> db.CreateSession() ) |> ignore; b)
                         configApp 
    
    do
      use scope = testServer.CreateScope()
      let session = scope.ServiceProvider.GetRequiredService<ISession>()
      session.SaveOrUpdate(p)      
    
    do
      use scope = testServer.CreateScope()
      use session = scope.ServiceProvider.GetRequiredService<ISession>()
      let p2 = session.Query<People>().First()
      Assert.NotEqual(0, p2.Id)
      Assert.Equal("John", p2.FirstName)
    
    
    use httpClient = testServer.CreateClient()

    do
      let response, body =
          task {
              let! response = httpClient.GetAsync("/abc")
              let! body = response.Content.ReadAsStringAsync()
              return (response, body)
          } |> Async.AwaitTask |> Async.RunSynchronously
              
      Assert.Equal (response.StatusCode, HttpStatusCode.OK)
      Assert.Equal("Hello world!", body)
    
    do
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
    
    