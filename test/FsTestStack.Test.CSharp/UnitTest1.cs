using System.Runtime.InteropServices.ComTypes;
using Autofac;
using FluentNHibernate.Cfg;
using FsTestStack.AspNetCore;
using FsTestStack.AspNetCore.Autofac;
using FsTestStack.AspNetCore.Default;
using FsTestStack.AspNetCore.InMemoryDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Core.CompilerServices;
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
        var configService = ConfigServices(sc => sc.AddTransient<object>());
        var bla = ComposeFunc(configService, configService);
        var factory1 = new AutofacApiFactFactory(ComposeFunc(configService, configService), FuncId);
        var factory2 = new DefaultApiFactFactory(ComposeFunc(configService, configService), FuncId);
        
        
        using TestHttpServer<ILifetimeScope> server = factory1.Launch();
        
        using TestHttpServer<ILifetimeScope> server1 = factory1.Launch(ComposeFunc(
            configService, 
            configService));
    }
}


public class TestDbFactory : InMemoryDbFactory
{
    public TestDbFactory() : 
        base(m => m.FluentMappings.AddFromAssemblyOf<UnitTest1>())
    {
    }
}