//----------------------------------------------------------------------------
//

open System
open System.IO
open System.Text
open System.Security.Cryptography

let argv = System.Environment.GetCommandLineArgs()
let targetDirectoryTree = argv.[argv.Length - 1]


module List = 
    let rec contains x l = match l with [] -> false | h::t -> x = h || contains x t
    let mem x l = contains x l

    let indexNotFound() = raise (new System.Collections.Generic.KeyNotFoundException("An index satisfying the predicate was not found in the collection"))

    let rec assoc x l = 
        match l with 
        | [] -> indexNotFound()
        | ((h,r)::t) -> if x = h then r else assoc x t

    let rec memAssoc x l = 
        match l with 
        | [] -> false
        | ((h,_)::t) -> x = h || memAssoc x t

    let rec removeAssoc x l = 
        match l with 
        | [] -> []
        | (((h,_) as p) ::t) -> if x = h then t else p:: removeAssoc x t


let sourcePath = "."  // no trailing / 

let gac20s =
  [ "bin\gac\FSharp.Compiler.CodeDom.dll",[];
    "bin\gac\FSharp.PowerPack.dll",[];
    "bin\gac\FSharp.PowerPack.Compatibility.dll",[];
    "bin\gac\FSharp.PowerPack.Linq.dll",[];
    "bin\gac\FSharp.PowerPack.Metadata.dll",[];
// Include these if we ever ship policy DLLs again
//    "bin/gac/policy.1.9.FSharp.PowerPack.dll",            ["bin/gac/policy.1.9.FSharp.PowerPack.dll.config"];
//    "bin/gac/policy.1.9.FSharp.PowerPack.Linq.dll",       ["bin/gac/policy.1.9.FSharp.PowerPack.Linq.dll.config"];
    ]

let shortcuts = 
  [ // "README-fsharp.html"  , "F# Release Notes" ;
    // "bin/fsi.exe", "F# Interactive (Console)";
    // "doc/README-vfsi.html", "F# Interactive (Visual Studio)";
    // "doc/README-samples.html", "F# Samples";
  ]

//----------------------------------------------------------------------------
// md5
//----------------------------------------------------------------------------

let md5 = new MD5CryptoServiceProvider()
let md5sum (text:string) =
    let bytes = System.Text.Encoding.ASCII.GetBytes text 
    md5.ComputeHash(bytes)

let md5Text (text:string) =
    let bytes = System.Text.Encoding.ASCII.GetBytes text 
    let bytes = md5.ComputeHash(bytes) 
    let x = Array.map int bytes 
    sprintf "%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x"
      x.[0] x.[1] x.[2]  x.[3]  x.[4]  x.[5]  x.[6]  x.[7]
      x.[8] x.[9] x.[10] x.[11] x.[12] x.[13] x.[14] x.[15]


//----------------------------------------------------------------------------
// read lines from file
//----------------------------------------------------------------------------

let copyFilesAndPreparePaths (installLstFile:string) =
    [ for line in System.IO.File.ReadAllLines installLstFile do
          match line.Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries) with 
          [| fullSourcePath; partialTargetDirectory |] -> 
               let fullTargetDirectory = Path.Combine(targetDirectoryTree, partialTargetDirectory)
               Directory.CreateDirectory fullTargetDirectory  |> ignore
               let partialTargetPath = Path.Combine (partialTargetDirectory, Path.GetFileName fullSourcePath)
               let fullTargetPath = Path.Combine (fullTargetDirectory, Path.GetFileName fullSourcePath)
               printfn "%s --> %s" fullSourcePath fullTargetPath
               if File.Exists fullTargetPath then 
                   File.SetAttributes(fullTargetPath, FileAttributes.Normal); 
               File.Copy(fullSourcePath, fullTargetPath, true)
               yield partialTargetPath
          | [| |] -> ()
          | _ -> failwith "Invalid file entry in install.lst: '%s'. Two entries expected on line" line ]
    |> Seq.distinct
    |> Seq.toList


