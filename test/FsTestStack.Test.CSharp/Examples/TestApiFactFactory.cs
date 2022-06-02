using FsTestStack.AspNetCore.Default;
using FsTestStack.Test.CSharp.Domains;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ISession = NHibernate.ISession;

namespace FsTestStack.Test.CSharp.Examples;

public class TestApiFactFactory : DefaultApiFactFactory
{
    // here setup your app server, usually, it is a combination of app config and test config
    public TestApiFactFactory(AspNetCore.InMemoryDb.InMemoryDb db): 
        base(b =>
        {
            // test config
            b.Services.AddScoped<ISession>(_ => db.CreateSession());
            b.Services.AddTransient<IGreetings, ProdGreetings>();
            return b;
        }, a =>
        {
            // app config
            IResult Handle(ISession s) => Results.Json(s.Query<People>().ToList());
            a.MapGet("/people", (ISession s) => Results.Json(s.Query<People>().ToList()));

            // IResult Handle(ISession s) => Results.Json(s.Query<People>().ToList());
            a.MapGet("/greetings",(IGreetings g) => Results.Text(g.Greetings()));
                
            return a;
        })
    {}
}