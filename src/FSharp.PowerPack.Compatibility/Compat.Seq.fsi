// (c) Microsoft Corporation 2005-2009. 

namespace Microsoft.FSharp.Compatibility.OCaml

    open System

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Seq = 


        [<Obsolete("This function will be removed in a future release. Use a sqeuence expression instead")>]
        val generate   : opener:(unit -> 'b) -> generator:('b -> 'T option) -> closer:('b -> unit) -> seq<'T>

