// (c) Microsoft Corporation 2005-2009. 

#nowarn "9"
   
namespace Microsoft.FSharp.Compatibility.OCaml

open System.Collections.Generic

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Seq = 

    let generate openf compute closef = 
        seq { let r = openf() 
              try 
                let x = ref None
                while (x := compute r; (!x).IsSome) do
                    yield (!x).Value
              finally
                 closef r }
    
