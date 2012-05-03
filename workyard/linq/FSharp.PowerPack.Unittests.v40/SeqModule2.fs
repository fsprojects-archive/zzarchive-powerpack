
namespace FSharp.Core.Unittests.FSharp_Core.Microsoft_FSharp_Collections

open System
open NUnit.Framework
open Microsoft.FSharp.Collections
open FSharp.Core.Unittests.LibraryTestFx
open System.Linq

[<TestFixture>]
type SeqModule2() =

    [<Test>]
    member this.Hd() =
             
        let IntSeq =
            seq { for i in 0 .. 9 do
                    yield i }
                    
        if PSeq.head IntSeq <> 0 then Assert.Fail()
                 
        // string Seq
        let strSeq = seq ["first"; "second";  "third"]
        if PSeq.head strSeq <> "first" then Assert.Fail()
         
        // Empty Seq
        let emptySeq = PSeq.empty
        CheckThrowsInvalidOperationExn ( fun() -> PSeq.head emptySeq)
      
        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () ->PSeq.head nullSeq) 
        () 
        
        
    [<Test>]
    member this.Init() =

        let funcInt x = x
        let init_finiteInt = PSeq.init 9 funcInt
        let expectedIntSeq = seq [ 0..8]
      
        VerifyPSeqsEqual expectedIntSeq  init_finiteInt
        
             
        // string Seq
        let funcStr x = x.ToString()
        let init_finiteStr = PSeq.init 5  funcStr
        let expectedStrSeq = seq ["0";"1";"2";"3";"4"]

        VerifyPSeqsEqual expectedStrSeq init_finiteStr
        
        // null Seq
        let funcNull x = null
        let init_finiteNull = PSeq.init 3 funcNull
        let expectedNullSeq = seq [ null;null;null]
        
        VerifyPSeqsEqual expectedNullSeq init_finiteNull
        () 
        
//    [<Test>]
//    member this.InitInfinite() =
//
//        let funcInt x = x
//        let init_infiniteInt = PSeq.initInfinite funcInt
//        let resultint = PSeq.find (fun x -> x =100) init_infiniteInt
//        
//        Assert.AreEqual(100,resultint)
//        
//             
//        // string Seq
//        let funcStr x = x.ToString()
//        let init_infiniteStr = PSeq.initInfinite  funcStr
//        let resultstr = PSeq.find (fun x -> x = "100") init_infiniteStr
//        
//        Assert.AreEqual("100",resultstr)
//       
       
    [<Test>]
    member this.IsEmpty() =
        
        //seq int
        let seqint = seq [1;2;3]
        let is_emptyInt = PSeq.isEmpty seqint
        
        Assert.IsFalse(is_emptyInt)
              
        //seq str
        let seqStr = seq["first";"second"]
        let is_emptyStr = PSeq.isEmpty  seqStr

        Assert.IsFalse(is_emptyInt)
        
        //seq empty
        let seqEmpty = PSeq.empty
        let is_emptyEmpty = PSeq.isEmpty  seqEmpty
        Assert.IsTrue(is_emptyEmpty) 
        
        //seq null
        let seqnull:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> PSeq.isEmpty seqnull |> ignore)
        ()
        
    [<Test>]
    member this.Iter() =
//        //seq int
//        let seqint =  seq [ 1..3]
//        let cacheint = ref 0
//       
//        let funcint x = cacheint := !cacheint + x
//        PSeq.iter funcint seqint
//        Assert.AreEqual(6,!cacheint)
//              
//        //seq str
//        let seqStr = seq ["first";"second"]
//        let cachestr =ref ""
//        let funcstr x = cachestr := !cachestr+x
//        PSeq.iter funcstr seqStr
//         
//        Assert.AreEqual("firstsecond",!cachestr, sprintf "Not equal! firstsecond <> %A" !cachestr)
        
         // empty array    
        let emptyseq = PSeq.empty
        let resultEpt = ref 0
        PSeq.iter (fun x -> Assert.Fail()) emptyseq   

        // null seqay
        let nullseq:seq<'a> =  null
        
        CheckThrowsArgumentNullException (fun () -> PSeq.iter (fun x -> ()) nullseq |> ignore)  
        ()
        
    [<Test>]
    member this.Iter2() =
    
//        //seq int
//        let seqint =  seq [ 1..3]
//        let cacheint = ref 0
//       
//        let funcint x y = cacheint := !cacheint + x+y
//        PSeq.iter2 funcint seqint seqint
//        Assert.AreEqual(12,!cacheint)
//              
//        //seq str
//        let seqStr = seq ["first";"second"]
//        let cachestr =ref ""
//        let funcstr x y = cachestr := !cachestr+x+y
//        PSeq.iter2 funcstr seqStr seqStr
//         
//        Assert.AreEqual("firstfirstsecondsecond",!cachestr)
//        
         // empty array    
        let emptyseq = PSeq.empty
        let resultEpt = ref 0
        PSeq.iter2 (fun x y-> Assert.Fail()) emptyseq  emptyseq 

        // null seqay
        let nullseq:seq<'a> =  null
        CheckThrowsArgumentNullException (fun () -> PSeq.iter2 (fun x y -> ()) nullseq nullseq |> ignore)  
        
        ()
        
    [<Test>]
    member this.Iteri() =
    
