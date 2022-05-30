using System.Net;
using System.Net.Http.Json;
using FsTestStack.AspNetCore.Autofac;
using FsTestStack.AspNetCore.Default;
using FsTestStack.AspNetCore.InMemoryDb;
using FsTestStack.Test.CSharp.Domains;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using static FsTestStack.AspNetCore.FuncConvert;
using ISession = NHibernate.ISession;

namespace FsTestStack.Test.CSharp;

public class UnitTest1
{
    [Fact]
    public async void should_be_able_to_create_a_simple_test_server()
    {

        var apiFactory = new DefaultApiFactFactory(FuncId, a =>
            {
                a.MapGet("/abc", () => "Hello world!");
                return a;
            }
        );

        using var testServer = apiFactory.Launch();
        using var httpClient = testServer.CreateClient();

        var response = await httpClient.GetAsync("/abc");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.Equal("Hello world!", body);
    }
    
    [Fact]
    public async void should_be_able_to_create_a_simple_test_server_for_autofac()
    {

        var apiFactory = new AutofacApiFactFactory(FuncId, a =>
            {
                a.MapGet("/abc", () => "Hello world!");
                return a;
            }
        );

        using var testServer = apiFactory.Launch();
        using var httpClient = testServer.CreateClient();

        var response = await httpClient.GetAsync("/abc");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.Equal("Hello world!", body);
    }

    [Fact]
    public void should_be_able_to_create_an_inmemory_database()
    {
        var dbFactory = new TestDbFactory();
        using var db = dbFactory.Create();
        {
            using var session = db.CreateSession();
            session.SaveOrUpdate(new People("John", "Doe"));
        }

        {
            using var session = db.CreateSession();
            var people = session.Query<People>().First();
            Assert.NotEqual(0, people.Id);
            Assert.Equal("John", people.FirstName);
            Assert.Equal("Doe", people.LastName);
        }
    }


    [Fact]
    public async void should_be_able_to_run_test_for_application_logic()
    {
        var dbFactory = new TestDbFactory();
        using var db = dbFactory.Create();

        var apiFactory = new DefaultApiFactFactory(b =>
        {
            b.Services.AddScoped<ISession>(_ => db.CreateSession());
            return b;
        }, a =>
        {
            IResult Handle(ISession s) => Results.Json(s.Query<People>().ToList());
            a.MapGet("/people", Handle);
            return a;
        });
        var session = db.CreateSession();
        await session.SaveOrUpdateAsync(new People("John", "Doe"));

        using var testServer = apiFactory.Launch();
        using var httpClient = testServer.CreateClient();
        var response = await httpClient.GetAsync("/people");
        var schema = new { id = default(int), firstName = default(string), lastName = default(string) };
        
        var body = await ReadFromJsonAsync(response, schema.OfArray());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var people = body[0];
        Assert.Equal("John", people.firstName);
        Assert.Equal("Doe", people.lastName);
        Assert.NotEqual(0, people.id);
    }

    private static async Task<T> ReadFromJsonAsync<T>(HttpResponseMessage response, T? _ = default)
    {
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }
}

public static class FuncHelper
{
    public static T[] OfArray<T>(this T _)
    {
        return Array.Empty<T>();
    }
}


public class TestDbFactory : InMemoryDbFactory
{
    public TestDbFactory() : 
        base(m => m.FluentMappings.AddFromAssemblyOf<PeopleMapping>())
    {
    }
}