namespace FsTestStack.AspNetCore.InMemoryDb.Interceptors

open NHibernate

type SqlCaptureInterceptor(output) =
  inherit EmptyInterceptor()
  override x.OnPrepareStatement sql =
    output (sql.ToString())
    sql