//        // seq int
//        let seqint =  seq [ 1..10]
//        let cacheint = ref 0
//       
//        let funcint x y = cacheint := !cacheint + x+y
//        PSeq.iteri funcint seqint
//        Assert.AreEqual(100,!cacheint)
//              
//        // seq str
//        let seqStr = seq ["first";"second"]
//        let cachestr =ref 0
//        let funcstr (x:int) (y:string) = cachestr := !cachestr+ x + y.Length
//        PSeq.iteri funcstr seqStr
//         
//        Assert.AreEqual(12,!cachestr)
//        
//         // empty array    
//        let emptyseq = PSeq.empty
//        let resultEpt = ref 0
//        PSeq.iteri funcint emptyseq
//        Assert.AreEqual(0,!resultEpt)

        // null seqay
        let nullseq:seq<'a> =  null
        CheckThrowsArgumentNullException (fun () -> PSeq.iteri (fun x i -> ()) nullseq |> ignore)  
        ()
        
    [<Test>]
    member this.Length() =

         // integer seq  
        let resultInt = PSeq.length {1..8}
        if resultInt <> 8 then Assert.Fail()
        
        // string Seq    
        let resultStr = PSeq.length (seq ["Lists"; "are";  "commonly" ; "list" ])
        if resultStr <> 4 then Assert.Fail()
        
        // empty Seq     
        let resultEpt = PSeq.length PSeq.empty
        if resultEpt <> 0 then Assert.Fail()

        // null Seq
        let nullSeq:seq<'a> = null     
        CheckThrowsArgumentNullException (fun () -> PSeq.length  nullSeq |> ignore)  
        
        ()
        
    [<Test>]
    member this.Map() =

         // integer Seq
        let funcInt x = 
                match x with
                | _ when x % 2 = 0 -> 10*x            
                | _ -> x
       
        let resultInt = PSeq.map funcInt { 1..10 }
        let expectedint = seq [1;20;3;40;5;60;7;80;9;100]
        
        VerifyPSeqsEqual expectedint resultInt
        
        // string Seq
        let funcStr (x:string) = x.ToLower()
        let resultStr = PSeq.map funcStr (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        let expectedSeq = seq ["lists"; "are";  "commonly" ; "list"]
        
        VerifyPSeqsEqual expectedSeq resultStr
        
        // empty Seq
        let resultEpt = PSeq.map funcInt PSeq.empty
        VerifyPSeqsEqual PSeq.empty resultEpt

        // null Seq
        let nullSeq:seq<'a> = null 
        CheckThrowsArgumentNullException (fun () -> PSeq.map funcStr nullSeq |> ignore)
        
        ()
        
    [<Test>]
    member this.Map2() =
         // integer Seq
        let funcInt x y = x+y
        let resultInt = PSeq.map2 funcInt { 1..10 } {2..2..20} 
        let expectedint = seq [3;6;9;12;15;18;21;24;27;30]
        
        VerifyPSeqsEqual expectedint resultInt
        
        // string Seq
        let funcStr (x:int) (y:string) = x+y.Length
        let resultStr = PSeq.map2 funcStr (seq[3;6;9;11]) (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        let expectedSeq = seq [8;9;17;15]
        
        VerifyPSeqsEqual expectedSeq resultStr
        
        // empty Seq
        let resultEpt = PSeq.map2 funcInt PSeq.empty PSeq.empty
        VerifyPSeqsEqual PSeq.empty resultEpt

        // null Seq
        let nullSeq:seq<'a> = null 
        let validSeq = seq [1]
        CheckThrowsArgumentNullException (fun () -> PSeq.map2 funcInt nullSeq validSeq |> ignore)
        
        ()
        
        
    member private this.MapWithSideEffectsTester (map : (int -> int) -> seq<int> -> pseq<int>) expectExceptions =
        let i = ref 0
        let f x = i := !i + 1; x*x
        let e = ([1;2] |> map f).GetEnumerator()
        
        if expectExceptions then
            CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
            Assert.AreEqual(0, !i)
        if not (e.MoveNext()) then Assert.Fail()
        Assert.AreEqual(1, !i)
        let _ = e.Current
        Assert.AreEqual(1, !i)
        let _ = e.Current
        Assert.AreEqual(1, !i)
        
        if not (e.MoveNext()) then Assert.Fail()
        Assert.AreEqual(2, !i)
        let _ = e.Current
        Assert.AreEqual(2, !i)
        let _ = e.Current
        Assert.AreEqual(2, !i)

        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(2, !i)
        if expectExceptions then
            CheckThrowsInvalidOperationExn (fun _ -> e.Current |> ignore)
            Assert.AreEqual(2, !i)

        
        i := 0
        let e = ([] |> map f).GetEnumerator()
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(0,!i)
        if e.MoveNext() then Assert.Fail()
        Assert.AreEqual(0,!i)
        
        
    member private this.MapWithExceptionTester (map : (int -> int) -> seq<int> -> pseq<int>) =
        let raiser x = if x > 0 then raise(NotSupportedException()) else x
        let raises = (map raiser [0; 1])
        CheckThrowsAggregateException(fun _ -> PSeq.toArray raises |> ignore)
       

//    [<Test>]
//    member this.MapWithSideEffects () =
//        this.MapWithSideEffectsTester PSeq.map true
        
    [<Test>]
    member this.MapWithException () =
        this.MapWithExceptionTester PSeq.map

        
//    [<Test>]
//    member this.SingletonCollectWithSideEffects () =
//        this.MapWithSideEffectsTester (fun f-> PSeq.collect (f >> PSeq.singleton)) true
        
    [<Test>]
    member this.SingletonCollectWithException () =
        this.MapWithExceptionTester (fun f-> PSeq.collect (f >> PSeq.singleton))

     
//    [<Test>]
//    member this.SystemLinqSelectWithSideEffects () =
//        this.MapWithSideEffectsTester (fun f s -> System.Linq.ParallelEnumerable.Select(s.AsParallel(), Func<_,_>(f))) false
//        
    [<Test>]
    member this.SystemLinqSelectWithException () =
        this.MapWithExceptionTester (fun f s -> System.Linq.ParallelEnumerable.Select(s.AsParallel(), Func<_,_>(f)))

        
//    [<Test>]
//    member this.MapiWithSideEffects () =
//        let i = ref 0
//        let f _ x = i := !i + 1; x*x
//        let e = ([1;2] |> PSeq.mapi f).GetEnumerator()
//        
//        CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
//        Assert.AreEqual(0, !i)
//        if not (e.MoveNext()) then Assert.Fail()
//        Assert.AreEqual(1, !i)
//        let _ = e.Current
//        Assert.AreEqual(1, !i)
//        let _ = e.Current
//        Assert.AreEqual(1, !i)
//        
//        if not (e.MoveNext()) then Assert.Fail()
//        Assert.AreEqual(2, !i)
//        let _ = e.Current
//        Assert.AreEqual(2, !i)
//        let _ = e.Current
//        Assert.AreEqual(2, !i)
//        
//        if e.MoveNext() then Assert.Fail()
//        Assert.AreEqual(2, !i)
//        CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
//        Assert.AreEqual(2, !i)
//        
//        i := 0
//        let e = ([] |> PSeq.mapi f).GetEnumerator()
//        if e.MoveNext() then Assert.Fail()
//        Assert.AreEqual(0,!i)
//        if e.MoveNext() then Assert.Fail()
//        Assert.AreEqual(0,!i)
        
//    [<Test>]
//    member this.Map2WithSideEffects () =
//        let i = ref 0
//        let f x y = i := !i + 1; x*x
//        let e = (PSeq.map2 f [1;2] [1;2]).GetEnumerator()
//        
//        CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
//        Assert.AreEqual(0, !i)
//        if not (e.MoveNext()) then Assert.Fail()
//        Assert.AreEqual(1, !i)
//        let _ = e.Current
//        Assert.AreEqual(1, !i)
//        let _ = e.Current
//        Assert.AreEqual(1, !i)
//        
//        if not (e.MoveNext()) then Assert.Fail()
//        Assert.AreEqual(2, !i)
//        let _ = e.Current
//        Assert.AreEqual(2, !i)
//        let _ = e.Current
//        Assert.AreEqual(2, !i)
//
//        if e.MoveNext() then Assert.Fail()
//        Assert.AreEqual(2,!i)
//        CheckThrowsInvalidOperationExn  (fun _ -> e.Current|>ignore)
//        Assert.AreEqual(2, !i)
//        
//        i := 0
//        let e = (PSeq.map2 f [] []).GetEnumerator()
//        if e.MoveNext() then Assert.Fail()
//        Assert.AreEqual(0,!i)
//        if e.MoveNext() then Assert.Fail()
//        Assert.AreEqual(0,!i)
        
    [<Test>]
    member this.Collect() =
         // integer Seq
        let funcInt x = seq [x+1]
        let resultInt = PSeq.collect funcInt { 1..10 } 
       
        let expectedint = seq {2..11}
        
        VerifyPSeqsEqual expectedint resultInt
        
        // string Seq
        let funcStr (y:string) = y+"ist"
       
        let resultStr = PSeq.collect funcStr (seq ["L"])
        
        
        let expectedSeq = seq ['L';'i';'s';'t']
        
        VerifyPSeqsEqual expectedSeq resultStr
        
        // empty Seq
        let resultEpt = PSeq.collect funcInt PSeq.empty
        VerifyPSeqsEqual PSeq.empty resultEpt

        // null Seq
        let nullSeq:seq<'a> = null 
       
        CheckThrowsArgumentNullException (fun () -> PSeq.collect funcInt nullSeq |> ignore)
        
        ()
        
    [<Test>]
    member this.Mapi() =

         // integer Seq
        let funcInt x y = x+y
        let resultInt = PSeq.mapi funcInt { 10..2..20 } 
        let expectedint = seq [10;13;16;19;22;25]
        
        VerifyPSeqsEqual expectedint resultInt
        
        // string Seq
        let funcStr (x:int) (y:string) =x+y.Length
       
        let resultStr = PSeq.mapi funcStr (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        let expectedStr = seq [5;4;10;7]
         
        VerifyPSeqsEqual expectedStr resultStr
        
        // empty Seq
        let resultEpt = PSeq.mapi funcInt PSeq.empty
        VerifyPSeqsEqual PSeq.empty resultEpt

        // null Seq
        let nullSeq:seq<'a> = null 
       
        CheckThrowsArgumentNullException (fun () -> PSeq.mapi funcInt nullSeq |> ignore)
        
        ()
        
    [<Test>]
    member this.Max() =
        // integer Seq
        let resultInt = PSeq.max { 10..20 } 
        Assert.AreEqual(20,resultInt)


        // integer64 Seq
        let resultInt64 = PSeq.max { 10L..20L } 
        Assert.AreEqual(20L,resultInt64)


        // float Seq
        let resultFloat = PSeq.max { 10.0..20.0 } 
        Assert.AreEqual(20.0,resultFloat)

        // float32 Seq
        let resultFloat32 = PSeq.max { 10.0f..20.0f } 
        Assert.AreEqual(20.0f,resultFloat32)

        // decimal Seq
        let resultDecimal = PSeq.max { (decimal 10)..(decimal 20) } 
        Assert.AreEqual((decimal 20),resultDecimal)

        // string Seq
       
        let resultStr = PSeq.max (seq ["Lists"; "Are";  "MaxString" ; "List" ])
        Assert.AreEqual("MaxString",resultStr)
          
        // empty Seq
        CheckThrowsInvalidOperationExn (fun () -> PSeq.max ( PSeq.empty : pseq<float>) |> ignore)
        
        // null Seq
        let nullSeq:seq<float> = null 
        CheckThrowsArgumentNullException (fun () -> PSeq.max nullSeq |> ignore)
        
        ()
        
    [<Test>]
    member this.MaxBy() =
    
        // integer Seq
        let funcInt x = - (x % 18)
        let resultInt = PSeq.maxBy funcInt { 2..2..20 } 
        Assert.AreEqual(18,resultInt)
        
        // string Seq
        let funcStr (x:string)  = x.Length 
        let resultStr = PSeq.maxBy funcStr (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        Assert.AreEqual("Commonly",resultStr)
         
        // empty Seq
        CheckThrowsInvalidOperationExn (fun () -> PSeq.maxBy funcInt (PSeq.empty : pseq<int>) |> ignore)
        
        // null Seq
        let nullSeq:seq<int> = null 
        CheckThrowsArgumentNullException (fun () ->PSeq.maxBy funcInt nullSeq |> ignore)
        

        ()
        
    [<Test>]
    member this.MinBy() =
    
        // integer Seq
        let funcInt x = decimal(x % 18)
        let resultInt = PSeq.minBy funcInt { 2..2..20 } 
        Assert.AreEqual(18,resultInt)
        
        // string Seq
        let funcStr (x:string)  = x.Length 
        let resultStr = PSeq.minBy funcStr (seq ["Lists"; "Are";  "Commonly" ; "List" ])
        Assert.AreEqual("Are",resultStr)
          
        // empty Seq
        CheckThrowsInvalidOperationExn (fun () -> PSeq.minBy funcInt (PSeq.empty : pseq<int>) |> ignore) 
        
        // null Seq
        let nullSeq:seq<int> = null 
        CheckThrowsArgumentNullException (fun () ->PSeq.minBy funcInt nullSeq |> ignore)
        
        ()
        
          
    [<Test>]
    member this.Min() =

        // integer Seq
        let resultInt = PSeq.min { 10..20 } 
        Assert.AreEqual(10,resultInt)


        // integer64 Seq
        let resultInt64 = PSeq.min { 10L..20L } 
        Assert.AreEqual(10L,resultInt64)


        // float Seq
        let resultFloat = PSeq.min { 10.0..20.0 } 
        Assert.AreEqual(10.0,resultFloat)

        // float32 Seq
        let resultFloat32 = PSeq.min { 10.0f..20.0f } 
        Assert.AreEqual(10.0f,resultFloat32)

        // decimal Seq
        let resultDecimal = PSeq.min { (decimal 10)..(decimal 20) } 
        Assert.AreEqual((decimal 10),resultDecimal)

        
//        // string Seq
//        let resultStr = PSeq.min (seq ["Lists"; "Are";  "minString" ; "List" ])
//        Assert.AreEqual("Are",resultStr)
          
        // empty Seq
        CheckThrowsInvalidOperationExn (fun () -> PSeq.min (PSeq.empty : pseq<int>) |> ignore) 
        
        // null Seq
        let nullSeq:seq<float> = null 
        CheckThrowsArgumentNullException (fun () -> PSeq.min nullSeq |> ignore)
        
        ()
        
    [<Test>]
    member this.Nth() =
         
        // Negative index
        for i = -1 downto -10 do
           CheckThrowsArgumentException (fun () -> PSeq.nth i { 10 .. 20 } |> ignore)
            
        // Out of range
        for i = 11 to 20 do
           CheckThrowsArgumentException (fun () -> PSeq.nth i { 10 .. 20 } |> ignore)
         
         // integer Seq
        let resultInt = PSeq.nth 3 { 10..20 } 
        Assert.AreEqual(13, resultInt)
        
        // string Seq
        let resultStr = PSeq.nth 3 (seq ["Lists"; "Are";  "nthString" ; "List" ])
        Assert.AreEqual("List",resultStr)
          
        // empty Seq
        CheckThrowsArgumentException(fun () -> PSeq.nth 0 (PSeq.empty : pseq<decimal>) |> ignore)
       
        // null Seq
        let nullSeq:seq<'a> = null 
        CheckThrowsArgumentNullException (fun () ->PSeq.nth 3 nullSeq |> ignore)
        
        ()
         
    [<Test>]
    member this.Of_Array() =
         // integer Seq
        let resultInt = PSeq.ofArray [|1..10|]
        let expectedInt = {1..10}
         
        VerifyPSeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr = PSeq.ofArray [|"Lists"; "Are";  "ofArrayString" ; "List" |]
        let expectedStr = seq ["Lists"; "Are";  "ofArrayString" ; "List" ]
        VerifyPSeqsEqual expectedStr resultStr
          
        // empty Seq 
        let resultEpt = PSeq.ofArray [| |] 
        VerifyPSeqsEqual resultEpt PSeq.empty
       
        ()
        
    [<Test>]
    member this.Of_List() =
         // integer Seq
        let resultInt = PSeq.ofList [1..10]
        let expectedInt = {1..10}
         
        VerifyPSeqsEqual expectedInt resultInt
        
        // string Seq
       
        let resultStr =PSeq.ofList ["Lists"; "Are";  "ofListString" ; "List" ]
        let expectedStr = seq ["Lists"; "Are";  "ofListString" ; "List" ]
        VerifyPSeqsEqual expectedStr resultStr
          
        // empty Seq 
        let resultEpt = PSeq.ofList [] 
        VerifyPSeqsEqual resultEpt PSeq.empty
        ()
        
          
//    [<Test>]
//    member this.Pairwise() =
//         // integer Seq
//        let resultInt = PSeq.pairwise {1..3}
//       
//        let expectedInt = seq [1,2;2,3]
//         
//        VerifyPSeqsEqual expectedInt resultInt
//        
//        // string Seq
//        let resultStr =PSeq.pairwise ["str1"; "str2";"str3" ]
//        let expectedStr = seq ["str1","str2";"str2","str3"]
//        VerifyPSeqsEqual expectedStr resultStr
//          
//        // empty Seq 
//        let resultEpt = PSeq.pairwise [] 
//        VerifyPSeqsEqual resultEpt PSeq.empty
//       
//        ()
        
    [<Test>]
    member this.Reduce() =
         
        // integer Seq
        let resultInt = PSeq.reduce (fun x y -> x + y) (seq [5;4;3;2;1])
        Assert.AreEqual(15,resultInt)
        
//        // string Seq
//        let resultStr = PSeq.reduce (fun (x:string) (y:string) -> x.Remove(0,y.Length)) (seq ["ABCDE";"A"; "B";  "C" ; "D" ])
//        Assert.AreEqual("E",resultStr) 
       
        // empty Seq 
        CheckThrowsInvalidOperationExn(fun () -> PSeq.reduce (fun x y -> x/y)  PSeq.empty |> ignore)
        
        // null Seq
        let nullSeq : seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> PSeq.reduce (fun (x:string) (y:string) -> x.Remove(0,y.Length))  nullSeq  |> ignore)   
        ()

         
//    [<Test>]
//    member this.Scan() =
//        // integer Seq
//        let funcInt x y = x+y
//        let resultInt = PSeq.scan funcInt 9 {1..10}
//        let expectedInt = seq [9;10;12;15;19;24;30;37;45;54;64]
//        VerifyPSeqsEqual expectedInt resultInt
//        
//        // string Seq
//        let funcStr x y = x+y
//        let resultStr =PSeq.scan funcStr "x" ["str1"; "str2";"str3" ]
//        
//        let expectedStr = seq ["x";"xstr1"; "xstr1str2";"xstr1str2str3"]
//        VerifyPSeqsEqual expectedStr resultStr
//          
//        // empty Seq 
//        let resultEpt = PSeq.scan funcInt 5 PSeq.empty 
//       
//        VerifyPSeqsEqual resultEpt (seq [ 5])
//       
//        // null Seq
//        let seqNull:seq<'a> = null
//        CheckThrowsArgumentNullException(fun() -> PSeq.scan funcInt 5 seqNull |> ignore)
//        ()
        
    [<Test>]
    member this.Singleton() =
        // integer Seq
        let resultInt = PSeq.singleton 1
       
        let expectedInt = seq [1]
        VerifyPSeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr =PSeq.singleton "str1"
        let expectedStr = seq ["str1"]
        VerifyPSeqsEqual expectedStr resultStr
         
        // null Seq
        let resultNull = PSeq.singleton null
        let expectedNull = seq [null]
        VerifyPSeqsEqual expectedNull resultNull
        ()
    
        
    [<Test>]
    member this.Skip() =
    
        // integer Seq
        let resultInt = PSeq.skip 2 (seq [1;2;3;4])
        let expectedInt = seq [3;4]
        VerifyPSeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr =PSeq.skip 2 (seq ["str1";"str2";"str3";"str4"])
        let expectedStr = seq ["str3";"str4"]
        VerifyPSeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = PSeq.skip 0 PSeq.empty 
        VerifyPSeqsEqual resultEpt PSeq.empty
        
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.skip 1 null |> ignore)
        ()
       
    [<Test>]
    member this.Skip_While() =
    
        // integer Seq
        let funcInt x = (x < 3)
        let resultInt = PSeq.skipWhile funcInt (seq [1;2;3;4;5;6])
        let expectedInt = seq [3;4;5;6]
        VerifyPSeqsEqual expectedInt resultInt
        
        // string Seq
        let funcStr (x:string) = x.Contains(".")
        let resultStr =PSeq.skipWhile funcStr (seq [".";"asdfasdf.asdfasdf";"";"";"";"";"";"";"";"";""])
        let expectedStr = seq ["";"";"";"";"";"";"";"";""]
        VerifyPSeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = PSeq.skipWhile funcInt PSeq.empty 
        VerifyPSeqsEqual resultEpt PSeq.empty
        
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.skipWhile funcInt null |> ignore)
        ()
       
    [<Test>]
    member this.Sort() =

        // integer Seq
        let resultInt = PSeq.sort (seq [1;3;2;4;6;5;7])
        let expectedInt = {1..7}
        VerifyPSeqsEqual expectedInt resultInt
        
        // string Seq
       
        let resultStr =PSeq.sort (seq ["str1";"str3";"str2";"str4"])
        let expectedStr = seq ["str1";"str2";"str3";"str4"]
        VerifyPSeqsEqual expectedStr resultStr

        // array Seq
       
        let resultArray =PSeq.sort (seq [[|1;2|]; [|5|]; [|3;4|]; [|4|]])
        let expectedArray = seq [[|4|]; [|5|]; [|1; 2|]; [|3; 4|]]
        VerifyPSeqsEqual expectedArray resultArray
        
        // empty Seq 
        let resultEpt = PSeq.sort PSeq.empty 
        VerifyPSeqsEqual resultEpt PSeq.empty
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.sort null  |> ignore)
        ()
        
    [<Test>]
    member this.SortBy() =

        // integer Seq
        let funcInt x = Math.Abs(x-5)
        let resultInt = PSeq.sortBy funcInt (seq [1;2;4;5;7])
        let expectedInt = seq [5;4;7;2;1]
        VerifyPSeqsEqual expectedInt resultInt
        
        // string Seq
        let funcStr (x:string) = x.IndexOf("key")
        let resultStr =PSeq.sortBy funcStr (seq ["st(key)r";"str(key)";"s(key)tr";"(key)str"])
        
        let expectedStr = seq ["(key)str";"s(key)tr";"st(key)r";"str(key)"]
        VerifyPSeqsEqual expectedStr resultStr
        
        // array Seq
        let resultArray =PSeq.sortBy (Array.toList) (seq [[|1;2|]; [|5|]; [|3;4|]; [|4|]])
        let expectedArray = seq [[|4|]; [|5|]; [|1; 2|]; [|3; 4|]]
        VerifyPSeqsEqual expectedArray resultArray

        // empty Seq 
        let resultEpt = PSeq.sortBy funcInt PSeq.empty 
        VerifyPSeqsEqual resultEpt PSeq.empty
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.sortBy funcInt null  |> ignore)
        ()
        
    [<Test>]
    member this.Sum() =
    
        // integer Seq
        let resultInt = PSeq.sum (seq [1..10])
        Assert.AreEqual(55,resultInt)

        // int64 Seq
        let resultInt64 = PSeq.sum (seq [1L..10L])
        Assert.AreEqual(55L,resultInt64)
        
        // float32 Seq
        let floatSeq = (seq [ 1.2f;3.5f;6.7f ])
        let resultFloat = PSeq.sum floatSeq
        if resultFloat <> 11.4f then Assert.Fail()
        
        // double Seq
        let doubleSeq = (seq [ 1.0;8.0 ])
        let resultDouble = PSeq.sum doubleSeq
        if resultDouble <> 9.0 then Assert.Fail()
        
        // decimal Seq
        let decimalSeq = (seq [ 0M;19M;19.03M ])
        let resultDecimal = PSeq.sum decimalSeq
        if resultDecimal <> 38.03M then Assert.Fail()      
          
      
        // empty float32 Seq
        let emptyFloatSeq = PSeq.empty<System.Single> 
        let resultEptFloat = PSeq.sum emptyFloatSeq 
        if resultEptFloat <> 0.0f then Assert.Fail()
        
        // empty double Seq
        let emptyDoubleSeq = PSeq.empty<System.Double> 
        let resultDouEmp = PSeq.sum emptyDoubleSeq 
        if resultDouEmp <> 0.0 then Assert.Fail()
        
        // empty decimal Seq
        let emptyDecimalSeq = PSeq.empty<System.Decimal> 
        let resultDecEmp = PSeq.sum emptyDecimalSeq 
        if resultDecEmp <> 0M then Assert.Fail()
       
        ()
        
    [<Test>]
    member this.SumBy() =

        // integer Seq
        let resultInt = PSeq.sumBy (fun x -> x + 1) (seq [1..10])
        Assert.AreEqual(65,resultInt)
        
        // int64 Seq
        let resultInt64 = PSeq.sumBy int (seq [1L..10L])
        Assert.AreEqual(55,resultInt64)

        // float32 Seq
        let floatSeq = (seq [ 1.2f;3.5f;6.7f ])
        let resultFloat = PSeq.sumBy float32 floatSeq
        if resultFloat <> 11.4f then Assert.Fail()
        
        // double Seq
        let doubleSeq = (seq [ 1.0;8.0 ])
        let resultDouble = PSeq.sumBy double doubleSeq
        if resultDouble <> 9.0 then Assert.Fail()
        
        // decimal Seq
        let decimalSeq = (seq [ 0M;19M;19.03M ])
        let resultDecimal = PSeq.sumBy decimal decimalSeq
        if resultDecimal <> 38.03M then Assert.Fail()      

        // empty float32 Seq
        let emptyFloatSeq = PSeq.empty<System.Single> 
        let resultEptFloat = PSeq.sumBy float32 emptyFloatSeq 
        if resultEptFloat <> 0.0f then Assert.Fail()
        
        // empty double Seq
        let emptyDoubleSeq = PSeq.empty<System.Double> 
        let resultDouEmp = PSeq.sumBy double emptyDoubleSeq 
        if resultDouEmp <> 0.0 then Assert.Fail()
        
        // empty decimal Seq
        let emptyDecimalSeq = PSeq.empty<System.Decimal> 
        let resultDecEmp = PSeq.sumBy decimal emptyDecimalSeq 
        if resultDecEmp <> 0M then Assert.Fail()
       
        ()
        
