// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation 2005-2011.
// This sample code is provided "as is" without warranty of any kind. 
// We disclaim all warranties, either express or implied, including the 
// warranties of merchantability and fitness for a particular purpose. 
// ----------------------------------------------------------------------------

namespace Microsoft.FSharp.Linq.Runtime

open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Linq.Runtime

[<AutoOpen>]
module TypeExtensions = 
  type System.Type with 
    /// Returns nicely formatted name of the type
    member t.NiceName =
      let sb = new System.Text.StringBuilder()
      let rec build (t:System.Type) =
        if t.IsGenericType then 
          // Remove the `1 part from generic names
          let tick = t.Name.IndexOf('`')
          let name = t.Name.Substring(0, tick) 
          Printf.bprintf sb "%s" t.Name
          Printf.bprintf sb "<"
          // Print generic type arguments recursively
          let args = t.GetGenericArguments()
          for i in 0 .. args.Length - 1 do 
            if i <> 0 then Printf.bprintf sb ", "
            build args.[i]
          Printf.bprintf sb ">"
        else
          // Print ordiary type name
          Printf.bprintf sb "%s" t.Name
      build t
      sb.ToString()

// ----------------------------------------------------------------------------

open TypeExtensions

/// Represents information about System.Type that has one or more
/// type parameters (and can be reconstructed when arguments are 
/// provided). The type can be pointer, byref, generic or array.
type ParameterizedType = 
  | Pointer
  | ByRef
  | Generic of System.Type
  | Array of int
  /// Provide arguments to the parameterized type
  member x.Rebuild(args:System.Type list) =
    match x, args with 
    | Pointer, [t] -> t.MakePointerType()
    | ByRef, [t] -> t.MakeByRefType()
    | Array n, [t] -> t.MakeArrayType(n)
    | Generic t, args -> t.MakeGenericType(args |> Array.ofSeq)
    | _ -> failwith "Cannot rebuild pointer, byref or array type using multiple type parameters!"

/// Wrapper for System.Type that implements the 'comparable'
/// constraint (to make it possible to use types as keys of Map)
type ComparableType(t:System.Type) = 
  member x.Type = t
  override x.Equals(y) = 
    x.Type.Equals((y :?> ComparableType).Type)
  override x.GetHashCode() = 
    x.Type.GetHashCode()
  interface System.IComparable with
    member x.CompareTo(o) =
      let y = (o :?> ComparableType)
      compare x.Type.AssemblyQualifiedName y.Type.AssemblyQualifiedName

[<AutoOpen>]
module TypePatterns =
  /// Decompose type into several options - A type can be
  /// primitive type, type parameter or parameterized type
  let (|Parameterized|Parameter|Primitive|) (typ : System.Type) =
    if typ.IsGenericType then 
      let generic = typ.GetGenericTypeDefinition()
      Parameterized(Generic(generic), typ.GetGenericArguments() |> List.ofSeq)
    elif typ.IsGenericParameter then Parameter(typ.GenericParameterPosition)
    elif not typ.HasElementType then Primitive(typ)
    elif typ.IsArray then Parameterized(Array(typ.GetArrayRank()), [typ.GetElementType()])
    elif typ.IsByRef then Parameterized(ByRef, [typ.GetElementType()])
    elif typ.IsPointer then Parameterized(Pointer, [typ.GetElementType()])
    else failwith "Cannot happen"

// ----------------------------------------------------------------------------

/// Represents a transformation of quotations that changes both 
/// expressions and types (e.g. to replace 'int' with 'string')
/// (Recursively processes the whole quotation or type)
type IQuotationTransformation =
  abstract TransformExpr : Expr -> Expr 
  abstract TransformType : Type -> Type

/// Represents a rule that modifies quotations in some way
/// Processing quotations and types recursively can be done using
/// IQuotationTransformation passed as argument
type IQuotationAdapter =
  abstract AdaptExpr : IQuotationTransformation * Expr -> Expr option
  abstract AdaptType : IQuotationTransformation * Type -> Type option

