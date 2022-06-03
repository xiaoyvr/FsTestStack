module FsTestStack.AspNetCore.FuncUtil

open System
  
let DefaultFunc2 def (func:Func<_, _>)  =
  match func with null -> def | _ -> func.Invoke
let FuncId<'T>(w:'T) :'T = w;
