
// TODO: print nice type variable names for inferred type variables

// Post beta2:

// TODO: layout events
// TODO: accessibility
// TODO: full member constraints 
// TODO: isOverGenerics
// TODO: layout base type in some way

#nowarn "62"

open System
open System.IO
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Xml
open Microsoft.FSharp.Metadata
open Microsoft.FSharp.Text.StructuredFormat
open Microsoft.FSharp.Text.StructuredFormat.LayoutOps
open Microsoft.FSharp.Text.StructuredFormat.Display

let showObsoleteMembers = false
let shortConstraints = true
let showConstraintTyparAnnotations = false

module String = 
    let uncapitalize (s:string) =
        if s.Length = 0 then  ""
        else s.[0..0].ToLowerInvariant() + s.[1..s.Length-1]

module ListSet =
    let rec contains f x l = 
        match l with 
        | [] -> false
        | x'::t -> f x x' || contains f x t

    let insert f x l = if contains f x l then l else x::l
    let setify f l = List.foldBack (insert f) (List.rev l) [] |> List.rev

module LibUtilities =


    let orderOn p pxOrder x xx = pxOrder (p x) (p xx)

    let tryDropSuffix (s:string) (t:string) = 
        if s.EndsWith(t) then
            Some s.[0..s.Length - t.Length - 1]
        else
            None

    let dropSuffix s t = 
        match tryDropSuffix s t with 
        | Some(res) -> res 
        | None -> failwith "dropSuffix"

    // Concatenate for HTML links
    let hrefConcat a b = a+"/"+b

    let rec split (c:char) (str:string) =
        let i = str.IndexOf c
        if i <> -1 then 
            str.[0..i-1] :: split c str.[i+1 .. str.Length-1]
        else
            [str]

    let isAllLower (s:string) = (s.ToLowerInvariant() = s)    

    let underscoreLowercase s =
      if isAllLower s then "_"+s else s

    let allButLast (xs: list<_>) = 
        if xs.IsEmpty then failwith "allButLast"
        List.rev (List.tail (List.rev xs)) //  xs.[0..xs.Length-2]

    let last (xs: list<_>) = 
        if xs.IsEmpty then failwith "last"
        xs.[xs.Length-1]

module LayoutUtilities =
    type Renderer<'T,'State> =
        (* exists 'State.
           -- could use object type to get "exists 'State" on private state,
        *)
        abstract Start    : unit -> 'State;
        abstract AddText  : 'State -> string -> 'State;
        abstract AddBreak : 'State -> int -> 'State;
        abstract AddTag   : 'State -> string * (string * string) list * bool -> 'State;
        abstract Finish   : 'State -> 'T
          
    let renderL (rr: Renderer<_,_>) layout =
        let rec addL z pos i = function
            (* pos is tab level *)
          | Leaf (jl,text,jr)                 -> 
              rr.AddText z (unbox text),i + (unbox<string> text).Length
          | Node (jl,l,jm,r,jr,Broken indent) -> 
              let z,i = addL z pos i l 
              let z,i = rr.AddBreak z (pos+indent),(pos+indent) 
              let z,i = addL z (pos+indent) i r 
              z,i
          | Node (jl,l,jm,r,jr,_)             -> 
              let z,i = addL z pos i l 
              let z,i = if jm then z,i else rr.AddText z " ",i+1 
              let pos = i 
              let z,i = addL z pos i r 
              z,i
          | Attr (tag,attrs,l)                -> 
              let z   = rr.AddTag z (tag,attrs,true) 
              let z,i = addL z pos i l 
              let z   = rr.AddTag z (tag,attrs,false) 
              z,i
        let pos = 0 
        let z,i = rr.Start(),0 
        let z,i = addL z pos i layout 
        rr.Finish z

    let spaces n = String.replicate n " "

    /// string render 
    let stringR =
      { new Renderer<string,string list> with 
          member x.Start () = []
          member x.AddText rstrs text = text::rstrs
          member x.AddBreak rstrs n = (spaces n) :: "\n" ::  rstrs 
          member x.AddTag z (_,_,_) = z
          member x.Finish rstrs = String.Join("",Array.ofList (List.rev rstrs)) }

    /// html render - wraps HTML encoding (REVIEW) and hyperlinks
    let htmlR (baseR : Renderer<'Res,'State>) =
      { new Renderer<'Res,'State> with 
          member r.Start () = baseR.Start()
          member r.AddText z s = baseR.AddText z s;  (* REVIEW: escape HTML chars *)
          member r.AddBreak z n = baseR.AddBreak z n
          member r.AddTag z (tag,attrs,start) =
             match tag,attrs with 
             | "html:a",[("href",link)] ->
                if start
                then baseR.AddText z (sprintf "<a href='%s'>" link)
                else baseR.AddText z (sprintf "</a>")
             | _ -> z
          member r.Finish z = baseR.Finish z }



module FSharpMetadataUtilities =

    open LibUtilities

    let isAttrib<'T> (attrib: FSharpAttribute)  =
        attrib.ReflectionType = typeof<'T> 

    let tryFindAttrib<'T> (attribs: ReadOnlyCollection<FSharpAttribute>)  =
        attribs |> Seq.tryPick (fun a -> if isAttrib<'T>(a) then Some (a.Value :?> 'T) else None)

    let hasAttrib<'T> (attribs: ReadOnlyCollection<FSharpAttribute>)  = tryFindAttrib<'T>(attribs).IsSome

    let getDocFromSig (xmlDocSig : string) (xmlDocMemberMap : Map<string,string>) =
        match xmlDocMemberMap.TryFind(xmlDocSig) with
        | Some(docstring) -> docstring.Replace("<c>","<tt>").Replace("</c>","</tt>")
        | None -> ""
    

    let indexedConstraints (tps : seq<FSharpGenericParameter>) = 
        tps |> Seq.map (fun gp -> (gp, (gp.Constraints |> Seq.toList))) |> Seq.toList


[<NoEquality; NoComparison>]
type DisplayEnv = 
  { thisAssembly : FSharpAssembly;
    html: bool;
    htmlHideRedundantKeywords: bool;
    htmlAssemMap: Map<string,string>; // where can the docs for f# assemblies be found? 
    showObsoleteMembers: bool; 
    showTyparBinding: bool; 
    suppressInlineKeyword: bool;
    showMemberContainers:bool;
    shortConstraints:bool;
    showAttributes:bool;
    showOverrides:bool;
    showConstraintTyparAnnotations: bool;
    abbreviateAdditionalConstraints: bool;
    showTyparDefaultConstraints : bool;
    contextAccessibility: FSharpAccessibility }

module FSharpMetadataPrintUtilities = 

    open LibUtilities
    open LayoutUtilities
    open FSharpMetadataUtilities

    let dummy = 1

    
    let bracketIfL x lyt = if x then bracketL lyt else lyt
    let squareAngleL denv x = 
        if denv.html then 
            leftL "[&lt;" ^^ x ^^ rightL "&gt;]"
        else
            leftL "[<" ^^ x ^^ rightL ">]"
        
    let angleL denv x = 
        if denv.html then 
            sepL "&lt;" ^^ x ^^ rightL "&gt;"  
        else
            sepL "<" ^^ x ^^ rightL ">"  

    let squareAngleSepListAboveL denv xs body = squareAngleL denv (sepListL (rightL ";") xs) @@ body 
    let linkL str ly = tagAttrL "html:a" [("href",str)] ly
    let hlinkL (url:string) l = linkL url l
    //let tagAttrL str attrs ly = Attr (str,attrs,ly)

    let layoutAccessibility (denv:DisplayEnv) accessibility itemL =   
        itemL

