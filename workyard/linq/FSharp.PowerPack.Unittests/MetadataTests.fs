namespace FSharp.PowerPack.Unittests
open NUnit.Framework
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Metadata

[<TestFixture>]
type public MetadataTests() =
  

    let fsharpLibrary = Microsoft.FSharp.Metadata.FSharpAssembly.FSharpLibrary

    let okAttribs (attrs:seq<FSharpAttribute>) = 
        attrs |> Seq.forall (fun attr -> let ty = attr.ReflectionType in ty <> typeof<System.ObsoleteAttribute> && ty <> typeof<CompilerMessageAttribute>)

    let okEntity (x:FSharpEntity) = x.Attributes |> okAttribs

    let okVal (x:FSharpEntity,v:FSharpMemberOrVal) = v.Attributes |> okAttribs

    let explore (library:FSharpAssembly) = 
      for x in library.Entities do 
        let rec loop (x:FSharpEntity) = 
            if okEntity x then
              logMessage (sprintf "entity %s : XMLDOC SIG = %s" x.DisplayName x.XmlDocSig)
              for x in x.NestedEntities do 
                 loop x
              for v in x.MembersOrValues do 
                   if okVal (x,v) then 
                     logMessage (sprintf "value/member %s : XMLDOC SIG = %A" v.DisplayName v.XmlDocSig)
        loop x
    let seqItem item s = (s |> Seq.toArray).[item]

    let assertEquality (o1:obj) (o2:obj) =
        Assert.IsTrue((o1 = o2))
        Assert.AreEqual(o1.GetHashCode(), o2.GetHashCode())

    let assertInequality (o1:obj) (o2:obj) =
        Assert.IsFalse((o1 = o2))
        if o1.GetHashCode() = o2.GetHashCode() then
            printfn "Same hashcode for unequal objects %A %A" o1 o2

    let strictZip c1 c2 =
        Assert.AreEqual(Seq.length c1, Seq.length c2)
        Seq.zip c1 c2


    [<Test>]
    member this.Test1() = 

        let fsharpEntity1 = fsharpLibrary.Entities |> Seq.find (fun y -> y.LogicalName = "ResizeArray`1")

        check "cnwoein" fsharpEntity1.IsAbbreviation true 

        // This gets the abbreviation 'ResizeArray<'T> = System.Collections.Generic.List<'T>'
        let fsharpType1 = fsharpEntity1.AbbreviatedType

        check "ckwjen0o" fsharpType1.IsNamed true

        let fsharpEntity2 = fsharpType1.NamedEntity

        check "ckwjen0o" fsharpEntity2.IsExternal true

        let systemType = fsharpEntity2.ReflectionType
        
        check "ckwjen01" systemType.Name "List`1"
        

    [<Test>]
    member this.Test2() = 

        check "ckwfew0o4" fsharpLibrary.QualifiedName fsharpLibrary.ReflectionAssembly.FullName
        check "ckwfew0o4" (fsharpLibrary.QualifiedName.Contains("FSharp.Core")) true

        let fsharpEntity1 = fsharpLibrary.GetEntity("Microsoft.FSharp.Core.sbyte")

        check "ckwfew0o441" fsharpEntity1.IsAbbreviation true    

        // This gets the abbreviation 'sbyte = System.SByte'
        let fsharpType1 = fsharpEntity1.AbbreviatedType

        check "ckwjen0o4" fsharpType1.IsNamed true

        let fsharpEntity2 = fsharpType1.NamedEntity

        check "ckwjen0o4" fsharpEntity2.IsExternal true

        let systemType = fsharpEntity2.ReflectionType
        
        check "ckwjen014" systemType typeof<sbyte>


    [<Test>]
    member this.Test3() = 
        
        let fsharpEntity1 = fsharpLibrary.GetEntity("Microsoft.FSharp.Core.sbyte")

        check "ckwfew0o41" fsharpEntity1.IsExternal false
        check "ckwfew0o42" fsharpEntity1.IsModule false
        check "ckwfew0o43" fsharpEntity1.IsValueType false
        check "ckwfew0o442" fsharpEntity1.LogicalName "sbyte"
        check "ckwfew0o443" fsharpEntity1.IsAbbreviation true
        check "ckwfew0o444" (fsharpEntity1.DeclarationLocation.Document.Contains("prim-types-prelude.fs")) true
        check "ckwfew0o445" fsharpEntity1.HasAssemblyCodeRepresentation false
        check "ckwfew0o446" fsharpEntity1.UnionCases.Count 0
        check "ckwfew0o447" fsharpEntity1.RecordFields.Count 0
        check "ckwfew0o448" fsharpEntity1.XmlDocSig "T:Microsoft.FSharp.Core.sbyte"

    // Test the entity used for an F# library module
    [<Test>]
    member this.Test4() = 
        
        let fsharpEntity1 = fsharpLibrary.GetEntity("Microsoft.FSharp.Core.LanguagePrimitives")

        check "ckwfew0o41q" fsharpEntity1.IsExternal false
        check "ckwfew0o42w" fsharpEntity1.IsModule true
        check "ckwfew0o43e" fsharpEntity1.IsValueType false
        check "ckwfew0o44r" fsharpEntity1.LogicalName "LanguagePrimitives"
        check "ckwfew0o44t" fsharpEntity1.IsAbbreviation false
        check "ckwfew0o44y" (fsharpEntity1.DeclarationLocation.Document.Contains("prim-types.fs")) true
        check "ckwfew0o44u" fsharpEntity1.HasAssemblyCodeRepresentation false
        check "ckwfew0o44i" fsharpEntity1.UnionCases.Count 0
        check "ckwfew0o44o" fsharpEntity1.RecordFields.Count 0
        check "ckwfew0o44p" fsharpEntity1.XmlDocSig "T:Microsoft.FSharp.Core.LanguagePrimitives"

    // Test the entity used for F# floating point types annotated with units of measure
    [<Test>]
    member this.Test5() = 
        
        let fsharpEntity1 = fsharpLibrary.GetEntity("Microsoft.FSharp.Core.float`1")

        check "ckwfew0o41" fsharpEntity1.IsExternal false
        check "ckwfew0o42" fsharpEntity1.IsModule false
        check "ckwfew0o43" fsharpEntity1.IsValueType false
        check "ckwfew0o44a" fsharpEntity1.LogicalName "float`1"
        check "ckwfew0o44s" fsharpEntity1.IsAbbreviation false
        check "ckwfew0o44d" (fsharpEntity1.DeclarationLocation.Document.Contains("prim-types.fs")) true
        check "ckwfew0o44f" fsharpEntity1.HasAssemblyCodeRepresentation true
        check "ckwfew0o44g" fsharpEntity1.UnionCases.Count 0
        check "ckwfew0o44h" fsharpEntity1.RecordFields.Count 0
        check "ckwfew0o44j" fsharpEntity1.XmlDocSig "T:Microsoft.FSharp.Core.float`1"

    // Test the entity used for F# array types
    [<Test>]
    member this.Test6() = 
        
        let fsharpEntity1 = fsharpLibrary.GetEntity("Microsoft.FSharp.Core.[]`1")

        check "ckwfew0o41" fsharpEntity1.IsExternal false
        check "ckwfew0o42" fsharpEntity1.IsModule false
        check "ckwfew0o43" fsharpEntity1.IsValueType false
        check "ckwfew0o44k" fsharpEntity1.LogicalName "[]`1"
        check "ckwfew0o44l" fsharpEntity1.IsAbbreviation false
        check "ckwfew0o44z" (fsharpEntity1.DeclarationLocation.Document.Contains("prim-types-prelude.fs")) true
        check "ckwfew0o44x" fsharpEntity1.HasAssemblyCodeRepresentation true
        check "ckwfew0o44c" fsharpEntity1.UnionCases.Count 0
        check "ckwfew0o44v" fsharpEntity1.RecordFields.Count 0
        check "ckwfew0o44b" fsharpEntity1.XmlDocSig "T:Microsoft.FSharp.Core.[]`1"

    // Test the value used for List.map
    [<Test>]
    member this.Test7() = 
        
        let fsharpEntity1 = fsharpLibrary.GetEntity("Microsoft.FSharp.Collections.ListModule")
        let v = fsharpEntity1.MembersOrValues |> Seq.find (fun x -> x.CompiledName = "Map") 
        
        
        check "ckwfew0o47qa" v.CompiledName "Map"
        check "ckwfew0o47qb" v.IsCompilerGenerated false
        check "ckwfew0o47qc" v.IsExtensionMember false
        check "ckwfew0o47qd" v.IsImplicitConstructor false
        check "ckwfew0o47qe" v.IsModuleValueOrMember true
        check "ckwfew0o47qf" v.InlineAnnotation Microsoft.FSharp.Metadata.FSharpInlineAnnotation.OptionalInline
        check "ckwfew0o47qg" v.IsMutable false
        check "ckwfew0o47qh" v.IsTypeFunction false
        check "ckwfew0o47qh" v.XmlDocSig "M:Microsoft.FSharp.Collections.ListModule.Map``2(Microsoft.FSharp.Core.FSharpFunc`2{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"
        
        check "ckwfew0o47w" v.CurriedParameterGroups.Count 2
        check "ckwfew0o47e" v.CurriedParameterGroups.[0].Count 1
        check "ckwfew0o47r" v.CurriedParameterGroups.[1].Count 1
        check "ckwfew0o47t" v.CurriedParameterGroups.[0].[0].Name "mapping"
        check "ckwfew0o47y" v.CurriedParameterGroups.[1].[0].Name "list"
        check "ckwfew0o47u" v.CurriedParameterGroups.[0].[0].Type.IsFunction true
        check "ckwfew0o47i" v.CurriedParameterGroups.[0].[0].Type.IsNamed false
        check "ckwfew0o47o" v.CurriedParameterGroups.[0].[0].Type.IsTuple false
        check "ckwfew0o47p" v.CurriedParameterGroups.[0].[0].Type.IsGenericParameter false

        check "ckwfew0o47a" (v.CurriedParameterGroups.[0].[0].Type.GenericArguments.[0].IsGenericParameter) true
        check "ckwfew0o47s" (v.CurriedParameterGroups.[0].[0].Type.GenericArguments.[0].GenericParameterIndex) 0
        check "ckwfew0o47d" (v.CurriedParameterGroups.[0].[0].Type.GenericArguments.[1].IsGenericParameter) true
        check "ckwfew0o47f" (v.CurriedParameterGroups.[0].[0].Type.GenericArguments.[1].GenericParameterIndex) 1

        check "ckwfew0o47g" v.CurriedParameterGroups.[1].[0].Type.IsFunction false
        check "ckwfew0o47h" v.CurriedParameterGroups.[1].[0].Type.IsNamed true
        check "ckwfew0o47j" v.CurriedParameterGroups.[1].[0].Type.IsTuple false
        check "ckwfew0o47k" v.CurriedParameterGroups.[1].[0].Type.IsGenericParameter false

        check "ckwfew0o47l" (v.CurriedParameterGroups.[1].[0].Type.NamedEntity.LogicalName) "list`1"
        check "ckwfew0o47z" (v.CurriedParameterGroups.[1].[0].Type.NamedEntity.IsAbbreviation) true
        check "ckwfew0o47x" (v.CurriedParameterGroups.[1].[0].Type.GenericArguments.Count) 1
        check "ckwfew0o47c" (v.CurriedParameterGroups.[1].[0].Type.GenericArguments.[0].IsGenericParameter) true
        check "ckwfew0o47v" (v.CurriedParameterGroups.[1].[0].Type.GenericArguments.[0].GenericParameterIndex) 0
        check "ckwfew0o47b" (v.CurriedParameterGroups.[1].[0].Type.NamedEntity.IsAbbreviation) true

        check "ckwfew0o47n" (v.ReturnParameter.Type.NamedEntity.LogicalName) "list`1"
        check "ckwfew0o47m" (v.ReturnParameter.Type.GenericArguments.Count) 1
        check "ckwfew0o474" (v.ReturnParameter.Type.GenericArguments.[0].IsGenericParameter) true
        check "ckwfew0o475" (v.ReturnParameter.Type.GenericArguments.[0].GenericParameterIndex) 1

    // Test the value for (|||)
    [<Test>]
    member this.Test7b() = 
        
        let fsharpEntity1 = fsharpLibrary.GetEntity("Microsoft.FSharp.Core.Operators")
        let v = fsharpEntity1.MembersOrValues |> Seq.find (fun x -> x.CompiledName = "op_BitwiseOr") 
        
        
        check "ckwfew0o47qa" v.CompiledName "op_BitwiseOr"
        check "ckwfew0o47qb" v.IsCompilerGenerated false
        check "ckwfew0o47qb" v.IsActivePattern false
        check "ckwfew0o47qc" v.IsExtensionMember false
        check "ckwfew0o47qd" v.IsImplicitConstructor false
        check "ckwfew0o47qe" v.IsModuleValueOrMember true
        check "ckwfew0o47qf" v.InlineAnnotation Microsoft.FSharp.Metadata.FSharpInlineAnnotation.PsuedoValue
        check "ckwfew0o47qg" v.IsMutable false
        check "ckwfew0o47qh" v.IsTypeFunction false
        

    // Test the value used for .Length on the list type
    [<Test>]
    member this.Test8() = 
        
        let fsharpEntity1 = fsharpLibrary.GetEntity("Microsoft.FSharp.Collections.List`1")
        fsharpEntity1.MembersOrValues |> Seq.iter (fun x -> printfn "Found member '%s' in List`1 metadata" x.CompiledName)
        let v = fsharpEntity1.MembersOrValues |> Seq.find (fun x -> x.CompiledName = "get_Length") 
        
        
        check "ckwfew0o48q" v.CompiledName "get_Length"
        // one argument group, no entries in it
        check "ckwfew0o48w" v.CurriedParameterGroups.Count 1
        check "ckwfew0o48w" v.CurriedParameterGroups.[0].Count 0

        check "ckwfew0o48n" (v.ReturnParameter.Type.NamedEntity.LogicalName) "int"
        check "ckwfew0o48m" (v.ReturnParameter.Type.GenericArguments.Count) 0


    [<Test>]
    member this.Test9() = 

        let fsharpEntity1 = fsharpLibrary.GetEntity("Microsoft.FSharp.Collections.List`1")

        check "ckwfew0o41" fsharpEntity1.IsExternal false
        check "ckwfew0o42" fsharpEntity1.IsModule false
        check "ckwfew0o43" fsharpEntity1.IsValueType false
        check "ckwfew0o44" fsharpEntity1.LogicalName "List`1"
        check "ckwfew0o44" fsharpEntity1.CompiledName "FSharpList`1"
        check "ckwfew0o44" fsharpEntity1.IsAbbreviation false
        check "ckwfew0o44" (fsharpEntity1.DeclarationLocation.Document.Contains("prim-types.fs")) true
        check "ckwfew0o44" fsharpEntity1.HasAssemblyCodeRepresentation false
        check "ckwfew0o44" fsharpEntity1.UnionCases.Count 2
        check "ckwfew0o44" fsharpEntity1.RecordFields.Count 0
        check "ckwfew0o44" fsharpEntity1.XmlDocSig "T:Microsoft.FSharp.Collections.FSharpList`1"


    [<Test>]
    member this.Test10() = 
        
        let fsharpEntity1 = fsharpLibrary.GetEntity("Microsoft.FSharp.Core.Ref`1")

        check "ckwfew0o41" fsharpEntity1.IsExternal false
        check "ckwfew0o42" fsharpEntity1.IsModule false
        check "ckwfew0o43" fsharpEntity1.IsValueType false
        check "ckwfew0o44" fsharpEntity1.LogicalName "Ref`1"
        check "ckwfew0o44" fsharpEntity1.CompiledName "FSharpRef`1"
        check "ckwfew0o44" fsharpEntity1.IsAbbreviation false
        check "ckwfew0o44" (fsharpEntity1.DeclarationLocation.Document.Contains("prim-types.fs")) true
        check "ckwfew0o44" fsharpEntity1.HasAssemblyCodeRepresentation false
        check "ckwfew0o44" fsharpEntity1.UnionCases.Count 0
        check "ckwfew0o44" fsharpEntity1.RecordFields.Count 1
        check "ckwfew0o44" fsharpEntity1.XmlDocSig "T:Microsoft.FSharp.Core.FSharpRef`1"



    [<Test>]
    member this.TraversalTestLookingForBadDocumentation() = 

         for x in fsharpLibrary.Entities do 
             if x.IsModule then 
                 printfn "module: Name = %s, QualifiedName = %s" x.LogicalName x.QualifiedName
                 
         for x in fsharpLibrary.Entities do 
             if not x.IsModule && not x.IsAbbreviation && not x.HasAssemblyCodeRepresentation then 
                 printfn "type: Name = %s, QualifiedName = %s" x.LogicalName x.QualifiedName

         for x in fsharpLibrary.Entities do 
             if x.HasAssemblyCodeRepresentation then 
                 printfn "mapped to assembly code: Name = %s" x.LogicalName 

         for x in fsharpLibrary.Entities do 
             if x.IsAbbreviation then 
                 printfn "type abbreviation: Name = %s" x.LogicalName 

         for x in fsharpLibrary.Entities do 
            for v in x.NestedEntities do 
               printfn "nested entity : Name = %s.%s" x.LogicalName v.LogicalName 

         for x in fsharpLibrary.Entities do 
            printfn "entity: Name = %s" x.LogicalName 
            for v in x.MembersOrValues do 
               printfn "member : Name = %s.%s" x.LogicalName v.CompiledName
               for ps in v.CurriedParameterGroups do 
                   for p in ps do 
                       printfn "       : Parameter %s" p.Name 

         printfn "--------------------- ERRORS----------------------------------"

             //x.IsAbbreviation  || 
             //x.HasAssemblyCodeRepresentation || 
             //(x.ReflectionType.GetCustomAttributes(typeof<System.ObsoleteAttribute>,true).Length = 0 && x.ReflectionType.GetCustomAttributes(typeof<OCamlCompatibilityAttribute>,true).Length = 0) 

            //not (v.CompiledName.Contains "_") && 
            // (v.CompiledName = ".ctor" 
            // || x.IsAbbreviation  
            // || x.HasAssemblyCodeRepresentation 
            // || (let minfos = x.ReflectionType.GetMethods() |> Array.filter (fun m -> m.Name = v.CompiledName) 
            //     minfos |> Array.exists (fun minfo -> minfo.GetCustomAttributes(typeof<System.ObsoleteAttribute>,true).Length = 0 && minfo.GetCustomAttributes(typeof<OCamlCompatibilityAttribute>,true).Length = 0)))
         
         for x in fsharpLibrary.Entities do 
            let rec loop (x:FSharpEntity) = 
                if okEntity x then
                  for x in x.NestedEntities do 
                     loop x
                  for v in x.MembersOrValues do 
                   if okVal (x,v) then 
                     for ps in v.CurriedParameterGroups do 
                       for p in ps do 
                           match p.Name with 
                           | null -> printfn "member %s.%s : NULL PARAMETER NAME" x.LogicalName v.CompiledName
                           | _ -> ()
            loop x
                          

    [<Test>]
    member this.ExploreFSharpCore() = explore fsharpLibrary

    [<Test>]
    member this.ExploreFSharpPowerPack() = explore (FSharpAssembly.FromFile(@"FSharp.PowerPack.dll"))
    
    [<Test>]
    member this.ExploreFSharpPowerPackLinq() = explore (FSharpAssembly.FromFile(@"FSharp.PowerPack.Linq.dll"))
                    
    [<Test>]
    member this.LoadPowerPack() = FSharpAssembly.FromFile(@"FSharp.PowerPack.dll") |> ignore

    [<Test>]
    member this.LoadPowerPackLinq() =  FSharpAssembly.FromFile(@"FSharp.PowerPack.Linq.dll") |> ignore

    [<Test>]
    member this.TestMultipleAssemblies() =        
        let fscore = FSharpAssembly.FSharpLibrary
        let list = fscore.GetEntity("Microsoft.FSharp.Collections.List`1")
        let fscore1 = FSharpAssembly.FromAssembly(typedefof<list<_>>.Assembly)
        let list1 = fscore1.GetEntity("Microsoft.FSharp.Collections.List`1")
        Assert.IsTrue((list = list1))
        Assert.IsTrue((fscore = fscore1))

    [<Test>]
    member this.TestEqualityOnEntities() =        
        let fscore = FSharpAssembly.FSharpLibrary
        let list1 = fscore.GetEntity("Microsoft.FSharp.Collections.List`1")
        let unitE = fscore.GetEntity("Microsoft.FSharp.Core.unit")
        let list2 = fscore.GetEntity("Microsoft.FSharp.Collections.List`1")
        assertEquality list1 list2
        assertInequality list1 unitE


        // Union cases
        for (uc1,uc2) in Seq.zip list1.UnionCases list2.UnionCases do
            Assert.IsTrue((uc1 = uc2), sprintf "%A <> %A" uc1 uc2)
            for (f1,f2) in strictZip uc1.Fields uc2.Fields do
                assertEquality f1 f2
        assertInequality (list1.UnionCases |> Seq.toArray).[0] (list1.UnionCases |> Seq.toArray).[1]
        let consE = (list1.UnionCases |> Seq.toArray).[1]
        assertInequality (consE.Fields |> Seq.toArray).[0]  (consE.Fields |> Seq.toArray).[1]

        // Generic type parameters
        let event1 = fscore.GetEntity "Microsoft.FSharp.Control.Event`2"
        let event2 = fscore.GetEntity "Microsoft.FSharp.Control.Event`2"
        assertEquality event1 event2
        for (p1,p2) in strictZip event1.GenericParameters event2.GenericParameters do
            assertEquality p1 p2

        assertInequality (event1.GenericParameters |> Seq.toArray).[0]  (event1.GenericParameters |> Seq.toArray).[1]
        assertInequality (event1.GenericParameters |> Seq.toArray).[0]  (event2.GenericParameters |> Seq.toArray).[1]

        // parameters
        let listMap () = 
            fscore.GetEntity("Microsoft.FSharp.Collections.ListModule").MembersOrValues |> Seq.find (fun m -> m.DisplayName = "map")
        let map1 = listMap()
        let map2 = listMap()
        assertEquality map1 map2
        for cp1,cp2 in strictZip map1.CurriedParameterGroups map2.CurriedParameterGroups do
            for p1,p2 in strictZip cp1 cp2 do
                assertEquality p1 p2
        let p1 = map1.CurriedParameterGroups |> seqItem 0 |> seqItem 0
        let p2 = map1.CurriedParameterGroups |> seqItem 1 |> seqItem 0
        assertInequality p1 p2

        // attributes
        let map1Attrs = map1.Attributes
        let map2Attrs = map2.Attributes
        for a1, a2 in strictZip map1Attrs map2Attrs do
            assertEquality a1 a2

        let zip =
            fscore.GetEntity("Microsoft.FSharp.Collections.ListModule").MembersOrValues |> Seq.find (fun m -> m.DisplayName = "zip")
        assertInequality (zip.Attributes |> seqItem 0) (map1Attrs |> seqItem 0)


    [<Test>]
    member this.TestTypeEquality() =
        let fscoreEnt = FSharpAssembly.FSharpLibrary.GetEntity
        let listModule = fscoreEnt "Microsoft.FSharp.Collections.ListModule"
        Assert.IsNotNull listModule
        let mapFun = listModule.MembersOrValues |> Seq.find (fun m -> m.DisplayName = "map")
        let t = mapFun.Type
        assertEquality t t