//----------------------------------------------------------------------------
// file projections
// leading/path/name/filename projections 
// fileID   = leading.path.name.filename    
// filePath = leading;path;name;filename as string list 
//----------------------------------------------------------------------------

let fixupID (str:string) =
  // Only a-z, digits, underscore, periods.
  let sb = new StringBuilder() 
  let add c =
    if Char.IsLetter(c) || Char.IsDigit(c) || c = '_' || c = '.' then
      ignore (sb.Append(c))
    else
      ignore (sb.Append(int c))
  sb.Append("ID") |> ignore;
  String.iter add str;
  let str = sb.ToString() 
  if str.Length < 64 then
    str
  else
    let hash = md5Text str 
    "HASH_" + hash + "_" + str.Substring(str.Length - 32)  // 32 hash + 32 trailing text = 64 + 8 spare for prefixes 
    
let filePath   (path:string) = path.Split([|'\\';'/'|]) |> Array.toList
let fileID     (path:string) = String.Join(".",Array.ofList<string> (filePath path)) |> fixupID
let trailID    trail         = String.Join(".",Array.ofList<string> trail)           |> fixupID
let fileName   (path:string) = Path.GetFileName(path)
let fileDir    (path:string) = Path.GetDirectoryName(path)
let fileDPath  (path:string) = Path.GetDirectoryName(path) |> filePath
let fileDirID  (path:string) = path |> fileDir |> fileID    
let fileCompID (path:string) = String.Join(".",Array.ofList ("FileComp" :: filePath path)) |> fixupID
let gacCompID  (path:string) = String.Join(".",Array.ofList ("GACComp"  :: filePath path)) |> fixupID    
let keyPathName (path:string) = String.Join(".",Array.ofList ("KeyPath"  :: filePath path)) // not an ID, but a registry key name, so no length limit 

let guidOfID id =
  let bytes = md5sum ("FSHARP_GUID:" + id) 
  let x = Array.map int bytes 
  sprintf "%02x%02x%02x%02x-%02x%02x-%02x%02x-%02x%02x-%02x%02x%02x%02x%02x%02x"
    x.[0] x.[1] x.[2]  x.[3]  x.[4]  x.[5]  x.[6]  x.[7]
    x.[8] x.[9] x.[10] x.[11] x.[12] x.[13] x.[14] x.[15]


//----------------------------------------------------------------------------
// dir tree structure
//----------------------------------------------------------------------------
    
type dir = {id:string;subs:(string*dir)list}
let rec addPath ndirs trail = function
  | []      -> ndirs
  | (p::ps) ->
      let trail = trail @ [p] 
      if List.memAssoc p ndirs then
        let dir,ndirs = List.assoc p ndirs, List.removeAssoc p ndirs 
        let dir = {dir with subs = addPath dir.subs trail ps} 
        (p,dir)::ndirs
      else
        let subs = addPath [] trail ps 
        let dir = {id = trailID trail; subs = subs } 
        (p,dir)::ndirs

let collectFileDT tree file = 
    let dname = Path.GetDirectoryName(file) 
    let dpath = dname.Split([|'\\'|]) |> Array.toList 
    if dname = "" then tree else
    addPath tree [] dpath


//----------------------------------------------------------------------------
// write - DirectoryTree, Component of Files
// Write Directory structure.
// Write Component for each directory containing it's files.
// Write final component group collecting all directory components together.
//----------------------------------------------------------------------------
    
let spaces n = String.replicate n " "

let mergeMap f xs =
    let infos = List.map f xs 
    List.collect (fun (a,b,d) -> a) infos,
    List.collect (fun (a,b,d) -> b) infos,
    List.collect (fun (a,b,d) -> d) infos

let mergeIds infos = mergeMap (fun x -> x) infos