//    [<Test>]
//    member this.Take() =
//        // integer Seq
//        
//        let resultInt = PSeq.take 3 (seq [1;2;4;5;7])
//       
//        let expectedInt = seq [1;2;4]
//        VerifyPSeqsEqual expectedInt resultInt
//        
//        // string Seq
//       
//        let resultStr =PSeq.take 2(seq ["str1";"str2";"str3";"str4"])
//     
//        let expectedStr = seq ["str1";"str2"]
//        VerifyPSeqsEqual expectedStr resultStr
//        
//        // empty Seq 
//        let resultEpt = PSeq.take 0 PSeq.empty 
//      
//        VerifyPSeqsEqual resultEpt PSeq.empty
//        
//         
//        // null Seq
//        CheckThrowsArgumentNullException(fun() -> PSeq.take 1 null |> ignore)
//        ()
        
    [<Test>]
    member this.takeWhile() =
        // integer Seq
        let funcInt x = (x < 6)
        let resultInt = PSeq.takeWhile funcInt (seq [1;2;4;5;6;7])
      
        let expectedInt = seq [1;2;4;5]
        VerifyPSeqsEqual expectedInt resultInt
        
        // string Seq
        let funcStr (x:string) = (x.Length < 4)
        let resultStr =PSeq.takeWhile funcStr (seq ["a"; "ab"; "abc"; "abcd"; "abcde"])
      
        let expectedStr = seq ["a"; "ab"; "abc"]
        VerifyPSeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = PSeq.takeWhile funcInt PSeq.empty 
        VerifyPSeqsEqual resultEpt PSeq.empty
        
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.takeWhile funcInt null |> ignore)
        ()
        
    [<Test>]
    member this.To_Array() =
        // integer Seq
        let resultInt = PSeq.toArray(seq [1;2;4;5;7])
     
        let expectedInt = [|1;2;4;5;7|]
        Assert.AreEqual(expectedInt,resultInt)
        
        // string Seq
        let resultStr =PSeq.toArray (seq ["str1";"str2";"str3"])
    
        let expectedStr =  [|"str1";"str2";"str3"|]
        Assert.AreEqual(expectedStr,resultStr)
        
        // empty Seq 
        let resultEpt = PSeq.toArray PSeq.empty 
        Assert.AreEqual([||],resultEpt)
        
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.toArray null |> ignore)
        ()
    
    [<Test>]
    member this.To_List() =
        // integer Seq
        let resultInt = PSeq.toList (seq [1;2;4;5;7])
        let expectedInt = [1;2;4;5;7]
        Assert.AreEqual(expectedInt,resultInt)
        
        // string Seq
        let resultStr =PSeq.toList (seq ["str1";"str2";"str3"])
        let expectedStr =  ["str1";"str2";"str3"]
        Assert.AreEqual(expectedStr,resultStr)
        
        // empty Seq 
        let resultEpt = PSeq.toList PSeq.empty 
        Assert.AreEqual([],resultEpt)
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.toList null |> ignore)
        ()
        
    [<Test>]
    member this.Truncate() =
        // integer Seq
        let resultInt = PSeq.truncate 3 (seq [1;2;4;5;7])
        let expectedInt = [1;2;4]
        VerifyPSeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr =PSeq.truncate 2 (seq ["str1";"str2";"str3"])
        let expectedStr =  ["str1";"str2"]
        VerifyPSeqsEqual expectedStr resultStr
        
        // empty Seq 
        let resultEpt = PSeq.truncate 0 PSeq.empty
        VerifyPSeqsEqual PSeq.empty resultEpt
        
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.truncate 1 null |> ignore)
        ()
        
    [<Test>]
    member this.tryFind() =
        // integer Seq
        let resultInt = PSeq.tryFind (fun x -> (x%2=0)) (seq [1;2;4;5;7])
        Assert.AreEqual(Some(2), resultInt)
        
         // integer Seq - None
        let resultInt = PSeq.tryFind (fun x -> (x%2=0)) (seq [1;3;5;7])
        Assert.AreEqual(None, resultInt)
        
        // string Seq
        let resultStr = PSeq.tryFind (fun (x:string) -> x.Contains("2")) (seq ["str1";"str2";"str3"])
        Assert.AreEqual(Some("str2"),resultStr)
        
         // string Seq - None
        let resultStr = PSeq.tryFind (fun (x:string) -> x.Contains("2")) (seq ["str1";"str4";"str3"])
        Assert.AreEqual(None,resultStr)
       
        
        // empty Seq 
        let resultEpt = PSeq.tryFind (fun x -> (x%2=0)) PSeq.empty
        Assert.AreEqual(None,resultEpt)

        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.tryFind (fun x -> (x%2=0))  null |> ignore)
        ()
        
    [<Test>]
    member this.TryFindIndex() =

        // integer Seq
        let resultInt = PSeq.tryFindIndex (fun x -> (x % 5 = 0)) [8; 9; 10]
        Assert.AreEqual(Some(2), resultInt)
        
         // integer Seq - None
        let resultInt = PSeq.tryFindIndex (fun x -> (x % 5 = 0)) [9;3;11]
        Assert.AreEqual(None, resultInt)
        
        // string Seq
        let resultStr = PSeq.tryFindIndex (fun (x:string) -> x.Contains("2")) ["str1"; "str2"; "str3"]
        Assert.AreEqual(Some(1),resultStr)
        
         // string Seq - None
        let resultStr = PSeq.tryFindIndex (fun (x:string) -> x.Contains("2")) ["str1"; "str4"; "str3"]
        Assert.AreEqual(None,resultStr)
       
        
        // empty Seq 
        let resultEpt = PSeq.tryFindIndex (fun x -> (x%2=0)) PSeq.empty
        Assert.AreEqual(None, resultEpt)
        
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.tryFindIndex (fun x -> (x % 2 = 0))  null |> ignore)
        ()
        
