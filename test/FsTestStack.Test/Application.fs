module FsTestStack.Test.App.Application

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Linq
open NHibernate
open FsTestStack.Test.App.Model

let configApp (a: WebApplication) :WebApplication =
  a.MapGet("/abc", Func<_>(fun () -> "Hello world!")) |> ignore
  a.MapGet("/people",  Func<_,_>(fun (s:ISession) -> Results.Json(s.Query<People>().ToList()) )  ) |> ignore
  a
