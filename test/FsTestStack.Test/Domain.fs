namespace FsTestStack.Test.App.Domain

open FluentNHibernate.Mapping
open Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter

type People(firstName, lastName) =
  
  abstract FirstName: string with get,set
  default val FirstName = firstName with get,set
  abstract LastName: string with get,set
  default val LastName = lastName with get,set
  
  abstract Id : int with get, set
  default val Id = 0 with get,set
  new() =
      People(null, null)
  
type PeopleMapping() =  
  inherit ClassMap<People>() with
  
    do    
      base.Id( QuotationToLambdaExpression <@  System.Func<People, obj>(fun i -> i.Id)@> ) |> ignore
      base.Map( QuotationToLambdaExpression <@  System.Func<People, obj>(fun i -> i.FirstName)@> ) |> ignore
      base.Map( QuotationToLambdaExpression <@  System.Func<People, obj>(fun i -> i.LastName)@> ) |> ignore
      base.Not.LazyLoad()
