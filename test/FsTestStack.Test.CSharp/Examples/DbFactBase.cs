using NHibernate;

namespace FsTestStack.Test.CSharp.Examples;

public class DbFactBase : IDisposable
{
    private static readonly TestDbFactory DbFactory = new();
    
    private readonly ISession session;
    protected AspNetCore.InMemoryDb.InMemoryDb Db { get; }

    protected DbFactBase()
    {
        Db = DbFactory.Create();
        session = Db.CreateSession();
    }

    protected void DbSave(object obj)
    {
        session.SaveOrUpdate(obj);
    }
    
    public virtual void Dispose()
    {
        session.Dispose();
        (Db as IDisposable).Dispose();
    }
}