//    [<Test>]
//    member this.Unfold() =
//        // integer Seq
//        
//        let resultInt = PSeq.unfold (fun x -> if x = 1 then Some(7,2) else  None) 1
//        
//        VerifyPSeqsEqual (seq [7]) resultInt
//          
//        // string Seq
//        let resultStr =PSeq.unfold (fun (x:string) -> if x.Contains("unfold") then Some("a","b") else None) "unfold"
//        VerifyPSeqsEqual (seq ["a"]) resultStr
//        ()
//        
        
//    [<Test>]
//    member this.Windowed() =
//        // integer Seq
//        let resultInt = PSeq.windowed 5 (seq [1..10])
//        let expectedInt = 
//            seq { for i in 1..6 do
//                    yield [| i; i+1; i+2; i+3; i+4 |] }
//        VerifyPSeqsEqual expectedInt resultInt
//        
//        // string Seq
//        let resultStr =PSeq.windowed 2 (seq ["str1";"str2";"str3";"str4"])
//        let expectedStr = seq [ [|"str1";"str2"|];[|"str2";"str3"|];[|"str3";"str4"|]]
//        VerifyPSeqsEqual expectedStr resultStr
//      
//        // empty Seq 
//        let resultEpt = PSeq.windowed 2 PSeq.empty
//        VerifyPSeqsEqual PSeq.empty resultEpt
//          
//        // null Seq
//        CheckThrowsArgumentNullException(fun() -> PSeq.windowed 2 null |> ignore)
//        ()
        
    [<Test>]
    member this.Zip() =
    
        // integer Seq
        let resultInt = PSeq.zip (seq [1..7]) (seq [11..17])
        let expectedInt = 
            seq { for i in 1..7 do
                    yield i, i+10 }
        VerifyPSeqsEqual expectedInt resultInt
        
        // string Seq
        let resultStr =PSeq.zip (seq ["str3";"str4"]) (seq ["str1";"str2"])
        let expectedStr = seq ["str3","str1";"str4","str2"]
        VerifyPSeqsEqual expectedStr resultStr
      
        // empty Seq 
        let resultEpt = PSeq.zip PSeq.empty PSeq.empty
        VerifyPSeqsEqual PSeq.empty resultEpt
          
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.zip null null |> ignore)
        CheckThrowsArgumentNullException(fun() -> PSeq.zip null (seq [1..7]) |> ignore)
        CheckThrowsArgumentNullException(fun() -> PSeq.zip (seq [1..7]) null |> ignore)
        ()
        
