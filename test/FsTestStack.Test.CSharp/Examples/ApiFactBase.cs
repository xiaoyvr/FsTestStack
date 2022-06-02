using FsTestStack.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FSharp.Core;
using ISession = NHibernate.ISession;

namespace FsTestStack.Test.CSharp.Examples;

public class ApiFactBase : IDisposable
{
    private static readonly TestDbFactory DbFactory = new();
    private readonly ISession session;
    private readonly TestApiFactFactory apiFactory;
    private TestHttpServer<IServiceScope>? server;
    private AspNetCore.InMemoryDb.InMemoryDb Db { get; }

    protected ApiFactBase()
    {
        Db = DbFactory.Create();
        session = Db.CreateSession();
        apiFactory = new TestApiFactFactory(Db);
    }
    
    protected void DbSave(object obj)
    {
        session.SaveOrUpdate(obj);
    }
    
    protected HttpClient Launch(Action<WebApplicationBuilder>? config = null)
    {
        server = apiFactory.Launch(FuncHelper.ToIdFunc(config));
        return server.CreateClient();
    }
    
    
    public void Dispose()
    {
        
        (server as IDisposable)?.Dispose();
        session.Dispose();
        (Db as IDisposable).Dispose();
    }
}