#if TODO
 // accessibility 
        let isInternalCompPath x = 
            match x with 
            | CompPath(ScopeRef_local,[]) -> true 
            | _ -> false
        let (|Public|Internal|Private|) (TAccess p) = 
            match p with 
            | [] -> Public 
            | _ when List.forall isInternalCompPath p  -> Internal 
            | _ -> Private
        match denv.contextAccessibility,accessibility with
        | Public,Internal  -> wordL "internal" ++ itemL    // print modifier, since more specific than context
        | Public,Private   -> wordL "private" ++ itemL     // print modifier, since more specific than context
        | Internal,Private -> wordL "private" ++ itemL     // print modifier, since more specific than context
        | _ -> itemL
#endif



        
    let trimPathByDisplayEnv (denv:DisplayEnv) p = p
    
    /// Layout a reference to a type or value, perhaps emitting a HTML hyperlink 
    let layoutTyconRef denv (tcref:FSharpEntity) = 
      let path = tcref.Namespace
      let basicText = tcref.DisplayName
      let shortNameL = wordL basicText
      let longNameL = 
          let pathText = trimPathByDisplayEnv denv path
          (if pathText = "" then shortNameL else leftL (pathText+".") ^^ shortNameL)        
      try
        
        if tcref.IsExternal then
            if denv.html then 
                try 
                    if tcref.ReflectionType.Assembly.GetName().GetPublicKeyToken().[0..4] = [| 0xB7uy; 0x7Auy; 0x5Cuy; 0x56uy; 0x19uy |] then
                        // cross link to the MSDN 2.0 documentation.  
                        // Generic types don't seem to have stable names, so just shell out to a search engine
                        if tcref.GenericParameters.Count = 0 then 
                            hlinkL (sprintf "http://msdn2.microsoft.com/en-us/library/%s.aspx" tcref.ReflectionType.FullName) shortNameL
                        else 
                            hlinkL (sprintf "http://www.bing.com/search?q=%s" tcref.ReflectionType.FullName) shortNameL
                    else
                        longNameL
                with _ -> 
                    longNameL
            else
                longNameL
        else 
            let arity = tcref.GenericParameters.Count
            let aritySuffix = if arity=0 then "" else "-" + string arity
            let kindPrefix = if tcref.IsModule then "" else "type_"
            if denv.html then 
                let nm = path + "." + kindPrefix + underscoreLowercase basicText + aritySuffix
                let assemName = tcref.ReflectionAssembly.GetName().Name
                match denv.htmlAssemMap.TryFind  assemName with 
                | Some root -> 
                    hlinkL (sprintf "%s/%s.html" root nm) shortNameL
                // otherwise assume it is installed parallel to this tree 
                | None -> 
                    if assemName = denv.thisAssembly.ReflectionAssembly.GetName().Name then 
                        hlinkL (sprintf "%s.html" nm)  shortNameL
                    else
                        hlinkL (sprintf "../%s/%s.html" assemName nm) shortNameL
            else
                longNameL
      with e -> 
          eprintfn "failed to reference to type '%s' -  '%s" basicText e.Message
          if System.Diagnostics.Debugger.IsAttached then 
               System.Diagnostics.Debugger.Break()
          longNameL

    let layoutAttribute denv (attrib:FSharpAttribute) = 
        wordL "(attribute)"


    /// Layout '[<attribs>]' above another block 
    let layoutAttribs denv (attrs: ReadOnlyCollection<FSharpAttribute>) restL = 
        let attrs = List.ofSeq attrs
        if denv.showAttributes then
            (* Don't display DllImport attributes in generated signatures and/or html *)
            let attrs = if denv.html then attrs |> List.filter (isAttrib<ClassAttribute> >> not) else attrs
            let attrs = if denv.html then attrs |> List.filter (isAttrib<StructAttribute> >> not) else attrs
            let attrs = if denv.html then attrs |> List.filter (isAttrib<InterfaceAttribute> >> not) else attrs
            let attrs = attrs |> List.filter (isAttrib<System.Runtime.InteropServices.DllImportAttribute> >> not)
            let attrs = attrs |> List.filter (isAttrib<CompiledNameAttribute> >> not)
            let attrs = attrs |> List.filter (isAttrib<ContextStaticAttribute> >> not)
            let attrs = attrs |> List.filter (isAttrib<ThreadStaticAttribute> >> not)
            let attrs = attrs |> List.filter (isAttrib<CompilerMessageAttribute> >> not)
            let attrs = attrs |> List.filter (isAttrib<EntryPointAttribute> >> not)
            let attrs = attrs |> List.filter (isAttrib<RequiresExplicitTypeArgumentsAttribute> >> not)
            let attrs = attrs |> List.filter (isAttrib<System.Runtime.InteropServices.MarshalAsAttribute> >> not)
            let attrs = attrs |> List.filter (isAttrib<ReflectedDefinitionAttribute> >> not)
            let attrs = attrs |> List.filter (isAttrib<System.Runtime.InteropServices.StructLayoutAttribute> >> not)
            let attrs = attrs |> List.filter (isAttrib<AutoSerializableAttribute> >> not)
            match attrs.Length with
            | 0 -> restL 
            | _ -> restL
        else
            restL

    let rec layoutTyparAttribs denv (typar:FSharpGenericParameter) restL =         
        let attrs = typar.Attributes |> List.ofSeq
        match attrs.Length, typar.IsMeasure with
        | 0, false -> restL 
        | _  -> squareAngleSepListAboveL denv ((if typar.IsMeasure then [wordL "Measure"] else []) @ List.map (layoutAttribute denv) attrs) restL

    and layoutTyparRef denv env (typar:FSharpGenericParameter) =
          wordL (sprintf "%s%s"
                   (if denv.showConstraintTyparAnnotations && typar.IsSolveAtCompileTime then "^" else "'")
                   typar.Name)

    and layoutTyparAux denv env (typar:FSharpGenericParameter) =
        let varL = layoutTyparRef denv env typar
        //let varL = layoutTyparAttribs denv typar varL
        varL

      
    /// Layout type parameter constraints, taking TypeSimplificationInfo into account  
    and layoutConstraints denv env (cxs: list<_ * list<FSharpGenericParameterConstraint>>) = 

        
        // Internally member constraints get attached to each type variable in their support. 
        // This means we get too many constraints being printed. 
        // So we normalize the constraints to eliminate duplicate member constraints 

        let cxs = 
           [ for (tp, tpcs) in cxs  do
               for tpc in tpcs do
                   yield (tp,tpc) ]

        // Merge trait constraints referring to the same member (when using short constraints)
        let cxs = 
            cxs  
            |> ListSet.setify (fun (tp1,cx1) (tp2,cx2) ->
                     (cx1.IsMemberConstraint && 
                      cx2.IsMemberConstraint && 
                      denv.shortConstraints && 
                      cx1.MemberName = cx2.MemberName))
                     
        let cxsL = 
           [ for (tp, tpc) in cxs  do
                yield! layoutConstraint denv env (tp,tpc) ]

        match cxsL with 
        | [] -> None
        | _ -> 
            if denv.abbreviateAdditionalConstraints then 
                Some (wordL "when <constraints>")
            elif denv.shortConstraints then 
                Some (leftL "(" ^^ wordL "requires" ^^ sepListL (wordL "and") cxsL ^^ rightL ")")
            else
                Some (wordL "when" ^^ sepListL (wordL "and") cxsL)

    and layoutConstraintsAfter denv env coreL cxs = 
        match layoutConstraints denv env cxs with 
        | None -> coreL
        | Some cxsL -> coreL --- cxsL

    /// Layout constraints, taking TypeSimplificationInfo into account  
    and layoutConstraint denv env (tp,tpc: FSharpGenericParameterConstraint) =
        if tpc.IsCoercesToConstraint then 
            [layoutTyparAux denv env tp ^^ wordL ":>" --- layoutTypeAux denv env tpc.CoercesToTarget ]

        elif tpc.IsMemberConstraint then 
            if denv.shortConstraints then 
               [ layoutTyparAux denv env tp ^^ wordL "has" ^^ wordL tpc.MemberName ]
            else
               // TODO: full member constraints 
               [ layoutTyparAux denv env tp ^^ wordL "has" ^^ wordL tpc.MemberName ]

        elif tpc.IsDefaultsToConstraint then 
           if denv.showTyparDefaultConstraints then 
               [wordL "default" ^^ layoutTyparAux denv env tp ^^ wordL " :" ^^ layoutTypeAux denv env tpc.DefaultsToTarget]
           else 
               []

        elif tpc.IsEnumConstraint then 
            if denv.shortConstraints then 
               [wordL "enum"]
            else
               [layoutTyparAux denv env tp ^^ wordL ":" ^^ layoutTypeApplication denv env (wordL "enum") 2 true [tpc.EnumConstraintTarget ]]

        elif tpc.IsDelegateConstraint then 
            if denv.shortConstraints then 
               [wordL "delegate"]
            else
               [layoutTyparAux denv env tp ^^ wordL ":" ^^ layoutTypeApplication denv env (wordL "delegate") 2 true [tpc.DelegateTupledArgumentType ; tpc.DelegateReturnType]]

        elif tpc.IsSupportsNullConstraint then 
           [layoutTyparAux denv env tp ^^ wordL ":" ^^ wordL "null" ]

        elif tpc.IsComparisonConstraint then 
           [layoutTyparAux denv env tp ^^ wordL ":" ^^ wordL "comparison" ]

        elif tpc.IsEqualityConstraint then 
           [layoutTyparAux denv env tp ^^ wordL ":" ^^ wordL "equality" ]

        elif tpc.IsNonNullableValueTypeConstraint then 
            if denv.shortConstraints then 
               [wordL "value type"]
            else
               [layoutTyparAux denv env tp ^^ wordL ":" ^^ wordL "struct" ]

        elif tpc.IsReferenceTypeConstraint then 
            if denv.shortConstraints then 
               [wordL "reference type"]
            else
               [layoutTyparAux denv env tp ^^ wordL ":" ^^ wordL "not struct" ]

        elif tpc.IsSimpleChoiceConstraint then 
           [layoutTyparAux denv env tp ^^ wordL ":" ^^ bracketL (sepListL (sepL "|") (List.map (layoutTypeAux denv env) (Seq.toList tpc.SimpleChoices))) ]

        elif tpc.IsRequiresDefaultConstructorConstraint then 
            if denv.shortConstraints then 
               [wordL "default constructor"]
            else
               [layoutTyparAux denv env tp ^^ wordL ":" ^^ bracketL (wordL "new : unit -> " ^^ (layoutTyparAux denv env tp))]
        else 
            [wordL "unknown"]

    /// Layout type arguments, either NAME<ty,...,ty> or (ty,...,ty) NAME 
    and layoutTypeApplication denv env tcL prec prefix args =
        if prefix then 
            match args with
            | [] -> tcL
            | [arg] -> tcL ^^ angleL denv (layoutTypeWithPrec denv env 4 arg)
            | args -> bracketIfL (prec <= 1) (tcL ^^ angleL denv (layoutTypesWithPrec denv env 2 (sepL ",") args))
        else
            match args with
            | [] -> tcL
            | [arg] -> layoutTypeWithPrec denv env 2 arg ^^ tcL 
            | args -> bracketIfL (prec <= 1) (bracketL (layoutTypesWithPrec denv env 2 (sepL ",") args) ^^ tcL)

    and (|MeasureProd|_|) (typ : FSharpType) = 
        if typ.IsNamed && typ.NamedEntity.LogicalName = "*" && typ.GenericArguments.Count = 2 then Some (typ.GenericArguments.[0], typ.GenericArguments.[1])
        else None

    and (|MeasureInv|_|) (typ : FSharpType) = 
        if typ.IsNamed && typ.NamedEntity.LogicalName = "/" && typ.GenericArguments.Count = 1 then Some typ.GenericArguments.[0]
        else None

    and (|MeasureOne|_|) (typ : FSharpType) = 
        if typ.IsNamed && typ.NamedEntity.LogicalName = "1" && typ.GenericArguments.Count = 0 then  Some ()
        else None

    /// Layout a type, taking precedence into account to insert brackets where needed 
    and layoutTypeWithPrec denv env prec (typ:FSharpType) =

        // Measure types are stored as named types with 'fake' constructors for products, "1" and inverses
        // of measures in a normalized form (see Andrew Kennedy technical reports). Here we detect this 
        // embedding and use an approximate set of rules for layout out normalized measures in a nice way. 
        
        match typ with 
        | MeasureProd (ty,MeasureOne) 
        | MeasureProd (MeasureOne, ty) -> layoutTypeWithPrec denv env prec ty
        | MeasureProd (ty1, MeasureInv ty2) 
        | MeasureProd (ty1, MeasureProd (MeasureInv ty2, MeasureOne)) -> layoutTypeWithPrec denv env 2 ty1 ^^ wordL "/" ^^ layoutTypeWithPrec denv env 2 ty2
        | MeasureProd (ty1,MeasureProd(ty2,MeasureOne)) 
        | MeasureProd (ty1,ty2) -> layoutTypeWithPrec denv env 2 ty1 ^^ wordL "*" ^^ layoutTypeWithPrec denv env 2 ty2
        | MeasureInv ty -> wordL "/" ^^ layoutTypeWithPrec denv env 1 ty
        | MeasureOne  -> wordL "1" 
        | _ -> 
        if typ.IsNamed then 
            let tcref = typ.NamedEntity 
            let tyargs = typ.GenericArguments |> Seq.toList
            // layout postfix array types
            layoutTypeApplication denv env (layoutTyconRef denv tcref) prec tcref.UsesPrefixDisplay tyargs 
            
        elif typ.IsTuple then 
            let tyargs = typ.GenericArguments |> Seq.toList
            bracketIfL (prec <= 2) (layoutTypesWithPrec denv env 2 (wordL "*") tyargs)

        elif typ.IsFunction then 
            let rec loop soFarL (typ:FSharpType) = 
              if typ.IsFunction then 
                  let domainTyp,retType = typ.GenericArguments.[0], typ.GenericArguments.[1]
                  loop (soFarL --- (layoutTypeWithPrec denv env 4 typ.GenericArguments.[0] ^^ wordL "->")) retType
              else 
                  soFarL --- layoutTypeWithPrec denv env 5 typ
            bracketIfL (prec <= 4) (loop emptyL typ)

        elif typ.IsGenericParameter then 
            layoutTyparAux denv env typ.GenericParameter

        else 
            wordL "(type)" 
            //typ.IsMeasure then 
            // | TType_measure unt -> layoutMeasureAux denv env 4 unt

    /// Layout a list of types, separated with the given separator, either '*' or ',' 
    and layoutTypesWithPrec denv env prec sep typl = 
        sepListL sep (List.map (layoutTypeWithPrec denv env prec) typl)

    /// Layout a single type
    and layoutTypeAux denv env typ = 
        layoutTypeWithPrec denv env 5 typ

    and layoutType denv typ  = 
        layoutTypeAux denv () typ


    
    /// Layout type parameters
    let layoutTyparDeclsAfter denv includeConstraints nmL (typars : seq<FSharpGenericParameter>) =
        let tpcs = if includeConstraints then indexedConstraints typars else []
        let typars = typars |> List.ofSeq 
        
        match typars with 
        | []  -> 
            nmL
        | _ -> 
            let coreL = sepListL (sepL ",") (List.map (layoutTyparAux denv ()) typars)
            let coreL = layoutConstraintsAfter denv () coreL tpcs
            nmL ^^ angleL denv coreL 


    let layoutMemberOrVal denv (v:FSharpMemberOrVal) = 
        let isCtor = (v.LogicalName = ".ctor")
        let isItemIndexer = (v.IsInstanceMember && v.DisplayName = "Item")

        let nameL = 
            let tyname = v.LogicalEnclosingEntity.DisplayName
            if isCtor then 
                wordL ("new " + tyname)
            elif v.IsInstanceMember  then 
                if isItemIndexer then 
                    wordL (String.uncapitalize tyname + ".[") 
                else
                    wordL (String.uncapitalize tyname + "." + v.DisplayName) 
                
            elif not v.IsMember && 
                 not (hasAttrib<RequireQualifiedAccessAttribute> v.LogicalEnclosingEntity.Attributes) && 
                 // Beta2 workaround: in FSharp.Core, option was not marked with RequireQualifiedAccess in beta2
                 (let compilingFslib = (denv.thisAssembly.ReflectionAssembly.GetName().Name = "FSharp.Core")
                  not (compilingFslib && v.LogicalEnclosingEntity.DisplayName = "Option")) then 
                // In FSharp.Core.dll filter out the attributes
                wordL v.DisplayName
            else 
                wordL (tyname + "." + v.DisplayName)

        //let nameL = if denv.htmlHideRedundantKeywords || v.IsInstanceMember || isCtor || not v.IsMember then kwL else wordL "static" ++ kwL
        //let nameL = if v.IsMutable then wordL "mutable" ++ nameL else nameL
        let nameL = layoutAccessibility denv v.Accessibility nameL
        let nameL = if denv.htmlHideRedundantKeywords || not  v.IsDispatchSlot then nameL else wordL "abstract" ++ nameL
        let nameL = 
            if not denv.html && v.InlineAnnotation = FSharpInlineAnnotation.AlwaysInline && not denv.suppressInlineKeyword then 
                wordL "inline" ++ nameL 
            else 
                nameL

        (* Drop the names from value arguments when printing them *)
        let argInfos = v.CurriedParameterGroups |> Seq.map Seq.toList |> Seq.toList 
        let retType = v.ReturnParameter.Type
        let argInfos, retType = 
            if v.IsGetterMethod then 
                match argInfos with 
                | [[]] -> [], Some retType
                | _ -> argInfos, Some retType
            elif v.IsSetterMethod then 
                match argInfos with 
                | [ args ] -> [ allButLast args ], Some (last args).Type
                | _ -> argInfos, None
            else 
                argInfos,Some retType

        let isOverGeneric = false 
#if TODO
        // TODO: 
        // let isOverGeneric = List.length (Zset.elements (free_in_type CollectTyparsNoCaching tau).FreeTypars) < List.length tps (* Bug: 1143 *)
#endif
        
        // Extension members can have apparent parents whcih are not F# types.
        // Hence getting the generic argument count if this is a little trickier
        let numGenericParamsOfApparentParent = 
            let pty = v.LogicalEnclosingEntity 
            if pty.IsExternal then 
                let ty = v.LogicalEnclosingEntity.ReflectionType 
                if ty.IsGenericType then ty.GetGenericArguments().Length 
                else 0 
            else 
                pty.GenericParameters.Count

        let tps = v.GenericParameters |> Seq.skip numGenericParamsOfApparentParent

        let usageL = 
            if v.IsTypeFunction || isOverGeneric || denv.showTyparBinding then 
                layoutTyparDeclsAfter denv false nameL tps
            else 
                nameL

        let cxs  = indexedConstraints v.GenericParameters 

        // Parenthesize the return type to match the topValInfo 
        let retTypeL  = 
            match retType with 
            | None -> wordL "unit"
            | Some ty -> layoutTypeWithPrec denv () 4 ty
        let retTypeL = wordL ":" --- retTypeL

        // Format each argument, including its name and type 
        let layoutArgUsage doc i (arg: FSharpParameter) = 
           
            // Detect an optional argument 
            let isOptionalArg = hasAttrib<OptionalArgumentAttribute> arg.Attributes
            let nm = match arg.Name with null -> "arg" + string i | nm -> nm
            let argName = if isOptionalArg then "?" + nm else nm
            if doc then leftL argName ^^ rightL ":" ^^ layoutTypeWithPrec denv () 2 arg.Type
            else wordL argName

        let usageL = 
            match argInfos with
            | [] -> 
                usageL ++ retTypeL 
            | _  -> 

               let argNumber = ref 0 
               let allArgsUsageL = 
                   argInfos 
                   |> List.map (fun xs  -> xs |> List.map (fun x -> incr argNumber; layoutArgUsage false !argNumber x))
                   |> List.map (function [] -> wordL "()" 
                                       | [arg] when not v.IsMember || isItemIndexer -> arg 
                                       | args when isItemIndexer -> sepListL (sepL ", ") args
                                       | args -> bracketL (sepListL (sepL ", ") args))

               let usageL = List.fold (---) usageL allArgsUsageL 
               let usageL = 
                   if isItemIndexer then usageL ++ wordL "]" 
                   else usageL
               usageL


        let docL = 
            let afterDocs = 
                [ let argCount = ref 0 
                  for xs in argInfos do 
                    for x in xs do 
                       incr argCount
                       yield layoutArgUsage true !argCount x

                  if not v.IsGetterMethod && not v.IsSetterMethod && retType.IsSome then 
                      yield wordL "returns" ++ retTypeL
                  match layoutConstraints denv () cxs with 
                  | None ->  ()
                  | Some cxsL -> yield cxsL ]
            match afterDocs with
            | [] -> emptyL
            | _ -> (List.reduce (@@) [ yield wordL ""; yield! afterDocs ])

        let noteL = 
            let noteDocs = 
                [ if cxs |> List.exists (snd >> List.exists (fun cx -> cx.IsMemberConstraint)) then
                     yield (wordL "Note: this operator is overloaded")  ]
            match noteDocs with
            | [] -> emptyL
            | _ -> (List.reduce (@@) [ yield wordL ""; yield! noteDocs ])
                
        let usageL = if v.IsSetterMethod then usageL --- wordL "<- v" else usageL
        
        //layoutAttribs denv v.Attributes 
        usageL  , docL, noteL
    

    /// Layout type definition "Type Description" section (doesn't include constructors and members)
    let layoutTypeDefn denv (tycon:FSharpEntity) =
        let typeWordL = 
            (if tycon.IsMeasure then wordL "[<Measure>] type" else wordL "type") 
        let lhsL =
            let tps = tycon.GenericParameters
            
            let tpsL = layoutTyparDeclsAfter denv false (wordL tycon.DisplayName) tycon.GenericParameters
            typeWordL ^^ tpsL
        let adhoc = 
            tycon.MembersOrValues
            |> Seq.toList 
            |> List.sortBy (fun m -> m.DisplayName, (m.CurriedParameterGroups |> Seq.map Seq.length |> Seq.toList))
#if TODO
            |> List.filter (fun v -> 
                                match v.MemberInfo.Value.ImplementedSlotSigs with 
                                | TSlotSig(_,oty,_,_,_,_) :: _ -> 
                                    // Don't print overrides in HTML docs
                                    denv.showOverrides && 
                                    // Don't print individual methods forming interface implementations - these are currently never exported 
                                    not (is_interface_typ denv.g oty)
                                | [] -> true)
#endif
            |> List.filter (fun v -> denv.showObsoleteMembers || 
                                     tycon.Attributes |> Seq.exists (fun attrib -> attrib.ReflectionType = typeof<System.ObsoleteAttribute>))

        if tycon.IsRecord then 
           (lhsL ^^ wordL "=") --- 
           (tycon.RecordFields 
               |> Seq.toList
               |> List.map (fun fld -> 
                      let lhs = wordL fld.Name 
                      let lhs = if fld.IsMutable then wordL "mutable" --- lhs else lhs
                      (lhs ^^ rightL ":") --- layoutType denv fld.Type)
               |> aboveListL |> braceL)
#if TODO
                let addReprAccessL l = accessibilityL denv tycon.TypeReprAccessibility l 
                let denv = denv_scope_access tycon.TypeReprAccessibility denv  
#endif
         elif tycon.IsAbbreviation then 
             (lhsL ^^ wordL "=") --- (layoutType denv tycon.AbbreviatedType)

         //elif tycon.IsDelegate then    
         //    let rty = GetFSharpViewOfReturnType denv.g rty
         //    (lhsL ^^ wordL "=") --- wordL "delegate of" --- topTypeL denv SimplifyTypes.typeSimplificationInfo0 (paraml |> List.mapSquared (fun sp -> (sp.Type, TopValInfo.unnamedTopArg1))) rty [])
         elif tycon.IsUnion then    
              (tycon.UnionCases 
                 |> Seq.toList
                 |> List.map (fun ucase ->
                      let nmL = wordL ucase.Name
                      match ucase.Fields |> Seq.toList |> List.map (fun rfld -> rfld.Type) with
                      | []     -> wordL "|" ^^ nmL
                      | argtys -> (wordL "|" ^^ nmL ^^ wordL "of") --- 
                                  sepListL (wordL "*") (List.map (layoutType denv) argtys))

                 |> aboveListL)

         elif (not tycon.IsAbbreviation && not tycon.HasAssemblyCodeRepresentation && tycon.ReflectionType.IsEnum) then    
              (lhsL ^^ wordL "=") --- 
              (tycon.ReflectionType.GetFields(System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic ||| System.Reflection.BindingFlags.Static)
               |> Seq.toList
               |> List.filter (fun f -> f.IsLiteral)
               |> List.map (fun f -> wordL "| " ^^ wordL f.Name  ^^ wordL " = " ^^ wordL (sprintf "%A" (f.GetRawConstantValue())) )
               |> aboveListL)
         else lhsL 

    let layoutExceptionDefn denv (exnc:FSharpEntity) =
        let exnL = wordL "exception" ^^ layoutAccessibility denv exnc.Accessibility (wordL exnc.DisplayName) 
        if exnc.IsAbbreviation then 
            exnL ^^ wordL "=" --- layoutType denv exnc.AbbreviatedType
        else
            match exnc.RecordFields |> Seq.toList |> List.map (fun rfld -> rfld.Type) with
             | []     -> exnL 
             | argtys -> exnL ^^ wordL "of" --- sepListL (wordL "*") (List.map (layoutType denv) argtys)




module HtmlDocWriter =
    open LayoutUtilities
    open LibUtilities
    open FSharpMetadataUtilities
    open FSharpMetadataPrintUtilities
    
    // bug://1813. 
    // The generated HTML docs contain these embedded URLs.
    let urlForFSharp                = "http://www.fsharp.net"
    // These manual links should be links to reference copies of the library documentation directory (without the /-slash)
    let urlForFSharpCoreManual      = "http://research.microsoft.com/fsharp/manual/FSharp.Core" (* appended with "/Microsoft.FSharp.Collections.List.html" etc... *)
    let urlForFSharpPowerPackManual = "http://research.microsoft.com/fsharp/manual/FSharp.PowerPack" (* appended with "/Microsoft.FSharp.Collections.List.html" etc... *)

    let pseudoCaseInsensitive (a:string) (b:string) = 
        let c1 = compare (a.ToLowerInvariant()) (b.ToLowerInvariant())
        if c1 <> 0 then c1 else 
        compare a b

    let widthVal  = 80
    let widthType = 80
    let widthException  = 80


    /// String for layout squashed to a given width.
    /// HTML translation and markup for TYPE and VAL syntax.
    /// Assumes in <pre> context (since no explicit <br> linebreaks).
    let outputL width layout =
        let baseR = htmlR stringR
        let render = 
            { new Renderer<_,_> with 
                 member r.Start () = baseR.Start()
                 member r.AddText z s = baseR.AddText z s;  (* REVIEW: escape HTML chars *)
                 member r.AddBreak z n = baseR.AddBreak z n
                 member r.Finish z = baseR.Finish z 
                 member x.AddTag z (tag,attrs,start) =
                          match tag,start with
                          | "TYPE",_     -> z
                          | "VAL" ,true  -> baseR.AddText z "<B>"
                          | "VAL" ,false -> baseR.AddText z "</B>"
                          | _     ,start -> baseR.AddTag z (tag,attrs,start)}
        renderL render (Display.squash_layout { FormatOptions.Default with PrintWidth=width } layout)

    type kind = PathK | TypeK of int (* int is typar arity *)

    let WriteHTMLDoc (assem: Microsoft.FSharp.Metadata.FSharpAssembly, outputDir, append, cssFile, namespaceFile, htmlDocLocalLinks, xmlFile : string) =
        let assemblyName = assem.ReflectionAssembly.GetName().Name
        let namespaceFileFullPath = Path.Combine(outputDir,namespaceFile)
        /// The name to use to link up to the namespace file 
        let namespaceFileUpOne = (hrefConcat ".." namespaceFile)
        let wrap (oc:TextWriter) (a:string) (b:string) f = 
            oc.WriteLine a;
            f ();
            oc.WriteLine b

        // Read in the supplied XML file, map its name attributes to document text 
        let settings = XmlReaderSettings()   
        let reader = XmlReader.Create(xmlFile,settings)
        let doc = XmlDocument()
        doc.Load(reader)
        let elemList = doc.GetElementsByTagName("member")
        let xmlMemberMap = [for e in elemList do yield (e.Attributes.[0].Value,e.InnerXml)]  |> Map.ofList

        let newExplicitFile append fullname bfilename title cssFile f = 
            use oc = 
                if append && File.Exists fullname then 
                    System.IO.File.AppendText fullname
                else 
                    System.IO.File.CreateText fullname

            fprintfn oc "<HTML><HEAD><TITLE>%s</TITLE><link rel=\"stylesheet\" type=\"text/css\"href=\"%s\"></link></HEAD><BODY>" title cssFile;
            let backlink = (bfilename,title)
            f backlink oc;
            let ver = assem.ReflectionAssembly.GetName().Version.ToString()
            fprintfn oc "<br /> <br/><p><i>Documentation for assembly %s, version %s, generated using the <a href='%s'>F# Programming Power Pack</a></i></p>" assemblyName ver urlForFSharp;
            fprintfn oc "</BODY></HTML>";

        let newFile fdir fileName title f =
            let dir = Path.Combine(outputDir, fdir)
            System.IO.Directory.CreateDirectory(dir) |> ignore
            let fullname = Path.Combine(dir, fileName)
            newExplicitFile false fullname fileName title (hrefConcat ".." cssFile) f

        let hlink url text = sprintf "<a href='%s'>%s</a>" url text
       
        (* Path *)
        let path0        = []
        let path1 x kind = [(x,kind)]
        let pathExtend xs x kind = xs @ [x,kind] in (* short shallow lists *)
        let pathText     xs = String.concat "." (List.map fst xs)
        let pathFilename xs =
            let encode = function
              | x,PathK   -> x
              | x,TypeK w ->
                  // Mangle to avoid colliding upper/lower names, 'complex' and 'Complex', to different filenames 
                  // See also tastops.fs which prints type hrefs 
                  "type_" + (underscoreLowercase x) + (if w=0 then "" else "-" + string w)
            String.concat "." (List.map encode xs) + ".html"

        let collapseStrings xs = String.concat "." xs
        let pathWrt (knownNamespaces:Set<string>) x =
            let xs = split '.' x
            let rec collapse front back = 
              match back with 
              | [] -> (if front = [] then [] else [collapseStrings front]) @ back
              | mid::back -> 
                  if knownNamespaces.Contains (collapseStrings (front@[mid]))  
                  then [collapseStrings (front@[mid])] @ back
                  else collapse (front@[mid]) back
            List.map (fun x -> (x,PathK)) (collapse [] xs)

        let nestBlock hFile f =
            wrap hFile "<br><dl>" "</dl>"
               (fun () -> f())

        let nestItem hFile f =
            wrap hFile "<dt></dt><dd>" "</dd>"
               (fun () -> f())
          
        (* TopNav - from paths *)
        let newPathTrail hFile kind path =
            let rec writer prior = function
              | []         -> ()
              | [x,k]      -> fprintf hFile "%s " x
              | (x,k)::xks -> let prior = pathExtend prior x k
                              let url   = pathFilename prior
                              let sep   = if xks=[] then "" else "."
                              let item  = hlink url x
                              fprintf hFile "%s%s" item sep;
                              writer prior xks
            let uplink = sprintf "[<a href='%s'>Home</a>] " namespaceFileUpOne
            nestItem hFile (fun () ->
              fprintf hFile "<h1>%s%s " uplink kind;
              writer path0 path;
              fprintf hFile "</h1>";
              fprintf hFile "<br>\n")

        let newPartitions hFile f =
            nestItem hFile (fun () ->
              wrap hFile "<table>" "</table>" (fun () -> 
                f()))
        
        let newPartition hFile desc f =
            wrap hFile (sprintf "  <tr valign='top'><td>%s" desc) (sprintf "  </td></tr>") (fun () -> 
              f())

        let newSectionInPartition hFile title f =
            wrap hFile (sprintf "  <dt><h3>%s</h3></dt><dd>" title) (sprintf "  </dd>") (fun () -> 
              f())

        let newPartitionsWithSeeAlsoBacklink hFile (bfilename,btitle) f = 
            newPartitions hFile (fun () ->
              f();
              newPartition hFile "" (fun () -> 
                newSectionInPartition hFile "See Also" (fun () -> 
                  fprintf hFile "<a href=\"%s\">%s</a>" bfilename btitle)))

        let newTable0 hFile title f = 
            newSectionInPartition hFile title (fun () -> 
              wrap hFile "<table width=\"100%%\">" "</table>" (fun () -> 
                f()))

        let newTable1 hFile title width1 h1 f = 
            newTable0 hFile title (fun () -> 
              wrap hFile (sprintf "<tr><th width=%d%%>%s</th></tr>" width1 h1) "" (fun () -> 
                f()))
        
        let newTable2 hFile title width1 h1 h2 f = 
            newTable0 hFile title (fun () -> 
              wrap hFile (sprintf "<tr><th width=%d%%>%s</th><th>%s</th></tr>" width1 h1 h2) "" (fun () -> 
                f()))
        
        let newNamespaceEntry hFile fdir path (allModules:FSharpEntity list) typeDefns =
            let fileName = pathFilename path
            let title = pathText path
            let url = fdir + "/" + fileName
            fprintf hFile "<tr valign='top'><td width='50%%'><a href='%s'>%s</a>\n" url title;

            // Sort sub-modules into alphabetical order 
            let allModules = 
                allModules 
                |> List.filter (fun x -> x.IsModule)
                |> List.sortBy (fun x -> x.DisplayName)

            // Make them hyperlink to fdir/<path>.html 
            let typeLinks = 
                typeDefns
                |> List.map (fun (tycon:FSharpEntity) ->
                               let path = pathExtend path tycon.DisplayName (TypeK tycon.GenericParameters.Count)
                               let url   = fdir + "/" + pathFilename path
                               //hlink url (sprintf "[%s]" (tycon.DisplayNameWithUnderscoreTypars.Replace("<","&lt;").Replace(">","&gt;")))) 
                               let nm = 
                                   if (typeDefns |> List.filter (fun tycon2 -> tycon.DisplayName = tycon2.DisplayName) |> List.length) > 1 then 
                                      tycon.DisplayName + (if tycon.GenericParameters.Count > 0 then "`" + string tycon.GenericParameters.Count else "")
                                   else
                                      tycon.DisplayName
                               
                               hlink url (sprintf "[%s]" nm)) 
                |> String.concat ", "
            let moduleLinks = 
                allModules
                |> List.map (fun modul ->
                               let path = pathExtend path modul.DisplayName PathK
                               let url   = fdir + "/" + pathFilename path
                               hlink url (sprintf "[%s]" modul.DisplayName)) 
                |> String.concat ", "
            fprintfn hFile "</td>" ;
            fprintfn hFile  
                "<td>%s%s</td>" 
                (if not typeDefns.IsEmpty then sprintf "Types: %s" typeLinks else "")
                (if not allModules.IsEmpty then sprintf "<br><br>Modules: %s<br>" moduleLinks else "");    
            fprintfn hFile "</tr>" ;
            ()

        let newEntry1 hFile0 title = 
            fprintfn hFile0 "<tr valign=\"top\"><td>%s</td></tr>" title 

        let newEntry2     hFile0 title desc = 
            fprintfn hFile0 "<tr valign=\"top\"><td>%s</td><td>%s</td></tr>" title desc

        let initialDisplayEnv = 
            let denv = 
                { thisAssembly = assem;
                  html=true;
                  htmlHideRedundantKeywords=false;
                  htmlAssemMap=
                       Map.ofList 
                         (if htmlDocLocalLinks
                          then []
                          else [("FSharp.Core",urlForFSharpCoreManual);
                                ("FSharp.PowerPack",urlForFSharpPowerPackManual)])
                  showObsoleteMembers=showObsoleteMembers;
                  showTyparBinding = false;
                  suppressInlineKeyword=true;
                  showMemberContainers=false;
                  showAttributes=true;
                  showOverrides=false;
                  showConstraintTyparAnnotations=showConstraintTyparAnnotations;
                  abbreviateAdditionalConstraints=false;
                  showTyparDefaultConstraints=false;
                  shortConstraints=shortConstraints;
                  contextAccessibility = Unchecked.defaultof<FSharpAccessibility> ;
                }

            denv

        newExplicitFile append namespaceFileFullPath namespaceFileUpOne "Namespaces" cssFile (fun blinkNamespacesFile hNamespacesFile -> 

          wrap hNamespacesFile (sprintf "<dl><dt><br/></dt><dd><table><tr><th>Namespaces in assembly %s</th><th>Description</th></tr>" assemblyName)
                               (        "</table></dl>") (fun () ->

            let obsoleteText (attribs: ReadOnlyCollection<FSharpAttribute>) = 
                match tryFindAttrib<System.ObsoleteAttribute> attribs with
                | Some(attr) -> sprintf "<p><b>Note</b>: %s</p>" attr.Message
                | _ -> ""

            let isObsolete attribs = hasAttrib<System.ObsoleteAttribute> attribs 

            let isUnseenVal (v:FSharpMemberOrVal) = 
                // not (IsValAccessible Infos.AccessibleFromEverywhere (mk_local_vref v)) ||
                v.IsCompilerGenerated ||
                (not initialDisplayEnv.showObsoleteMembers &&  isObsolete v.Attributes) 

            let isUnseenEntity (e:FSharpEntity) = 
                // not (IsEntityAccessible Infos.AccessibleFromEverywhere (mk_local_tcref e)) ||
                (not initialDisplayEnv.showObsoleteMembers &&  isObsolete e.Attributes) 

            let rec doValue denv fdir hFile path (v:FSharpMemberOrVal) = 
                let denv = { denv with htmlHideRedundantKeywords=true }
                let usageL, docL, noteL = FSharpMetadataPrintUtilities.layoutMemberOrVal denv v
                newEntry2 hFile ("<pre>"+outputL widthVal usageL+"</pre>") (obsoleteText v.Attributes + getDocFromSig v.XmlDocSig xmlMemberMap + "<pre>" + outputL widthVal docL + "</pre>" + "<p>" + outputL widthVal noteL + "</p>")

            let rec doValues denv fdir hFile path title item (vals:FSharpMemberOrVal list) = 
                let vals = vals |> List.filter (fun v -> not v.IsCompilerGenerated)
                let vals = vals |> List.sortBy (fun v -> v.DisplayName) 
                if not vals.IsEmpty then 
                  newTable2 hFile title 40 item  "Description" (fun () -> 
                    vals |> List.iter (doValue denv fdir hFile path))

            let rec doTypeDefn denv fdir hFile path (tycon:FSharpEntity) = 
                newSectionInPartition hFile "Type Description" (fun () ->
                  fprintf hFile "<pre>%s</pre>" (outputL widthType (FSharpMetadataPrintUtilities.layoutTypeDefn denv  tycon))) ;
                let vals = 
                    tycon.MembersOrValues 
                    |> List.ofSeq
                    |> List.filter (isUnseenVal >> not)

                let ivals,svals = vals |> List.partition (fun v -> v.IsInstanceMember)
                let cvals,svals = svals |> List.partition (fun v -> v.CompiledName = ".ctor")

                let iimpls = if (not tycon.IsAbbreviation && not tycon.HasAssemblyCodeRepresentation && tycon.ReflectionType.IsInterface) then [] else tycon.Implements |> Seq.toList 

                // TODO: layout base type in some way
                if not iimpls.IsEmpty then 
                  newTable1 hFile "Interfaces" 40 "Type"  (fun () -> 
                    iimpls |> List.iter (fun i -> 
                        newEntry1 hFile ("<pre>"+outputL widthVal (layoutType denv i)+"</pre>"))) 
                // TODO: layout union cases as a table, with documentation
                // TODO: layout record fields as a table, with documentation
                doValues denv fdir hFile path "Constructors" "Member" cvals;
                doValues denv fdir hFile path "Instance Members" "Member" ivals;
                doValues denv fdir hFile path "Static Members" "Member"   svals;
                //doValues denv fdir hFile path "Deprecated Members" "Member" dvals;

                // TODO: layout events
                // TODO: layout extension members

                // mailboxProcessor.add_Error (null)
                   
            let rec doTypeDefns denv fdir blinkFile hFile path title (typeDefns:FSharpEntity list) = 
                if typeDefns <> [] then  
                  newTable2 hFile title 30 "Type" "Description" (fun () -> 
                    let typeDefns = typeDefns |> List.sortBy (fun tc -> tc.CompiledName) 
                    typeDefns |> List.iter (fun tycon ->
                      let tyname = tycon.DisplayName
                      let path  = pathExtend path tyname (TypeK tycon.GenericParameters.Count)
                      let fileName  = pathFilename path
                      let title  = pathText path    (* used as html page title *)
                      let text = obsoleteText tycon.Attributes
                      let text = text + (getDocFromSig tycon.XmlDocSig xmlMemberMap)
                      let text = 
                        if tycon.IsAbbreviation then 
                            text + "</p> <p> Note: an abbreviation for "+("<tt>"+outputL widthType (FSharpMetadataPrintUtilities.layoutType denv tycon.AbbreviatedType)+"</tt>")
                        else 
                            text
                      let tytext = outputL widthType (layoutTyparDeclsAfter denv false (wordL tycon.DisplayName) tycon.GenericParameters)
                      
                      newEntry2 hFile ("type " + hlink fileName tytext) text;

                      newFile fdir fileName title (fun blinkFile2 hFile2 ->
                        nestBlock hFile2 (fun () ->
                          newPathTrail hFile2 "Type" path;
                          newPartitionsWithSeeAlsoBacklink hFile2 blinkFile  (fun () -> 
                            newPartition hFile2 text (fun () ->
                              doTypeDefn denv fdir hFile2 path tycon))))))
            
            let rec doExceptionDefn denv fdir hFile path (exnc: FSharpEntity) = 
                newSectionInPartition hFile "Description" (fun () ->
                  let doc = getDocFromSig exnc.XmlDocSig xmlMemberMap
                  fprintf hFile "<pre>%s</pre><p>%s</p>" (outputL widthException (FSharpMetadataPrintUtilities.layoutExceptionDefn denv exnc)) doc)

            let rec doExceptionDefns denv fdir blinkFile hFile path (exceptionDefns:FSharpEntity list) = 
                if not exceptionDefns.IsEmpty then  
                    newTable2 hFile "Exceptions" 40 "Exception" "Description" (fun () -> 
                        let exceptionDefns = exceptionDefns |> List.sortBy (fun exnc -> exnc.DisplayName) 
                        exceptionDefns |> List.iter (fun exnc ->
                            let path  = pathExtend path exnc.DisplayName (TypeK exnc.GenericParameters.Count)
                            let fileName  = pathFilename path
                            let exname = exnc.DisplayName
                            let title  = pathText path   (* used as html page title *)
                            let text = obsoleteText exnc.Attributes
                            let text = text + (getDocFromSig exnc.XmlDocSig xmlMemberMap)
                            let text = 
                              if exnc.IsAbbreviation then 
                                  text+"</p> <p> Note: an abbreviation for "+("<tt>"+outputL widthException (FSharpMetadataPrintUtilities.layoutType denv exnc.AbbreviatedType)+"</tt>")
                              else 
                                  text
                            newEntry2 hFile ("exception " + hlink fileName exname) text;
                            newFile fdir fileName title (fun blinkFile2 hFile2 ->
                              nestBlock hFile2 (fun () ->
                                newPathTrail hFile2 "Exception" path;
                                newPartitionsWithSeeAlsoBacklink hFile2 blinkFile  (fun () -> 
                                  newPartition hFile2 text (fun () ->
                                    doExceptionDefn denv fdir hFile2 path exnc))))))
            
            let rec doModule denv fdir path blinkFile hFile (modul:FSharpEntity) = 

                let modules = 
                    modul.NestedEntities
                    |> List.ofSeq
                    |> List.filter (fun x -> x.IsModule)
                    |> List.sortBy (fun x -> x.DisplayName) 
                    |> List.filter (isUnseenEntity >> not)

                let typeDefns = 
                    modul.NestedEntities
                    |> List.ofSeq
                    |> List.filter (fun x -> not x.IsModule)
                    |> List.filter (fun x -> not x.IsExceptionDeclaration)
                    |> List.filter (isUnseenEntity >> not)

                let exceptionDefns = 
                    modul.NestedEntities
                    |> List.ofSeq
                    |> List.filter (fun x -> x.IsExceptionDeclaration)
                    |> List.filter (isUnseenEntity >> not)

                let vals = 
                    modul.MembersOrValues
                        |> Seq.toList
                        |> List.filter (fun x -> not x.IsMember)
                        |> List.filter (isUnseenVal >> not)

                let extensionMembers = 
                    modul.MembersOrValues
                        |> Seq.toList
                        |> List.filter (fun x -> x.IsExtensionMember)
                        |> List.filter (isUnseenVal >> not)

                let apvals,vals = 
                    vals 
                        |> List.partition (fun x -> x.IsActivePattern)

                doModules denv fdir blinkFile hFile path modules; 
                doTypeDefns denv fdir blinkFile hFile path "Type Definitions" typeDefns;
                doExceptionDefns denv fdir blinkFile hFile path exceptionDefns;
                doValues denv fdir hFile path "Values" "Value" vals; 
                doValues denv fdir hFile path "Active Patterns" "Active Pattern" apvals; 
                doValues denv fdir hFile path "Extension Members" "Extension Member" extensionMembers; 
                //doTypeDefns denv fdir blinkFile hFile path "Deprecated/Unsafe Type Definitions" dtycons;
                //doValues denv fdir hFile path "Deprecated Values" "Value" dvals 

            and doModules denv fdir blinkFile hFile path (modules:FSharpEntity list) = 
                if modules <> [] then  
                  newTable2 hFile (sprintf "Modules (as contributed by assembly '%s')" assemblyName) 30 "Module" "Description" (fun () -> 
                    let modules = modules |> List.sortBy (fun x -> x.DisplayName) 
                    modules |> List.iter (fun modul -> 
                      let path = pathExtend path modul.DisplayName PathK
                      let fileName = pathFilename path
                      let title = pathText path
                      let text = obsoleteText modul.Attributes
                      let text  = getDocFromSig modul.XmlDocSig xmlMemberMap
                      newEntry2 hFile (hlink fileName title) text;
                      newFile fdir fileName title (fun blinkFile2 hFile2 ->
                        nestBlock hFile2 (fun () ->
                          newPathTrail hFile2 "Module" path;
                          newPartitionsWithSeeAlsoBacklink hFile2 blinkFile  (fun () -> 
                            newPartition hFile2 text (fun () ->
                              doModule denv fdir path blinkFile2 hFile2 modul))))))

            let doEntities denv fdir path knownNamespaces (entities:FSharpEntity list) = 

(*
                let path = 
                    /// skip the first item in the path which is the assembly name 
                    match path with 
                    | None    -> Some ""
                    | Some "" -> Some modul.DisplayName
                    | Some p  -> Some (p+"."+ modul.DisplayName)
*)
                let path = 
                    match path with
                    | None   -> path0
                    | Some t -> pathWrt knownNamespaces t

                let allModules = 
                    entities
                    |> List.filter (fun x -> x.IsModule)
                    |> List.sortBy (fun x -> x.DisplayName) 
                    |> List.filter (isUnseenEntity >> not)
                    |> List.filter (fun m -> m.IsModule) 

                let exceptionDefns = 
                    entities
                    |> List.filter (fun x -> x.IsExceptionDeclaration)
                    |> List.filter (isUnseenEntity >> not)

                let typeDefns = 
                    entities
                    |> List.filter (fun x -> not x.IsModule)
                    |> List.sortBy (fun x -> x.CompiledName) 
                    |> List.filter (fun x -> not x.IsExceptionDeclaration)
                    |> List.filter (isUnseenEntity >> not)

                // In FSharp.Core.dll filter out the attributes
                let compilingFslib = (assemblyName = "FSharp.Core")

                let typeDefns = 
                   if compilingFslib then 
                       typeDefns |> List.filter (fun tycon -> 
                           // Filter out some things in FSharp.Core.dll docs
                           not (tycon.DisplayName = "PrintfFormat") && 
                           not (tycon.DisplayName = "FSharpTypeFunc") && 
                           not (tycon.DisplayName = "FSharpFunc") && 
                           not (tycon.DisplayName.StartsWith("Tuple",System.StringComparison.Ordinal)) && 
                           // In FSharp.Core.dll filter out the attributes
                           not (tycon.DisplayName.EndsWith("Attribute",System.StringComparison.Ordinal))
                           )
                   else
                       typeDefns 


                let fileName = pathFilename path
                let title = pathText path
                newFile fdir fileName title (fun blinkFile hFile ->
                    newNamespaceEntry hNamespacesFile fdir path allModules typeDefns;
                    doModules denv fdir blinkFile hFile path allModules;
                    doTypeDefns denv fdir blinkFile hFile path "Type Definitions" typeDefns;
                    doExceptionDefns denv fdir blinkFile hFile path exceptionDefns);
                 

            let allTopEntityGroups = 
                assem.Entities |> Seq.groupBy (fun e -> e.Namespace)  |> Seq.sortBy (fun (nsp,_) -> nsp)

            //let doEntities denv fdir path knownNamespaces (entities:FSharpEntity list) = 
            for (groupNameSpace,entityGroup) in allTopEntityGroups do 
                let knownNamespaces = set [groupNameSpace]
                doEntities initialDisplayEnv assemblyName (Some groupNameSpace) knownNamespaces (Seq.toList entityGroup)))

let mutable infiles = []
let mutable outputDir = "."
let mutable cssFile = "msdn.css"
let mutable namespaceFile = "namespaces.html"
let mutable htmlDocLocalLinks = false

ArgParser.Parse(
    [ ArgInfo("--outdir", ArgType.String (fun s -> outputDir <- s), "output directory");
      ArgInfo("--cssfile", ArgType.String (fun s -> cssFile <- s), "name of CSS file (defaults to msdn.css)");
      ArgInfo("--namespacefile", ArgType.String (fun s -> namespaceFile <- s), "name of the file in the output directory shared between multiple assemblies"); 
      ArgInfo("--locallinks", ArgType.Unit (fun () -> htmlDocLocalLinks <- true), "<internal use only>");
    ],
    (fun s -> infiles <- infiles @ [s]) ,
    "fshtmldoc.exe")

try 
    let mutable count = 0
    if infiles.Length <= 0 then 
       eprintfn "expected at least one DLL as input"
       exit 1;
    if not (System.IO.Directory.Exists outputDir) then 
        System.IO.Directory.CreateDirectory outputDir |> ignore;
    for infile in infiles do 
        let append = (count > 0 )
        count <- count + 1 
        printfn "Processing '%s'..." infile
        let xmlFile = System.IO.Path.ChangeExtension(infile, ".xml")
        if not (System.IO.File.Exists xmlFile) then 
             eprintfn "expected to find XML file at '%s'" xmlFile;
             exit 1;
           
        HtmlDocWriter.WriteHTMLDoc (FSharpAssembly.FromFile infile, outputDir, append, cssFile, namespaceFile, htmlDocLocalLinks, xmlFile);
    printfn "Done, docs written to directory '%s', namespace file is '%s' " outputDir namespaceFile
with e -> 
    printfn "Unexpected failure while writing HTML docs: %s" e.Message

