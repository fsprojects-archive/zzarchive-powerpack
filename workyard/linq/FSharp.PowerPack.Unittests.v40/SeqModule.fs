
namespace FSharp.Core.Unittests.FSharp_Core.Microsoft_FSharp_Collections

open System
open NUnit.Framework
open Microsoft.FSharp.Collections

open FSharp.Core.Unittests.LibraryTestFx

// Various tests for the:
// Microsoft.FSharp.Collections.seq type

(*
[Test Strategy]
Make sure each method works on:
* Integer Seq (value type)
* String Seq  (reference type)
* Empty Seq   (0 elements)
* Null Seq    (null)
*)

[<TestFixture>]
type SeqModule() =

//    [<Test>]
//    member this.CachedSeq_Clear() =
//     
//        let evaluatedItems : int list ref = ref []
//        let cachedSeq = 
//            PSeq.initInfinite (fun i -> evaluatedItems := i :: !evaluatedItems; i)
//            |> PSeq.cache
//        
//        // Verify no items have been evaluated from the Seq yet
//        Assert.AreEqual(List.length !evaluatedItems, 0)
//        
//        // Force evaluation of 10 elements
//        PSeq.take 10 cachedSeq
//        |> PSeq.toList
//        |> ignore
//        
//        // verify ref clear switch length
//        Assert.AreEqual(List.length !evaluatedItems, 10)
//
//        // Force evaluation of 10 elements
//        PSeq.take 10 cachedSeq
//        |> PSeq.toList
//        |> ignore
//        
//        // Verify ref clear switch length (should be cached)
//        Assert.AreEqual(List.length !evaluatedItems, 10)
//
//        
//        // Clear
//        (box cachedSeq :?> System.IDisposable) .Dispose()
//        
//        // Force evaluation of 10 elements
//        PSeq.take 10 cachedSeq
//        |> PSeq.toList
//        |> ignore
//        
//        // Verify length of evaluatedItemList is 20
//        Assert.AreEqual(List.length !evaluatedItems, 20)
//        ()
        
    [<Test>]
    member this.Append() =

        // empty Seq 
        let emptySeq1 = PSeq.empty
        let emptySeq2 = PSeq.empty
        let appendEmptySeq = PSeq.append emptySeq1 emptySeq2
        let expectResultEmpty = PSeq.empty
           
        VerifyPSeqsEqual expectResultEmpty appendEmptySeq
          
        // Integer Seq  
        let integerSeq1:seq<int> = seq [0..4]
        let integerSeq2:seq<int> = seq [5..9]
         
        let appendIntergerSeq = PSeq.append integerSeq1 integerSeq2
       
        let expectResultInteger = seq { for i in 0..9 -> i}
        
        VerifyPSeqsEqual expectResultInteger appendIntergerSeq
        
        
        // String Seq
        let stringSeq1:seq<string> = seq ["1";"2"]
        let stringSeq2:seq<string> = seq ["3";"4"]
        
        let appendStringSeq = PSeq.append stringSeq1 stringSeq2
        
        let expectedResultString = seq ["1";"2";"3";"4"]
        
        VerifyPSeqsEqual expectedResultString appendStringSeq
        
        // null Seq
        let nullSeq1 = seq [null;null]

        let nullSeq2 =seq [null;null]

        let appendNullSeq = PSeq.append nullSeq1 nullSeq2
        
        let expectedResultNull = seq [ null;null;null;null]
        
        VerifyPSeqsEqual expectedResultNull appendNullSeq
        ()
        
        
    [<Test>]
    member this.Average() =
        // empty Seq 
        let emptySeq:pseq<double> = PSeq.empty<double>
        
        CheckThrowsInvalidOperationExn (fun () ->  PSeq.average emptySeq |> ignore)
        
            
        // double Seq
        let doubleSeq:seq<double> = seq [1.0;2.2;2.5;4.3]
        
        let averageDouble = PSeq.average doubleSeq
        
        Assert.IsFalse( averageDouble <> 2.5)
        
        // float32 Seq
        let floatSeq:seq<float32> = seq [ 2.0f;4.4f;5.0f;8.6f]
        
        let averageFloat = PSeq.average floatSeq
        
        Assert.IsFalse( averageFloat <> 5.0f)
        
        // decimal Seq
        let decimalSeq:seq<decimal> = seq [ 0M;19M;19.03M]
        
        let averageDecimal = PSeq.average decimalSeq
        
        Assert.IsFalse( averageDecimal <> 12.676666666666666666666666667M )
        
        // null Seq
        let nullSeq:seq<double> = null
            
        CheckThrowsArgumentNullException (fun () -> PSeq.average nullSeq |> ignore) 
        ()
        
        
    [<Test>]
    member this.AverageBy() =
        // empty Seq 
        let emptySeq:pseq<double> = PSeq.empty<double>
        
        CheckThrowsInvalidOperationExn (fun () ->  PSeq.averageBy (fun x -> x+1.0) emptySeq |> ignore)
        
        // double Seq
        let doubleSeq:seq<double> = seq [1.0;2.2;2.5;4.3]
        
        let averageDouble = PSeq.averageBy (fun x -> x-2.0) doubleSeq
        
        Assert.IsFalse( averageDouble <> 0.5 )
        
        // float32 Seq
        let floatSeq:seq<float32> = seq [ 2.0f;4.4f;5.0f;8.6f]
        
        let averageFloat = PSeq.averageBy (fun x -> x*3.3f)  floatSeq
        
        Assert.IsFalse( averageFloat <> 16.5f )
        
        // decimal Seq
        let decimalSeq:seq<decimal> = seq [ 0M;19M;19.03M]
        
        let averageDecimal = PSeq.averageBy (fun x -> x/10.7M) decimalSeq
        
        Assert.IsFalse( averageDecimal <> 1.1847352024922118380062305296M )
        
        // null Seq
        let nullSeq:seq<double> = null
            
        CheckThrowsArgumentNullException (fun () -> PSeq.averageBy (fun (x:double)->x+4.0) nullSeq |> ignore) 
        ()
        
