using FsTestStack.AspNetCore.InMemoryDb;
using FsTestStack.Test.CSharp.Domains;

namespace FsTestStack.Test.CSharp;

public class TestDbFactory : InMemoryDbFactory
{
    public TestDbFactory() : 
        base(m => m.FluentMappings.AddFromAssemblyOf<PeopleMapping>())
    {
    }
}