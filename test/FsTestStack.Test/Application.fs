module FsTestStack.Test.App.Application

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System.Linq
open NHibernate
open FsTestStack.Test.App.Model

let ListPeople (context: HttpContext) :Task =
  let session = context.RequestServices.GetRequiredService<ISession>()
  let peopleList = session.Query<People>().ToList()
  context.Response.WriteAsJsonAsync(peopleList)


let configApp (a: WebApplication) :WebApplication=
  a.MapGet("/abc", Func<string>(fun _ -> "Hello world!")) |> ignore
  a.MapGet("/people", ListPeople ) |> ignore
  a
