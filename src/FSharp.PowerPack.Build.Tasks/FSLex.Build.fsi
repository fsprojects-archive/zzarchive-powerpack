// (c) Microsoft Corporation 2005-2009.

namespace Microsoft.FSharp.Build

type FsLex =
    inherit Microsoft.Build.Utilities.ToolTask

    new : unit -> FsLex
    
    override GenerateCommandLineCommands : unit -> System.String
    override GenerateFullPathToTool : unit -> System.String
    override ToolName : System.String
    
    member internal InternalGenerateCommandLineCommands : unit -> System.String
    
    member InputFile  : string with set
    [<Microsoft.Build.Framework.Output>]
    member OutputFile : string with set
    member CodePage   : string with set
    member OtherFlags : string with set
    member Unicode    : bool   with set
    member ToolPath   : string with set
    override ToolExe    : string with set    