let writeGACComponent gacTab fp indent manFile =
    let gacId = gacCompID manFile 
    let guid  = guidOfID gacId 
    let manFileID   = "GF" + fileID manFile 
    fprintfn fp "%s<Component Id='%s' Guid='%s' DiskId='1' >" (spaces indent) gacId guid;
    fprintfn fp "%s<File Id='%s' Name='%s' Source='%s/%s' Assembly='.net' KeyPath='yes' />"
      (spaces (indent+2)) manFileID (fileName manFile) sourcePath manFile;
    List.assoc manFile gacTab |> List.iter (fun auxFile -> 
      let auxFileID   = "GF" + fileID auxFile 
      fprintfn fp "%s<File Id='%s' Name='%s' Source='%s/%s'  />"
        (spaces (indent+2)) auxFileID (fileName auxFile) sourcePath auxFile
    );
    fprintfn fp "%s</Component>" (spaces indent);  
    gacId


let writeDirectoryFileComponent fp indent dirID dpath file =
    if dpath = fileDPath file then
      let compId = fileCompID file 
      let guid   = guidOfID compId 
      let fid = fileID file 
      if List.memAssoc file gac20s then 
          [],[],[writeGACComponent gac20s fp indent file ]
      elif (gac20s |> List.exists (fun (man,auxs) -> List.mem file auxs)) then 
          [],[],[] // written by writeGACComponent 
      else 
          let isShortcut = List.memAssoc file shortcuts
          fprintfn fp "%s<Component Id='%s' Guid='%s' DiskId='1' >" (spaces indent) compId guid;
          fprintfn fp "%s<File Id='%s' Name='%s' Source='%s/%s'>" (spaces (indent+2)) fid (fileName file) sourcePath file;
          if isShortcut then 
            fprintfn fp "%s<Shortcut Id='Shortcut.%s'  Directory='FSharpMenu' Name='%s'  Description='Shortcut to %s' Advertise='no' />" (spaces (indent+4)) fid (List.assoc file shortcuts) file;
          fprintfn fp "%s</File>" (spaces (indent+2));
          if isShortcut then 
            fprintfn fp "%s<RegistryValue Root='HKCU' Key='Software\\Microsoft Research\\FSharp\\ILX-VERSION\\%s' Type='string' Value='1' KeyPath='yes' />" (spaces (indent+2)) (keyPathName file);
          if (fileName file) = "FSharp.ProjectSystem.FSharp.dll" then 
            fprintf fp @"
              <CreateFolder/>
              "
          fprintfn fp "%s</Component>" (spaces indent);
          [compId],[],[]
    else
      [],[],[]

let writeDirectoryFileComponents fp indent dirID dpath files =
    mergeMap (writeDirectoryFileComponent fp indent dirID dpath) files

let rec writeDTree fp indent files dpath (name,dir) =
    let dpath = dpath @ [name] 
    fprintfn fp "%s<Directory Id='%s' Name='%s'>" (spaces indent) dir.id name;
    let comps    = writeDirectoryFileComponents fp (indent+2) dir.id dpath files in
    let subcomps = mergeMap (writeDTree fp (indent+2) files dpath) dir.subs in
    fprintfn fp "%s</Directory>" (spaces indent);
    mergeIds [comps;subcomps]

let writeComponentGroup fp indent id compIds =
    fprintfn fp "%s<ComponentGroup Id='%s'>" (spaces indent) id;
    let writeComponentRef compId =
      fprintfn fp "%s<ComponentRef Id='%s' />" (spaces (indent+2)) compId
    List.iter writeComponentRef compIds;
    fprintfn fp "%s</ComponentGroup>" (spaces indent)


//----------------------------------------------------------------------------
// generate merge module wxs for filelist-
//----------------------------------------------------------------------------

