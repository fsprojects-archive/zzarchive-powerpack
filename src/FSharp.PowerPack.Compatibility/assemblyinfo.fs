namespace Microsoft.FSharp
open System.Reflection
[<assembly:AssemblyDescription("FSharp.PowerPack.Compatibility.dll")>]
[<assembly:AssemblyCompany("F# PowerPack CodePlex Project")>]
[<assembly:AssemblyTitle("FSharp.PowerPack.Compatibility.dll")>]
[<assembly:AssemblyProduct("F# Power Pack")>]
//[<assembly: System.Security.SecurityTransparent>]
[<assembly: AutoOpen("Microsoft.FSharp.Compatibility.OCaml")>]
[<assembly: AutoOpen("Microsoft.FSharp")>]
do()

#if FX_NO_SECURITY_PERMISSIONS
#else
#if FX_SIMPLE_SECURITY_PERMISSIONS
[<assembly: System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum)>]
#else
#endif
#endif

[<assembly: System.Runtime.InteropServices.ComVisible(false)>]

[<assembly: System.CLSCompliant(true)>]


#if FX_NO_DEFAULT_DEPENDENCY_TYPE
#else
[<assembly: System.Runtime.CompilerServices.Dependency("FSharp.Core",System.Runtime.CompilerServices.LoadHint.Always)>] 
#endif

do ()
