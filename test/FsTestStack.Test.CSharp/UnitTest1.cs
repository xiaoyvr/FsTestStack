using System.Runtime.InteropServices.ComTypes;
using FluentNHibernate.Cfg;
using FsTestStack.AspNetCore.Autofac;
using FsTestStack.AspNetCore.Default;
using FsTestStack.AspNetCore.InMemoryDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FSharp.Core;
using static FsTestStack.AspNetCore.Configurators;
using static FsTestStack.AspNetCore.FuncConvert;

namespace FsTestStack.Test.CSharp;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var factory = new TestDbFactory();
        using var db = factory.Create();
    }

    [Fact]
    public void Test2()
    {
        Func<WebApplicationBuilder, WebApplicationBuilder>? configService = ConfigServices(sc => sc.AddTransient<object>());
        
        var factory1 = new AutofacApiFactFactory(ComposeFunc(configService, configService), FuncId);
        var factory2 = new DefaultApiFactFactory(ComposeFunc(configService, configService), FuncId);
        
        
        using var server = factory1.Launch();
        
        using var server1 = factory1.Launch(ComposeFunc(
            configService, 
            configService));
    }

    private static T Id<T>(T w) => w;
}


public class TestDbFactory : InMemoryDbFactory
{
    public TestDbFactory() : 
        base(m => m.FluentMappings.AddFromAssemblyOf<UnitTest1>())
    {
    }
}