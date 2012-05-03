namespace FSharp.PowerPack.Unittests

open NUnit.Framework
open System.Collections.Generic

#nowarn "40"

type ComparerA() = 
  interface IComparer<int> with
     override __.Compare(x,y) = compare (unbox<int> x) (unbox<int> y)

type ComparerB() = 
  interface IComparer<int> with
     override __.Compare(x,y) = compare (unbox<int> y) (unbox<int> x)

[<TestFixture>]
type public MapFunctorTests() =
  
    let Z5Map = Map.Make(fun (x:int) (y:int) -> compare (x mod 5) (y mod 5))
    let z5MapEqRange n m x = 

      for i = n to m do 
        test "ew9wef" ((Z5Map.find i x / 100) mod 5 = i mod 5);
      done;
      for i = n to m do 
        test "ew9wef" (match Z5Map.tryfind i x with Some n -> (n/100) % 5 = i % 5 | None -> failwith "test failed");
      done


    [<Test>]
    member this.BasicTests_z5test39342() = 
          let x = Z5Map.empty 
          let x = Z5Map.add 1 100 x
          let x = Z5Map.add 2 200 x
          let x = Z5Map.add 3 300 x
          let x = Z5Map.add 4 400 x
          let x = Z5Map.add 5 500 x
          let x = Z5Map.add 6 600 x
          let x = Z5Map.add 7 700 x
          let x = Z5Map.add 8 800 x
          let x = Z5Map.add 9 900 x
          let x = Z5Map.add 10 1000 x
          let x = Z5Map.add 11 1100 x
          let x = Z5Map.add 12 1200 x
          let x = Z5Map.add 13 1300 x
          let x = Z5Map.add 14 1400 x
          let x = Z5Map.add 15 1500 x
          z5MapEqRange 1 15 x 

    [<Test>]
    member this.BasicTests_z5test39343() = 
          let x = Z5Map.empty 
          let x = Z5Map.add 15 1500 x
          let x = Z5Map.add 14 1400 x
          let x = Z5Map.add 13 1300 x
          let x = Z5Map.add 12 1200 x
          let x = Z5Map.add 11 1100 x
          let x = Z5Map.add 10 1000 x
          let x = Z5Map.add 9 900 x
          let x = Z5Map.add 8 800 x
          let x = Z5Map.add 7 700 x
          let x = Z5Map.add 6 600 x
          let x = Z5Map.add 5 500 x
          let x = Z5Map.add 4 400 x
          let x = Z5Map.add 3 300 x
          let x = Z5Map.add 2 200 x
          let x = Z5Map.add 1 100 x
          z5MapEqRange 1 15 x 

    [<Test>]
    member this.BasicTests_z5test39344() = 
          let x = Z5Map.empty 
          let x = Z5Map.add 4 400 x
          z5MapEqRange 4 4 x 


    [<Test>]
    member this.BasicTests_z5test39345() = 
        let x = Z5Map.empty 
        let x = Z5Map.add 4 400 x
        let x = Z5Map.add 4 400 x
        z5MapEqRange 4 4 x 


    [<Test>]
    member this.BasicTests_z5test39346() = 
        let x = Z5Map.empty 
        let x = Z5Map.add 4 400 x
        let x = Z5Map.remove 4 x
        z5MapEqRange 4 3 x 


    [<Test>]
    member this.BasicTests_z5test39347() = 
        let x = Z5Map.empty 
        let x = Z5Map.add 1 100 x
        let x = Z5Map.add 2 200 x
        let x = Z5Map.add 3 300 x
        let x = Z5Map.add 4 400 x
        let x = Z5Map.add 5 500 x
        let x = Z5Map.remove 0 x
        let x = Z5Map.remove 2 x
        let x = Z5Map.remove 1 x
        z5MapEqRange 3 4 x 

    [<Test>]
    member this.BasicTests_Comparer() = 
        check "4i4cnio1" (Z5Map.empty.Comparer.Compare (2,7)) 0
        check "4i4cnio2" (Z5Map.empty.Comparer.Compare (2,3)) -1
        check "4i4cnio3" (Z5Map.empty.Comparer.Compare (2,8)) -1
        check "4i4cnio4" (Z5Map.empty.Comparer.Compare (3,2)) 1
        check "4i4cnio5" (Z5Map.empty.Comparer.Compare (8,2)) 1

    [<Test>]
    member this.Repro4884() = 
        (* #r "FSharp.PowerPack" *)
        let m0 = Tagged.Map<int,int,Comparer<int>>.Empty(Comparer.Default)
        let m1 = Tagged.Map<int,int,Comparer<int>>.Empty(Comparer.Default)
        let mres = m0 > m1
        do check "set of sets comparison" false mres

type intSet = int Tagged.Set
type intSetSet = intSet Tagged.Set

[<TestFixture>]
type public SetFunctorTests() =
  
    let Z3Set = Set.Make(fun (x:int) (y:int) -> compare (x mod 3) (y mod 3))

    [<Test>]
    member this.BasicTests1() = 

        do check "set comparison" 0 (Z3Set.compare Z3Set.empty Z3Set.empty)
        do check "set comparison" 0   (Z3Set.compare (Z3Set.add 1 Z3Set.empty) (Z3Set.add 1 Z3Set.empty))
        do check "set comparison" 0   (Z3Set.compare (Z3Set.add 3 Z3Set.empty) (Z3Set.add 0 Z3Set.empty))
        do check "set comparison" 0   (Z3Set.compare (Z3Set.add 1 (Z3Set.add 2 Z3Set.empty)) (Z3Set.add 2 (Z3Set.add 1 Z3Set.empty)))
        do check "set comparison" 0   (Z3Set.compare (Z3Set.add 1 (Z3Set.add 2 (Z3Set.add 3 Z3Set.empty))) (Z3Set.add 3 (Z3Set.add 2 (Z3Set.add 1 Z3Set.empty))))
        do check "set comparison" 0   (Z3Set.compare (Z3Set.add 1 (Z3Set.add 2 (Z3Set.add 0 Z3Set.empty))) (Z3Set.add 3 (Z3Set.add 2 (Z3Set.add 1 Z3Set.empty))))
           
           
        do check "set comparison" false (0 = Z3Set.compare Z3Set.empty (Z3Set.add 1 Z3Set.empty))
        do check "set comparison" false  (0 = Z3Set.compare (Z3Set.add 1 Z3Set.empty) (Z3Set.add 2 Z3Set.empty))
        do check "set comparison" false  (0 = Z3Set.compare (Z3Set.add 0 (Z3Set.add 1 Z3Set.empty)) (Z3Set.add 2 (Z3Set.add 1 Z3Set.empty)))
        do check "set comparison" false  (0 = Z3Set.compare (Z3Set.add 1 (Z3Set.add 0 (Z3Set.add 1 Z3Set.empty))) (Z3Set.add 5 (Z3Set.add 4 (Z3Set.add 3 Z3Set.empty))))


        let Z3SetSet = Set.Make(Z3Set.compare)

        let one = Z3Set.add 0 Z3Set.empty
        let two = Z3Set.add 1 one
        let three = Z3Set.add 2 two

        do check "set of sets comparison" 0 (Z3SetSet.compare Z3SetSet.empty Z3SetSet.empty)
        do check "set of sets comparison" 0   (Z3SetSet.compare (Z3SetSet.add one Z3SetSet.empty) (Z3SetSet.add one Z3SetSet.empty))
        do check "set of sets comparison" 0   (Z3SetSet.compare (Z3SetSet.add one (Z3SetSet.add two Z3SetSet.empty)) (Z3SetSet.add two (Z3SetSet.add one Z3SetSet.empty)))
        do check "set of sets comparison" 0   (Z3SetSet.compare (Z3SetSet.add one (Z3SetSet.add two (Z3SetSet.add three Z3SetSet.empty))) (Z3SetSet.add three (Z3SetSet.add two (Z3SetSet.add one Z3SetSet.empty))))
           
        do check "set of sets comparison" false (0 = Z3SetSet.compare Z3SetSet.empty (Z3SetSet.add one Z3SetSet.empty))
        do check "set of sets comparison" false  (0 = Z3SetSet.compare (Z3SetSet.add one Z3SetSet.empty) (Z3SetSet.add two Z3SetSet.empty))
        do check "set of sets comparison" false  (0 = Z3SetSet.compare (Z3SetSet.add one (Z3SetSet.add two Z3SetSet.empty)) (Z3SetSet.add three (Z3SetSet.add one Z3SetSet.empty)))

        let checkReflexive f x y = (f x y = - f y x)
        do check "set of sets comparison" true (checkReflexive Z3SetSet.compare Z3SetSet.empty (Z3SetSet.add one Z3SetSet.empty))
        do check "set of sets comparison" true (checkReflexive Z3SetSet.compare (Z3SetSet.add one Z3SetSet.empty) (Z3SetSet.add two Z3SetSet.empty))
        do check "set of sets comparison" true (checkReflexive Z3SetSet.compare (Z3SetSet.add one (Z3SetSet.add two Z3SetSet.empty)) (Z3SetSet.add three (Z3SetSet.add one Z3SetSet.empty)))

    member this.BasicTests_Comparer() = 
        check "4i4cnio1" (Z3Set.empty.Comparer.Compare (2,5)) 0
        check "4i4cnio2" (Z3Set.empty.Comparer.Compare (0,1)) -1
        check "4i4cnio3" (Z3Set.empty.Comparer.Compare (0,4)) -1
        check "4i4cnio4" (Z3Set.empty.Comparer.Compare (1,0)) 1
        check "4i4cnio5" (Z3Set.empty.Comparer.Compare (4,0)) 1

    [<Test>]
    member this.Repro4884() = 
        let s0 = Tagged.Set<int,Comparer<int>>.Empty(Comparer.Default)
        let s1 = Tagged.Set<int,Comparer<int>>.Empty(Comparer.Default)
        let sres = s0 > s1
        do  System.Console.WriteLine("Regression 4884")
        do check "set of sets comparison" false sres


    [<Test>]
    // Bug 4883: TaggedCollections should override Equals()
    member this.Repro4883() = 
        let compA = ComparerA()
        let nA1  = Tagged.Map<int,int,ComparerA>.Empty(compA)
        let nA2  = Tagged.Map<int,int,ComparerA>.Empty(compA)
             
        let mA1  = Tagged.Map<int,int,ComparerA>.Empty(ComparerA())
        let mA2  = Tagged.Map<int,int,ComparerA>.Empty(ComparerA())
        let mA1x = mA1.Add(1,1)
        let mA2x = mA2.Add(1,1)
        let mB1  = Tagged.Map<int,int,ComparerB>.Empty(ComparerB())
        let mB2  = Tagged.Map<int,int,ComparerB>.Empty(ComparerB())
        let mB1x = mB1.Add(1,1)
        let mB2x = mB2.Add(1,1)

        (*let check str x y = if x=y then printf "-OK-: %s\n" str else printf "FAIL: %s\n" str*)
             
        do  check "Bug4883.map.A1  = A1"   (mA1  = mA1)            true   (* empty     / empty     and identical *)
        do  check "Bug4883.map.A1  = A2"   (mA1  = mA2)            true   (* empty     / empty     and non-identical *)
        do  check "Bug4883.map.A1  = A1x"  (mA1  = mA1x)           false  (* empty     / non-empty *)
        do  check "Bug4883.map.A1x = A1x"  (mA1x = mA1x)           true   (* non-empty / non-empty and identical *)
        do  check "Bug4883.map.A1x = A2x"  (mA1x = mA2x)           true   (* non-empty / non-empty and non-identical *)

        do  check "Bug4883.map.A1  eq A1"  (mA1.Equals(box mA1))   true
        do  check "Bug4883.map.A1  eq A2"  (mA1.Equals(box mA2))   true
        do  check "Bug4883.map.A1  eq A1x" (mA1.Equals(box mA1x))  false
        do  check "Bug4883.map.A1x eq A1x" (mA1x.Equals(box mA1x)) true
        do  check "Bug4883.map.A1x eq A2x" (mA1x.Equals(box mA2x)) true

        do  check "Bug4883.map.A1  eq B1"  (mA1.Equals(box mB1))   false  (* empty     / empty     and different comparers *)
        do  check "Bug4883.map.A1  eq B1x" (mA1.Equals(box mB1))   false  (* empty     / non-empty and different comparers *)
        do  check "Bug4883.map.A1x eq B1x" (mA1x.Equals(box mB1))  false  (* non-empty / empty     and different comparers *)
        do  check "Bug4883.map.A1x eq B1x" (mA1x.Equals(box mB1x)) false  (* non-empty / non-empty and different comparers *)

        let sA1  = Tagged.Set<int,ComparerA>.Empty(ComparerA())
        let sA2  = Tagged.Set<int,ComparerA>.Empty(ComparerA())
        let sA1x = sA1.Add(1)
        let sA2x = sA2.Add(1)
        let sB1  = Tagged.Set<int,ComparerB>.Empty(ComparerB())
        let sB2  = Tagged.Set<int,ComparerB>.Empty(ComparerB())
        let sB1x = sB1.Add(1)
        let sB2x = sB2.Add(1)       

        do  check "Bug4883.map.A1  = A1"   (sA1  = sA1)            true   (* empty     / empty     and identical *)
        do  check "Bug4883.map.A1  = A2"   (sA1  = sA2)            true   (* empty     / empty     and non-identical *)
        do  check "Bug4883.map.A1  = A1x"  (sA1  = sA1x)           false  (* empty     / non-empty *)
        do  check "Bug4883.map.A1x = A1x"  (sA1x = sA1x)           true   (* non-empty / non-empty and identical *)
        do  check "Bug4883.map.A1x = A2x"  (sA1x = sA2x)           true   (* non-empty / non-empty and non-identical *)

        do  check "Bug4883.map.A1  eq A1"  (sA1.Equals(box sA1))   true
        do  check "Bug4883.map.A1  eq A2"  (sA1.Equals(box sA2))   true
        do  check "Bug4883.map.A1  eq A1x" (sA1.Equals(box sA1x))  false
        do  check "Bug4883.map.A1x eq A1x" (sA1x.Equals(box sA1x)) true
        do  check "Bug4883.map.A1x eq A2x" (sA1x.Equals(box sA2x)) true

        do  check "Bug4883.map.A1  eq B1"  (sA1.Equals(box sB1))   false  (* empty     / empty     and different comparers *)
        do  check "Bug4883.map.A1  eq B1x" (sA1.Equals(box sB1))   false  (* empty     / non-empty and different comparers *)
        do  check "Bug4883.map.A1x eq B1x" (sA1x.Equals(box sB1))  false  (* non-empty / empty     and different comparers *)
        do  check "Bug4883.map.A1x eq B1x" (sA1x.Equals(box sB1x)) false  (* non-empty / non-empty and different comparers *)

        do  System.Console.WriteLine("Regression 4883")