//    [<Test>]
//    member this.Cache() =
//        // empty Seq 
//        let emptySeq:pseq<double> = PSeq.empty<double>
//        
//        let cacheEmpty = PSeq.cache emptySeq
//        
//        let expectedResultEmpty = PSeq.empty
//        
//        VerifyPSeqsEqual expectedResultEmpty cacheEmpty
//               
//        // double Seq
//        let doubleSeq:seq<double> = seq [1.0;2.2;2.5;4.3]
//        
//        let cacheDouble = PSeq.cache doubleSeq
//        
//        VerifyPSeqsEqual doubleSeq cacheDouble
//        
//            
//        // float32 Seq
//        let floatSeq:seq<float32> = seq [ 2.0f;4.4f;5.0f;8.6f]
//        
//        let cacheFloat = PSeq.cache floatSeq
//        
//        VerifyPSeqsEqual floatSeq cacheFloat
//        
//        // decimal Seq
//        let decimalSeq:seq<decimal> = seq [ 0M; 19M; 19.03M]
//        
//        let cacheDecimal = PSeq.cache decimalSeq
//        
//        VerifyPSeqsEqual decimalSeq cacheDecimal
//        
//        // null Seq
//        let nullSeq = seq [null]
//        
//        let cacheNull = PSeq.cache nullSeq
//        
//        VerifyPSeqsEqual nullSeq cacheNull
//        ()

    [<Test>]
    member this.Case() =

        // integer Seq
        let integerArray = [|1;2|]
        let integerSeq = PSeq.cast integerArray
        
        let expectedIntegerSeq = seq [1;2]
        
        VerifyPSeqsEqual expectedIntegerSeq integerSeq
        
        // string Seq
        let stringArray = [|"a";"b"|]
        let stringSeq = PSeq.cast stringArray
        
        let expectedStringSeq = seq["a";"b"]
        
        VerifyPSeqsEqual expectedStringSeq stringSeq
        
        // empty Seq
        let emptySeq = PSeq.cast PSeq.empty
        let expectedEmptySeq = PSeq.empty
        
        VerifyPSeqsEqual expectedEmptySeq PSeq.empty
        
        // null Seq
        let nullArray = [|null;null|]
        let NullSeq = PSeq.cast nullArray
        let expectedNullSeq = seq [null;null]
        
        VerifyPSeqsEqual expectedNullSeq NullSeq
        
        
        ()
        
    [<Test>]
    member this.Choose() =
        
        // int Seq
        let intSeq = seq [1..20]    
        let funcInt x = if (x%5=0) then Some x else None       
        let intChoosed = PSeq.choose funcInt intSeq
        let expectedIntChoosed = seq { for i = 1 to 4 do yield i*5}
        
        
       
        VerifyPSeqsEqual expectedIntChoosed intChoosed
        
        // string Seq
        let stringSrc = seq ["list";"List"]
        let funcString x = match x with
                           | "list"-> Some x
                           | "List" -> Some x
                           | _ -> None
        let strChoosed = PSeq.choose funcString stringSrc   
        let expectedStrChoose = seq ["list";"List"]
      
        VerifyPSeqsEqual expectedStrChoose strChoosed
        
        // empty Seq
        let emptySeq = PSeq.empty
        let emptyChoosed = PSeq.choose funcInt emptySeq
        
        let expectedEmptyChoose = PSeq.empty
        
        VerifyPSeqsEqual expectedEmptyChoose emptySeq
        

        // null Seq
        let nullSeq:seq<'a> = null    
        
        CheckThrowsArgumentNullException (fun () -> PSeq.choose funcInt nullSeq |> ignore) 
        ()
    