module Transformations = 

  /// Transform a specified type using quotation adapter
  /// (This recursively processes all type parameters of the type as well.)
  let rec transformType (adapt:IQuotationAdapter) trans typ = 
    match adapt.AdaptType(trans, typ) with 
    | Some replacement -> replacement
    | None ->
    match typ with
    | Parameter _ -> failwith "Parameter not expected in applyTypeTransformation"
    | Primitive _ -> typ
    | Parameterized(shape, args) ->
        let args' = args |> List.map (transformType adapt trans)
        if Seq.forall2 (=) args args' then typ
        else shape.Rebuild(args')

  /// Transform quotation using the specified quotation transformation.
  /// This replaces expressions according to the 'TransformExpr' and 
  /// replaces types according to the 'TransformType' method.
  let rec transformQuotation (adapt:IQuotationAdapter) ctx (quot:Expr) = 
    // Create transformation to be used for processing of nested 
    // quotations (if needed by 'AdaptExpr') using current context
    let rec trans = 
      { new IQuotationTransformation with
          member x.TransformExpr e = transformQuotation adapt ctx e
          member x.TransformType t = transformType adapt trans t }

    // Run the adapter to see if it changes the expression
    match adapt.AdaptExpr(trans, quot) with
    | Some(nquot:Expr) -> nquot
    | _ ->

    // Decompose quotation and recursively process sub-parts
    match quot with 
    | Patterns.Let(v, init, body) ->
        // We may need to change type of the variable if transformed
        // initialization expression has different type
        let init' = transformQuotation adapt ctx init
        let v', ctx =
          // We could transform type using 'transformType', but we don't
          // need to because quotation has type already 
          if init'.Type <> init.Type then
            let v' = new Var(v.Name, init'.Type, v.IsMutable)
            v', Map.add v v' ctx
          else v, ctx
        // Transform the body
        let body' = transformQuotation adapt ctx body
        Expr.Let(v', init', body')

    | Patterns.NewTuple(elems) ->
        // Recreate tuple - this infers new type of tuple
        Expr.NewTuple (elems |> List.map (transformQuotation adapt ctx))

    | Patterns.NewUnionCase(ucase, args) ->
        // Recreate union case constructor
        // (We may need to change the type of the union type)
        let args' = args |> List.map (transformQuotation adapt ctx)
        let nty = transformType adapt trans ucase.DeclaringType
        let ucase' = FSharpType.GetUnionCases(nty) |> Seq.find (fun c -> c.Tag = ucase.Tag)
        Expr.NewUnionCase(ucase', args')

    | Patterns.Call(inst, mi, args) ->
        // Recreate invocation
        // (We may need to change type arguments of generic method call)
        let args' = args |> List.map (transformQuotation adapt ctx)
        let inst' = inst |> Option.map (transformQuotation adapt ctx)
        let mi' = 
          if mi.IsGenericMethod then
            // Replace type arguments of generic method
            let gmi = mi.GetGenericMethodDefinition()
            let types = mi.GetGenericArguments()
                        |> Array.map (transformType adapt trans)
            gmi.MakeGenericMethod(types)
          else mi
        match inst' with
        | None -> Expr.Call(mi', args')
        | Some(inst') -> Expr.Call(inst', mi', args')

    | ExprShape.ShapeLambda(v, body) -> 
        // Recreate lambda 
        // (Use type transformation to calculate new type of variable)
        match transformType adapt trans v.Type with 
        | nty when nty = v.Type -> Expr.Lambda(v, transformQuotation adapt ctx body)
        | nty ->
            // If the type is different, create new variable
            let v' = new Var(v.Name, nty, v.IsMutable)
            Expr.Lambda(v', transformQuotation adapt (Map.add v v' ctx) body)

    | Patterns.Coerce(expr, targetType) ->
        // Recreate coercion - transform the target type
        let targetType' = transformType adapt trans targetType 
        Expr.Coerce(transformQuotation adapt ctx expr, targetType')        
          
    | ExprShape.ShapeCombination(shape, args) -> 
        // Recursively process all arguments & recreate
        let args = args |> List.map (transformQuotation adapt ctx)
        ExprShape.RebuildShapeCombination(shape, args)

    | ExprShape.ShapeVar(v) -> 
        match Map.tryFind v ctx with
        | Some(v) -> Expr.Var(v)
        | _ -> quot

// ----------------------------------------------------------------------------

module Adapters = 

  /// Declare symbol inside pattern. For example:
  ///  | Let 1 (i, <NestedPattern#1>) 
  ///  | Let 2 (i, <NestedPattern#2>) ->
  ///      // i will be either 1 or 2
  let (|Let|) v e = (v, e)


  /// Adapts records in quotations (Replaces uses of records with tuples
  /// which can be later removed using TupleAdapter)
  type RecordAdapter() =
    interface IQuotationAdapter with

      /// Turns all top-level uses of records into tuples
      member x.AdaptType(trans, t) =
        if FSharpType.IsRecord(t) then
          let types = [| for f in FSharpType.GetRecordFields(t) -> f.PropertyType |]
          Some(FSharpType.MakeTupleType(types))
        else None

      /// Turns all tupl-level uses of 'NewTuple' and 'TupleGet' into
      /// corresponding calls (constructor / property get) on mutable tuples
      member x.AdaptExpr(trans, q) = 
        match q with
        | Patterns.PropertyGet(Some(inst), pi, args) when FSharpType.IsRecord(pi.DeclaringType) ->
            let idx = FSharpType.GetRecordFields(pi.DeclaringType) |> Seq.findIndex ((=) pi)
            Some(Expr.TupleGet(trans.TransformExpr(inst), idx))
        | Patterns.NewRecord(typ, args) ->
            Some(Expr.NewTuple(args))
        | _ -> None


  /// Adapts tuples in quotations.
  /// (Replace uses of the tuple type with 'MutableTuple' type which
  /// has get/set properties & parameterless constructor and can be used
  /// in LINQ to Entities)
  type TupleAdapter() =

    // Get arrays of types & map of transformations
    let tupleTypes = 
      [| yield typedefof<System.Tuple<_>>, typedefof<MutableTuple<_>>
         yield typedefof<_ * _>, typedefof<MutableTuple<_, _>>
         yield typedefof<_ * _ * _>, typedefof<MutableTuple<_, _, _>>
         yield typedefof<_ * _ * _ * _>, typedefof<MutableTuple<_, _, _, _>>
         yield typedefof<_ * _ * _ * _ * _>, typedefof<MutableTuple<_, _, _, _, _>>
         yield typedefof<_ * _ * _ * _ * _ * _>, typedefof<MutableTuple<_, _, _, _, _, _>>
         yield typedefof<_ * _ * _ * _ * _ * _ * _>, typedefof<MutableTuple<_, _, _, _, _, _, _>>
         yield typedefof<_ * _ * _ * _ * _ * _ * _ * _>, typedefof<MutableTuple<_, _, _, _, _, _, _, _>> |]
    let mutableTuples = tupleTypes |> Array.map snd
    let map = tupleTypes |> dict

    interface IQuotationAdapter with

      /// Turns all top-level uses of tuple into mutable tuples
      member x.AdaptType(trans, t) =
        // Tuples are generic, so lookup only for generic types 
        if t.IsGenericType then
          let generic = t.GetGenericTypeDefinition()
          match map.TryGetValue(generic) with
          | true, tupleType ->
              // Recursively transform type arguments
              let args = t.GetGenericArguments() |> Array.map trans.TransformType
              Some(tupleType.MakeGenericType(args))
          | _ -> None
        else None

      /// Turns all tupl-level uses of 'NewTuple' and 'TupleGet' into
      /// corresponding calls (constructor / property get) on mutable tuples
      member x.AdaptExpr(trans, q) = 
        match q with
        | Patterns.NewTuple(args) ->
            let createTuple (args:Expr list) =
              // Will fit into a single tuple type
              let typ = mutableTuples.[args.Length - 1]
              let typ = typ.MakeGenericType [| for a in args -> a.Type |]
              let ctor = typ.GetConstructor [| |]
              let var = Var.Global("newTuple", typ)

              // Create quotation for 'new MutableTuple(Item1=<e1>, ...)`
              let assignments = 
                args |> List.mapi (fun i arg ->
                  Expr.PropertySet(Expr.Var(var), typ.GetProperty(sprintf "Item%d" (i+1)), arg) )
              let body = 
                assignments |> List.rev
                |> List.fold (fun st v -> Expr.Sequential(st, v)) (Expr.Value(()))
              let body = Expr.Sequential(body, Expr.Var(var))
              Expr.Let(var, Expr.NewObject(ctor, []), body)

            let rec create (args:Expr list) = 
              match args with 
              | x1::x2::x3::x4::x5::x6::x7::x8::[] ->
                  createTuple [ x1; x2; x3; x4; x5; x6; x7; createTuple [x8] ]
              | x1::x2::x3::x4::x5::x6::x7::x8::tail ->
                  // Too long to fit single tuple - nested tuple after first 7
                  createTuple [ x1; x2; x3; x4; x5; x6; x7; create (x8::tail) ]
              | args -> createTuple args

            // Recursively transform arguments & create mutable tuple
            Some(args |> List.map trans.TransformExpr |> create)

        | Patterns.TupleGet(e, i) ->
            // Recursively generate tuple get 
            // (may be nested e.g. TupleGet(<e>, 9) ~> <e>.Item8.Item3)
            let rec walk i (inst:Expr) (t:Type) = 
              // Transform type of the tuple we're accessing
              let newType = map.[t.GetGenericTypeDefinition()]
              let args = t.GetGenericArguments() |> Array.map trans.TransformType
              let newType = newType.MakeGenericType(args)

              // Get property (at most the last one)
              let prop = sprintf "Item%d" (1 + (min i 7))
              let propInfo = newType.GetProperty(prop)
              let res = Expr.PropertyGet(inst, propInfo)
              // Do we need to add another property get for the last property?
              if i < 7 then res 
              else walk (i - 7) res (t.GetGenericArguments().[7]) 
            
            Some(walk i (trans.TransformExpr e) e.Type)

        | Let 0 (i, DerivedPatterns.SpecificCall <@ fst @> (None, tys, [tuple])) 
        | Let 1 (i, DerivedPatterns.SpecificCall <@ snd @> (None, tys, [tuple])) ->
            // Transform calls to 'snd' and 'fst' functions 
            // (Just create 'TupleGet' quotation and transform it)
            let expr = Expr.TupleGet(tuple, i)
            (x :> IQuotationAdapter).AdaptExpr(trans, expr)

        | _ -> None

// ----------------------------------------------------------------------------

type Grouping<'K, 'T>(key:'K, values:seq<'T>) =
  interface System.Linq.IGrouping<'K, 'T> with
    member x.Key = key
  interface System.Collections.IEnumerable with
    member x.GetEnumerator() = values.GetEnumerator() :> System.Collections.IEnumerator
  interface System.Collections.Generic.IEnumerable<'T> with
    member x.GetEnumerator() = values.GetEnumerator()

module Execution =
  open Microsoft.FSharp.Linq.QuotationEvaluation

  let SeqMap = 
    match <@ Seq.map : (int -> int) -> int seq -> int seq @> with
    DerivedPatterns.Lambdas(_, Patterns.Call(_, mi, _)) -> mi.GetGenericMethodDefinition()
    | _ -> failwith "Cannot extract info for 'Seq.map'"

  let wrapQuery queryUntyped (q:Expr<'T>) : 'T = 
    let qorig = q :> Expr
    let qtrans = 
      qorig
        |> Transformations.transformQuotation (Adapters.RecordAdapter()) Map.empty
        |> Transformations.transformQuotation (Adapters.TupleAdapter()) Map.empty

    let rec generateTupleAccess (sourceType:System.Type) (expr:Expr) i =
      let prop = sourceType.GetProperty(sprintf "Item%d" (min i 8))
      let expr = Expr.PropertyGet(expr, prop)
      if i > 7 then 
        let sourceType = sourceType.GetGenericArguments().[7]
        generateTupleAccess sourceType expr (i - 7)
        else expr

    let rec createConversion (sourceType:System.Type) (targetType:System.Type) expr = 
      printfn " from '%s'\n to '%s'\n\n" sourceType.NiceName targetType.NiceName
      match sourceType, targetType with
      | Parameterized(Generic t1, [tyarg1]), Parameterized(Generic t2, [tyarg2]) 
            when t1 = typedefof<seq<_>> && t2 = typedefof<seq<_>> && tyarg1 <> tyarg2 ->
          // 
          let var = new Var("v", tyarg1)
          let convExpr : Expr = createConversion tyarg1 tyarg2 (Expr.Var(var))
          let meth = SeqMap.MakeGenericMethod [| tyarg1; tyarg2 |]
          Expr.Call(meth, [ Expr.Lambda(var, convExpr); expr ])

      | Parameterized(Generic t1, args1), Parameterized(Generic t2, args2) 
          when FSharpType.IsTuple(targetType) ->

          let var = new Var("t", sourceType)
          let args = 
            FSharpType.GetTupleElements(targetType) |> Seq.mapi (fun i targetT ->
                let acc = generateTupleAccess sourceType (Expr.Var(var)) (i + 1)
                createConversion acc.Type targetT acc)
          let body = Expr.NewTuple(List.ofSeq args)
          Expr.Let(var, expr, body)

      | Parameterized(Generic t1, [keyt1; valt1]), Parameterized(Generic t2, [keyt2; valt2])
          when t1 = typedefof<System.Linq.IGrouping<_, _>> && t2 = t1 ->
          if keyt1 <> keyt2 then failwith "Tuples or records are not supported as grouping keys!"
        
          let targetGrouping = typedefof<Grouping<_, _>>.MakeGenericType [| keyt2; valt2 |]
          let targetIGrouping = typedefof<System.Linq.IGrouping<_, _>>.MakeGenericType [| keyt2; valt2 |]

          let prop = sourceType.GetProperty("Key")
          let var = new Var("v", valt1)
          let convExpr : Expr = createConversion valt1 valt2 (Expr.Var(var))
          let meth = SeqMap.MakeGenericMethod [| valt1; valt2 |]
            
          let args = 
            [ Expr.PropertyGet(expr, prop)
              Expr.Call(meth, [ Expr.Lambda(var, convExpr); expr ]) ]

          Expr.Coerce(Expr.NewObject(targetGrouping.GetConstructors() |> Seq.head, args), targetIGrouping)
      
      | Parameterized(Generic t1, args1), Primitive t2 
          when FSharpType.IsRecord(targetType) ->

          let var = new Var("r", sourceType)
          let args = 
            FSharpType.GetRecordFields(targetType) |> Seq.mapi (fun i prop ->
                let acc = generateTupleAccess sourceType (Expr.Var(var)) (i + 1)
                createConversion acc.Type prop.PropertyType acc)
          let body = Expr.NewRecord(targetType, List.ofSeq args)
          Expr.Let(var, expr, body)

      | _ -> 
          expr

    let result : obj = queryUntyped qtrans
    if qtrans.Type <> qorig.Type then
      let arg = Expr.Value(result, qtrans.Type)
      let converted = createConversion qtrans.Type qorig.Type arg
      unbox (converted.EvalUntyped())
    else
      printfn "ok"
      unbox result