//    [<Test>]
//    member this.Zip3() =
//        // integer Seq
//        let resultInt = PSeq.zip3 (seq [1..7]) (seq [11..17]) (seq [21..27])
//        let expectedInt = 
//            seq { for i in 1..7 do
//                    yield i, (i + 10), (i + 20) }
//        VerifyPSeqsEqual expectedInt resultInt
//        
//        // string Seq
//        let resultStr =PSeq.zip3 (seq ["str1";"str2"]) (seq ["str11";"str12"]) (seq ["str21";"str22"])
//        let expectedStr = seq ["str1","str11","str21";"str2","str12","str22" ]
//        VerifyPSeqsEqual expectedStr resultStr
//      
//        // empty Seq 
//        let resultEpt = PSeq.zip3 PSeq.empty PSeq.empty PSeq.empty
//        VerifyPSeqsEqual PSeq.empty resultEpt
//          
//        // null Seq
//        CheckThrowsArgumentNullException(fun() -> PSeq.zip3 null null null |> ignore)
//        CheckThrowsArgumentNullException(fun() -> PSeq.zip3 null (seq [1..7]) (seq [1..7]) |> ignore)
//        CheckThrowsArgumentNullException(fun() -> PSeq.zip3 (seq [1..7]) null (seq [1..7]) |> ignore)
//        CheckThrowsArgumentNullException(fun() -> PSeq.zip3 (seq [1..7]) (seq [1..7]) null |> ignore)
//        ()
        
//    [<Test>]
//    member this.tryPick() =
//         // integer Seq
//        let resultInt = PSeq.tryPick (fun x-> if x = 1 then Some("got") else None) (seq [1..5])
//         
//        Assert.AreEqual(Some("got"),resultInt)
//        
//        // string Seq
//        let resultStr = PSeq.tryPick (fun x-> if x = "Are" then Some("got") else None) (seq ["Lists"; "Are"])
//        Assert.AreEqual(Some("got"),resultStr)
//        
//        // empty Seq   
//        let resultEpt = PSeq.tryPick (fun x-> if x = 1 then Some("got") else None) PSeq.empty
//        Assert.IsNull(resultEpt)
//       
//        // null Seq
//        let nullSeq : seq<'a> = null 
//        let funcNull x = Some(1)
//        
//        CheckThrowsArgumentNullException(fun () -> PSeq.tryPick funcNull nullSeq |> ignore)
//   
//        ()