//    [<Test>]
//    member this.Compare() =
//    
//        // int Seq
//        let intSeq1 = seq [1;3;7;9]    
//        let intSeq2 = seq [2;4;6;8] 
//        let funcInt x y = if (x>y) then x else 0
//        let intcompared = PSeq.compareWith funcInt intSeq1 intSeq2
//       
//        Assert.IsFalse( intcompared <> 7 )
//        
//        // string Seq
//        let stringSeq1 = seq ["a"; "b"]
//        let stringSeq2 = seq ["c"; "d"]
//        let funcString x y = match (x,y) with
//                             | "a", "c" -> 0
//                             | "b", "d" -> 1
//                             |_         -> -1
//        let strcompared = PSeq.compareWith funcString stringSeq1 stringSeq2  
//        Assert.IsFalse( strcompared <> 1 )
//         
//        // empty Seq
//        let emptySeq = PSeq.empty
//        let emptycompared = PSeq.compareWith funcInt emptySeq emptySeq
//        
//        Assert.IsFalse( emptycompared <> 0 )
//       
//        // null Seq
//        let nullSeq:seq<int> = null    
//         
//        CheckThrowsArgumentNullException (fun () -> PSeq.compareWith funcInt nullSeq emptySeq |> ignore)  
//        CheckThrowsArgumentNullException (fun () -> PSeq.compareWith funcInt emptySeq nullSeq |> ignore)  
//        CheckThrowsArgumentNullException (fun () -> PSeq.compareWith funcInt nullSeq nullSeq |> ignore)  
//
//        ()
        
    [<Test>]
    member this.Concat() =
         // integer Seq
        let seqInt = 
            seq { for i in 0..9 do                
                    yield seq {for j in 0..9 do
                                yield i*10+j}}
        let conIntSeq = PSeq.concat seqInt
        let expectedIntSeq = seq { for i in 0..99 do yield i}
        
        VerifyPSeqsEqual expectedIntSeq conIntSeq
         
        // string Seq
        let strSeq = 
            seq { for a in 'a' .. 'b' do
                    for b in 'a' .. 'b' do
                        yield seq [a; b] }
     
        let conStrSeq = PSeq.concat strSeq
        let expectedStrSeq = seq ['a';'a';'a';'b';'b';'a';'b';'b';]
        VerifyPSeqsEqual expectedStrSeq conStrSeq
        
