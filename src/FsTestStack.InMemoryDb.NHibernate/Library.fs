namespace FsTestStack.AspNetCore.InMemoryDb

open System
open System.Data.Common
open System.Diagnostics.CodeAnalysis
open FluentNHibernate.Cfg
open FluentNHibernate.Cfg.Db
open NHibernate
open NHibernate.Cfg
open NHibernate.Engine
open NHibernate.Mapping
open NHibernate.Tool.hbm2ddl

type InMemoryDb(sessionFactory: ISessionFactory, dbConnection: DbConnection) =

  member this.CreateSession interceptor =
    let builder = sessionFactory.WithOptions().Interceptor(interceptor).Connection(dbConnection).FlushMode(FlushMode.Commit)
    let session = builder.OpenSession();
    session
  member this.CreateSession() =
    this.CreateSession EmptyInterceptor.Instance

  interface IDisposable with
    member x.Dispose() =
      (dbConnection :> IDisposable).Dispose()
      (sessionFactory :> IDisposable).Dispose()


type InMemoryDbFactory(configMapping: Action<MappingConfiguration>) =

  let PatchColumn (column: ISelectable) =
    match column with
      | :? Column as c ->
            if not (String.IsNullOrEmpty c.DefaultValue) then
              c.DefaultValue <- null;
            if c.SqlType = "nvarchar(max)" then
              c.SqlType <-"TEXT"
      | _ -> ()

  let PatchProperty (p: Property) =
    if p.Type.Name = "DateTime2" then
      (p.Value :?> SimpleValue).TypeName <- "datetime"
    p.ColumnIterator |> Seq.iter PatchColumn


  let PatchConfig (cfg: Configuration) =
    cfg.ClassMappings |> Seq.iter (fun cm -> cm.PropertyIterator |> Seq.iter PatchProperty )
    cfg

  let connectionString = "Data Source=:memory:;Version=3;New=True;DateTimeFormatString=yyyy-MM-dd HH:mm:ss.FFFFFFF;"
  let sqliteCfg = SQLiteConfiguration
                    .Standard
                    .InMemory()
                    .ConnectionString(connectionString)
                    // .Raw("connection.release", "on_close")
  let configuration = Fluently.Configure()
                        .Database(sqliteCfg)
                        .Mappings(configMapping)
                        .BuildConfiguration() // |> PatchConfig
                        
  
  member this.Create() : [<NotNull>]_=
    let sessionFactory = configuration.BuildSessionFactory()
    let dbConnection = (sessionFactory :?> ISessionFactoryImplementor).ConnectionProvider.GetConnection()
    SchemaExport(configuration).Execute(false, true, false, dbConnection, null)
    new InMemoryDb(sessionFactory, dbConnection)
//  new (configMapping: MappingConfiguration -> unit) = 
//    InMemoryDbFactory (Action<_>(configMapping))
    

//[<Extension>]
//type Extensions =
//  [<Extension>]
//  static member UseInMemoryDb (b: IWebHostBuilder) (db:InMemoryDb) =
//    b.ConfigureServices(fun sc -> sc.AddScoped<ISession>(fun c -> db.CreateSession() ) |> ignore )
//
