using FluentNHibernate.Cfg;
using FsTestStack.AspNetCore.InMemoryDb;
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
}

public class TestDbFactory : InMemoryDbFactory
{
    public TestDbFactory() : 
        base(m => m.FluentMappings.AddFromAssemblyOf<UnitTest1>())
    {
    }
}