let files    = copyFilesAndPreparePaths "install.lst"
List.iter (fun f -> printfn "File: %s" f) files
let _ =
    use zipArgs = System.IO.File.CreateText "zip.args"
    files |> List.iter (fun f -> fprintfn zipArgs "%s\%s " targetDirectoryTree  f)

// Sanity check called-out files are present
let fileSet     = Set.ofList files
let gac20Set    = Set.ofList (List.map fst gac20s)
let shortcutSet = Set.ofList (List.map fst shortcuts)
let checkEmpty descr a = if not (Set.isEmpty a) then
                            failwith ("Checking " + descr + " files. Could not find: " + String.concat " and " (Set.toList a))
checkEmpty "gac20"    (gac20Set    - fileSet)
checkEmpty "shortcut" (shortcutSet - fileSet)

// Write files.wxs...
let fp = System.IO.File.CreateText "files.wxs"

fprintf fp @"<?xml version='1.0'?>
<Wix xmlns='http://schemas.microsoft.com/wix/2006/wi'
     xmlns:netfx='http://schemas.microsoft.com/wix/NetFxExtension'>
   <Fragment Id='CoreFilesFragment'>
      <DirectoryRef Id='INSTALLDIR'>
"
let indent = 6
let tree = List.fold collectFileDT [] files
let compIds,comp20Ids,gac20Ids =
  mergeIds [writeDirectoryFileComponents fp (indent+3) "" [""] files;
            mergeMap (writeDTree fp (indent+3) files []) tree]
fprintfn fp "      </DirectoryRef>"

writeComponentGroup fp indent "CoreFiles1ComponentGroup" compIds
writeComponentGroup fp indent "CoreFiles2ComponentGroup" comp20Ids
writeComponentGroup fp indent "GACFiles2ComponentGroup"  gac20Ids
fprintf fp @"
   </Fragment>
</Wix>
"

fp.Close()
exit 0
(*
            <ProgId Id='VisualStudio.fs.9.0' Description='Visual F# Source file' Icon='IDbin.FSharp.ProjectSystem.FSharp.dll' IconIndex='0' >
              <Extension Id='fs' ContentType='application/fsharp-source' >
                <Verb Id ='open' Command='Open' TargetProperty='VS90DEVENV' Argument='""%%1""' />
              </Extension>
            </ProgId>
            <ProgId Id='VisualStudio.fsi.9.0' Description='Visual F# Signature file' Icon='IDbin.FSharp.ProjectSystem.FSharp.dll' IconIndex='1' >           
              <Extension Id='fsi' ContentType='application/fsharp-signature' >
                <Verb Id ='open' Command='Open' TargetProperty='VS90DEVENV' Argument='""%%1""' />
              </Extension>
            </ProgId>
            <ProgId Id='VisualStudio.fsx.9.0' Description='Visual F# Script file' Icon='IDbin.FSharp.ProjectSystem.FSharp.dll' IconIndex='2' >
              <Extension Id='fsx' ContentType='application/fsharp-script' >
                <Verb Id ='open'       Command='Open'                    TargetProperty='VS90DEVENV' Argument='""%%1""' />
              </Extension>
            </ProgId>
            <ProgId Id='VisualStudio.fsscript.9.0' Description='Visual F# Script file' Icon='IDbin.FSharp.ProjectSystem.FSharp.dll' IconIndex='2' >
              <Extension Id='fsscript' ContentType='application/fsharp-script' >
                <Verb Id ='open'       Command='Open'                    TargetProperty='VS90DEVENV' Argument='""%%1""' />
              </Extension>
            </ProgId>
            <ProgId Id='VisualStudio.fsproj.9.0' Description='Visual F# Project file' Icon='IDbin.FSharp.ProjectSystem.FSharp.dll' IconIndex='3' >
              <Extension Id='fsproj' ContentType='application/fsharp-project' >
                <Verb Id ='open'       Command='Open'                    TargetProperty='VS90DEVENV' Argument='""%%1""' />
              </Extension>
            </ProgId>
*)
