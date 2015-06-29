//==========================================================================
// (c) Microsoft Corporation 2005-2008.  The interface to the module 
// is similar to that found in versions of other ML implementations, 
// but is not an exact match.  The type signatures in this interface
// are an edited version of those generated automatically by running 
// "bin\fsc.exe -i" on the implementation file.
//===========================================================================

/// Compatibility module to display data about exceptions.
[<CompilerMessage("This module is for ML compatibility. This message can be disabled using '--nowarn:62' or '#nowarn \"62\"'.", 62, IsHidden=true)>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Microsoft.FSharp.Compatibility.OCaml.Printexc

#if FX_NO_STDIN
#else
val print: mapping:('a -> 'b) -> 'a -> 'b
#endif
val to_string: exn -> string

