using System.Runtime.InteropServices.ComTypes;
using FluentNHibernate.Cfg;
using FsTestStack.AspNetCore.Autofac;
using FsTestStack.AspNetCore.Default;
using FsTestStack.AspNetCore.InMemoryDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.FSharp.Core;

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
        // var factory = new AutofacApiFactFactory();
        var factory = new DefaultApiFactFactory(b => b, w => w);
        using var server = factory.Launch(b => b, w => w);
    }
}

public class TestDbFactory : InMemoryDbFactory
{
    public TestDbFactory() : 
        base(m => m.FluentMappings.AddFromAssemblyOf<UnitTest1>())
    {
    }
}