//        // Empty Seq
//        let emptySeqs = seq [seq[ PSeq.empty;PSeq.empty];seq[ PSeq.empty;PSeq.empty]]
//        let conEmptySeq = PSeq.concat emptySeqs
//        let expectedEmptySeq =seq { for i in 1..4 do yield PSeq.empty}
//        
//        VerifyPSeqsEqual expectedEmptySeq conEmptySeq   

        // null Seq
        let nullSeq:seq<'a> = null
        
        CheckThrowsArgumentNullException (fun () -> PSeq.concat nullSeq  |> ignore) 
 
        () 
        
    [<Test>]
    member this.CountBy() =
        // integer Seq
        let funcIntCount_by (x:int) = x%3 
        let seqInt = 
            seq { for i in 0..9 do                
                    yield i}
        let countIntSeq = PSeq.countBy funcIntCount_by seqInt
         
        let expectedIntSeq = seq [0,4;1,3;2,3]
        
        VerifyPSeqsEqual expectedIntSeq countIntSeq
         
        // string Seq
        let funcStrCount_by (s:string) = s.IndexOf("key")
        let strSeq = seq [ "key";"blank key";"key";"blank blank key"]
       
        let countStrSeq = PSeq.countBy funcStrCount_by strSeq
        let expectedStrSeq = seq [0,2;6,1;12,1]
        VerifyPSeqsEqual expectedStrSeq countStrSeq
        
        // Empty Seq
        let emptySeq = PSeq.empty
        let countEmptySeq = PSeq.countBy funcIntCount_by emptySeq
        let expectedEmptySeq =seq []
        
        VerifyPSeqsEqual expectedEmptySeq countEmptySeq  

        // null Seq
        let nullSeq:seq<'a> = null
       
        CheckThrowsArgumentNullException (fun () -> PSeq.countBy funcIntCount_by nullSeq  |> ignore) 
        () 
    
    [<Test>]
    member this.Distinct() =
        
        // integer Seq
        let IntDistinctSeq =  
            seq { for i in 0..9 do                
                    yield i % 3 }
       
        let DistinctIntSeq = PSeq.distinct IntDistinctSeq
       
        let expectedIntSeq = seq [0;1;2]
        
        VerifyPSeqsEqual expectedIntSeq DistinctIntSeq
     
        // string Seq
        let strDistinctSeq = seq ["elementDup"; "ele1"; "ele2"; "elementDup"]
       
        let DistnctStrSeq = PSeq.distinct strDistinctSeq
        let expectedStrSeq = seq ["elementDup"; "ele1"; "ele2"]
        VerifyPSeqsEqual expectedStrSeq DistnctStrSeq

        // array Seq
        let arrDistinctSeq = seq [[|1|];[|1;2|]; [|1|];[|3|]]
       
        let DistnctArrSeq = PSeq.distinct arrDistinctSeq
        let expectedArrSeq = seq [[|1|]; [|1; 2|]; [|3|]]
        VerifyPSeqsEqual expectedArrSeq DistnctArrSeq
        
        
        // Empty Seq
        let emptySeq : pseq<decimal * unit>         = PSeq.empty
        let distinctEmptySeq : pseq<decimal * unit> = PSeq.distinct emptySeq
        let expectedEmptySeq : pseq<decimal * unit> = PSeq.ofList []
       
        VerifyPSeqsEqual expectedEmptySeq distinctEmptySeq

        // null Seq
        let nullSeq:seq<unit> = null
       
        CheckThrowsArgumentNullException(fun () -> PSeq.distinct nullSeq  |> ignore) 
        () 
    
    [<Test>]
    member this.DistinctBy () =
        // integer Seq
        let funcInt x = x % 3 
        let IntDistinct_bySeq =  
            seq { for i in 0..9 do                
                    yield i }
       
        let distinct_byIntSeq = PSeq.distinctBy funcInt IntDistinct_bySeq
        
        Assert.AreEqual(3, PSeq.length distinct_byIntSeq )
        
        let mappedBack = distinct_byIntSeq |> PSeq.map funcInt

        let expectedIntSeq = seq [0;1;2]
        
        VerifyPSeqsEqual expectedIntSeq mappedBack 
             
        // string Seq
        let funcStrDistinct (s:string) = s.IndexOf("key")
        let strSeq = seq [ "key"; "blank key"; "key dup"; "blank key dup"]
       
        let DistnctStrSeq = PSeq.distinctBy funcStrDistinct strSeq
        let expectedStrSeq = seq ["key"; "blank key"]
        VerifyPSeqsEqual expectedStrSeq DistnctStrSeq
        
        // Empty Seq
        let emptySeq            : pseq<int> = PSeq.empty
        let distinct_byEmptySeq : pseq<int> = PSeq.distinctBy funcInt emptySeq
        let expectedEmptySeq    : pseq<int> = PSeq.ofList []
       
        VerifyPSeqsEqual expectedEmptySeq distinct_byEmptySeq

        // null Seq
        let nullSeq : seq<'a> = null
       
        CheckThrowsArgumentNullException(fun () -> PSeq.distinctBy funcInt nullSeq  |> ignore) 
        () 
    
    [<Test>]
    member this.Exists() =

        // Integer Seq
        let funcInt x = (x % 2 = 0) 
        let IntexistsSeq =  
            seq { for i in 0..9 do                
                    yield i}
       
        let ifExistInt = PSeq.exists funcInt IntexistsSeq
        
        Assert.IsTrue( ifExistInt) 
            
        // String Seq
        let funcStr (s:string) = s.Contains("key")
        let strSeq = seq ["key"; "blank key"]
       
        let ifExistStr = PSeq.exists funcStr strSeq
        
        Assert.IsTrue( ifExistStr)
        
        // Empty Seq
        let emptySeq = PSeq.empty
        let ifExistsEmpty = PSeq.exists funcInt emptySeq
        
        Assert.IsFalse( ifExistsEmpty)
       
        

        // null Seq
        let nullSeq:seq<'a> = null
           
        CheckThrowsArgumentNullException (fun () -> PSeq.exists funcInt nullSeq |> ignore) 
        () 
    
    [<Test>]
    member this.Exists2() =
        // Integer Seq
        let funcInt x y = (x+y)%3=0 
        let Intexists2Seq1 =  seq [1;3;7]
        let Intexists2Seq2 = seq [1;6;3]
            
        let ifExist2Int = PSeq.exists2 funcInt Intexists2Seq1 Intexists2Seq2
        Assert.IsTrue( ifExist2Int)
             
        // String Seq
        let funcStr s1 s2 = ((s1 + s2) = "CombinedString")
        let strSeq1 = seq [ "Combined"; "Not Combined"] |> PSeq.ordered
        let strSeq2 = seq [ "String";   "Other String"] |> PSeq.ordered
        let ifexists2Str = PSeq.exists2 funcStr strSeq1 strSeq2
        Assert.IsTrue(ifexists2Str)
        
        // Empty Seq
        let emptySeq = PSeq.empty
        let ifexists2Empty = PSeq.exists2 funcInt emptySeq emptySeq
        Assert.IsFalse( ifexists2Empty)
       
        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> PSeq.exists2 funcInt nullSeq nullSeq |> ignore) 
        () 
    
    
    [<Test>]
    member this.Filter() =
        // integer Seq
        let funcInt x = if (x % 5 = 0) then true else false
        let IntSeq =
            seq { for i in 1..20 do
                    yield i }
                    
        let filterIntSeq = PSeq.filter funcInt IntSeq
          
        let expectedfilterInt = seq [ 5;10;15;20]
        
        VerifyPSeqsEqual expectedfilterInt filterIntSeq
        
        // string Seq
        let funcStr (s:string) = s.Contains("Expected Content")
        let strSeq = seq [ "Expected Content"; "Not Expected"; "Expected Content"; "Not Expected"]
        
        let filterStrSeq = PSeq.filter funcStr strSeq
        
        let expectedfilterStr = seq ["Expected Content"; "Expected Content"]
        
        VerifyPSeqsEqual expectedfilterStr filterStrSeq    
        // Empty Seq
        let emptySeq = PSeq.empty
        let filterEmptySeq = PSeq.filter funcInt emptySeq
        
        let expectedEmptySeq =seq []
       
        VerifyPSeqsEqual expectedEmptySeq filterEmptySeq
       
        

        // null Seq
        let nullSeq:seq<'a> = null
        
        CheckThrowsArgumentNullException (fun () -> PSeq.filter funcInt nullSeq  |> ignore) 
        () 
    
    [<Test>]
    member this.Find() =
        
        // integer Seq
        let funcInt x = if (x % 5 = 0) then true else false
        let IntSeq =
            seq { for i in 1..20 do
                    yield i }
                    
        let findInt = PSeq.find funcInt IntSeq
        Assert.AreEqual(findInt, 5)  
             
        // string Seq
        let funcStr (s:string) = s.Contains("Expected Content")
        let strSeq = seq [ "Expected Content";"Not Expected"]
        
        let findStr = PSeq.find funcStr strSeq
        Assert.AreEqual(findStr, "Expected Content")
        
        // Empty Seq
        let emptySeq = PSeq.empty
        
        CheckThrowsInvalidOperationExn (fun () -> PSeq.find funcInt emptySeq |> ignore)
       
        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> PSeq.find funcInt nullSeq |> ignore) 
        ()
    
    [<Test>]
    member this.FindIndex() =
        
        // integer Seq
        let digits = [1 .. 100] |> PSeq.ofList
        let idx = digits |> PSeq.findIndex (fun i -> i.ToString().Length > 1)
        Assert.AreEqual(idx, 9)

        // empty Seq 
        CheckThrowsInvalidOperationExn (fun () -> PSeq.findIndex (fun i -> true) PSeq.empty |> ignore)
         
        // null Seq
        CheckThrowsArgumentNullException(fun() -> PSeq.findIndex (fun i -> true) null |> ignore)
        ()
    
    [<Test>]
    member this.Pick() =
    
        let digits = [| 1 .. 10 |] |> PSeq.ofArray
        let result = PSeq.pick (fun i -> if i > 5 then Some(i.ToString()) else None) digits
        Assert.AreEqual(result, "6")
        
        // Empty seq (Bugged, 4173)
        CheckThrowsKeyNotFoundException (fun () -> PSeq.pick (fun i -> Some('a')) ([| |] : int[]) |> ignore)

        // Null
        CheckThrowsArgumentNullException (fun () -> PSeq.pick (fun i -> Some(i + 0)) null |> ignore)
        ()
        
    [<Test>]
    member this.Fold() =
        let funcInt x y = x+y
             
        let IntSeq =
            seq { for i in 1..10 do
                    yield i}
                    
        let foldInt = PSeq.fold funcInt 1 IntSeq
        if foldInt <> 56 then Assert.Fail()
        
        // string Seq
        let funcStr (x:string) (y:string) = x+y
        let strSeq = seq ["B"; "C";  "D" ; "E"]
        let foldStr = PSeq.fold  funcStr "A" strSeq
      
        if foldStr <> "ABCDE" then Assert.Fail()
        
        
        // Empty Seq
        let emptySeq = PSeq.empty
        let foldEmpty = PSeq.fold funcInt 1 emptySeq
        if foldEmpty <> 1 then Assert.Fail()

        // null Seq
        let nullSeq:seq<'a> = null
        
        CheckThrowsArgumentNullException (fun () -> PSeq.fold funcInt 1 nullSeq |> ignore) 
        () 
        
    [<Test>]
    member this.ForAll() =

        let funcInt x  = if x%2 = 0 then true else false
        let IntSeq =
            seq { for i in 1..10 do
                    yield i*2}
        let for_allInt = PSeq.forall funcInt  IntSeq
           
        if for_allInt <> true then Assert.Fail()
        
             
        // string Seq
        let funcStr (x:string)  = x.Contains("a")
        let strSeq = seq ["a"; "ab";  "abc" ; "abcd"]
        let for_allStr = PSeq.forall  funcStr strSeq
       
        if for_allStr <> true then Assert.Fail()
        
        
        // Empty Seq
        let emptySeq = PSeq.empty
        let for_allEmpty = PSeq.forall funcInt emptySeq
        
        if for_allEmpty <> true then Assert.Fail()
        
        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> PSeq.forall funcInt  nullSeq |> ignore) 
        () 
        
    [<Test>]
    member this.ForAll2() =

        let funcInt x y = if (x+y)%2 = 0 then true else false
        let IntSeq =
            seq { for i in 1..10 do
                    yield i}
            |> PSeq.ordered
                    
        let for_all2Int = PSeq.forall2 funcInt  IntSeq IntSeq
           
        if for_all2Int <> true then Assert.Fail()
        
        // string Seq
        let funcStr (x:string) (y:string)  = (x+y).Length = 5
        let strSeq1 = seq ["a"; "ab";  "abc" ; "abcd"] |> PSeq.ordered
        let strSeq2 = seq ["abcd"; "abc";  "ab" ; "a"] |> PSeq.ordered
        let for_all2Str = PSeq.forall2  funcStr strSeq1 strSeq2
       
        if for_all2Str <> true then Assert.Fail()
        
        // Empty Seq
        let emptySeq = PSeq.empty
        let for_all2Empty = PSeq.forall2 funcInt emptySeq emptySeq
        
        if for_all2Empty <> true then Assert.Fail()

        // null Seq
        let nullSeq:seq<'a> = null
        
        CheckThrowsArgumentNullException (fun () -> PSeq.forall2 funcInt  nullSeq nullSeq |> ignore) 
        
    [<Test>]
    member this.GroupBy() =
        
        let funcInt x = x%5
             
        let IntSeq =
            seq { for i in 0 .. 9 do
                    yield i }
                    
        let group_byInt = PSeq.groupBy funcInt IntSeq |> PSeq.map (fun (i, v) -> i, PSeq.toList v |> set)
        
        let expectedIntSeq = 
            seq { for i in 0..4 do
                     yield i, set [i; i+5] }
                   
        VerifyPSeqsEqual group_byInt expectedIntSeq
             
        // string Seq
        let funcStr (x:string) = x.Length
        let strSeq = seq ["length7"; "length 8";  "length7" ; "length  9"]
        
        let group_byStr = PSeq.groupBy  funcStr strSeq |> PSeq.map (fun (i, v) -> i, PSeq.toList v |> set)
        let expectedStrSeq = 
            seq {
                yield 7, set ["length7"; "length7"]
                yield 8, set ["length 8"]
                yield 9, set ["length  9"] }
       
        VerifyPSeqsEqual expectedStrSeq group_byStr


        // array keys
        let funcStr (x:string) = x.ToCharArray() |> Array.filter (fun c -> Char.IsUpper(c))
        let strSeq = seq ["Hello"; "Goodbye";  "Hello"; "How Are You?"]
        
        let group_byStr = PSeq.groupBy funcStr strSeq |> PSeq.map (fun (i, v) -> i, PSeq.toList v|> set)
        let expectedStrSeq = 
            seq {
                yield [|'H'|], set ["Hello"; "Hello"]
                yield [|'G'|], set ["Goodbye"]
                yield [|'H';'A';'Y'|], set ["How Are You?"] }
       
        VerifyPSeqsEqual expectedStrSeq group_byStr

        
//        // Empty Seq
//        let emptySeq = PSeq.empty
//        let group_byEmpty = PSeq.groupBy funcInt emptySeq
//        let expectedEmptySeq = seq []
//
//        VerifyPSeqsEqual expectedEmptySeq group_byEmpty
        
        // null Seq
        let nullSeq:seq<'a> = null
        CheckThrowsArgumentNullException (fun () -> PSeq.iter (fun _ -> ()) (PSeq.groupBy funcInt nullSeq)) 
        () 