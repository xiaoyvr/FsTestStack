using System.Data.Common;
using System.Net;
using FsTestStack.AspNetCore.Autofac;
using FsTestStack.AspNetCore.Default;
using FsTestStack.Test.CSharp.Domains;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using static FsTestStack.AspNetCore.FuncUtil;
using ISession = NHibernate.ISession;

namespace FsTestStack.Test.CSharp;

public class Facts
{
    [Fact]
    public async void should_be_able_to_create_a_simple_test_server_for_autofac()
    {

        var apiFactory = new AutofacApiFactFactory(FuncId, a =>
            {
                a.MapGet("/greetings", () => "Hello world!");
                return a;
            }
        );

        using var testServer = apiFactory.Launch();
        using var httpClient = testServer.CreateClient();

        var response = await httpClient.GetAsync("/greetings");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.Equal("Hello world!", body);
    }

}